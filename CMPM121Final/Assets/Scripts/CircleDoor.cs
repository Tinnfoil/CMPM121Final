using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDoor : Triggerable
{
    public Animator animator;

    public override void Trigger()
    {
        base.Trigger();
        if (CanTrigger())
        {
            animator.SetTrigger("Open");
        }
    }

    public override void Reset()
    {
        base.Reset();
        animator.SetTrigger("Close");
    }

}
