using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CutTapeScript : MonoBehaviour
{
    private bool isSolved = false;
    
    public DefuseBombManager defuseBombManager;

    public UnityEvent onPressed, onReleased;

    [Header("Cut Validation")]
    [Tooltip("How far (in world units) the cutter must travel while inside to count as a cut.")]
    public float requiredCutDistance = 0.1f;

    [Tooltip("Minimum time (seconds) the cutter must stay inside the trigger.")]
    public float minCutTime = 0.1f;

    private bool isCutting = false;
    private float cutDistance = 0f;
    private float cutTime = 0f;
    private Vector3 lastCutterPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cutter") && !isSolved)
        {
            isCutting = true;
            cutDistance = 0f;
            cutTime = 0f;
            lastCutterPosition = other.transform.position;

            onPressed?.Invoke();
            Debug.Log("Cutter started cutting...");
        }
    }

    // While the cutter stays inside, track movement and time
    private void OnTriggerStay(Collider other)
    {
        if (!isCutting || !other.CompareTag("Cutter") || isSolved)
            return;

        Vector3 currentPos = other.transform.position;
        cutDistance += Vector3.Distance(currentPos, lastCutterPosition);
        lastCutterPosition = currentPos;

        cutTime += Time.deltaTime;
        // Debug.Log($"CutDistance: {cutDistance}, CutTime: {cutTime}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cutter") && isCutting && !isSolved)
        {
            isCutting = false;
            Debug.Log("Cutter has stopped cutting");

            bool cutSuccessful = cutDistance >= requiredCutDistance && cutTime >= minCutTime;

            if (cutSuccessful)
            {
                onReleased?.Invoke();
                PuzzleSolved();
            }
            else
            {
                Debug.Log("Cut failed: not enough movement or time.");
            }

            // reset for next attempt if you want multiple tries
            cutDistance = 0f;
            cutTime = 0f;
        }
    }

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
