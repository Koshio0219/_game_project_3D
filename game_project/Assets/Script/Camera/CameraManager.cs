using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace Game.Camera
{
    public enum CameraMode { Free, LockOn, Special }

    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance;
        public CameraMode currentMode;

        [Header("Cinemachine References")]
        public CinemachineCamera freeLookCamera;
        public CinemachineTargetGroup targetGroup;
        public CinemachineCamera specialCam;

        private Transform player;
        private Transform lockOnTarget;

        void Awake() => Instance = this;

        void Start() 
        {
            player = GameObject.FindWithTag("Player").transform; 
            SwitchMode(CameraMode.Free);
            freeLookCamera.Follow = player;
        }

        public void SwitchMode(CameraMode mode)
        {
            currentMode = mode;
            freeLookCamera.gameObject.SetActive(mode == CameraMode.Free);
            targetGroup.gameObject.SetActive(mode == CameraMode.LockOn);
            specialCam.gameObject.SetActive(mode == CameraMode.Special);
        }

        public void SetLockOnTarget(Transform target)
        {
            lockOnTarget = target;
            targetGroup.Targets = new List<CinemachineTargetGroup.Target>
            {
            new() { Object = player, Weight = 1f, Radius = 1f },
            new() { Object = lockOnTarget, Weight = 1f, Radius = 2f }
            };
        }

        public void ClearLockOnTarget()
        {
            lockOnTarget = null;
            targetGroup.Targets = new List<CinemachineTargetGroup.Target>
            {
            new CinemachineTargetGroup.Target { Object = player, Weight = 1f, Radius = 1f }
            };
        }

        public IEnumerator TriggerSpecialCam(float duration = 0.4f)
        {
            SwitchMode(CameraMode.Special);
            yield return new WaitForSeconds(duration);
            SwitchMode(CameraMode.Free);
        }
    }
}
