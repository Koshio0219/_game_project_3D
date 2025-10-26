using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRigidBodyMover : MonoBehaviour
    {
        public Animator animator;

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

        private InputSystem_Actions input;
        private UnityEngine.Camera mainCamera;

        private Vector2 moveInput;
        private bool RunningInput;
        //是否按下跳跃
        private bool jumpPressed;
        //是否正在跳跃中
        private bool isJumping = false;
        [Header("Jumpping Settings")]
        [SerializeField] private float jumpDelay = 0.2f;

        private Rigidbody rb;
        private bool useRigidbody = true; // 切换控制方式（true=刚体，false=CharacterController）

        private PlayerAnimatorState state = PlayerAnimatorState.Idle;
        public PlayerAnimatorState State
        {
            get => state;
            set => OnPlayerStateChange(value);
        }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            rb = GetComponent<Rigidbody>();
            mainCamera = UnityEngine.Camera.main;

            // 创建输入系统实例
            input = new InputSystem_Actions();

            // 注册输入事件回调
            input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

            input.Player.Sprint.performed += ctx => RunningInput = true;
            input.Player.Sprint.canceled += ctx => RunningInput = false;

            input.Player.Jump.performed += ctx => jumpPressed = true;
            input.Player.Jump.canceled += ctx => jumpPressed = false;
        }

        void OnEnable() => input.Enable();
        void OnDisable() => input.Disable();

        void Update()
        {
            // --- Ground Check ---
            //每帧检测地形消耗极大，改用其他方式
            //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            //if (isGrounded && velocity.y < 0)
            //    velocity.y = -2f;
            GroundCheck();

            // --- Movement ---
            //Movement(Time.deltaTime);

            // --- Jump ---
            CheckJump();

            // ---IdleCheck---
            CheckIdle();

            //if (useRigidbody)
            //    return;
            //// --- Gravity ---
            //velocity.y += gravity * Time.deltaTime;
            //controller.Move(velocity * Time.deltaTime);
        }

        void FixedUpdate()
        {
            Movement(Time.fixedDeltaTime);

            // 重力
            if (useRigidbody)
                return;

            velocity.y += gravity * Time.fixedDeltaTime;
            controller.Move(velocity * Time.fixedDeltaTime);
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
            var h = moveInput.x;
            var v = moveInput.y;

            if (h == 0 && v == 0)
                return;

            //fix move direction bug
            var forward = mainCamera.transform.TransformDirection(Vector3.forward);
            forward.y = 0;

            var right = mainCamera.transform.TransformDirection(Vector3.right);

            var targetDirection = h * right + v * forward;
            if (targetDirection == Vector3.zero)
                return;
            transform.forward = targetDirection;

            //transform.Translate(dt * moveSpeed * targetDirection);
            float moveSpeed = RunningInput && isGrounded ? runSpeed : walkSpeed;
            //transform.position += dt * moveSpeed * transform.forward;
            //controller.Move(dt * moveSpeed * transform.forward);
            if (useRigidbody)
            {
                // 切换为刚体移动（非瞬移）
                Vector3 newVelocity = rb.linearVelocity;
                newVelocity.x = targetDirection.normalized.x * moveSpeed;
                newVelocity.z = targetDirection.normalized.z * moveSpeed;
                rb.linearVelocity = newVelocity;
                

                // 模拟 CharacterController 的“粘地”
                if (isGrounded && velocity.y < 0)
                    velocity.y = -2f;
            }
            else
            {
                // 原 CharacterController 逻辑
                controller.Move(dt * moveSpeed * transform.forward);
            }
            //rig.linearVelocity = Vector3.zero;
            //rig.AddForce(transform.forward * moveSpeed * dt);
            //rig.velocity = new Vector3(transform.position.x, 0, transform.position.z) * moveSpeed * dt;

            if (isGrounded && !isJumping)
                State = PlayerAnimatorState.Running;
        }

        private void CheckJump()
        {
            if (jumpPressed && isGrounded && !isJumping)
            {
                JumpAsync().Forget();
            }
        }

        private async UniTaskVoid JumpAsync()
        {
            isJumping = true;

            // 播放起跳动画（动画里脚开始下蹲的时机）
            State = PlayerAnimatorState.Jump;

            // 等待动画的起跳前置时间
            await UniTask.Delay(TimeSpan.FromSeconds(jumpDelay), cancellationToken: this.GetCancellationTokenOnDestroy());

            // 执行物理跳跃
            if (isGrounded) // 确保此时仍在地面上
            {
                if (useRigidbody)
                    rb.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * gravity), ForceMode.VelocityChange);
                else
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                isGrounded = false;
            }

            // 等待角色落地
            await UniTask.WaitUntil(() => controller.isGrounded);
            isJumping = false;
        }


        private void CheckIdle()
        {
            var noMove = moveInput == Vector2.zero;
            var noJump = !isJumping && velocity.y < 0;
            if (noMove && noJump)
            {
                State = PlayerAnimatorState.Idle;
            }
        }

        private void OnPlayerStateChange(PlayerAnimatorState to)
        {
            if (to == state)
                return;

            switch (to)
            {
                case PlayerAnimatorState.Idle:
                    {
                        animator.CrossFade("Idle", .2f);
                        //animator.speed = 1f;
                        break;
                    }
                case PlayerAnimatorState.Running:
                    {
                        animator.CrossFade("Running", .2f);
                        //animator.speed = 1f;
                        break;
                    }
                case PlayerAnimatorState.Jump:
                    {
                        animator.CrossFade("Jump", .2f);
                        //animator.speed = 1.5f;
                        break;
                    }
            }

            state = to;
        }
    }
}
