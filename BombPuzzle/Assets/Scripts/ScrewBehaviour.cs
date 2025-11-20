using System;
using UnityEngine;


[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class ScrewBehaviour : MonoBehaviour
{
  [Header("Thread settings")]
  [Tooltip("Meters moved per full revolution (e.g., 0.002 = 2mm per rev)")]
  public float threadPitch = 0.002f;
  [Tooltip("Linear distance the screw must move before it's considered removed")]
  public float totalUnscrewDistance = 0.03f;
  [Tooltip("If your model's local axis points the other way, set to -1 to invert movement")]
  public int unscrewDirection = 1;
  [Tooltip("Flip this if rotation direction from the tip should be inverted")]
  public bool reverseDirectionToUnscrew = false;

  [Header("Unscrew axis (local)")]
  [Tooltip("Local axis along which the screw will translate while unscrewing (default = local Z)")]
  public Vector3 unscrewLocalAxis = Vector3.forward;
  [Tooltip("Multiplier applied to the tip rotation when rotating the screw (degrees)")]
  public float rotationMultiplier = 1f;

  [HideInInspector]
  public DrillController currentDrill;

  // Contact event for lid or other listeners: passes the new contact state
  public event Action<bool> OnContactChanged;

  // Public read-only accessor for current contact state
  public bool IsContact => isContact;

  Rigidbody rb;
  Collider col;
  bool isContact = false;
  bool removed = false;
  float accumulatedDistance = 0f;
  Transform originalParent;
  Vector3 startLocalPosition;
  [Header("Grabbing")]
  [Tooltip("Name of the layer to assign to the screw after it is removed so Interactors can grab it (Default = 'Default')")]
  public string grabbableLayerName = "Default";

  void Awake()
  {
    rb = GetComponent<Rigidbody>();
    col = GetComponent<Collider>();
    originalParent = transform.parent;
    startLocalPosition = transform.localPosition;
    rb.isKinematic = true;
    rb.useGravity = false;
  }

  void SetLayerRecursive(Transform t, int layer)
  {
    if (t == null) return;
    t.gameObject.layer = layer;
    for (int i = 0; i < t.childCount; ++i)
      SetLayerRecursive(t.GetChild(i), layer);
  }

  public void SetContact(bool contact)
  {
    if (isContact == contact) return;
    isContact = contact;
    OnContactChanged?.Invoke(isContact);
  }

  // Called by the drill when the tip rotates (degrees for this frame)
  public void OnTipRotation(float degrees)
  {
    if (removed) return;
    if (!isContact) return;
    if (currentDrill == null || !currentDrill.IsActive) return;

    float signedDegrees = reverseDirectionToUnscrew ? -degrees : degrees;
    float deltaDistance = (signedDegrees / 360f) * threadPitch; // meters
    deltaDistance *= unscrewDirection;

    // Move in local space along the configurable local axis
    Vector3 localPos = transform.localPosition;
    Vector3 axis = unscrewLocalAxis.normalized;
    localPos += axis * deltaDistance;
    transform.localPosition = localPos;

    // Rotate the screw around its local unscrew axis so it visibly spins as it is unscrewed
    // degrees is the tip rotation in degrees for this frame; apply multiplier to tune visual
    float rotDeg = signedDegrees * rotationMultiplier;
    // Rotate in local space
    transform.Rotate(axis * rotDeg, Space.Self);

    accumulatedDistance += Mathf.Abs(deltaDistance);

    if (accumulatedDistance >= totalUnscrewDistance)
    {
      RemoveScrew();
    }
  }

  void MakeGrabbable()
  {
    // Find XRGrabInteractable and XRGeneralGrabTransformer types by scanning loaded assemblies
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
      catch { /* ignore assemblies we can't reflect over */ }
      if (grabType != null && transformerType != null) break;
    }

    if (grabType != null)
    {
      var grabComp = GetComponent(grabType);
      if (grabComp == null)
      {
        gameObject.AddComponent(grabType);
        Debug.Log($"Screw '{name}': XRGrabInteractable added at runtime (via {grabType.Assembly.GetName().Name})");
      }
    }
    else
    {
      Debug.LogWarning($"Screw '{name}': XRGrabInteractable type not found. Install XR Interaction Toolkit to enable grabbing.");
    }

    if (transformerType != null)
    {
      var transComp = GetComponent(transformerType);
      if (transComp == null)
      {
        gameObject.AddComponent(transformerType);
        Debug.Log($"Screw '{name}': XRGeneralGrabTransformer added at runtime (via {transformerType.Assembly.GetName().Name})");
      }
    }
    else
    {
      Debug.LogWarning($"Screw '{name}': XRGeneralGrabTransformer type not found. Transformer will not be attached.");
    }

    // Ensure the screw has an enabled, non-trigger collider (XR interaction needs a collider)
    if (col != null)
    {
      col.isTrigger = false;
      col.enabled = true;
    }
  }

  void RemoveScrew()
  {
    if (removed) return;
    removed = true;

    // ensure listeners know it's no longer contacting
    if (isContact)
    {
      isContact = false;
      OnContactChanged?.Invoke(false);
    }

    // detach from parent so it can fall
    transform.SetParent(null, true);

    // set layer so Interactors (and other systems) can pick it up - apply to the whole hierarchy
    int layerIndex = LayerMask.NameToLayer(grabbableLayerName);
    if (layerIndex < 0) layerIndex = 0; // fallback to Default (0)
    SetLayerRecursive(transform, layerIndex);
    Debug.Log($"Screw '{name}': set layer to '{LayerMask.LayerToName(layerIndex)}' ({layerIndex}) to make it grabbable");

    // Prepare collider for dynamic physics: if a MeshCollider exists it must be convex when used with a non-kinematic Rigidbody
    var meshCol = GetComponent<MeshCollider>();
    if (meshCol != null && !meshCol.convex)
    {
      Debug.Log($"Screw '{name}': setting MeshCollider.convex = true to allow dynamic Rigidbody");
      meshCol.convex = true;
    }

    // enable physics with safer parameters to avoid being thrown out of the scene
    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    rb.interpolation = RigidbodyInterpolation.Interpolate;
    rb.mass = Mathf.Max(rb.mass, 0.05f);
    // Use newer properties if available: angularDamping / linearVelocity. Use compatibility helpers.
#if UNITY_2023_1_OR_NEWER
    rb.angularDamping = Mathf.Max(rb.angularDamping, 0.05f);
    rb.linearVelocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
#else
  rb.angularDrag = Mathf.Max(rb.angularDrag, 0.05f);
  // zero velocities to avoid inheriting large velocities on parent detach
  rb.velocity = Vector3.zero;
  rb.angularVelocity = Vector3.zero;
#endif

    rb.isKinematic = false;
    rb.useGravity = true;

    // Apply a small impulse that nudges the screw away from the lid but mostly downwards so it falls into the world
    Vector3 outward = transform.TransformDirection(unscrewLocalAxis.normalized);
    Vector3 downward = Vector3.down;
    Vector3 impulse = (outward * 0.02f + downward * 0.02f);
    rb.AddForce(impulse, ForceMode.Impulse);

    // make screw grabbable
    MakeGrabbable();

    // notify lid (if any)
    LidBehaviour lid = null;
    if (originalParent != null)
    {
      // try to find a LidBehaviour under the same root (covers cases where screws are siblings of the lid)
      var root = originalParent.root;
      if (root != null) lid = root.GetComponentInChildren<LidBehaviour>(true);
    }
    // fallback: try to find any LidBehaviour in the scene
    if (lid == null) lid = UnityEngine.Object.FindAnyObjectByType<LidBehaviour>();
    if (lid != null)
    {
      Debug.Log($"Screw '{name}' removed — notifying lid '{lid.name}'");
      lid.NotifyScrewRemoved(this);
    }
    else
    {
      Debug.Log($"Screw '{name}' removed — no LidBehaviour found to notify");
    }
  }
}
