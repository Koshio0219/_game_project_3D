using UnityEngine;

namespace Game.Player
{
    public class PlayerInputHandler: MonoBehaviour
    {
        private InputSystem_Actions input;

        private Vector2 moveInput;
        public Vector2 MoveInput => moveInput;
        private bool runningInput;
        public bool RunningInput => runningInput;
        private bool jumpPressed;
        public bool JumpPressed => jumpPressed;
        private bool attackPressed;
        public bool AttackPressed => attackPressed;
        private bool skillPressed;
        public bool SkillPressed => skillPressed;
        private bool parryPressed;
        public bool ParryPressed => parryPressed;
        private bool dodgePressed;
        public bool DodgePressed => dodgePressed;

        public void OnEnable() => input.Enable();
        public void OnDisable() => input.Disable();

        private void Awake()
        {
            input = new InputSystem_Actions();
            InitMove();
            InitJump();
            InitRun();
            InitAttack();
            InitSkill();
            InitDodge();
            InitParry();
        }

        public void InitMove(bool canceled =true)
        {
            if(input== null)  return;
            input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            if (canceled)
                input.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        }

        public void InitJump(bool canceled = true)
        {
            if (input == null) return;
            input.Player.Jump.performed += ctx => jumpPressed = true;
            if (canceled)
                input.Player.Jump.canceled += ctx => jumpPressed = false;
        }

        public void InitRun(bool canceled = true)
        {
            if (input == null) return;
            input.Player.Sprint.performed += ctx => runningInput = true;
            if (canceled)
                input.Player.Sprint.canceled += ctx => runningInput = false;
        }

        public void InitAttack(bool canceled = true)
        {
            if (input == null) return;
            input.Player.Attack.performed += ctx => attackPressed = true;
            if (canceled)
                input.Player.Attack.canceled += ctx => attackPressed = false;
        }

        public void InitSkill(bool canceled = true)
        {
            if (input == null) return;
            input.Player.Skill.performed += ctx => skillPressed = true;
            if (canceled)
                input.Player.Skill.canceled += ctx => skillPressed = false;
        }

        public void InitParry(bool canceled = true)
        {
            if (input == null) return;
            input.Player.Parry.performed += ctx => parryPressed = true;
            if (canceled)
                input.Player.Parry.canceled += ctx => parryPressed = false;
        }

        public void InitDodge(bool canceled = true)
        {
            if (input == null) return;
            input.Player.Dodge.performed += ctx => dodgePressed = true;
            if (canceled)
                input.Player.Dodge.canceled += ctx => dodgePressed = false;
        }
    }
}
