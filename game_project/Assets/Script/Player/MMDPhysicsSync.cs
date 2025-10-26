using UnityEngine;

namespace Game.Player
{
    public class MMDPhysicsSync : MonoBehaviour
    {
        public MMD4MecanimModel mmdModel;
        private MMD4MecanimBone rootBone;

        private Vector3 lastPos;

        void Start()
        {
            if (mmdModel != null)
                rootBone = mmdModel.GetRootBone();

            lastPos = transform.position;
        }

        private float syncTimer;

        void FixedUpdate()
        {
            syncTimer += Time.fixedDeltaTime;
            if (syncTimer < 0.05f) return; // 每 0.05 秒同步一次
            syncTimer = 0f;

            Vector3 deltaPos = transform.position - lastPos;
            rootBone.userPosition = deltaPos * 0.5f; // 缓步追随
            //rootBone.userRotation = transform.rotation;

            lastPos = transform.position;
        }

    }
}
