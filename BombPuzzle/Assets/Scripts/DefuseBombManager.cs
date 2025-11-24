using UnityEngine;
using UnityEngine.Events;
using System;

public class DefuseBombManager : MonoBehaviour
{

    public ShapesPuzzleScript shapesPuzzle;
    public CutTapeScript cutTape;
    public WiresMonitor wiresMonitor;
    public KeypadLock keypadLock;
    public BombTimer bombTimer;
    public bool isDefused = false;
    public UnityEvent bombDefusedEvent, bombExplodedEvent;
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
        if (keypadLock == null)
        {
            throw new Exception("KeypadLock reference is not set in DefuseBombManager.");
        }
    }

    public void UpdatePuzzleState()
    {
        if (isDefused || !keypadLock.IsSolved())
        {
            return;
        }

        BombDefused();
    }

    public void BombDefused()
    {
        Debug.Log("Bomb Defused!");
        isDefused = true;
        
        // Stop the timer
        if (bombTimer != null)
        {
            bombTimer.StopTimer();
        }
        
        bombDefusedEvent?.Invoke();
    }

    public void BombExploded()
    {
        Debug.Log("Bomb Exploded!");
        bombExplodedEvent?.Invoke();
    }
}
