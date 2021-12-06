using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public Triggerable triggeredObject;

    public Material[] TriggeredMaterials;

    private bool Triggered;

    public void TriggerObject()
    {
        if (Triggered) return;
        Triggered = true;
        triggeredObject.Trigger();
        GetComponent<Animator>().SetTrigger("Spin");

        GetComponentInChildren<MeshRenderer>().materials = TriggeredMaterials;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
