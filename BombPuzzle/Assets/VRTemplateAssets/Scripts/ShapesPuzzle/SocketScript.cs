using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SocketScript : MonoBehaviour
{
    [Header("What shape is allowed in this socket? (tag)")]
    public string acceptedTag = "";

    [Header("What audio should be played on socket fill?")]
    public AudioClip audioClip;
    private bool isFilled = false;
    private XRSocketInteractor socket;

    private AudioSource audioSource;
    void Awake()
    {
        if (acceptedTag == "")
        {
            Debug.LogError("SocketScript on " + gameObject.name + " has no acceptedTag set!");
        }
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnSelectEntered);
        socket.selectExited.AddListener(OnSelectExited);

        audioSource = GetComponent<AudioSource>();
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        audioSource.clip = audioClip;
        audioSource.Play(); 

        if (args.interactableObject.transform.CompareTag(acceptedTag))
        {
            isFilled = true;
            ShapesPuzzleScript.Instance.OnSlotFilled(this);
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isFilled = false;

        var t = args.interactableObject.transform;
        var rb = t.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        ShapesPuzzleScript.Instance.OnSlotEmptied(this);
    }

    public bool IsFilled()
    {
        return isFilled;
    }
}
