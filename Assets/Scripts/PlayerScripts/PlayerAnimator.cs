using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GoodbyeBuddy
{
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Animator _anim;

        [SerializeField] private GameObject _effectsParent;
        [SerializeField] private Transform _trailRenderer;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private TrailRenderer _trail;


        [Header("Particles")][SerializeField] private ParticleSystem _jumpParticles;
        [SerializeField] private ParticleSystem _launchParticles;
        [SerializeField] private ParticleSystem _moveParticles;
        [SerializeField] private ParticleSystem _landParticles;
        [SerializeField] private ParticleSystem _doubleJumpParticles;

        [Header("Audio Clips")]
        [SerializeField]
        private AudioClip _doubleJumpClip;

        [SerializeField] private AudioClip[] _jumpClips;
        [SerializeField] private AudioClip[] _splats;
        [SerializeField] private AudioClip[] _slideClips;


        private AudioSource _source;
        private IPlayerController _player;
        private Vector2 _defaultSpriteSize;
        private GeneratedCharacterSize _character;
        private Vector3 _trailOffset;
        private Vector2 _trailVel;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _player = GetComponentInParent<IPlayerController>();
            _character = _player.Stats.CharacterSize.GenerateCharacterSize();
            _defaultSpriteSize = new Vector2(1, _character.Height);

            _trailOffset = _trailRenderer.localPosition;
            _trailRenderer.SetParent(null);
            _originalTrailTime = _trail.time;
        }

        private void OnEnable()
        {
            _player.Jumped += OnJumped;
            _player.GroundedChanged += OnGroundedChanged;
            _player.Repositioned += PlayerOnRepositioned;
            _player.ToggledPlayer += PlayerOnToggledPlayer;

            _moveParticles.Play();
        }

        private void OnDisable()
        {
            _player.Jumped -= OnJumped;
            _player.GroundedChanged -= OnGroundedChanged;
            _player.Repositioned -= PlayerOnRepositioned;
            _player.ToggledPlayer -= PlayerOnToggledPlayer;

            _moveParticles.Stop();
        }

        private void Update()
        {
            if (_player == null) return;

            var xInput = _player.Input.x;

            SetParticleColor(-_player.Up, _moveParticles);

            HandleSpriteFlip(xInput);

            HandleIdleSpeed(xInput);

            HandleCharacterTilt(xInput);

            HandleGrowing();
        }

        private void LateUpdate()
        {
            _trailRenderer.position = Vector2.SmoothDamp(_trailRenderer.position, transform.position + _trailOffset, ref _trailVel, 0.02f);
        }

        #region Squish

        [Header("Squish")][SerializeField] private ParticleSystem.MinMaxCurve _squishMinMaxX;
        [SerializeField] private ParticleSystem.MinMaxCurve _squishMinMaxY;
        [SerializeField] private float _minSquishForce = 6f;
        [SerializeField] private float _maxSquishForce = 30f;
        [SerializeField] private float _minSquishDuration = 0.1f;
        [SerializeField] private float _maxSquishDuration = 0.4f;
        private bool _isSquishing;

        private IEnumerator SquishPlayer(float force)
        {
            force = Mathf.Abs(force);
            if (force < _minSquishForce) yield break;
            _isSquishing = true;

            var elapsedTime = 0f;

            var point = Mathf.InverseLerp(_minSquishForce, _maxSquishForce, force);
            var duration = Mathf.Lerp(_minSquishDuration, _maxSquishDuration, point);

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / duration;

                var squishFactorY = Mathf.Lerp(_squishMinMaxY.curveMax.Evaluate(t), _squishMinMaxY.curveMin.Evaluate(t), point);
                var squishFactorX = Mathf.Lerp(_squishMinMaxX.curveMax.Evaluate(t), _squishMinMaxX.curveMin.Evaluate(t), point);
                _sprite.size = new Vector3(_defaultSpriteSize.x * squishFactorX, _defaultSpriteSize.y * squishFactorY);

                yield return null;
            }

            _sprite.size = _defaultSpriteSize;
            _isSquishing = false;
        }

        private void CancelSquish()
        {
            _isSquishing = false;
            if (_squishRoutine != null) StopCoroutine(_squishRoutine);
        }

        #endregion

        #region Animation

        [Header("Idle")]
        [SerializeField, Range(1f, 3f)]
        private float _maxIdleSpeed = 2;

        // Speed up idle while running
        private void HandleIdleSpeed(float xInput)
        {
            var inputStrength = Mathf.Abs(xInput);
            _anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));
            _moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale,
                Vector3.one * inputStrength, 2 * Time.deltaTime);
        }

        private void HandleSpriteFlip(float xInput)
        {
            if (_player.Input.x != 0) _sprite.flipX = xInput < 0;
        }

        #endregion

        #region Tilt

        [Header("Tilt")][SerializeField] private float _runningTilt = 7; // In degrees around the Z axis
        [SerializeField] private float _maxTilt = 12; // In degrees around the Z axis
        [SerializeField] private float _tiltSmoothTime = 0.1f;

        private Vector3 _currentTiltVelocity;

        private void HandleCharacterTilt(float xInput)
        {

            var runningTilt = _grounded ? Quaternion.Euler(0, 0, -_runningTilt * xInput) : Quaternion.identity;
            var targetRot = _grounded && _player.GroundNormal != _player.Up ? runningTilt * _player.GroundNormal : runningTilt * _player.Up;

            // Calculate the smooth damp effect
            var smoothRot = Vector3.SmoothDamp(_anim.transform.up, targetRot, ref _currentTiltVelocity, _tiltSmoothTime);

            if (Vector3.Angle(_player.Up, smoothRot) > _maxTilt)
            {
                smoothRot = Vector3.RotateTowards(_player.Up, smoothRot, Mathf.Deg2Rad * _maxTilt, 0f);
            }

            // Rotate towards the smoothed target
            _anim.transform.up = smoothRot;
        }
        #endregion

        #region Grow & Slide

        private bool _growing;
        private Vector2 _currentGrowSizeVelocity;

        private void HandleGrowing()
        {
            if (!_growing && _player.Growing)
            {
                _source.PlayOneShot(_slideClips[Random.Range(0, _slideClips.Length)], Mathf.InverseLerp(0, 5, Mathf.Abs(_player.Velocity.x)));
                _growing = true;
                CancelSquish();
            }
            else if (_growing && !_player.Growing)
            {
                _growing = false;
            }
            if (!_isSquishing)
            {
                var heightPercentage = _character.GrowingHeight / _character.Height;
                var widthPercentage = _character.GrowingWidth / _character.Width;
                _sprite.size = Vector2.SmoothDamp(_sprite.size, new Vector2(_growing ? _character.Width * widthPercentage : _character.Width, _growing ? _character.Height * heightPercentage : _character.Height), ref _currentGrowSizeVelocity, 0.03f);
            }
        }

        #endregion

        #region Event Callbacks

        private void OnJumped(JumpType type)
        {
            if (type is JumpType.Jump or JumpType.Coyote)
            {
                _anim.SetTrigger(JumpKey);
                _anim.ResetTrigger(GroundedKey);
                PlayRandomSound(_jumpClips, 0.2f, Random.Range(0.98f, 1.02f));

                // Only play particles when grounded (avoid coyote)
                if (type is JumpType.Jump)
                {
                    SetColor(_jumpParticles);
                    SetColor(_launchParticles);
                    _jumpParticles.Play();
                }
            }
            else if (type is JumpType.AirJump)
            {
                _source.PlayOneShot(_doubleJumpClip);
                _doubleJumpParticles.Play();
            }
        }

        private bool _grounded;
        private Coroutine _squishRoutine;

        private void OnGroundedChanged(bool grounded, float impact)
        {
            _grounded = grounded;

            if (grounded)
            {
                _anim.SetBool(GroundedKey, true);
                CancelSquish();
                _squishRoutine = StartCoroutine(SquishPlayer(Mathf.Abs(impact)));
                _source.PlayOneShot(_splats[Random.Range(0, _splats.Length)], 0.5f);
                _moveParticles.Play();

                _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
                SetColor(_landParticles);


                _landParticles.Stop();
                _landParticles.Clear();
                _landParticles.Play();

                Debug.Log("Land particles played.");
            }
            else
            {
                _anim.SetBool(GroundedKey, false);
                _moveParticles.Stop();
            }
        }

        #endregion

        private float _originalTrailTime;
        private void PlayerOnRepositioned(Vector2 newPosition)
        {
            StartCoroutine(ResetTrail());

            IEnumerator ResetTrail()
            {
                _trail.time = 0;
                yield return new WaitForSeconds(0.1f);
                _trail.time = _originalTrailTime;
            }
        }

        private void PlayerOnToggledPlayer(bool on)
        {
            _effectsParent.SetActive(on);
        }

        #region Helpers

        private ParticleSystem.MinMaxGradient _currentGradient;

        private void SetParticleColor(Vector2 detectionDir, ParticleSystem system)
        {
            var ray = Physics2D.Raycast(transform.position, detectionDir, 2);
            if (!ray) return;

            _currentGradient = ray.transform.TryGetComponent(out SpriteRenderer r)
                ? new ParticleSystem.MinMaxGradient(r.color * 0.9f, r.color * 1.2f)
                : new ParticleSystem.MinMaxGradient(Color.white);

            SetColor(system);
        }

        private void SetColor(ParticleSystem ps)
        {
            var main = ps.main;
            main.startColor = _currentGradient;
        }

        private void PlayRandomSound(IReadOnlyList<AudioClip> clips, float volume = 1, float pitch = 1)
        {
            PlaySound(clips[Random.Range(0, clips.Count)], volume, pitch);
        }
        
        private void PlaySound(AudioClip clip, float volume = 1, float pitch = 1)
        {
            _source.pitch = pitch;
            _source.PlayOneShot(clip, volume);
        }

        #endregion

        #region Animation Keys

        private static readonly int GroundedKey = Animator.StringToHash("Grounded");
        private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
        private static readonly int JumpKey = Animator.StringToHash("Jump");

        #endregion
    }
}
