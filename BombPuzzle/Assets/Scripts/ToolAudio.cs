using System.Collections;
using UnityEngine;

/// <summary>
/// Reusable audio component for tools. Configure per-tool in the Inspector:
/// - enableContinuous: use the looping (held) clip (e.g. drill)
/// - enableOneShot: use the one-shot clip (e.g. pliers click)
/// Supports optional fade-in/fade-out for the looping clip.
/// </summary>
public class ToolAudio : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip continuousClip; // e.g. electric screwdriver looping sound
    public AudioClip clickClip;      // e.g. pliers click

    [Header("Mode")]
    [Tooltip("Enable the looping (held) clip for this tool (e.g. drill)")]
    public bool enableContinuous = true;
    [Tooltip("Enable the one-shot click clip for this tool (e.g. pliers)")]
    public bool enableOneShot = true;

    [Header("Volumes")]
    [Range(0f, 1f)] public float continuousVolume = 1f;
    [Range(0f, 1f)] public float clickVolume = 1f;

    [Header("Fade (loop)")]
    [Tooltip("Seconds to fade in when starting the loop. 0 = immediate")]
    public float fadeInTime = 0.05f;
    [Tooltip("Seconds to fade out when stopping the loop. 0 = immediate")]
    public float fadeOutTime = 0.05f;

    [Header("Spatial / Playback")]
    [Tooltip("0 = 2D, 1 = fully 3D (recommended for world objects)")]
    [Range(0f, 1f)] public float spatialBlend = 1f;

    private AudioSource loopSource;
    private AudioSource oneShotSource;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // create dedicated audio sources so loop and one-shots don't interfere
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = true;
        loopSource.spatialBlend = spatialBlend;

        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.playOnAwake = false;
        oneShotSource.loop = false;
        oneShotSource.spatialBlend = spatialBlend;

        // assign clips if set in inspector
        if (continuousClip != null) loopSource.clip = continuousClip;
        if (clickClip != null) oneShotSource.clip = clickClip;

        // initialize volumes
        loopSource.volume = 0f;
        oneShotSource.volume = clickVolume;
    }

    /// <summary>
    /// Call when the trigger starts being held. Will start the looping clip if enabled.
    /// </summary>
    public void StartContinuous()
    {
        if (!enableContinuous || continuousClip == null) return;
        if (loopSource.clip == null) loopSource.clip = continuousClip;

        // stop any existing fade coroutine
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (!loopSource.isPlaying)
        {
            loopSource.Play();
        }

        if (fadeInTime > 0f)
        {
            fadeCoroutine = StartCoroutine(FadeLoop(loopSource.volume, continuousVolume, fadeInTime));
        }
        else
        {
            loopSource.volume = continuousVolume;
        }
    }

    /// <summary>
    /// Call when the trigger is released. Stops the loop (with optional fade).
    /// </summary>
    public void StopContinuous()
    {
        if (!enableContinuous || !loopSource.isPlaying) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (fadeOutTime > 0f)
        {
            fadeCoroutine = StartCoroutine(FadeOutAndStop(fadeOutTime));
        }
        else
        {
            loopSource.Stop();
            loopSource.volume = 0f;
        }
    }

    /// <summary>
    /// Play a one-shot click sound (e.g. pliers quick action) if enabled.
    /// </summary>
    public void PlayClick()
    {
        if (!enableOneShot || clickClip == null) return;
        oneShotSource.PlayOneShot(clickClip, clickVolume);
    }

    // convenience: toggle continuous state
    public void SetContinuous(bool on)
    {
        if (on) StartContinuous(); else StopContinuous();
    }

    private IEnumerator FadeLoop(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            loopSource.volume = Mathf.Lerp(from, to, t);
            yield return null;
        }
        loopSource.volume = to;
        fadeCoroutine = null;
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        float start = loopSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            loopSource.volume = Mathf.Lerp(start, 0f, t);
            yield return null;
        }
        loopSource.volume = 0f;
        loopSource.Stop();
        fadeCoroutine = null;
    }

    /// <summary>
    /// Optional runtime helper to configure this component via code.
    /// </summary>
    public void Configure(AudioClip continuous, AudioClip click, bool useContinuous, bool useOneShot)
    {
        continuousClip = continuous;
        clickClip = click;
        enableContinuous = useContinuous;
        enableOneShot = useOneShot;

        if (loopSource != null && continuousClip != null) loopSource.clip = continuousClip;
        if (oneShotSource != null && clickClip != null) oneShotSource.clip = clickClip;
    }
}
