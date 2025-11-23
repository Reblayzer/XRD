using UnityEngine;
using TMPro;

public class KeypadLock : MonoBehaviour
{
    [Header("Keypad Password")]
    public string code = "1234";
    private string enterCode = "";
    private bool isSolved = false;

    [Header("Feedback")]
    public Color correctColor = Color.green;
    public AudioSource successSound;
    public Color incorrectColor = Color.red;
    public AudioSource failureSound;
    public Color defaultColor = Color.white;
    public AudioSource feedbackSound;
    public float feedbackDuration = 1f;

    private Coroutine feedbackCoroutine;

    public void Start()
    {
        UpdatePasscodeDisplay();
        if (passcodeDisplay != null)
        {
            passcodeDisplay.color = defaultColor;
        }
    }

    public void AddDigit(string digit)
    {   
        feedbackSound.Play();
        if (enterCode.Length < code.Length && !isSolved)
        {   
            enterCode += digit;
            UpdatePasscodeDisplay();
            Debug.Log("Current code" + enterCode);
        }
    }

    public void CheckCode()
    {
        if (enterCode == code && !isSolved)
        {
            feedbackSound.Play();
            Debug.Log("Its correct passcode");
            isSolved = true;
            ShowFeedback(true);
            DefuseBombManager manager = FindFirstObjectByType<DefuseBombManager>();
            if (manager != null)
            {
                manager.UpdatePuzzleState();
            }
        }
        else if (!isSolved)
        {
            Debug.Log("Code is Incorrect");
            ShowFeedback(false);
        }
    }

    private void ShowFeedback(bool isCorrect)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(FeedbackCoroutine(isCorrect));
    }

    private System.Collections.IEnumerator FeedbackCoroutine(bool isCorrect)
    {
        if (passcodeDisplay != null)
        {
            passcodeDisplay.color = isCorrect ? correctColor : incorrectColor;
            if(isCorrect)
            {
                successSound.Play();
            }
            else
            {
                failureSound.Play();
            }
            yield return new WaitForSeconds(feedbackDuration);

            if(!isCorrect)
            {
                passcodeDisplay.color = defaultColor;
            }
        }
    }

    public void ClearCode()
    {
        feedbackSound.Play();
        if (isSolved)
        {
            Debug.Log("Cannot clear code, already solved.");
            return;
        }
        enterCode = "";
        UpdatePasscodeDisplay();
        Debug.Log("Screen is clear");
    }

    // test display
    [Header("Passcode Display GameObject")]
    public TextMeshPro passcodeDisplay;

    private void UpdatePasscodeDisplay()
    {
        if (passcodeDisplay != null)
        {
            passcodeDisplay.text = enterCode;
        }
    }

    public bool IsSolved()
    {
        return isSolved;
    }
}
