using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GoodbyeBuddy
{
    public class PlayerInput : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        private PlayerInputActions _actions;
        private InputAction _move, _jump, _grow;
        private bool _isGrowing; 

        private void Awake()
        {
            _actions = new PlayerInputActions();
            _move = _actions.Player.Move;
            _jump = _actions.Player.Jump;
            _grow = _actions.Player.Grow;
            _isGrowing = false; 
        }

        private void OnEnable() => _actions.Enable();

        private void OnDisable() => _actions.Disable();

        public FrameInput Gather()
        {
            if (_grow.WasPressedThisFrame())
            {
                _isGrowing = !_isGrowing;
            }

            return new FrameInput
            {
                JumpDown = _jump.WasPressedThisFrame(),
                JumpHeld = _jump.IsPressed(),
                Move = _move.ReadValue<Vector2>(),
                Grow = _isGrowing 
            };
        }
#else
        private bool _isGrowing;

        public FrameInput Gather()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                _isGrowing = !_isGrowing;
            }

            return new FrameInput
            {
                JumpDown = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C),
                JumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.C),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), 0),
                Grow = _isGrowing
            };
        }
#endif
        }

        public struct FrameInput
        {
            public Vector2 Move;
            public bool JumpDown;
            public bool JumpHeld;
            public bool Grow;
        }
}
