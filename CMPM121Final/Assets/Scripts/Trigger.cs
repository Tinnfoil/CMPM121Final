using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public Triggerable triggeredObject;
    public Material[] NormalMaterials;
    public Material[] TriggeredMaterials;

    public bool Target = true;
    private bool Triggered;

    public void TriggerObject()
    {
        if (Triggered && Target) return;
        Triggered = true;
        triggeredObject.Trigger();
        if (Target) GetComponent<Animator>().SetTrigger("Spin");

        if (Target) GetComponentInChildren<MeshRenderer>().materials = TriggeredMaterials;
    }

    public void Reset()
    {
        Triggered = false;
        if (Target) GetComponentInChildren<MeshRenderer>().materials = NormalMaterials;
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
