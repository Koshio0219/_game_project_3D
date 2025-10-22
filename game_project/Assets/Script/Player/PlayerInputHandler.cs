using System;
using UnityEngine;

namespace Game.Player
{
    public class PlayerInputHandler
    {
        private InputSystem_Actions input;
        private Vector2 Move;
        private bool Jump;
        private bool Run;

        public void Enable() => input.Enable();
        public void Disable() => input.Disable();

        public PlayerInputHandler()
        {
            input = new InputSystem_Actions();
        }

        public void InitMove(bool canceled =true)
        {
            if(input== null)  return;
            input.Player.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
            if (canceled)
                input.Player.Move.canceled += ctx => Move = Vector2.zero;
        }

        public void InitJump(bool canceled = true)
        {
            if (input == null) return;
            input.Player.Jump.performed += ctx => Jump = true;
            if (canceled)
                input.Player.Jump.canceled += ctx => Jump = false;
        }

        public void InitRun(bool canceled = true)
        {
            if (input == null) return;
            input.Player.Sprint.performed += ctx => Run = true;
            if (canceled)
                input.Player.Sprint.canceled += ctx => Run = false;
        }

        public Vector2 GetMoveInput() => Move;
        public bool JumpPressed() => Jump;
        public bool RunPressed() => Run;
    }
}
