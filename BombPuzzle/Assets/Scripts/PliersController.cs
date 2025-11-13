using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum PliersSide { Left = 0, Right = 1 }

/// <summary>
/// Controls two-handed pliers: when both sides are grabbed the distance between the hands
/// maps to an open amount. Left hinge rotates from 0 to -maxAngle, Right from 0 to +maxAngle.
/// When the pliers are closed and both side colliders are touching the same WireCuttable, the wire is cut.
/// </summary>
public class PliersController : MonoBehaviour
{
  public enum SingleHandMode { ExactCopy = 0, MirroredAroundPivot = 1 }
  [Header("Single-hand behavior")]
  public SingleHandMode singleHandMode = SingleHandMode.ExactCopy;

  [Header("Hinges")]
  public Transform leftHinge;    // hinge transform to rotate (local Y)
  public Transform rightHinge;
  [Header("Grab points")]
  public Transform leftGrabPoint; // the transform on the left side that is grabbed by the player
  public Transform rightGrabPoint; // the transform on the right side that is grabbed by the player

  [Header("Grab distance mapping (meters)")]
  public float closedDistance = 0.06f; // distance between grabs when pliers are closed
  public float openDistance = 0.20f;   // distance between grabs when pliers are fully open

  [Header("Angles")]
  public float maxOpenAngle = 20f; // degrees for each side (left will be negative)

  [Header("Cutting")]
  [Range(0f, 0.5f)] public float closeThreshold = 0.08f; // percent threshold considered "closed" (0..1)

  // runtime state
  bool leftGrabbed = false;
  bool rightGrabbed = false;
  Transform leftHand;
  Transform rightHand;
  // offsets to preserve root transform when a single hand grabs
  Vector3 leftHandOffsetPos;
  Quaternion leftHandOffsetRot;
  Vector3 rightHandOffsetPos;
  Quaternion rightHandOffsetRot;

  Vector3 leftHingeStartEuler;
  Vector3 rightHingeStartEuler;

  // track wires touched by each side
  HashSet<WireCuttable> leftTouched = new HashSet<WireCuttable>();
  HashSet<WireCuttable> rightTouched = new HashSet<WireCuttable>();

  // actual side transforms (the GameObjects that represent the left/right jaws). We try to auto-find these
  // from child PliersSideBridge components so we copy the visible jaw transforms directly.
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
    }

    // fallback: if left/right grab points are set in inspector and side transforms are missing, use those
    if (leftSideTransform == null && leftGrabPoint != null) leftSideTransform = leftGrabPoint;
    if (rightSideTransform == null && rightGrabPoint != null) rightSideTransform = rightGrabPoint;

    // Ensure any XRGrabInteractable on the side bridges uses the configured grab points as attach transforms.
    // Use reflection so this compiles even if XR Interaction Toolkit isn't present at compile time.
    try
    {
      // find the XRGrabInteractable type in loaded assemblies
      System.Type grabType = null;
      foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
      {
        grabType = asm.GetType("UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable");
        if (grabType != null) break;
      }

      if (grabType != null)
      {
        foreach (var b in bridges)
        {
          if (b == null) continue;
          var comp = b.gameObject.GetComponent(grabType);
          if (comp == null) continue;
          // set the attachTransform property if available
          var prop = grabType.GetProperty("attachTransform", BindingFlags.Public | BindingFlags.Instance);
          if (prop != null)
          {
            if (b.side == PliersSide.Left && leftGrabPoint != null) prop.SetValue(comp, leftGrabPoint);
            if (b.side == PliersSide.Right && rightGrabPoint != null) prop.SetValue(comp, rightGrabPoint);
          }
        }
      }
    }
    catch { /* fail silently if XR toolkit not present */ }
  }

  void Update()
  {
    // If only one side is grabbed, move/rotate the whole pliers so the grabbed point follows the hand
    if (leftGrabbed ^ rightGrabbed)
    {
      // preserve the relative offset between the grabbed hand and the pliers root
      if (leftGrabbed && leftHand != null)
      {
        transform.rotation = leftHand.rotation * leftHandOffsetRot;
        transform.position = leftHand.position + leftHand.rotation * leftHandOffsetPos;
        // update the other side depending on the selected single-hand mode
        Transform other = rightSideTransform != null ? rightSideTransform : rightGrabPoint;
        if (other != null)
        {
          if (singleHandMode == SingleHandMode.ExactCopy)
          {
            other.position = leftHand.position;
            other.rotation = leftHand.rotation;
          }
          else // MirroredAroundPivot
          {
            Vector3 centerWorld = (leftGrabPoint != null && rightGrabPoint != null)
              ? transform.TransformPoint((leftGrabPoint.localPosition + rightGrabPoint.localPosition) * 0.5f)
              : transform.position;
            Vector3 offset = leftHand.position - centerWorld;
            Vector3 syntheticPos = centerWorld - offset;
            other.position = syntheticPos;
            // mirror rotation around center by flipping direction toward center
            Vector3 mirroredForward = (centerWorld - leftHand.position).normalized;
            other.rotation = Quaternion.LookRotation(mirroredForward, leftHand.up);
          }
        }
      }
      else if (rightGrabbed && rightHand != null)
      {
        transform.rotation = rightHand.rotation * rightHandOffsetRot;
        transform.position = rightHand.position + rightHand.rotation * rightHandOffsetPos;
        // update the other side depending on the selected single-hand mode
        Transform other = leftSideTransform != null ? leftSideTransform : leftGrabPoint;
        if (other != null)
        {
          if (singleHandMode == SingleHandMode.ExactCopy)
          {
            other.position = rightHand.position;
            other.rotation = rightHand.rotation;
          }
          else // MirroredAroundPivot
          {
            Vector3 centerWorld = (leftGrabPoint != null && rightGrabPoint != null)
              ? transform.TransformPoint((leftGrabPoint.localPosition + rightGrabPoint.localPosition) * 0.5f)
              : transform.position;
            Vector3 offset = rightHand.position - centerWorld;
            Vector3 syntheticPos = centerWorld - offset;
            other.position = syntheticPos;
            Vector3 mirroredForward = (centerWorld - rightHand.position).normalized;
            other.rotation = Quaternion.LookRotation(mirroredForward, rightHand.up);
          }
        }
      }
      // do not change hinge angles while single-handed; they stay at last values
    }

    // only compute hinge open/close when both hands are grabbed
    if (leftGrabbed && rightGrabbed && leftHand != null && rightHand != null)
    {
      // copy full world transform from each real hand to its side so both sides follow their hands
      // copy to the actual visible side transforms if available (preferred), otherwise use grab points
      if (leftSideTransform != null)
      {
        leftSideTransform.position = leftHand.position;
        leftSideTransform.rotation = leftHand.rotation;
      }
      else if (leftGrabPoint != null)
      {
        leftGrabPoint.position = leftHand.position;
        leftGrabPoint.rotation = leftHand.rotation;
      }

      if (rightSideTransform != null)
      {
        rightSideTransform.position = rightHand.position;
        rightSideTransform.rotation = rightHand.rotation;
      }
      else if (rightGrabPoint != null)
      {
        rightGrabPoint.position = rightHand.position;
        rightGrabPoint.rotation = rightHand.rotation;
      }

      float dist = Vector3.Distance(leftHand.position, rightHand.position);
      float t = Mathf.InverseLerp(closedDistance, openDistance, dist);
      t = Mathf.Clamp01(t);

      // left angle goes 0 -> -maxOpenAngle
      float leftAngle = Mathf.Lerp(0f, -maxOpenAngle, t);
      float rightAngle = Mathf.Lerp(0f, maxOpenAngle, t);

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

      // check for any common touched wires and cut when closed enough
      if (t <= closeThreshold)
      {
        TryCutTouchedWires();
      }
    }
  }



  void TryCutTouchedWires()
  {
    if (leftTouched.Count == 0 || rightTouched.Count == 0) return;
    // find intersection
    foreach (var w in leftTouched)
    {
      if (w == null) continue;
      if (rightTouched.Contains(w) && !w.IsCut)
      {
        Debug.Log($"Pliers: cutting wire '{w.name}'");
        w.Cut();
      }
    }
  }

  // called by side bridges when a hand grabs/releases a side
  public void SetGrabbed(PliersSide side, bool grabbed, Transform hand)
  {
    if (side == PliersSide.Left)
    {
      leftGrabbed = grabbed;
      leftHand = grabbed ? hand : null;

      if (grabbed && leftHand != null && !rightGrabbed)
      {
        // store offset from hand to pliers root so we can preserve relative pose while following
        leftHandOffsetPos = Quaternion.Inverse(leftHand.rotation) * (transform.position - leftHand.position);
        leftHandOffsetRot = Quaternion.Inverse(leftHand.rotation) * transform.rotation;
      }
    }
    else
    {
      rightGrabbed = grabbed;
      rightHand = grabbed ? hand : null;

      if (grabbed && rightHand != null && !leftGrabbed)
      {
        rightHandOffsetPos = Quaternion.Inverse(rightHand.rotation) * (transform.position - rightHand.position);
        rightHandOffsetRot = Quaternion.Inverse(rightHand.rotation) * transform.rotation;
      }
    }
  }

  // called by side bridges when their box collider enters/exits a WireCuttable
  public void SideTouchEnter(PliersSide side, WireCuttable wire)
  {
    if (wire == null) return;
    if (side == PliersSide.Left) leftTouched.Add(wire);
    else rightTouched.Add(wire);
  }

  public void SideTouchExit(PliersSide side, WireCuttable wire)
  {
    if (wire == null) return;
    if (side == PliersSide.Left) leftTouched.Remove(wire);
    else rightTouched.Remove(wire);
  }
}
