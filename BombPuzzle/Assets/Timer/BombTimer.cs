using UnityEngine;
using TMPro;
using System;

public class BombTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    [Header("Time Values")]
    [Range(0, 60)] public int seconds;
    [Range(0, 60)] public int minutes;
    [Range(0, 60)] public int hours;

    public Color fontColor;
    public bool showMilliseconds;

    [Header("Bomb Manager")]
    public DefuseBombManager bombManager;
    public AudioSource tickingSound;

    [Header("Tick Speed Settings")]
    [Tooltip("Delay between ticks when the timer has just started (in seconds).")]
    public float slowestTickInterval = 1.0f;

    [Tooltip("Delay between ticks when the timer is about to hit 0 (in seconds).")]
    public float fastestTickInterval = 0.15f;

    private float currentSeconds;
    private float timerDefault;
    private bool timerStopped = false;
    private bool timerStarted = false;
    private Coroutine tickRoutine;

    void Start()
    {
        fontColor.a = 1f;
        timerText.color = fontColor;

        timerDefault = seconds + (minutes * 60) + (hours * 60 * 60);
        currentSeconds = timerDefault;

        // Display initial time but don't start countdown
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (timerStopped || !timerStarted)
            return;

        currentSeconds -= Time.deltaTime;

        if (currentSeconds <= 0f)
        {
            currentSeconds = 0f;
            TimeUp();
        }
        else
        {
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (showMilliseconds)
            timerText.text = TimeSpan.FromSeconds(currentSeconds).ToString(@"hh\:mm\:ss\:fff");
        else
            timerText.text = TimeSpan.FromSeconds(currentSeconds).ToString(@"hh\:mm\:ss");
    }

    private void TimeUp()
    {
        if (showMilliseconds)
            timerText.text = "00:00:00:000";
        else
            timerText.text = "00:00:00";

        timerStopped = true;

        // Stop ticking
        if (tickRoutine != null)
        {
            StopCoroutine(tickRoutine);
            tickRoutine = null;
        }

        if (tickingSound != null)
            tickingSound.Stop();

        if (bombManager != null)
        {
            bombManager.BombExploded();
        }
    }

    public bool IsActive
    {
        get { return timerStarted && !timerStopped; }
    }

    public void StartTimer()
    {
        if (timerStarted) return;

        timerStarted = true;
        timerStopped = false;
        Debug.Log("Bomb timer started!");

        if (tickingSound != null && tickRoutine == null && timerDefault > 0f)
        {
            tickRoutine = StartCoroutine(TickLoop());
        }
    }

    public void StopTimer()
    {
        timerStopped = true;

        if (tickRoutine != null)
        {
            StopCoroutine(tickRoutine);
            tickRoutine = null;
        }

        if (tickingSound != null)
            tickingSound.Stop();
    }

    private System.Collections.IEnumerator TickLoop()
    {
        while (!timerStopped && timerStarted)
        {
            tickingSound.Play();

            // How far through the timer are we? 0 = just started, 1 = about to explode
            float normalized = 1f - Mathf.Clamp01(currentSeconds / Mathf.Max(timerDefault, 0.0001f));

            // Interpolate between slow and fast intervals
            float interval = Mathf.Lerp(slowestTickInterval, fastestTickInterval, normalized);

            yield return new WaitForSeconds(interval);
        }
    }
}
