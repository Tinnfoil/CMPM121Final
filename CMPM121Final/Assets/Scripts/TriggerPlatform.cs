using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPlatform : Triggerable
{
    private Vector3 startPosition;
    private Quaternion startRotation;

    public Vector3 triggeredPosition;
    public Vector3 triggeredRotation;

    public bool triggered = false;

    // public Vector3 triggeredEuler;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = gameObject.transform.position;
        startRotation = gameObject.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Trigger() {
        if (!triggered) {
            LeanTween.move(gameObject, triggeredPosition, 1);
            // LeanTween.rotate(gameObject, triggeredRotation, 1);
            // LeanTween.rotateX(gameObject, triggeredRotation.x , 1);
            LeanTween.value(gameObject, 0, triggeredRotation.x, 1).setOnUpdate((float x) => {transform.localEulerAngles = new Vector3(x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);});
        } else {
            LeanTween.move(gameObject, startPosition, 1);
            // LeanTween.rotate(gameObject, startRotation.eulerAngles, 1);
            // LeanTween.rotateX(gameObject, startRotation.eulerAngles.x , 1);
            LeanTween.value(gameObject, triggeredRotation.x, 0, 1).setOnUpdate((float x) => {transform.localEulerAngles = new Vector3(x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);});
            // Debug.Log(startRotation.eulerAngles.x);
        }

        triggered = !triggered;
    }

    public override void Reset() {
        transform.position = startPosition;
        transform.rotation = startRotation;
    }
}
