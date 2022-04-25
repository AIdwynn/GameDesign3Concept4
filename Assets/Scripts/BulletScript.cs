using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public class BulletScript : MonoBehaviour
{
    public BulletTypes BulletTypesList = BulletTypes.Normal;
    public float Speed;
    public float LifeTime;
    public float LeprMod;
    public float ElapsedTime;
    public float LerpT;
    public bool delete;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BulletScript>() != null)
            return;

        var charCon = other.GetComponent<PlayerController>();
        if(charCon != null)
            charCon.OnHit();
        delete = true;
    }
}


