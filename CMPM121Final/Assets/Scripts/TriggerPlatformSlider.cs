using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPlatformSlider : Triggerable
{
    private Vector3 startPosition;

    public Transform triggeredTransform;
    private Vector3 triggeredPosition;

    public bool triggered = false;

    public float moveTime = 1f;
    public float hangTime = 0f;

    void Start()
    {
        startPosition = gameObject.transform.position;
        triggeredPosition = triggeredTransform.position;
    }

    public override void Trigger() {
        if (!triggered) {
            LeanTween.move(gameObject, triggeredPosition, moveTime).setEaseOutExpo();
            triggered = true;
            Invoke("MoveBack", moveTime + hangTime);
        }
    }

    public override void Reset() {
        transform.position = startPosition;
    }

    public void MoveBack() {
        LeanTween.move(gameObject, startPosition, moveTime).setEaseInExpo().setOnComplete(() => { triggered = false; });
    }
}
