using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bridge script placed on the drill tip collider. Supports both trigger and collision callbacks so
/// it works with MeshColliders (trigger or non-trigger). It keeps a set of currently-contacted
/// screws to avoid duplicate subscriptions.
/// </summary>
public class TipTriggerBridge : MonoBehaviour
{
  [Tooltip("Assign the DrillController on the drill root")]
  public DrillController drillController;

  // Track screws currently in contact so we don't double-subscribe
  HashSet<ScrewBehaviour> contacted = new HashSet<ScrewBehaviour>();

  void AddContact(ScrewBehaviour screw)
  {
    if (screw == null) return;
    if (drillController != null)
    {
      // Only accept screws that match the drill's allowed layer mask
      int screwLayer = screw.gameObject.layer;
      if ((drillController.screwLayer.value & (1 << screwLayer)) == 0)
        return;
    }
    if (contacted.Contains(screw)) return;
    contacted.Add(screw);
    screw.SetContact(true);
    screw.currentDrill = drillController;
    if (drillController != null)
      drillController.OnTipRotated += screw.OnTipRotation;
  }

  void RemoveContact(ScrewBehaviour screw)
  {
    if (screw == null) return;
    if (!contacted.Contains(screw)) return;
    contacted.Remove(screw);
    screw.SetContact(false);
    if (drillController != null)
      drillController.OnTipRotated -= screw.OnTipRotation;
    screw.currentDrill = null;
  }

  // Trigger-based callbacks (works if tip collider is set as trigger)
  void OnTriggerEnter(Collider other)
  {
    var screw = other.GetComponentInParent<ScrewBehaviour>();
    AddContact(screw);
  }

  void OnTriggerExit(Collider other)
  {
    var screw = other.GetComponentInParent<ScrewBehaviour>();
    RemoveContact(screw);
  }

  // Collision-based callbacks (works if using non-trigger MeshColliders)
  void OnCollisionEnter(Collision collision)
  {
    var screw = collision.collider.GetComponentInParent<ScrewBehaviour>();
    AddContact(screw);
  }

  void OnCollisionExit(Collision collision)
  {
    var screw = collision.collider.GetComponentInParent<ScrewBehaviour>();
    RemoveContact(screw);
  }

  // Safety: clear any stale contacts when the object is disabled/destroyed
  void OnDisable()
  {
    foreach (var screw in contacted)
    {
      if (screw != null)
      {
        screw.SetContact(false);
        if (drillController != null) drillController.OnTipRotated -= screw.OnTipRotation;
        screw.currentDrill = null;
      }
    }
    contacted.Clear();
  }
}
