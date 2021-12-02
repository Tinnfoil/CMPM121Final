using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public bool GunReady = true;
    public void SetGunReady() {
        GunReady = true;
    }

    public void SetGunNotReady() {
        GunReady = false;
    }
}
