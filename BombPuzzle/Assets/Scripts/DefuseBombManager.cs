using UnityEngine;
using System;

public class DefuseBombManager : MonoBehaviour
{

    public ShapesPuzzleScript shapesPuzzle;
    public bool isDefused = false;

    private void Awake()
    {
        if (shapesPuzzle == null)
        {
            throw new Exception("ShapesPuzzleScript reference is not set in DefuseBombManager.");
        }
    }

    public void UpdatePuzzleState()
    {
        if (!shapesPuzzle.IsSolved())
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
