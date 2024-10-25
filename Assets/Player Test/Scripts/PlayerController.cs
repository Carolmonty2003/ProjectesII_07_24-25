using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoodbyeBuddy
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(CapsuleCollider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController, IPhysicsObject
    {
        #region References

        private BoxCollider2D _collider;
        private CapsuleCollider2D _airborneCollider;
        private ConstantForce2D _constantForce;
        private Rigidbody2D _rb;
        private PlayerInput _playerInput;

        #endregion

        #region Interface

        [field: SerializeField] public PlayerStats Stats { get; private set; }
        public ControllerState State { get; private set; }
        public event Action<JumpType> Jumped;
        public event Action<bool, float> GroundedChanged;
        public event Action<Vector2> Repositioned;
        public event Action<bool> ToggledPlayer;

        public bool Active { get; private set; } = true;
        public Vector2 Up { get; private set; }
        public Vector2 Right { get; private set; }
        public bool Growing { get; private set; }
        public Vector2 Input => _frameInput.Move;
        public Vector2 GroundNormal { get; private set; }
        public Vector2 Velocity { get; private set; }

        public void AddFrameForce(Vector2 force, bool resetVelocity = false)
        {
            if (resetVelocity) SetVelocity(Vector2.zero);
            _forceToApplyThisFrame += force;
        }

        public void LoadState(ControllerState state)
        {
            RepositionImmediately(state.Position);
            _rb.rotation = state.Rotation;
            SetVelocity(state.Velocity);

            if (state.Grounded) ToggleGrounded(true);
        }

        public void RepositionImmediately(Vector2 position, bool resetVelocity = false)
        {
            _rb.position = position;
            if (resetVelocity) SetVelocity(Vector2.zero);
            Repositioned?.Invoke(position);
        }

        public void TogglePlayer(bool on)
        {
            Active = on;

            _rb.isKinematic = !on;
            ToggledPlayer?.Invoke(on);
        }

        #endregion

        [SerializeField] private bool _drawGizmos = true;

        #region Loop

        private float _delta, _time;

        private void Awake()
        {
            if (!TryGetComponent(out _playerInput)) _playerInput = gameObject.AddComponent<PlayerInput>();
            if (!TryGetComponent(out _constantForce)) _constantForce = gameObject.AddComponent<ConstantForce2D>();

            SetupCharacter();

            PhysicsSimulator.Instance.AddPlayer(this);
        }

        private void OnDestroy() => PhysicsSimulator.Instance.RemovePlayer(this);

        public void OnValidate() => SetupCharacter();

        public void TickUpdate(float delta, float time)
        {
            _delta = delta;
            _time = time;

            GatherInput();
        }

        public void TickFixedUpdate(float delta)
        {
            _delta = delta;

            if (!Active) return;

            RemoveTransientVelocity();

            SetFrameData();

            CalculateCollisions();
            CalculateDirection();
            CalculateJump();

            CalculateExternalModifiers();

            TraceGround();
            Move();

            CalculateGrow();

            CleanFrameData();

            SaveCharacterState();
        }

        #endregion

        #region Setup

        private bool _cachedQueryMode, _cachedQueryTriggers;
        private GeneratedCharacterSize _character;
        private const float GRAVITY_SCALE = 1;

        private void SetupCharacter()
        {
            _character = Stats.CharacterSize.GenerateCharacterSize();
            _cachedQueryMode = Physics2D.queriesStartInColliders;

            _rb = GetComponent<Rigidbody2D>();
            _rb.hideFlags = HideFlags.NotEditable;

            // Primary collider
            _collider = GetComponent<BoxCollider2D>();
            _collider.edgeRadius = CharacterSize.COLLIDER_EDGE_RADIUS;
            _collider.hideFlags = HideFlags.NotEditable;
            _collider.sharedMaterial = _rb.sharedMaterial;
            _collider.enabled = true;

            // Airborne collider
            _airborneCollider = GetComponent<CapsuleCollider2D>();
            _airborneCollider.hideFlags = HideFlags.NotEditable;
            _airborneCollider.size = new Vector2(_character.Width - SKIN_WIDTH * 2, _character.Height - SKIN_WIDTH * 2);
            _airborneCollider.offset = new Vector2(0, _character.Height / 2);
            _airborneCollider.sharedMaterial = _rb.sharedMaterial;

            SetColliderMode(ColliderMode.Airborne);
        }

        #endregion

        #region Input

        private FrameInput _frameInput;

        private void GatherInput()
        {
            _frameInput = _playerInput.Gather();


            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        #endregion

        #region Frame Data

        private bool _hasInputThisFrame;
        private Vector2 _trimmedFrameVelocity;
        private Vector2 _framePosition;

        private void SetFrameData()
        {
            var rot = _rb.rotation * Mathf.Deg2Rad;
            Up = new Vector2(-Mathf.Sin(rot), Mathf.Cos(rot));
            Right = new Vector2(Up.y, -Up.x);
            _framePosition = _rb.position;

            _hasInputThisFrame = _frameInput.Move.x != 0;

            Velocity = _rb.velocity;
            _trimmedFrameVelocity = new Vector2(Velocity.x, 0);
        }

        private void RemoveTransientVelocity()
        {
            var currentVelocity = _rb.velocity;
            var velocityBeforeReduction = currentVelocity;

            currentVelocity -= _totalTransientVelocityAppliedLastFrame;
            SetVelocity(currentVelocity);

            _frameTransientVelocity = Vector2.zero;
            _totalTransientVelocityAppliedLastFrame = Vector2.zero;
        }

        private void CleanFrameData()
        {
            _jumpToConsume = false;
            _forceToApplyThisFrame = Vector2.zero;
        }

        #endregion

        #region Collisions

        private const float SKIN_WIDTH = 0.02f;
        private const int RAY_SIDE_COUNT = 5;
        private RaycastHit2D _groundHit;
        private bool _grounded;
        private float _currentStepDownLength;
        private float GrounderLength => _character.StepHeight + SKIN_WIDTH;

        private Vector2 RayPoint => _framePosition + Up * (_character.StepHeight + SKIN_WIDTH);

        private void CalculateCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Is the middle ray good?
            var isGroundedThisFrame = PerformRay(RayPoint);

            // If not, zigzag rays from the center outward until we find a hit
            if (!isGroundedThisFrame)
            {
                foreach (var offset in GenerateRayOffsets())
                {
                    isGroundedThisFrame = PerformRay(RayPoint + Right * offset) || PerformRay(RayPoint - Right * offset);
                    if (isGroundedThisFrame) break;
                }
            }

            if (isGroundedThisFrame && !_grounded) ToggleGrounded(true);
            else if (!isGroundedThisFrame && _grounded) ToggleGrounded(false);

            Physics2D.queriesStartInColliders = _cachedQueryMode;

            bool PerformRay(Vector2 point)
            {
                _groundHit = Physics2D.Raycast(point, -Up, GrounderLength + _currentStepDownLength, Stats.CollisionLayers);
                if (!_groundHit) return false;

                if (Vector2.Angle(_groundHit.normal, Up) > Stats.MaxWalkableSlope)
                {
                    return false;
                }

                return true;
            }
        }

        private IEnumerable<float> GenerateRayOffsets()
        {
            var extent = _character.StandingColliderSize.x / 2 - _character.RayInset;
            var offsetAmount = extent / RAY_SIDE_COUNT;
            for (var i = 1; i < RAY_SIDE_COUNT + 1; i++)
            {
                yield return offsetAmount * i;
            }
        }

        private void ToggleGrounded(bool grounded)
        {
            _grounded = grounded;
            if (grounded)
            {
                GroundedChanged?.Invoke(true, _lastFrameY);
                _rb.gravityScale = 0;
                SetVelocity(_trimmedFrameVelocity);
                _constantForce.force = Vector2.zero;
                _currentStepDownLength = _character.StepHeight;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                ResetAirJumps();
                SetColliderMode(ColliderMode.Standard);
            }
            else
            {
                GroundedChanged?.Invoke(false, 0);
                _timeLeftGrounded = _time;
                _rb.gravityScale = GRAVITY_SCALE;
                SetColliderMode(ColliderMode.Airborne);
            }
        }

        private void SetColliderMode(ColliderMode mode)
        {
            _airborneCollider.enabled = mode == ColliderMode.Airborne;

            switch (mode)
            {
                case ColliderMode.Standard:
                    _collider.size = _character.StandingColliderSize;
                    _collider.offset = _character.StandingColliderCenter;
                    break;
                case ColliderMode.Growing:
                    _collider.size = _character.GrowColliderSize;
                    _collider.offset = _character.GrowingColliderCenter;
                    break;
                case ColliderMode.Airborne:
                    break;
            }
        }

        private enum ColliderMode
        {
            Standard,
            Growing,
            Airborne
        }

        #endregion

        #region Direction

        private Vector2 _frameDirection;

        private void CalculateDirection()
        {
            _frameDirection = new Vector2(_frameInput.Move.x, 0);

            if (_grounded)
            {
                GroundNormal = _groundHit.normal;
                var angle = Vector2.Angle(GroundNormal, Up);
                if (angle < Stats.MaxWalkableSlope) _frameDirection.y = _frameDirection.x * -GroundNormal.x / GroundNormal.y;
            }

            _frameDirection = _frameDirection.normalized;
        }

        #endregion

        #region Jump

        private const float JUMP_CLEARANCE_TIME = 0.25f;
        private bool IsWithinJumpClearance => _lastJumpExecutedTime + JUMP_CLEARANCE_TIME > _time;
        private float _lastJumpExecutedTime;
        private bool _bufferedJumpUsable;
        private bool _jumpToConsume;
        private float _timeJumpWasPressed;
        private Vector2 _forceToApplyThisFrame;
        private bool _endedJumpEarly;
        private float _endedJumpForce;
        private int _airJumpsRemaining;
        private bool _coyoteUsable;
        private float _timeLeftGrounded;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + Stats.BufferedJumpTime && !IsWithinJumpClearance;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _timeLeftGrounded + Stats.CoyoteTime;
        private bool CanAirJump => !_grounded && _airJumpsRemaining > 0;

        private void CalculateJump()
        {
            if ((_jumpToConsume || HasBufferedJump) && CanStand)
            {
                if (_grounded) ExecuteJump(JumpType.Jump);
                else if (CanUseCoyote) ExecuteJump(JumpType.Coyote);
                else if (CanAirJump) ExecuteJump(JumpType.AirJump);
            }

            if ((!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && Velocity.y > 0) || Velocity.y < 0) _endedJumpEarly = true; // Early end detection
        }

        private void ExecuteJump(JumpType jumpType)
        {
            SetVelocity(_trimmedFrameVelocity);
            _endedJumpEarly = false;
            _bufferedJumpUsable = false;
            _lastJumpExecutedTime = _time;
            _currentStepDownLength = 0;


            if (jumpType is JumpType.Jump or JumpType.Coyote or JumpType.AirJump)
            {
                _coyoteUsable = false;

                if (Growing)
                {

                    AddFrameForce(new Vector2(0, Stats.JumpPower * 0.75f)); 
                }
                else
                {
                    AddFrameForce(new Vector2(0, Stats.JumpPower));
                }
            }
            else
            {}

            Jumped?.Invoke(jumpType);
        }

        private void ResetAirJumps() => _airJumpsRemaining = Stats.MaxAirJumps;

        #endregion

        #region Growing

        private float _timeStartedGrowing;
        private bool GrowPressed => _frameInput.Move.y < -Stats.VerticalDeadZoneThreshold;

        private bool CanStand => IsStandingPosClear(_rb.position + _character.StandingColliderCenter);

        object IPlayerController.transform { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private bool IsStandingPosClear(Vector2 pos) => CheckPos(pos, _character.StandingColliderSize - SKIN_WIDTH * Vector2.one);

        private void CalculateGrow()
        {
            if (_frameInput.Grow && !Growing)
            {
                ToggleGrowing(true);
            }
            else if (!_frameInput.Grow && Growing)
            {
                ToggleGrowing(false);
            }
        }

        private void ToggleGrowing(bool shouldGrow)
        {
            if (shouldGrow)
            {
                _timeStartedGrowing = _time;
                Growing = true;
            }
            else
            {
                if (!CanStand) return;
                Growing = false;
            }

            SetColliderMode(Growing ? ColliderMode.Growing : ColliderMode.Standard);
        }

        private bool CheckPos(Vector2 pos, Vector2 size)
        {
            Physics2D.queriesHitTriggers = false;
            var hit = Physics2D.OverlapBox(pos, size, 0, Stats.CollisionLayers);
            Physics2D.queriesHitTriggers = _cachedQueryMode;
            return !hit;
        }

        #endregion

        #region Move

        private Vector2 _frameTransientVelocity;
        private Vector2 _immediateMove;
        private Vector2 _decayingTransientVelocity;
        private Vector2 _totalTransientVelocityAppliedLastFrame;
        private Vector2 _frameSpeedModifier, _currentFrameSpeedModifier = Vector2.one;
        private const float SLOPE_ANGLE_FOR_EXACT_MOVEMENT = 0.7f;
        private IPhysicsMover _lastPlatform;
        private float _lastFrameY;

        private void TraceGround()
        {
            IPhysicsMover currentPlatform = null;

            if (_grounded && !IsWithinJumpClearance)
            {
                var distanceFromGround = _character.StepHeight - _groundHit.distance;
                if (distanceFromGround != 0)
                {
                    var requiredMove = Vector2.zero;
                    requiredMove.y += distanceFromGround;

                    if (Stats.PositionCorrectionMode is PositionCorrectionMode.Velocity) _frameTransientVelocity = requiredMove / _delta;
                    else _immediateMove = requiredMove;
                }

                if (_groundHit.transform.TryGetComponent(out currentPlatform))
                {
                    _activatedMovers.Add(currentPlatform);
                }
            }

            if (_lastPlatform != currentPlatform)
            {
                if (_lastPlatform is { UsesBounding: false })
                {
                    _activatedMovers.Remove(_lastPlatform);
                    ApplyMoverExitVelocity(_lastPlatform);
                }

                _lastPlatform = currentPlatform;
            }

            foreach (var platform in _activatedMovers)
            {
                if (_framePosition.y < platform.FramePosition.y - SKIN_WIDTH) continue;

                _frameTransientVelocity += platform.FramePositionDelta / _delta;
            }
        }

        private void ApplyMoverExitVelocity(IPhysicsMover mover)
        {
            var platformVel = mover.TakeOffVelocity;
            if (platformVel.y < 0) platformVel.y *= Stats.NegativeYVelocityNegation;
            _decayingTransientVelocity += platformVel;
        }

        private void Move()
        {
            if (_forceToApplyThisFrame != Vector2.zero)
            {
                _rb.velocity += AdditionalFrameVelocities();
                _rb.AddForce(_forceToApplyThisFrame * _rb.mass, ForceMode2D.Impulse);
                return;
            }

            var extraForce = new Vector2(0, _grounded ? 0 : -Stats.ExtraConstantGravity * (_endedJumpEarly && Velocity.y > 0 ? Stats.EndJumpEarlyExtraForceMultiplier : 1));
            _constantForce.force = extraForce * _rb.mass;

            var targetSpeed = _hasInputThisFrame ? Stats.BaseSpeed : 0;

            if (Growing)
            {
                var growPoint = Mathf.InverseLerp(0, Stats.GrowSlowDownTime, _time - _timeStartedGrowing);
                targetSpeed *= Mathf.Lerp(1, Stats.GrowSpeedModifier, growPoint);
            }

            var step = _hasInputThisFrame ? Stats.Acceleration : Stats.Friction;

            var xDir = (_hasInputThisFrame ? _frameDirection : Velocity.normalized);

            if (Vector3.Dot(_trimmedFrameVelocity, _frameDirection) < 0) step *= Stats.DirectionCorrectionMultiplier;

            Vector2 newVelocity;
            step *= _delta;
            if (_grounded)
            {
                var speed = Mathf.MoveTowards(Velocity.magnitude, targetSpeed, step);

                var targetVelocity = xDir * speed;

                var newSpeed = Mathf.MoveTowards(Velocity.magnitude, targetVelocity.magnitude, step);

                var smoothed = Vector2.MoveTowards(Velocity, targetVelocity, step);
                var direct = targetVelocity.normalized * newSpeed;
                var slopePoint = Mathf.InverseLerp(0, SLOPE_ANGLE_FOR_EXACT_MOVEMENT, Mathf.Abs(_frameDirection.y));

                newVelocity = Vector2.Lerp(smoothed, direct, slopePoint);
            }
            else
            {
                step *= Stats.AirFrictionMultiplier;

                var targetX = Mathf.MoveTowards(_trimmedFrameVelocity.x, xDir.x * targetSpeed, step);
                newVelocity = new Vector2(targetX, _rb.velocity.y);
            }

            SetVelocity((newVelocity + AdditionalFrameVelocities()) * _currentFrameSpeedModifier);

            Vector2 AdditionalFrameVelocities()
            {
                if (_immediateMove.sqrMagnitude > SKIN_WIDTH)
                {
                    _rb.MovePosition(_framePosition + _immediateMove);
                }

                _totalTransientVelocityAppliedLastFrame = _frameTransientVelocity + _decayingTransientVelocity;
                return _totalTransientVelocityAppliedLastFrame;
            }
        }

        private void SetVelocity(Vector2 newVel)
        {
            _rb.velocity = newVel;
            Velocity = newVel;
        }

        #endregion

        private void SaveCharacterState()
        {
            State = new ControllerState
            {
                Position = _framePosition,
                Rotation = _rb.rotation,
                Velocity = Velocity,
                Grounded = _grounded
            };
        }

        #region External Triggers

        private const int MAX_ACTIVE_MOVERS = 5;
        private readonly HashSet<IPhysicsMover> _activatedMovers = new(MAX_ACTIVE_MOVERS);
        private readonly HashSet<ISpeedModifier> _modifiers = new();
        private Vector2 _frameSpeedModifierVelocity;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out ISpeedModifier modifier)) _modifiers.Add(modifier);
            else if (other.TryGetComponent(out IPhysicsMover mover) && !mover.RequireGrounding) _activatedMovers.Add(mover);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out ISpeedModifier modifier)) _modifiers.Remove(modifier);
            else if (other.TryGetComponent(out IPhysicsMover mover)) _activatedMovers.Remove(mover);
        }

        private void CalculateExternalModifiers()
        {
            _frameSpeedModifier = Vector2.one;
            foreach (var modifier in _modifiers)
            {
                if ((modifier.OnGround && _grounded) || (modifier.InAir && !_grounded))
                    _frameSpeedModifier += modifier.Modifier;
            }

            _currentFrameSpeedModifier = Vector2.SmoothDamp(_currentFrameSpeedModifier, _frameSpeedModifier, ref _frameSpeedModifierVelocity, 0.1f);
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;

            var pos = (Vector2)transform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(pos + Vector2.up * _character.Height / 2, new Vector3(_character.Width, _character.Height));
            Gizmos.color = Color.magenta;

            var rayStart = pos + Vector2.up * _character.StepHeight;
            var rayDir = Vector3.down * _character.StepHeight;
            Gizmos.DrawRay(rayStart, rayDir);
            foreach (var offset in GenerateRayOffsets())
            {
                Gizmos.DrawRay(rayStart + Vector2.right * offset, rayDir);
                Gizmos.DrawRay(rayStart + Vector2.left * offset, rayDir);
            }
        }

        #endregion
    }

    public enum JumpType
    {
        Jump,
        Coyote,
        AirJump
    }

    public interface IPlayerController
    {
        public PlayerStats Stats { get; }
        public ControllerState State { get; }
        public event Action<JumpType> Jumped;
        public event Action<bool, float> GroundedChanged;
        public event Action<Vector2> Repositioned;
        public event Action<bool> ToggledPlayer;

        public bool Active { get; }
        public Vector2 Up { get; }
        public bool Growing { get; }
        public Vector2 Input { get; }
        public Vector2 GroundNormal { get; }
        public Vector2 Velocity { get; }
        object transform { get; set; }

        // External force
        public void AddFrameForce(Vector2 force, bool resetVelocity = false);

        // Utility
        public void LoadState(ControllerState state);
        public void RepositionImmediately(Vector2 position, bool resetVelocity = false);
        public void TogglePlayer(bool on);
    }

    public interface ISpeedModifier
    {
        public bool InAir { get; }
        public bool OnGround { get; }
        public Vector2 Modifier { get; }
    }

    // Used to save and load character state
    public struct ControllerState
    {
        public Vector2 Position;
        public float Rotation;
        public Vector2 Velocity;
        public bool Grounded;
    }
}
