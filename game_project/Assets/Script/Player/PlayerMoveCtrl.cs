using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMoveCtrl : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 5f;
        public float runSpeed = 10f;
        public float jumpHeight = 3f;
        public float gravity = -9.81f;

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;

        private InputSystem_Actions input;

        private Vector2 moveInput;
        private bool isRunning;
        private bool jumpPressed;

        void Awake()
        {
            controller = GetComponent<CharacterController>();

            // ✅ 创建输入系统实例
            input = new InputSystem_Actions();

            // ✅ 注册输入事件回调
            input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

            input.Player.Sprint.performed += ctx => isRunning = true;
            input.Player.Sprint.canceled += ctx => isRunning = false;

            input.Player.Jump.performed += ctx => jumpPressed = true;
        }

        void OnEnable() => input.Enable();
        void OnDisable() => input.Disable();

        void Update()
        {
            // --- Ground Check ---
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;

            // --- Movement ---
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            float currentSpeed = isRunning && isGrounded ? runSpeed : walkSpeed;
            controller.Move(move * currentSpeed * Time.deltaTime);

            // --- Jump ---
            if (jumpPressed && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpPressed = false;
            }

            // --- Gravity ---
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
