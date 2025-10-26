using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(MMD4MecanimModelImpl))]
    public class PlayerPhysiceSetting : MonoBehaviour
    {
        private void Awake()
        {
            var bulletPhysics = GetComponent<MMD4MecanimModelImpl>().bulletPhysics;

            // --- Morph 复位 ---
            bulletPhysics.useCustomResetTime = true;
            bulletPhysics.resetMorphTime = 0f;
            bulletPhysics.resetWaitTime = 0f;

            // --- 世界参数 ---
            bulletPhysics.worldProperty.gravityScale = 25f;
            bulletPhysics.worldProperty.gravityNoise = 0f;
            bulletPhysics.worldProperty.worldSolverInfoNumIterations = 20;
            bulletPhysics.worldProperty.worldSolverInfoSplitImpulse = true;
            bulletPhysics.worldProperty.multiThreading = true;

            // --- 模型参数 ---
            bulletPhysics.mmdModelProperty.rigidBodyMassRate = 10f;
            bulletPhysics.mmdModelProperty.rigidBodyLinearDampingRate = 16f;
            bulletPhysics.mmdModelProperty.rigidBodyAngularDampingRate = 18f;
            bulletPhysics.mmdModelProperty.rigidBodyFrictionRate = 12f;
            bulletPhysics.mmdModelProperty.rigidBodyIsUseCcd = true;
            bulletPhysics.mmdModelProperty.rigidBodyCcdMotionThreshold = 0.05f;
            bulletPhysics.mmdModelProperty.rigidBodyAntiJitterRate = 15f;
            bulletPhysics.mmdModelProperty.rigidBodyIsEnableSleeping = false;
        }

    }
}
