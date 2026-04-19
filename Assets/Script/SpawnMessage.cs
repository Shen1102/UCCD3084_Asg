using UnityEngine;
using System.Collections;

public class SpawnMessage : MonoBehaviour
{
    public GameObject messageText;

    void Start()
    {
        // 1. Try to find the object if it's not assigned
        if (messageText == null)
            messageText = GameObject.Find("RestartMessage");

        if (messageText != null && GameRestartHandler.WasRestarted)
        {
            // 2. FORCE POSITION: Move it to the camera's exact spot
            messageText.transform.SetParent(this.transform);

            // 3. SET COORDINATES: 0,0 is center of your eyes. 0.6f is just over half a meter away.
            messageText.transform.localPosition = new Vector3(0, 0, 0.6f);
            messageText.transform.localRotation = Quaternion.identity;

            // 4. FORCE SCALE: UI in VR must be very small
            messageText.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

            StartCoroutine(ShowMessageRoutine());
        }
    }

    IEnumerator ShowMessageRoutine()
    {
        messageText.SetActive(true);
        yield return new WaitForSeconds(2f);
        messageText.SetActive(false);
        GameRestartHandler.WasRestarted = false;
    }
}