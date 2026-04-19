using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    [SerializeField] private GameObject ExitCylinder;
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private AudioClip newBackgroundClip;
    [SerializeField] private AudioSource alarmSound;

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log(">>> TRIGGER HIT BY: " + other.name);

        // 1. Check if it's the player
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            Debug.Log(">>> PLAYER DETECTED!");

            // 2. Add this specific check for the GameManager data
            if (GameManager.Instance != null && GameManager.Instance.IsMissionComplete())
            {
                // This line triggers the Win UI
                GameManager.Instance.PlayerExited();

                if (backgroundAudioSource != null && newBackgroundClip != null)
                {
                    backgroundAudioSource.clip = newBackgroundClip;
                    backgroundAudioSource.Play();
                }

                alarmSound.Stop();
                Destroy(ExitCylinder);
            }
            else
            {
                // If the UI says 0/2 or 1/2, this message will appear in your Console
                Debug.Log("Mission not complete yet!");
            }
        }
    }
}