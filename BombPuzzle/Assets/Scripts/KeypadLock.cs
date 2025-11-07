using UnityEngine;
using TMPro;

public class KeypadLock : MonoBehaviour
{
    [Header("Keypad Password")]
    public string code = "1234";
    private string enterCode = "";

    public void Start()
    {
        UpdatePasscodeDisplay();
    }

    public void AddDigit(string digit)
    {
        if (enterCode.Length < code.Length)
        {
            enterCode += digit;
            UpdatePasscodeDisplay();
            Debug.Log("Current code" + enterCode);
        }
    }

    public void CheckCode()
    {
        if (enterCode == code)
        {
            Debug.Log("Its correct passcode");
        }
        else
        {
            Debug.Log("Code is Incorrect");
            ClearCode();
        }
    }

    public void ClearCode()
    {
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
}
