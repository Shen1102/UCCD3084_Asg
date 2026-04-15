using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinController : MonoBehaviour
{
    public bool isPulled = false;
    public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // stays fixed at start
    }

    public void PullPin()
    {
        if (!isPulled)
        {
            isPulled = true;
            transform.parent = null;       // detach from extinguisher
            rb.isKinematic = false;        // now physics enabled
            rb.useGravity = true;
        }
    }
}
