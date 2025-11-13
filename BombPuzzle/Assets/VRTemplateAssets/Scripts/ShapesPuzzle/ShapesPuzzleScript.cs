using UnityEngine;

public class ShapesPuzzleScript : MonoBehaviour
{
    public static ShapesPuzzleScript Instance;
    public SocketScript[] slots;
    private bool isSolved = false;

    private void Awake()
    {
        Instance = this;
        slots = GetComponentsInChildren<SocketScript>();
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
    }

    void PuzzleSolved()
    {
        if (isSolved) return;
        isSolved = true;
        Debug.Log("Shapes Puzzle Solved!");
        // trigger your win UI, sound, etc.
    }
}
