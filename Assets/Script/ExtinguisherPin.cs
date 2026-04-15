using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class ExtinguisherPin : MonoBehaviour
{
    [Header("Links")]
    public FireExtinguisherPinInputSystem mainExtinguisherSystem;
    public Transform lockedPosition;
    public XRGrabInteractable hoseGrabInteractable;
    public List<Rigidbody> hoseBones = new List<Rigidbody>();

    [Header("Effects")]
    public ParticleSystem foamParticles; // Drag your Nozzle Particle System here
    public AudioSource hissSound;        // Drag your Nozzle Audio Source here

    [Header("Settings")]
    public float unlockDistance = 0.1f;
    public bool isUnlocked = false;

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (rb != null) rb.isKinematic = true;

        foreach (Rigidbody boneRB in hoseBones) { boneRB.isKinematic = true; }

        if (hoseGrabInteractable != null) { hoseGrabInteractable.enabled = false; }

        if (grabInteractable != null)
        {
            grabInteractable.selectExited.AddListener(OnPinDropped);
        }
    }

    private void Update()
    {
        if (!isUnlocked && lockedPosition != null)
        {
            float dist = Vector3.Distance(transform.position, lockedPosition.position);
            if (dist > unlockDistance) { UnlockPin(); }
        }
    }

    void UnlockPin()
    {
        if (isUnlocked) return;
        isUnlocked = true;
        if (rb != null) rb.isKinematic = false;
        if (mainExtinguisherSystem != null) { mainExtinguisherSystem.isPinPulled = true; }
        if (hoseGrabInteractable != null) { hoseGrabInteractable.enabled = true; }
        foreach (Rigidbody boneRB in hoseBones) { boneRB.isKinematic = false; }
    }

    // --- NEW FOAM LOGIC ---

    public void SqueezeHandle()
    {
        // This check is key for your FYP logic!
        if (isUnlocked)
        {
            if (foamParticles != null && !foamParticles.isPlaying) foamParticles.Play();
            if (hissSound != null && !hissSound.isPlaying) hissSound.Play();
        }
        else
        {
            Debug.Log("Handle squeezed, but pin is still in!");
        }
    }

    public void ReleaseHandle()
    {
        if (foamParticles != null) foamParticles.Stop();
        if (hissSound != null) hissSound.Stop();
    }

    private void OnPinDropped(SelectExitEventArgs args)
    {
        if (isUnlocked && rb != null) { rb.isKinematic = false; }
    }
}