using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CutTapeScript : MonoBehaviour
{
    //Time that the button is set inactive after release
    // public float deadTime = 1.0f;
    //Bool used to lock down button during its set dead time
    // private bool _deadTimeActive = false;

    //public Unity Events we can use in the editor and tie other functions to.
    
    private bool isSolved = false;
    
    public DefuseBombManager defuseBombManager;

    public UnityEvent onPressed, onReleased;

    //Checks if the current collider entering is the Button and sets off OnPressed event.
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Cutter") //&& !_deadTimeActive)
        {
            onPressed?.Invoke();
            Debug.Log("Cutter is cutting...");
        }
    }

    //Checks if the current collider exiting is the Button and sets off OnReleased event.
    //It will also call a Coroutine to make the button inactive for however long deadTime is set to.
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Cutter") //&& !_deadTimeActive)
        {
            onReleased?.Invoke();
            Debug.Log("Cutter has stopped cutting");
            
            PuzzleSolved();
            // StartCoroutine(WaitForDeadTime());
        }
    }

    //Locks button activity until deadTime has passed and reactivates button activity.
    // IEnumerator WaitForDeadTime()
    // {
    //     _deadTimeActive = true;
    //     yield return new WaitForSeconds(deadTime);
    //     _deadTimeActive = false;
    // }
    
    void PuzzleSolved()
    {
        if (isSolved) return;
        isSolved = true;
        Debug.Log("Tape puzzle Solved!");
        defuseBombManager.UpdatePuzzleState();
    }

    public bool IsSolved()
    {
        return isSolved;
    }
}