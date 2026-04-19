using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

public class SimulatorPlayerLinker : MonoBehaviour
{
    void Awake()
    {
        // Find the simulator that survived the restart
        XRDeviceSimulator sim = Object.FindFirstObjectByType<XRDeviceSimulator>();

        if (sim != null)
        {
            // Manually point the simulator to this object (the Player-tagged camera)
            sim.cameraTransform = this.transform;

            // Wake it up
            sim.enabled = false;
            sim.enabled = true;

            Debug.Log("Simulator linked to 'Player' tagged camera.");
        }
    }
}