using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

public class SimulatorTagFixer : MonoBehaviour
{
    void Start()
    {
        // Find the Simulator that survived the restart in DontDestroyOnLoad
        var simulator = Object.FindFirstObjectByType<XRDeviceSimulator>();

        if (simulator != null)
        {
            // Manually assign THIS camera (tagged Player) to the simulator
            simulator.cameraTransform = this.transform;

            // Toggle it to refresh the input focus
            simulator.enabled = false;
            simulator.enabled = true;

            Debug.Log("Simulator successfully linked to Player-tagged camera.");
        }
    }
}