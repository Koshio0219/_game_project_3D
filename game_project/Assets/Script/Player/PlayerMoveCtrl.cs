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
        //public Animator animator;

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

        //private InputSystem_Actions input;
        private PlayerInputHandler input;
        private PlayerStateHandler stateHandler;
        private UnityEngine.Camera mainCamera;

        //private Vector2 MoveInput;
        //private bool RunningInput;
        ////是否按下跳跃
        //private bool JumpPressed;
        //是否正在跳跃中
        private bool isJumping = false;
        [Header("Jumpping Settings")]
        [SerializeField] private float jumpDelay = 0.2f;

        //private PlayerAnimatorState state = PlayerAnimatorState.Idle;
        //public PlayerAnimatorState State
        //{
        //    get => state;
        //    set => OnPlayerStateChange(value);
        //}

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputHandler>();
            stateHandler = GetComponent<PlayerStateHandler>();
            mainCamera = UnityEngine.Camera.main;

            //// 创建输入系统实例
            //input = new InputSystem_Actions();

            //// 注册输入事件回调
            //input.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            //input.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

            //input.Player.Sprint.performed += ctx => RunningInput = true;
            //input.Player.Sprint.canceled += ctx => RunningInput = false;

            //input.Player.Jump.performed += ctx => JumpPressed = true;
            //input.Player.Jump.canceled += ctx => JumpPressed = false;
        }

        //void OnEnable() => input.Enable();
        //void OnDisable() => input.Disable();

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

            //transform.Translate(dt * moveSpeed * targetDirection);
            float moveSpeed = input.RunningInput && isGrounded ? runSpeed : walkSpeed;
            //transform.position += dt * moveSpeed * transform.forward;
            controller.Move(dt * moveSpeed * transform.forward);

            //rig.linearVelocity = Vector3.zero;
            //rig.AddForce(transform.forward * moveSpeed * dt);
            //rig.velocity = new Vector3(transform.position.x, 0, transform.position.z) * moveSpeed * dt;

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
            var noMove = input.MoveInput == Vector2.zero;
            var noJump = !isJumping && velocity.y < 0;
            if (noMove && noJump)
            {
                stateHandler.State = PlayerAnimatorState.Idle;
            }
        }

        //private void OnPlayerStateChange(PlayerAnimatorState to)
        //{
        //    if (to == state)
        //        return;

        //    switch (to)
        //    {
        //        case PlayerAnimatorState.Idle:
        //            {
        //                animator.CrossFade("Idle", .2f);
        //                //animator.speed = 1f;
        //                break;
        //            }
        //        case PlayerAnimatorState.Running:
        //            {
        //                animator.CrossFade("Running", .2f);
        //                //animator.speed = 1f;
        //                break;
        //            }
        //        case PlayerAnimatorState.Jump:
        //            {
        //                animator.CrossFade("Jump", .2f);
        //                //animator.speed = 1.5f;
        //                break;
        //            }
        //    }

        //    state = to;
        //}
    }
}
