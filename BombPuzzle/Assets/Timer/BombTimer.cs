using UnityEngine;
using TMPro;
using System;

public class BombTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    [Header("Time Values")]
    [Range(0, 60)]
    public int seconds;
    [Range(0, 60)]
    public int minutes;
    [Range(0, 60)]
    public int hours;

    public Color fontColor;

    public bool showMilliseconds;

    [Header("Bomb Manager")]
    public DefuseBombManager bombManager;

    private float currentSeconds;
    private int timerDefault;
    private bool timerStopped = false;

    void Start()
    {
        fontColor.a = 1f;
        timerText.color = fontColor;
        timerDefault = 0;
        timerDefault += (seconds + (minutes * 60) + (hours * 60 * 60));
        currentSeconds = timerDefault;
    }

    void Update()
    {
        if (timerStopped)
            return;

        if ((currentSeconds -= Time.deltaTime) <= 0)
        {
            TimeUp();
        }
        else
        {
            if (showMilliseconds)
                timerText.text = TimeSpan.FromSeconds(currentSeconds).ToString(@"hh\:mm\:ss\:fff");
            else
                timerText.text = TimeSpan.FromSeconds(currentSeconds).ToString(@"hh\:mm\:ss");
        }
    }

    private void TimeUp()
    {
        if (showMilliseconds)
            timerText.text = "00:00:00:000";
        else
            timerText.text = "00:00:00";

        timerStopped = true;

        if (bombManager != null && !bombManager.isDefused)
        {
            bombManager.BombExploded();
        }
    }

    public void StopTimer()
    {
        timerStopped = true;
    }
}
