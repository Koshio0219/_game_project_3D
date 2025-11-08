using UnityEngine;
using Game.Framework; 

namespace Game.Player
{
    public class PlayerInputHandler : MonoSingleton<PlayerInputHandler>
    {
        private InputSystem_Actions input;

        // ====== 公开属性 ======
        public Vector2 MoveInput { get; private set; }
        public bool RunningInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool SkillPressed { get; private set; }
        public bool ParryPressed { get; private set; }
        public bool DodgePressed { get; private set; }

        // ====== 生命周期 ======
        public override void Init()
        {
            input = new InputSystem_Actions();
            input.Enable();

            InitMove();
            InitRun();
            InitJump();
            InitAttack();
            InitSkill();
            InitParry();
            InitDodge();

            Debug.Log("[InputHandler] Initialized");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            input?.Disable();
        }

        // ====== 输入初始化 ======
        private void InitMove(bool canceled = true)
        {
            input.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            if (canceled)
                input.Player.Move.canceled += ctx => MoveInput = Vector2.zero;
        }

        private void InitRun(bool canceled = true)
        {
            input.Player.Sprint.performed += ctx => RunningInput = true;
            if (canceled)
                input.Player.Sprint.canceled += ctx => RunningInput = false;
        }

        private void InitJump(bool canceled = true)
        {
            input.Player.Jump.performed += ctx => JumpPressed = true;
            if (canceled)
                input.Player.Jump.canceled += ctx => JumpPressed = false;
        }

        private void InitAttack(bool canceled = true)
        {
            input.Player.Attack.performed += ctx => AttackPressed = true;
            if (canceled)
                input.Player.Attack.canceled += ctx => AttackPressed = false;
        }

        private void InitSkill(bool canceled = true)
        {
            input.Player.Skill.performed += ctx => SkillPressed = true;
            if (canceled)
                input.Player.Skill.canceled += ctx => SkillPressed = false;
        }

        private void InitParry(bool canceled = true)
        {
            input.Player.Parry.performed += ctx => ParryPressed = true;
            if (canceled)
                input.Player.Parry.canceled += ctx => ParryPressed = false;
        }

        private void InitDodge(bool canceled = true)
        {
            input.Player.Dodge.performed += ctx => DodgePressed = true;
            if (canceled)
                input.Player.Dodge.canceled += ctx => DodgePressed = false;
        }

        // ====== 工具方法 ======
        public InputSystem_Actions InputActions => input;
    }
}
