using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarheadVFX : MonoBehaviour
{
    public GameObject TrailVFX;

    // Update is called once per frame
    void FixedUpdate()
    {
        //model.transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
    }

    public void DestroySelf()
    {
        TrailVFX.transform.parent = null;
        TrailVFX.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(TrailVFX, 2f);
    }


}
