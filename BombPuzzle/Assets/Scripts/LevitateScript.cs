using UnityEngine;
using System.Collections;

public class LevitateScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;      // Units per second
    public float distance = 5f;   // How far to move upward

    private bool isMoving = false;

    // Call this from another script to start the movement
    public void StartMovingUp()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveUpCoroutine());
        }
    }

    private IEnumerator MoveUpCoroutine()
    {
        isMoving = true;

        float moved = 0f;

        while (moved < distance)
        {
            float step = speed * Time.deltaTime;

            // Make sure we don't overshoot the target distance
            if (moved + step > distance)
            {
                step = distance - moved;
            }

            // Move THIS object (the one this script is attached to)
            transform.Translate(Vector3.up * step, Space.World);

            moved += step;

            yield return null; // wait for next frame
        }

        isMoving = false;
    }
}
