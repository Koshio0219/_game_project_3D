using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Framework;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Unit
{
    [System.Serializable]
    public class BulletProp
    {
        public float speed;

        public float lifeTime = 3f;

        public float angleSpeed = 120f;

        public float acceleration = 3f;

        public float maxSpeed = 60f;

        public BulletProp(float sp, float lt, float ans, float acc, float ms)
        {
            speed = sp;
            lifeTime = lt;
            angleSpeed = ans;
            acceleration = acc;
            maxSpeed = ms;
        }

        public BulletProp(BulletProp prop)
        {
            speed = prop.speed;
            lifeTime = prop.lifeTime;
            angleSpeed = prop.angleSpeed;
            acceleration = prop.acceleration;
            maxSpeed = prop.maxSpeed;
        }
    }

    public class Bullet : MonoBehaviour, IInit<GameObject>
    {
        public BulletProp prop;
        public GameObject Target { get; private set; }

        private CancellationToken token = CancellationToken.None;
        private CancellationTokenSource tokenSource = null;

        private BulletProp initProp;
        private Vector3 targetPos;

        private void Awake()
        {
            initProp = new BulletProp(prop);
        }

        public void Init(GameObject target)
        {
            // 先把旧任务停掉（如果是池里复用）
            if (tokenSource != null && !tokenSource.IsCancellationRequested)
                tokenSource.Cancel();

            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            // 重置属性
            prop = new BulletProp(initProp);

            // 目标
            Target = target;
            if (Target != null)
            {
                targetPos = Target.transform.position.FixHeight(Target.transform.position.y + .6f);
            }

            // 启动生命周期 & 移动逻辑
            InitAction();
        }

        private void InitAction()
        {
            // 寿命结束回收
            UniTask.Void(async _ =>
            {
                try
                {
                    await UniTask.Delay(
                        System.TimeSpan.FromSeconds(prop.lifeTime),
                        cancellationToken: token
                    );
                    Recycle();
                }
                catch { /* 被取消就算了 */ }
            }, token);

            // 持续移动
            UniTask.Void(async _ =>
            {
                try
                {
                    while (this && isActiveAndEnabled && !token.IsCancellationRequested)
                    {
                        Move();
                        await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate, token);
                    }
                }
                catch { /* token 取消正常退出 */ }
            }, token);
        }

        public void Recycle()
        {
            if (!this) return;

            if (tokenSource != null && !tokenSource.IsCancellationRequested)
                tokenSource.Cancel();

            GameObjectPool.Instance.RecycleObj(gameObject);
        }


        private void Move()
        {
            float deltaTime = Time.fixedDeltaTime;

            if (Target != null && prop.angleSpeed > 0)
            {
                var offset = (targetPos - transform.position).normalized;
                if (offset.sqrMagnitude > 0.0001f)
                {
                    float angle = Vector3.Angle(transform.forward, offset);
                    float needTime = Mathf.Max(angle / prop.angleSpeed, 0.0001f);
                    transform.forward = Vector3.Lerp(transform.forward, offset, deltaTime / needTime).normalized;
                }
            }

            if (prop.speed < prop.maxSpeed)
                prop.speed += deltaTime * prop.acceleration;

            transform.position += deltaTime * prop.speed * transform.forward;
        }
    }
}