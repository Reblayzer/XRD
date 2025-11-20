using UnityEngine;

public class ShapesPuzzleScript : MonoBehaviour
{
    public static ShapesPuzzleScript Instance;
    public SocketScript[] slots;
    private bool isSolved = false;
    public DefuseBombManager defuseBombManager;

    private void Awake()
    {
        Instance = this;
        slots = GetComponentsInChildren<SocketScript>();
        if (defuseBombManager == null)
        {
            throw new System.Exception("DefuseBombManager reference is not set in ShapesPuzzleScript.");
        }
    }

    public void OnSlotFilled(SocketScript slot)
    {
        foreach (var s in slots)
        {
            if (!s.isFilled)
                return;
        }

        PuzzleSolved();
    }

    public void OnSlotEmptied(SocketScript slot)
    {
        if (!isSolved) return;
        isSolved = false;
        Debug.Log("A shape was removed, puzzle is no longer solved.");
        defuseBombManager.UpdatePuzzleState();
    }

    void PuzzleSolved()
    {
        if (isSolved) return;
        isSolved = true;
        Debug.Log("Puzzle Solved!");
        defuseBombManager.UpdatePuzzleState();
    }

    public bool IsSolved()
    {
        return isSolved;
    }
}
