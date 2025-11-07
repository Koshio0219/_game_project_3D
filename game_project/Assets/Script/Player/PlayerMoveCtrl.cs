using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Game.Framework;

namespace Game.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerStateHandler))]
    public class PlayerMoveCtrl : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 5f;
        public float runSpeed = 10f;
        public float jumpHeight = 3f;
        public float gravity = -9.81f;

        [Header("Ground Check")]
        //public Transform groundCheck;
        //public float groundDistance = 0.4f;
        //public LayerMask groundMask;
        public float groundedRememberTime = 0.2f;
        private float lastGroundedTime;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;

        private PlayerInputHandler input;
        private PlayerStateHandler stateHandler;
        private Camera mainCamera;

        private bool isJumping = false;
        [Header("Jumpping Settings")]
        [SerializeField] private float jumpDelay = 0.2f;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputHandler>();
            stateHandler = GetComponent<PlayerStateHandler>();
            mainCamera = Camera.main;
        }

        void Update()
        {
            // --- Ground Check ---
            //每帧检测地形消耗极大，改用其他方式
            //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            //if (isGrounded && velocity.y < 0)
            //    velocity.y = -2f;
            GroundCheck();

            // --- Movement ---
            Movement(Time.deltaTime);

            // --- Jump ---
            CheckJump();

            // ---IdleCheck---
            CheckIdle();

            // --- Gravity ---
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void GroundCheck()
        {
            if (controller.isGrounded)
            {
                isGrounded = true;
                lastGroundedTime = Time.time;
                if (velocity.y < 0)
                    velocity.y = -2f; // 保持粘地
            }
            else
            {
                // 若刚离地不久仍认为是地面（防止在坡地边缘闪烁）
                if (Time.time - lastGroundedTime > groundedRememberTime)
                    isGrounded = false;
            }
        }

        private void Movement(float dt)
        {
            // 排除特殊状态（闪避、招架），防止中途被移动逻辑打断
            if (stateHandler.State == PlayerAnimatorState.Dodge ||
                stateHandler.State == PlayerAnimatorState.Parry) 
                return;

            var h = input.MoveInput.x;
            var v = input.MoveInput.y;

            if (h == 0 && v == 0)
                return;

            //fix move direction bug
            var forward =mainCamera.transform.TransformDirection(Vector3.forward);
            forward.y = 0;

            var right = mainCamera.transform.TransformDirection(Vector3.right);

            var targetDirection = h * right + v * forward;
            if (targetDirection == Vector3.zero)
                return;
            transform.forward = targetDirection;

            float moveSpeed = input.RunningInput && isGrounded ? runSpeed : walkSpeed;
            controller.Move(dt * moveSpeed * transform.forward);

            if (isGrounded && !isJumping)
                stateHandler.State = PlayerAnimatorState.Running;
        }

        private void CheckJump()
        {
            if (input.JumpPressed && isGrounded && !isJumping)
            {
                JumpAsync().Forget();
            }
        }

        private async UniTaskVoid JumpAsync()
        {
            isJumping = true;

            // 播放起跳动画（动画里脚开始下蹲的时机）
            stateHandler.State = PlayerAnimatorState.Jump;

            // 等待动画的起跳前置时间
            await UniTask.Delay(TimeSpan.FromSeconds(jumpDelay), cancellationToken: this.GetCancellationTokenOnDestroy());

            // 执行物理跳跃
            if (isGrounded) // 确保此时仍在地面上
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                isGrounded = false;
            }

            // 等待角色落地
            await UniTask.WaitUntil(() => controller.isGrounded);
            isJumping = false;
        }

        private void CheckIdle()
        {
            // 只有当前状态属于可自由移动状态时，才判断是否Idle
            if (stateHandler.State != PlayerAnimatorState.Running && 
                stateHandler.State != PlayerAnimatorState.Jump)
                return;

            var noMove = input.MoveInput == Vector2.zero;
            var noJump = !isJumping && velocity.y < 0;
            if (noMove && noJump)
            {
                stateHandler.State = PlayerAnimatorState.Idle;
            }
        }
    }
}
