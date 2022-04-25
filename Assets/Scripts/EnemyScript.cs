using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletTypes
{
    Wave, Upwards, Downwards, Tracer, Normal
}
public class EnemyScript : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _bodyRadius;
    [SerializeField] private float _bulletSpeed;
    private Dictionary<GameObject, BulletStruct> Bullets = new Dictionary<GameObject, BulletStruct>();
    private BulletManager _bulletManager;

    private struct BulletStruct
    {
        public List<BulletTypes> BulletTypes;
        public float LerpMod;
        public float LifeTime;
        public float LerpT;
        public float ElapsedTime;

        public BulletStruct(List<BulletTypes> bulletTypes) : this()
        {
            BulletTypes = bulletTypes;
            LerpMod = 1;
            LifeTime = 10;
            LerpT = 0.5f;
            ElapsedTime = 0;
        }
    }

    private float _lerpT;
    private bool _turning;
    private float _elapsedTime;
    private float _angle;
    private float _lerpTime;
    private int _numberOfBullets;
    void Start()
    {
        _bulletManager = this.GetComponent<BulletManager>();
        _angle = this.transform.rotation.y;
        Attack();   
    }

    private void Attack()
    {
        var _timeBetweenAttacks = 0f;
        var _numberOfAttacks = 0;
        var _attackName = "";
        var random = Random.Range(0, 5);
        switch (random)
        {
            case 0:
                _numberOfAttacks = 5;
                _timeBetweenAttacks = 0.2f;
                _numberOfBullets = 50;
                _attackName = "RingAttack";
                break;
            case 1:
                _numberOfAttacks =100;
                _timeBetweenAttacks = 0.05f;
                _numberOfBullets = 40;
                _attackName = "BulletSinWave";
                break;
            case 2:
                _numberOfAttacks = 20;
                _timeBetweenAttacks = 0.2f;
                _numberOfBullets = 50;
                _attackName = "TurningRingAttack";
                break;
            case 3:
                _numberOfAttacks = 5;
                _timeBetweenAttacks = 2f;
                _attackName = "MoreThenSixTest";
                _numberOfBullets = 50;
                break;
            case 4:
                _numberOfAttacks =100;
                _timeBetweenAttacks =0.05f;
                _numberOfBullets = 40;
                _attackName = "Whirlpool";
                break;

        }
        _lerpTime = _numberOfAttacks * _timeBetweenAttacks;
        _elapsedTime = 0;

        InvokeRepeating(_attackName, 0, _timeBetweenAttacks);
        Invoke("CanInv", _timeBetweenAttacks * (_numberOfAttacks));
    }
    private void CanInv()
    {
        CancelInvoke();
        _turning = false;
        Invoke("Attack", 0.2f);
    }

    private void Whirlpool()
    {
        _turning = true;
        var types = new List<BulletTypes>();
        types.Add(BulletTypes.Wave);
        var bulletStruct = new BulletStruct(types);

        var newAngleOffset = 360 / _numberOfBullets;
        for (int i = 0; i < _numberOfBullets; i++)
        {
            var newAngle = newAngleOffset * i + Mathf.Lerp(_angle, _angle + 180, _lerpT);
            Shoot(newAngle, bulletStruct);
        }
    }
        private void RingAttack()
    {
        var types = new List<BulletTypes>();
        types.Add(BulletTypes.Normal);
        var bulletStruct = new BulletStruct(types);

        var newAngleOffset = 360 / _numberOfBullets;
        for (int i = 0; i < _numberOfBullets; i++)
        {
            var newAngle = newAngleOffset * i + _angle;
            Shoot(newAngle, bulletStruct);
        }
    }

    private void BulletSinWave()
    {
        var types = new List<BulletTypes>();
        types.Add(BulletTypes.Wave);
        var bulletStruct = new BulletStruct(types);
        var newAngleOffset = 360 / _numberOfBullets;
        for (int i = 0; i < _numberOfBullets; i++)
        {
            var newAngle = newAngleOffset * i + _angle;
            Shoot(newAngle, bulletStruct);
        }
    }

    private void TurningRingAttack()
    {
        var types = new List<BulletTypes>();
        types.Add(BulletTypes.Normal);
        var bulletStruct = new BulletStruct(types);
        _turning = true;
        var newAngleOffset = 360 / _numberOfBullets;
        for (int i = 0; i < _numberOfBullets; i++)
        {
            var newAngle = newAngleOffset * i + Mathf.Lerp(_angle, _angle + 180, _lerpT);
            Shoot(newAngle, bulletStruct);
        }
    }

    private void MoreThenSixTest()
    {
        var types = new List<BulletTypes>();
        types.Add(BulletTypes.Normal);
        var bulletStruct = new BulletStruct(types);
        var newAngleOffset = 360 / _numberOfBullets;
        for (int i = 0; i < _numberOfBullets; i++)
        {
            var newAngle = newAngleOffset * i + _angle;
            Shoot(newAngle, bulletStruct);
        }
    }

    private void Shoot(float newAngle, BulletStruct bulletTypes)
    {
        var bullet = Instantiate(_bulletPrefab);
        var newRotation = new Vector3(this.transform.rotation.x, newAngle, this.transform.rotation.z);
        var forward = new Vector3(Mathf.Sin(Mathf.Deg2Rad * newAngle), 0, Mathf.Cos(Mathf.Deg2Rad *newAngle));
        bullet.transform.SetPositionAndRotation(this.transform.position+ forward.normalized*_bodyRadius,Quaternion.Euler(newRotation));
        var script = bullet.GetComponent<BulletScript>();
        script.BulletTypesList = bulletTypes.BulletTypes[0];
        _bulletManager.Bullets.Add(script);
        //Bullets.Add(bullet, bulletTypes);
    }

    private void FixedUpdate()
    {
        if (_turning)
        {
            _lerpT = _elapsedTime / _lerpTime;
            var newAngle = Mathf.Lerp(_angle, _angle + 180, _lerpT);
            this.gameObject.transform.rotation = Quaternion.Euler( this.transform.rotation.x, newAngle, this.transform.rotation.z);
        }
        _elapsedTime += Time.deltaTime;
        //foreach (var bullet in Bullets.Keys)
        //{

        //    Bullets.TryGetValue(bullet, out var bulletStruct);
        //    var _velocity = _bulletSpeed * Time.deltaTime * bullet.transform.forward;
        //    if (bulletStruct.BulletTypes.Contains(BulletTypes.Wave))
        //    {
        //        _velocity.y = Mathf.Lerp(-1, 1, bulletStruct.LerpT);
        //        if (bulletStruct.LerpT >= 1 || bulletStruct.LerpT <= 0)
        //            bulletStruct.LerpMod *= -1;
        //        bulletStruct.LerpT += Time.deltaTime * bulletStruct.LerpMod;
        //    }

        //    bullet.transform.position += _velocity;
        //    bulletStruct.ElapsedTime += Time.deltaTime;
        //    if (bulletStruct.ElapsedTime >= bulletStruct.LifeTime)
        //        Destroy(bullet.gameObject);
        //}
    }
}
