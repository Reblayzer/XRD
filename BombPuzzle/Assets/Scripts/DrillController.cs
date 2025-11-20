using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class DrillController : MonoBehaviour
{
  [Header("Tip rotation")]
  public Transform rotatingTip; // the visual front part to rotate
  public float tipRPM = 1200f; // rotation speed in RPM
  public LayerMask screwLayer; // layer assigned to screws

  // Event: degrees rotated this frame (in degrees)
  public event Action<float> OnTipRotated;

  public bool IsActive { get; private set; }

  UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

  void Awake()
  {
    grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
  }

  void OnEnable()
  {
    if (grab != null)
    {
      grab.activated.AddListener(OnActivated);
      grab.deactivated.AddListener(OnDeactivated);
    }
  }

  void OnDisable()
  {
    if (grab != null)
    {
      grab.activated.RemoveListener(OnActivated);
      grab.deactivated.RemoveListener(OnDeactivated);
    }
  }

  void OnActivated(ActivateEventArgs args)
  {
    IsActive = true;
  }

  void OnDeactivated(DeactivateEventArgs args)
  {
    IsActive = false;
  }

  void Update()
  {
    if (rotatingTip == null) return;

    if (IsActive)
    {
      // convert RPM to degrees per frame
      float degreesThisFrame = tipRPM * 360f / 60f * Time.deltaTime;
      // rotate visually around local forward axis
      rotatingTip.Rotate(rotatingTip.forward, degreesThisFrame, Space.World);
      OnTipRotated?.Invoke(degreesThisFrame);
    }
  }
}
