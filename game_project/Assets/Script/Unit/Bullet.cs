using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Framework;
using System.Collections;
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

        private UnityAction recycleAction = null;
        private UnityAction moveAction = null;

        private BulletProp initProp;
        private Vector3 targetPos;

        private void Awake()
        {
            initProp = new BulletProp(prop);
        }

        public void Init(GameObject target)
        {
            //token = this.GetCancellationTokenOnDestroy();
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            //token = this.GetCancellationTokenOnDisable();

            InitAction();
            recycleAction.Invoke();
            moveAction.Invoke();

            if (target == null) return;
            Target = target;
            //fix to player center with height 0.6f
            targetPos = Target.transform.position.FixHeight(Target.transform.position.y + .6f);
        }

        public void Recycle()
        {
            ResetAction();
            if (this == null) return;
            GameObjectPool.Instance.RecycleObj(gameObject);
        }

        private void InitAction()
        {
            UniTask.Void(async (_) =>
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(prop.lifeTime), cancellationToken: token);
                Recycle();
            }, token);
            UniTask.Void(async (_) =>
            {
                while (this && isActiveAndEnabled)
                {
                    Move();
                    await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate, token);
                }
            }, token);
        }

        private void Move()
        {
            float deltaTime = Time.fixedDeltaTime;
            if (Target != null && prop.angleSpeed > 0)
            {
                Vector3 offset = (targetPos- transform.position).normalized;

                float angle = Vector3.Angle(transform.forward, offset);

                float needTime = angle * 1.0f / prop.angleSpeed;

                transform.forward = Vector3.Lerp(transform.forward, offset, deltaTime / needTime).normalized;
            }
            if (prop.speed < prop.maxSpeed)
            {
                prop.speed += deltaTime * prop.acceleration;
            }
            transform.position += deltaTime * prop.speed * transform.forward;
        }

        private void ResetAction()
        {
            tokenSource.Cancel();
            prop = new BulletProp(initProp);
            recycleAction = () => { };
            moveAction = () => { };
            tokenSource = null;
            token = CancellationToken.None;
        }
    }
}