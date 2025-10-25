using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace Game.Player
{
    public enum PlayerState
    {
        Idle,
        Running,
        Jumpping,
    }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerMoveCtrl : MonoBehaviour
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
        private bool isRunning;
        private bool jumpPressed;

        private PlayerState state = PlayerState.Idle;
        public PlayerState State
        {
            get => state;
            set => OnPlayerStateChange(value);
        }

        void Awake()
        {
            controller = GetComponent<CharacterController>();

            // 创建输入系统实例
            input = new InputSystem_Actions();
            mainCamera = UnityEngine.Camera.main;

            // 注册输入事件回调
            input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

            input.Player.Sprint.performed += ctx => isRunning = true;
            input.Player.Sprint.canceled += ctx => isRunning = false;

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
            if (!isGrounded)
                return;

            var h = moveInput.x;
            var v = moveInput.y;

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
            float moveSpeed = isRunning && isGrounded ? runSpeed : walkSpeed;
            //transform.position += dt * moveSpeed * transform.forward;
            controller.Move(dt * moveSpeed * transform.forward);
               
            //rig.linearVelocity = Vector3.zero;
            //rig.AddForce(transform.forward * moveSpeed * dt);
            //rig.velocity = new Vector3(transform.position.x, 0, transform.position.z) * moveSpeed * dt;
            State = PlayerState.Running;
        }

        private void CheckJump()
        {
            if (jumpPressed && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                State = PlayerState.Jumpping;
                isGrounded = false;
                //jumpPressed = false;
            }
        }

        private void CheckIdle()
        {
            var noMove = moveInput == Vector2.zero;
            var noJump = isGrounded && velocity.y < 0;
            if (noMove && noJump)
            {
                State = PlayerState.Idle;
            }
        }

        private void OnPlayerStateChange(PlayerState to)
        {
            if (to == state)
                return;

            switch (to)
            {
                case PlayerState.Idle:
                    {
                        animator.CrossFade("Idle", .2f);
                        break;
                    }
                case PlayerState.Running:
                    {
                        animator.CrossFade("Running", .2f);
                        break;
                    }
                case PlayerState.Jumpping:
                    {
                        animator.CrossFade("Jump", .2f);
                        //animator.Play("Jumping@loop");
                        break;
                    }
            }

            state = to;
        }

        //test
        //void OnDrawGizmosSelected()
        //{
        //    if (groundCheck == null) return;
        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        //}
    }
}
