using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableBox : Triggerable
{
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    // Start is called before the first frame update
    void Start()
    {
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Reset()
    {
        base.Reset();
        transform.position = startingPosition;
        transform.rotation = startingRotation;
    }
}
