using UnityEngine;
using System;

public class DefuseBombManager : MonoBehaviour
{

    public ShapesPuzzleScript shapesPuzzle;
    public CutTapeScript cutTape;
    public WiresMonitor wiresMonitor;
    public bool isDefused = false;

    private void Awake()
    {
        if (cutTape == null)
        {
            throw new Exception("CutTapeScript reference is not set in DefuseBombManager.");
        }
        if (shapesPuzzle == null)
        {
            throw new Exception("ShapesPuzzleScript reference is not set in DefuseBombManager.");
        }
        if (wiresMonitor == null)
        {
            throw new Exception("WiresMonitor reference is not set in DefuseBombManager.");
        }
    }

    public void UpdatePuzzleState()
    {
        if (isDefused || !cutTape.IsSolved() || !shapesPuzzle.IsSolved() || !wiresMonitor.IsSolved())
        {
            return;
        }

        BombDefused();
    }

    public void BombDefused()
    {
        Debug.Log("Bomb Defused!");
        isDefused = true;
    }

    public void BombExploded()
    {
        Debug.Log("Bomb Exploded!");
    }
}
