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
    public AudioSource puzzleSolvedAudioSource;
    protected bool isDefused = false;
    public UnityEvent bombDefusedEvent, bombExplodedEvent;
    private void Awake()
    {
        if (cutTape == null)
        {
            throw new Exception("CutTapeScript reference is not set in DefuseBombManager.");
        }
        if (bombTimer == null)
        {
            throw new Exception("BombTimer reference is not set in DefuseBombManager.");
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
        if (puzzleSolvedAudioSource == null)
        {
            throw new Exception("PuzzleSolvedAudioSource reference is not set in DefuseBombManager.");
        }
    }

    public void UpdatePuzzleState()
    {   
        if(bombTimer.IsActive == false)
        {
            bombTimer.StartTimer();
            return;
        }
        else if (isDefused || !cutTape.IsSolved() || !shapesPuzzle.IsSolved()  || !wiresMonitor.IsSolved() || !keypadLock.IsSolved())
        {
            puzzleSolvedAudioSource.Play();
            return;
        }
        puzzleSolvedAudioSource.Play();
        BombDefused();
    }

    public void BombDefused()
    {
        if (isDefused)
        {
            return;
        }
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
        if (isDefused)
        {
            return;
        }
        Debug.Log("Bomb Exploded!");
        isDefused = true;
        bombExplodedEvent?.Invoke();
    }
}
