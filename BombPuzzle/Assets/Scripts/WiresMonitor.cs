using UnityEngine;

/// <summary>
/// Attach this to the `Wires` parent. Assign the four wire parents (Wire1..Wire4)
/// in the inspector and choose which one is the defuse wire (the "good" wire).
/// The script watches the `IsCut` property on each WireCuttable and logs when
/// a wire transitions to cut. If the defuse wire is cut it logs "bomb defused",
/// otherwise it logs "bomb exploded".
/// </summary>
public class WiresMonitor : MonoBehaviour
{
    [Tooltip("Assign the four wire parents (Wire1..Wire4) in any order.")]
    public WireCuttable[] wires = new WireCuttable[4];

    [Tooltip("The wire that defuses the bomb when cut. Assign one of the wires above.")]
    public WireCuttable defuseWire;
    bool[] wasCut;
    private bool isSolved = false;
    public DefuseBombManager defuseBombManager;


    void Awake()
    {
        if (wires == null || wires.Length != 4)
            wires = new WireCuttable[4];
        wasCut = new bool[wires.Length];
    }

    void Update()
    {
        for (int i = 0; i < wires.Length; ++i)
        {
            var w = wires[i];
            if (w == null) continue;
            if (w.IsCut && !wasCut[i])
            {
                wasCut[i] = true;
                // ensure there is a defuseWire assigned; default to wires[0] if not
                if (defuseWire == null && wires.Length > 0) defuseWire = wires[0];

                if (ReferenceEquals(w, defuseWire))
                    PuzzleSolved();
                else
                    defuseBombManager.BombExploded();
            }
        }
    }

        
    void PuzzleSolved()
    {
        if (isSolved) return;
        isSolved = true;
        Debug.Log("Wire puzzle Solved!");
        defuseBombManager.UpdatePuzzleState();
    }

    public bool IsSolved()
    {
        return isSolved;
    }
}
