using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LidBehaviour : MonoBehaviour
{
  [Tooltip("Total number of screws that hold this lid")]
  public int totalScrews = 4;
  int removedCount = 0;
  int contactCount = 0;
  Rigidbody rb;
  ScrewBehaviour[] screws;
  Transform screwsParent;
  Coroutine delayedReleaseCoroutine;

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
    rb.isKinematic = true;
    rb.useGravity = false;

    // Find screws that are under the same root as this lid (covers sibling screws under the same parent)
    var root = transform.root;
    screws = root.GetComponentsInChildren<ScrewBehaviour>(true);
    if (screws != null && screws.Length > 0)
    {
      totalScrews = screws.Length;
      contactCount = 0;
      foreach (var s in screws)
      {
        if (s == null) continue;
        s.OnContactChanged += OnScrewContactChanged;
        if (s.IsContact) contactCount++;
      }
      // record the parent container that originally held the screws (if any)
      screwsParent = screws[0].transform.parent;
      if (screwsParent != null) Debug.Log($"Lid '{name}' screwsParent set to '{screwsParent.name}' with {screwsParent.childCount} children");
      Debug.Log($"Lid '{name}' found {screws.Length} screws under root. totalScrews set to {totalScrews}. initial contactCount={contactCount}");
    }
    else
    {
      Debug.Log($"Lid '{name}' found no screws under root '{root.name}'");
    }
  }

  void OnDisable()
  {
    if (screws == null) return;
    foreach (var s in screws)
    {
      if (s == null) continue;
      s.OnContactChanged -= OnScrewContactChanged;
    }
  }

  void OnScrewContactChanged(bool contacting)
  {
    if (contacting) contactCount++; else contactCount--;
    contactCount = Mathf.Clamp(contactCount, 0, totalScrews);
    // Re-evaluate release conditions (debounced)
    CheckRelease();
  }

  public void NotifyScrewRemoved(ScrewBehaviour screw)
  {
    removedCount++;
    Debug.Log($"Lid '{name}' notified: screw removed ({removedCount}/{totalScrews})");
    // Re-evaluate release conditions (debounced)
    CheckRelease();
  }

  void CheckRelease()
  {
    // Immediate release if all screws were removed programmatically
    if (removedCount >= totalScrews)
    {
      ReleaseLid();
      return;
    }

    // If the original screws container is empty and none of the tracked screws are currently contacting, release after a short debounce
    bool parentEmpty = (screwsParent != null) && (screwsParent.childCount == 0);
    bool anyContact = false;
    if (screws != null)
    {
      foreach (var s in screws)
      {
        if (s == null) continue;
        if (s.IsContact) { anyContact = true; break; }
      }
    }

    if (parentEmpty && !anyContact)
    {
      StartDelayedRelease();
    }
    else
    {
      CancelDelayedRelease();
    }
  }

  void StartDelayedRelease()
  {
    if (delayedReleaseCoroutine != null) return;
    delayedReleaseCoroutine = StartCoroutine(DelayedReleaseCoroutine());
  }

  void CancelDelayedRelease()
  {
    if (delayedReleaseCoroutine == null) return;
    StopCoroutine(delayedReleaseCoroutine);
    delayedReleaseCoroutine = null;
  }

  System.Collections.IEnumerator DelayedReleaseCoroutine()
  {
    // small debounce to avoid transient releases when the tip briefly touches or colliders overlap
    yield return new WaitForSeconds(0.2f);
    // re-check conditions
    bool parentEmpty = (screwsParent != null) && (screwsParent.childCount == 0);
    bool anyContact = false;
    if (screws != null)
    {
      foreach (var s in screws)
      {
        if (s == null) continue;
        if (s.IsContact) { anyContact = true; break; }
      }
    }
    if (parentEmpty && !anyContact)
    {
      Debug.Log($"Lid '{name}': delayed release conditions met -> releasing lid");
      ReleaseLid();
    }
    delayedReleaseCoroutine = null;
  }

  void MakeGrabbable()
  {
    // Find and add XRGrabInteractable and XRGeneralGrabTransformer by scanning loaded assemblies
    Type grabType = null;
    Type transformerType = null;
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
      try
      {
        if (grabType == null)
        {
          foreach (var t in asm.GetTypes()) if (t.Name == "XRGrabInteractable") { grabType = t; break; }
        }
        if (transformerType == null)
        {
          foreach (var t in asm.GetTypes()) if (t.Name == "XRGeneralGrabTransformer") { transformerType = t; break; }
        }
      }
      catch { }
      if (grabType != null && transformerType != null) break;
    }

    if (grabType != null)
    {
      var grabComp = GetComponent(grabType);
      if (grabComp == null)
      {
        gameObject.AddComponent(grabType);
        Debug.Log($"Lid '{name}': XRGrabInteractable added at runtime (via {grabType.Assembly.GetName().Name})");
      }
    }
    else
    {
      Debug.LogWarning($"Lid '{name}': XRGrabInteractable type not found. Install XR Interaction Toolkit to enable grabbing.");
    }

    if (transformerType != null)
    {
      var transComp = GetComponent(transformerType);
      if (transComp == null)
      {
        gameObject.AddComponent(transformerType);
        Debug.Log($"Lid '{name}': XRGeneralGrabTransformer added at runtime (via {transformerType.Assembly.GetName().Name})");
      }
    }
    else
    {
      Debug.LogWarning($"Lid '{name}': XRGeneralGrabTransformer type not found. Transformer will not be attached.");
    }

    // Ensure lid has an enabled non-trigger collider so it can be grabbed
    var c = GetComponent<Collider>();
    if (c != null)
    {
      c.isTrigger = false;
      c.enabled = true;
    }
  }

  void ReleaseLid()
  {
    // If already non-kinematic, nothing to do
    if (!rb.isKinematic) return;
    // Prepare collider for dynamic physics: MeshCollider must be convex for non-kinematic Rigidbody
    var meshCol = GetComponent<MeshCollider>();
    if (meshCol != null && !meshCol.convex)
    {
      Debug.Log($"Lid '{name}': setting MeshCollider.convex = true to allow dynamic Rigidbody");
      meshCol.convex = true;
    }

    // Tweak physics settings to avoid explosive behavior
    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    rb.interpolation = RigidbodyInterpolation.Interpolate;
    rb.mass = Mathf.Max(rb.mass, 0.5f);
#if UNITY_2023_1_OR_NEWER
    rb.angularDamping = Mathf.Max(rb.angularDamping, 0.5f);
    rb.linearVelocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
#else
    rb.angularDrag = Mathf.Max(rb.angularDrag, 0.5f);
    rb.velocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
#endif

    rb.isKinematic = false;
    rb.useGravity = true;
    // Small downward nudge so the lid falls into the scene rather than being ejected
    rb.AddForce(Vector3.down * 0.02f, ForceMode.Impulse);

    // Make the lid grabbable so the player can pick it up
    MakeGrabbable();
  }
}
