using UnityEngine;
using System.Collections;

/// <summary>
/// Moves the GameObject up to a target world Y position over a duration,
/// then starts a subtle bobbing (levitation) effect. Coroutines run on the
/// bomb GameObject itself so they remain active as long as the bomb is active.
/// </summary>
public class LevitateScript : MonoBehaviour
{
    [Header("Default Move Settings")]
    [Tooltip("Default duration (seconds) to reach the target height when StartMoveToHeight is called without a duration")]
    public float defaultMoveDuration = 1.2f;
    public float defaultTargetWorldY = 1.5f;

    [Header("Default Bobbing")]
    public float defaultBobAmplitude = 0.02f;
    public float defaultBobFrequency = 1.2f;

    private Coroutine moveCoroutine;
    private Coroutine bobCoroutine;
    private Vector3 bobBasePosition;

    /// <summary>
    /// Start moving this object from its current world Y to <paramref name="targetWorldY"/>
    /// over <paramref name="duration"/> seconds, then start bobbing using the provided
    /// bob amplitude and frequency.
    /// </summary>
    public void StartMovingUp()
    {
        float startY = transform.position.y;
        moveCoroutine = StartCoroutine(MoveToHeightAndStartBob(startY, defaultTargetWorldY, defaultMoveDuration, defaultBobAmplitude, defaultBobFrequency));
    }

    private IEnumerator MoveToHeightAndStartBob(float startY, float targetY, float duration, float bobAmplitude, float bobFrequency)
    {
        float elapsed = 0f;

        if (duration <= 0f)
        {
            var p = transform.position;
            p.y = targetY;
            transform.position = p;
        }
        else
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = Mathf.SmoothStep(0f, 1f, t);
                float newY = Mathf.Lerp(startY, targetY, eased);
                var p = transform.position;
                p.y = newY;
                transform.position = p;
                yield return null;
            }

            var finalPos = transform.position;
            finalPos.y = targetY;
            transform.position = finalPos;
        }

        // Start bobbing
        bobBasePosition = transform.position;
        bobCoroutine = StartCoroutine(BobRoutine(bobAmplitude, bobFrequency));
        moveCoroutine = null;
    }

    private IEnumerator BobRoutine(float amplitude, float frequency)
    {
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime;
            float yOffset = Mathf.Sin(time * frequency * Mathf.PI * 2f) * amplitude;
            var p = transform.position;
            p.y = bobBasePosition.y + yOffset;
            transform.position = p;
            yield return null;
        }
    }
}
