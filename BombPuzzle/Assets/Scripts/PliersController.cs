using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public enum PliersSide { Left = 0, Right = 1 }

/// <summary>
/// Controls two-handed pliers: when both sides are grabbed the distance between the hands
/// maps to an open amount. Left hinge rotates from 0 to -maxAngle, Right from 0 to +maxAngle.
/// When the pliers are closed and both side colliders are touching the same WireCuttable, the wire is cut.
/// </summary>
public class PliersController : MonoBehaviour
{
  [Header("Hinges")]
  public Transform leftHinge;    // hinge transform to rotate (local Y)
  public Transform rightHinge;
  
  [Header("Angles")]
  public float maxOpenAngle = 20f; // degrees for each side (left will be negative)

  [Header("Cutting")]
  [Range(0f, 0.5f)] public float closeThreshold = 0.08f; // percent threshold considered "closed" (0..1)

  // runtime state
  bool isHeld = false;          // whether the pliers parent is currently held by the player
  bool isClosed = false;        // whether the pliers are currently closed (trigger pressed)

  Vector3 leftHingeStartEuler;
  Vector3 rightHingeStartEuler;

  // actual side transforms (the GameObjects that represent the left/right jaws).
  Transform leftSideTransform;
  Transform rightSideTransform;

  void Awake()
  {
    if (leftHinge != null) leftHingeStartEuler = leftHinge.localEulerAngles;
    if (rightHinge != null) rightHingeStartEuler = rightHinge.localEulerAngles;

    // try to auto-fill side transforms from PliersSideBridge children
    var bridges = GetComponentsInChildren<PliersSideBridge>();
    foreach (var b in bridges)
    {
      if (b == null) continue;
      if (b.side == PliersSide.Left && leftSideTransform == null) leftSideTransform = b.transform;
      if (b.side == PliersSide.Right && rightSideTransform == null) rightSideTransform = b.transform;
      // ensure the bridge has a reference to this controller
      if (b.controller == null) b.controller = this;
    }

    // If this GameObject has an XRGrabInteractable, subscribe to its events so parent-only grabbing works.
    var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    if (grab != null)
    {
      grab.selectEntered.AddListener(OnParentSelectEntered);
      grab.selectExited.AddListener(OnParentSelectExited);
      grab.activated.AddListener(OnParentActivated);
      grab.deactivated.AddListener(OnParentDeactivated);
    }
  }

  // XR event handlers for parent interactable
  void OnParentSelectEntered(SelectEnterEventArgs args)
  {
    OnPickedUp();
  }

  void OnParentSelectExited(SelectExitEventArgs args)
  {
    OnDropped();
  }

  void OnParentActivated(ActivateEventArgs args)
  {
    OnActivated();
  }

  void OnParentDeactivated(DeactivateEventArgs args)
  {
    OnDeactivated();
  }
  void Update()
  {
    // Apply simple single-hand behaviour based on hold/close state.
    if (isHeld)
    {
      if (isClosed)
      {
        // closed -> both sides at 0 relative angle
        SetHingeAngles(0f, 0f);
      }
      else
      {
        // open -> left negative, right positive
        SetHingeAngles(-maxOpenAngle, maxOpenAngle);
      }
    }
    else
    {
      // not held -> rest at 0
      SetHingeAngles(0f, 0f);
    }
  }


  // called by side bridges when a hand grabs/releases a side
  public void SetGrabbed(PliersSide side, bool grabbed, Transform hand)
  {
    // Simplified: any side being grabbed is treated as picking up the whole pliers.
    if (grabbed)
    {
      OnPickedUp();
    }
    else
    {
      OnDropped();
    }
  }

  // called by side bridges when their box collider enters/exits a WireCuttable
  public void SideTouchEnter(PliersSide side, WireCuttable wire)
  {
    // Track which wire each side is touching. If both sides are touching the same
    // wire and the jaws are currently closed while held, attempt to cut it.
    if (side == PliersSide.Left)
      leftTouchedWire = wire;
    else
      rightTouchedWire = wire;

    if (isHeld && isClosed)
      TryCutTouchedWires();
  }

  public void SideTouchExit(PliersSide side, WireCuttable wire)
  {
    // Clear tracking when a side leaves contact with a wire.
    if (side == PliersSide.Left)
    {
      if (leftTouchedWire == wire) leftTouchedWire = null;
    }
    else
    {
      if (rightTouchedWire == wire) rightTouchedWire = null;
    }
  }

  // ----- Simple single-hand API (can be wired to XRGrabInteractable UnityEvents) -----
  public void OnPickedUp()
  {
    isHeld = true;
    isClosed = false;
  }

  public void OnDropped()
  {
    isHeld = false;
    isClosed = false;
  }

  // called when the interactor activates (e.g. trigger pressed while holding)
  public void OnActivated()
  {
    if (!isHeld) return;
    isClosed = true;
    // When the user presses the trigger while holding the pliers, check for a
    // valid cut condition immediately.
    TryCutTouchedWires();
  }

  // called when the interactor deactivates (e.g. trigger released while holding)
  public void OnDeactivated()
  {
    if (!isHeld) return;
    isClosed = false;
  }

  void SetHingeAngles(float leftAngle, float rightAngle)
  {
    if (leftHinge != null)
    {
      var e = leftHingeStartEuler;
      e.y = leftHingeStartEuler.y + leftAngle;
      leftHinge.localEulerAngles = e;
    }
    if (rightHinge != null)
    {
      var e = rightHingeStartEuler;
      e.y = rightHingeStartEuler.y + rightAngle;
      rightHinge.localEulerAngles = e;
    }
  }

  // --- Cutting state ---
  // Tracks which WireCuttable (if any) each side is currently touching.
  WireCuttable leftTouchedWire;
  WireCuttable rightTouchedWire;

  void TryCutTouchedWires()
  {
    if (leftTouchedWire == null || rightTouchedWire == null) return;
    // require both sides to be touching the exact same wire instance
    if (!ReferenceEquals(leftTouchedWire, rightTouchedWire)) return;
    var wire = leftTouchedWire;
    if (wire.IsCut) return;
    // perform the cut
    wire.Cut();
  }
}
