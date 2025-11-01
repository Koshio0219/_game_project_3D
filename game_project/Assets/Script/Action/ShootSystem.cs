using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Game.Base;
using Game.Data;
using Game.Framework;
using Game.Unit;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Action
{
    public enum AngleOffseMode
    {
        NotOffse,
        XoY_Plane,
        XoZ_Plane
    }

    [System.Serializable]
    public class BaseProp
    {
        //public AssetReference bulletPrefab;
        public GameObject bulletPrefab;
        public Transform firePos;
        public GameObject target;

        public float damage = 10f;
    }

    [System.Serializable]
    public class AdvanceProp
    {
        public int extraCount = 0;
        public float timeInterval = .1f;
        public float offseDistance = 0f;
        public AngleOffseMode angleOffseMode = AngleOffseMode.NotOffse;
        //only if the angleOffseMode is LeftRightByFireForward or UpDownByFireForward effective
        public float offseAngle = 0f;
    }

    public class ShootSystem : FireBase
    {
        public BaseProp baseProp;

        public AdvanceProp advanceProp;

        private CancellationTokenSource tokenSource = null;
        private CancellationTokenSource TokenSource
        {
            get
            {
                tokenSource ??= new CancellationTokenSource();
                return tokenSource;
            }
        }

        public override  void FireBegin(int creatorId = 0, AttackState attackState = AttackState.Normal)
        {
            //test
            CreatBullet(creatorId).Forget();
        }

        public override void FireShut()
        {
            TokenSource.Cancel();
            tokenSource = null;
        }

        private async UniTask CreatBullet(int creatorId)
        {
            //CreatOne(creatorId).Forget();
            float startAngle = - advanceProp.extraCount * advanceProp.offseAngle * 0.5f; 
            for (int i = 0; i <= advanceProp.extraCount; i++)
            {
                var angle = startAngle + i * advanceProp.offseAngle;
                var dir = CalBulletDirection(angle);
                var pos = baseProp.firePos.position + dir * advanceProp.offseDistance;
                CreatOne(creatorId,pos,dir).Forget();
                var delay = advanceProp.timeInterval;
                if (delay == 0) continue;
                await UniTask.Delay((int)(delay * 1000), cancellationToken: TokenSource.Token);
            }
        }

        private async UniTask CreatOne(int creatorId, Vector3 position, Vector3 direction)
        {
            //move
            //var bullet = await AssetLoader.Instance.Load<Bullet>
            //    (baseProp.bulletPrefab, TokenSource.Token);
            var bullet = GameObjectPool.Instance.GetObj(baseProp.bulletPrefab).GetComponent<Bullet>();
            bullet.transform.SetParent(null);
            bullet.transform.position = position;
            bullet.transform.forward = direction;
            bullet.Init(baseProp.target);

            //hit
            var hit = bullet.GetComponent<BulletHit>();
            hit.Init((creatorId, baseProp.damage));
            //var cts = new CancellationTokenSource();
            //var enterTrigger = hit.GetAsyncTriggerEnterTrigger();
            //var enter = await enterTrigger.OnTriggerEnterAsync(cts.Token);
            //hit.OnEnterHit(enter, creatorId, baseProp.damage);
            //cts.Cancel();

            //recycle
            //var self = transform.GetRootParent();
            //var up = enter.transform.GetRootParent().gameObject;
            //if (up.GetComponent<Bullet>()) return;
            //if (up.GetInstanceID() == self.GetInstanceID()) return;
            //bullet.Recycle();
        }

        private Vector3 CalBulletDirection(float angle)
        {
            switch (advanceProp.angleOffseMode)
            {
                default:
                case AngleOffseMode.NotOffse:
                    return baseProp.firePos.forward;
                case AngleOffseMode.XoY_Plane:
                    {
                        //var dir1 = Vector3.ProjectOnPlane(baseProp.firePos.forward, Vector3.forward).normalized;
                        var dir2 = GameHelper.RotateDirection
                            (baseProp.firePos.forward, angle, Vector3.forward);
                        return dir2.normalized;
                    }
                case AngleOffseMode.XoZ_Plane:
                    {
                        //var dir1 = Vector3.ProjectOnPlane(baseProp.firePos.forward, Vector3.up).normalized;
                        var dir2 = GameHelper.RotateDirection
                            (baseProp.firePos.forward, angle, Vector3.up);
                        return dir2.normalized;
                    }
            }
        }

        #region setProp
        public override FireBase SetTarget(GameObject tar)
        {
            baseProp.target = tar;
            return base.SetTarget(tar);
        }

        public override FireBase SetFirePos(Transform pos)
        {
            baseProp.firePos = pos;
            return base.SetFirePos(pos);
        }

        public override FireBase SetDamage(float damage, float damageUpSpeed = 0, int maxDamage = 0, float exCritRate = 0)
        {
            baseProp.damage = damage;
            return base.SetDamage(baseProp.damage, damageUpSpeed, maxDamage, exCritRate);
        }
        #endregion
    }
}