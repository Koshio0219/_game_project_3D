using Cysharp.Threading.Tasks;
using Game.Framework;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.CameraSystem
{
    public class CameraManager : MonoSingleton<CameraManager>
    {
        public CinemachineCamera mainCamera;
        public CinemachineCamera parryCamera;
        public CinemachineCamera dodgeCamera;

        public float blendDuration = 0.4f;
        public float parryDuration = 0.8f;
        public float dodgeDuration = 0.6f;

        public float shakeAmplitude = 1.2f;
        public float shakeFrequency = 2.0f;
        public float shakeTime = 0.25f;

        private CinemachineBrain brain;
        private CinemachineBasicMultiChannelPerlin noiseComp;
        private Transform player;

        protected override bool ShouldPersist => false;

        protected override void Awake()
        {
            base.Awake();
            brain = Camera.main.GetComponent<CinemachineBrain>();
            if (mainCamera != null)
                noiseComp = mainCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Stage")
                RebindPlayer().Forget();
        }

        private void Start()
        {
            RebindPlayer().Forget();
            SetActiveCamera(mainCamera);
        }

        private async UniTask RebindPlayer()
        {
            await UniTask.DelayFrame(1); // 等待Cinemachine初始化完毕
            player = GameObject.FindWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogWarning("CameraManager: 未找到 Player 对象");
                return;
            }

            AssignTarget(mainCamera, player, player);
            AssignTarget(parryCamera, player, player);
            AssignTarget(dodgeCamera, player, player);

            Debug.Log("CameraManager: 成功绑定新 Player。");
        }

        private void AssignTarget(CinemachineCamera cam, Transform follow, Transform lookAt)
        {
            if (cam == null) return;
            cam.Follow = follow;
            cam.LookAt = lookAt;
        }

        private void SetActiveCamera(CinemachineCamera cam)
        {
            if (mainCamera) mainCamera.gameObject.SetActive(false);
            if (parryCamera) parryCamera.gameObject.SetActive(false);
            if (dodgeCamera) dodgeCamera.gameObject.SetActive(false);

            if (cam) cam.gameObject.SetActive(true);
        }

        public async UniTaskVoid PlayParryCamera()
        {
            if (parryCamera == null) return;

            SetActiveCamera(parryCamera);
            ShakeCamera();

            await UniTask.Delay(TimeSpan.FromSeconds(parryDuration));
            await UniTask.Delay(TimeSpan.FromSeconds(blendDuration));
            SetActiveCamera(mainCamera);

        }

        public async UniTaskVoid PlayDodgeCamera()
        {
            if (dodgeCamera == null) return;

            SetActiveCamera(dodgeCamera);
            ShakeCamera();

            await UniTask.Delay(TimeSpan.FromSeconds(dodgeDuration));
            await UniTask.Delay(TimeSpan.FromSeconds(blendDuration));
            SetActiveCamera(mainCamera);

        }

        private async void ShakeCamera()
        {
            if (noiseComp == null) return;
            noiseComp.AmplitudeGain = shakeAmplitude;
            noiseComp.FrequencyGain = shakeFrequency;

            await UniTask.Delay(TimeSpan.FromSeconds(shakeTime));

            noiseComp.AmplitudeGain = 0;
            noiseComp.FrequencyGain = 0;
        }
    }
}
