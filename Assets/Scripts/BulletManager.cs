using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

public class BulletManager : MonoBehaviour
{
    public List<BulletScript> Bullets = new List<BulletScript>();


    private void Update()
    {
        if (Bullets == null)
            return;

        for (int i = Bullets.Count - 1; i >= 0; i--)
        {
            var bullet = Bullets[i];
            if (bullet.delete)
            {
                Bullets.Remove(bullet);
                Destroy(bullet.gameObject);
            }

        }

        NativeArray<float3> positions = new NativeArray<float3>(Bullets.Count, Allocator.TempJob);
        NativeArray<float3> forwards = new NativeArray<float3>(Bullets.Count, Allocator.TempJob);
        NativeArray<float> speeds = new NativeArray<float>(Bullets.Count, Allocator.TempJob);
        NativeArray<float> lifeTimes = new NativeArray<float>(Bullets.Count, Allocator.TempJob);
        NativeArray<float> lerpMods = new NativeArray<float>(Bullets.Count, Allocator.TempJob);
        NativeArray<float> elapsedtimes = new NativeArray<float>(Bullets.Count, Allocator.TempJob);
        NativeArray<float> lerpTs = new NativeArray<float>(Bullets.Count, Allocator.TempJob);
        NativeArray<BulletTypes> bulletTypes = new NativeArray<BulletTypes>(Bullets.Count, Allocator.TempJob);
        NativeArray<bool> deletes = new NativeArray<bool>(Bullets.Count, Allocator.TempJob);

        for (int i = 0; i < Bullets.Count; i++)
        {
            forwards[i] = Bullets[i].transform.forward;
            positions[i] = Bullets[i].transform.position;
            speeds[i] = Bullets[i].Speed;
            lifeTimes[i] = Bullets[i].LifeTime;
            lerpMods[i] = Bullets[i].LeprMod;
            elapsedtimes[i] = Bullets[i].ElapsedTime;
            lerpTs[i] = Bullets[i].LerpT;
            bulletTypes[i] = Bullets[i].BulletTypesList;
            deletes[i] = Bullets[i].delete;
        }

        ParrallelBullets parrallelBullets = new ParrallelBullets {
            deltaTime = Time.deltaTime,
            Speed = speeds,
            Position = positions,
            Forward = forwards,
            LifeTime = lifeTimes,
            LeprMod = lerpMods,
            ElapsedTime = elapsedtimes,
            LerpT = lerpTs,
            BulletTypesList = bulletTypes,
            Delete = deletes
        };

        JobHandle handle =  parrallelBullets.Schedule(Bullets.Count, 100);

        handle.Complete();

        for (int i = 0; i < Bullets.Count; i++)
        {
            Bullets[i].transform.position = positions[i];
            Bullets[i].LeprMod = lerpMods[i];
            Bullets[i].ElapsedTime = elapsedtimes[i];
            Bullets[i].LerpT = lerpTs[i];
            Bullets[i].delete = deletes[i];
        }

        positions.Dispose();
        forwards.Dispose();
        speeds.Dispose();
        lifeTimes.Dispose();
        lerpMods.Dispose();
        elapsedtimes.Dispose();
        lerpTs.Dispose();
        bulletTypes.Dispose();
        deletes.Dispose();

    }

    [BurstCompile]
    public struct ParrallelBullets : IJobParallelFor
    {

        public NativeArray<float> ElapsedTime;
        public NativeArray<float> LerpT;
        public NativeArray<BulletTypes> BulletTypesList;
        public float deltaTime;
        public NativeArray<float> LeprMod;
        public NativeArray<float> LifeTime;
        public NativeArray<float> Speed;
        public NativeArray<float3> Position;
        public NativeArray<float3> Forward;
        public NativeArray<bool> Delete;

        public void Execute(int index)
        {

            var velocity = Speed[index] * deltaTime * Forward[index];
            if (BulletTypesList[index] == BulletTypes.Wave)
            {
                velocity.y = Mathf.Lerp(-1, 1, LerpT[index]);
                if (LerpT[index] >= 1 || LerpT[index] <= 0)
                    LeprMod[index] *= -1;
                LerpT[index] += deltaTime * LeprMod[index];
            }

            Position[index] += velocity;
            ElapsedTime[index] += deltaTime;
            if (ElapsedTime[index] >= LifeTime[index])
                Delete[index] = true;
            Position[index] = Position[index];
        }
    }
}

