
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TeleportProgram : UdonSharpBehaviour
{
    public void keypadGranted()
    {
        Debug.Log("Teleporting player!");
        Networking.LocalPlayer.TeleportTo(transform.position, transform.rotation);
    }
}

