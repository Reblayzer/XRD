using UnityEngine;

public class KeypadButton : MonoBehaviour
{
    public KeypadLock keypadLock;
    public string digitOrAction;

    public void pressButton()
    {
        if (digitOrAction == "Enter")
        {
            keypadLock.CheckCode();
        }
        else if (digitOrAction == "Clear")
        {
            keypadLock.ClearCode();
        }
        else
        {
            keypadLock.AddDigit(digitOrAction);
        }
    }
}
