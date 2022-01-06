using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    public Rigidbody rBody;
    public float depthBeforeSubmerged = 1f;
    public float displacementAmount = 3f;
    public float waterDrag = 0.99f;
    public float waterAngularDrag = 0.5f;

    private void FixedUpdate()
    {
        float waveHeight = WaveManager.instance.GetWaveHeight(transform.position.x, transform.position.y);
        if(transform.position.y < waveHeight)
        {
            float displacementMultiplier = Mathf.Clamp01(waveHeight - transform.position.y / depthBeforeSubmerged) * displacementAmount;
            rBody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f),transform.position, ForceMode.Force);
            rBody.AddForce(displacementMultiplier * -rBody.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rBody.AddTorque(displacementMultiplier * -rBody.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}
