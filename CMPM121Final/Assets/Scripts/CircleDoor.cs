using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDoor : Triggerable
{
    public Animator animator;
    public override void Trigger()
    {
        animator.SetTrigger("Open");
    }

}
