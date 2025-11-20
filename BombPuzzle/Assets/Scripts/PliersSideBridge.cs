using UnityEngine;

/// <summary>
/// Attach this to the small box collider GameObject on each pliers side. The collider should be a trigger.
/// It forwards Select/Unselect (grab) and Trigger Enter/Exit events to the PliersController.
/// The side GameObject should also have an XRGrabInteractable to allow grabbing the side.
/// </summary>
public class PliersSideBridge : MonoBehaviour
{
  public PliersController controller;
  public PliersSide side = PliersSide.Left;

  // These methods are called by the XR system on select enter/exit if this object has an XRGrabInteractable
  public void OnSelectEntered(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
  {
    if (controller != null)
      controller.SetGrabbed(side, true, args.interactorObject.transform);
  }

  public void OnSelectExited(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
  {
    if (controller != null)
      controller.SetGrabbed(side, false, null);
  }

  void OnTriggerEnter(Collider other)
  {
    var w = other.GetComponentInParent<WireCuttable>();
    if (w != null && controller != null)
      controller.SideTouchEnter(side, w);
  }

  void OnTriggerExit(Collider other)
  {
    var w = other.GetComponentInParent<WireCuttable>();
    if (w != null && controller != null)
      controller.SideTouchExit(side, w);
  }
}
