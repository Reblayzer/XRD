using UnityEngine;

/// <summary>
/// Simple wire component that swaps from intact to broken when Cut() is called.
/// Place the intact mesh/parent in the `intact` field and the broken prefab/parent in the `broken` field.
/// </summary>
public class WireCuttable : MonoBehaviour
{
  public GameObject intact;
  public GameObject broken;

  public bool IsCut { get; private set; } = false;

  public void Cut()
  {
    if (IsCut) return;
    IsCut = true;
    if (intact != null) intact.SetActive(false);
    if (broken != null) broken.SetActive(true);
    Debug.Log($"Wire '{name}' was cut");
  }
}
