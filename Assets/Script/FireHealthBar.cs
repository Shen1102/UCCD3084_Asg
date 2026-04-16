using UnityEngine;
using UnityEngine.UI;

public class FireHealthBar : MonoBehaviour
{
    public Fire fireTarget; // Drag the fire object here
    public Slider healthSlider; // Drag the UI Slider here
    public bool hideWhenExtinguished = true;

    void Start()
    {
        if (fireTarget == null) fireTarget = GetComponentInParent<Fire>();
        if (healthSlider == null) healthSlider = GetComponent<Slider>();
    }

    void Update()
    {
        if (fireTarget != null && healthSlider != null)
        {
            // Update the slider value based on the fire's intensity
            healthSlider.value = fireTarget.GetCurrentIntensity();

            // Optional: Hide the bar if the fire is out
            if (hideWhenExtinguished && fireTarget.IsExtinguished())
            {
                gameObject.SetActive(false);
            }
        }
    }
}