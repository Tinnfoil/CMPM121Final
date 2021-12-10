using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triggerable : MonoBehaviour, ITriggerable
{
    /// <summary>
    /// how many triggers this needs to actually trigger
    /// </summary>
    public int triggerAmount = 1;
    protected int currentTriggers = 0;

    public virtual void Trigger()
    {
        currentTriggers++;
    }
    public virtual void Reset()
    {
        currentTriggers = 0;
    }

    protected bool CanTrigger()
    {
        return currentTriggers >= triggerAmount;
    }
}
