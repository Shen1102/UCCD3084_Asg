using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinFall : MonoBehaviour
{
    [SerializeField] private AudioSource SoundEffect;

    [SerializeField] private string groundTag = "Ground";

    private void OnCollisionEnter(Collision collision)
    {
        // Only play when hitting the ground, and only once per drop
        if (collision.collider.CompareTag(groundTag))
        {
             SoundEffect.Play();
        }
    }
}
