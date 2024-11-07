using System;
using System.Collections;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private PlayerMovementData _data;

    //animacion

    public PlayerMovementData NormalProfile;
    public PlayerMovementData BigProfile;
    public PlayerMovementData LittleProfile;

    #region Variables
    public Rigidbody2D RB { get; private set; }
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public float LastOnGroundTime { get; private set; }

    private bool _isJumpCut;
    private bool _isJumpFalling;

    private Vector2 _moveInput;

    public float LastPressedJumpTime { get; private set; }

    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;


    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;

    //standing plataform 
    [HideInInspector] public Rigidbody2D standingPlatform;
    #endregion

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        //animaciones
        _data = NormalProfile;
    }
    void Start()
    {
        SetGravityScale(_data.gravityScale);
        IsFacingRight = true;
    }

    private void OnEnable()
    {
        InputManager.jumped += OnJumpInput;
        InputManager.jumping += OnJumpUpInput;
    }
    private void OnDisable()
    {
        InputManager.jumped -= OnJumpInput;
        InputManager.jumping -= OnJumpUpInput;
    }
    void Update()
    {
        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
        #endregion

        //falta
        #region INPUT HANDLER
        //pause menu state
        #endregion

        #region COLLISION CHECKS
        if (Physics2D.OverlapBox(_groundCheckPoint.position,
                _groundCheckSize, 0, _groundLayer) && !IsJumping)
        {
            LastOnGroundTime = _data.coyoteTime;
        }

        //otros objetos de collision?
        #endregion

        #region JUMP CHECKS
        if (IsJumping && RB.velocity.y <= 0)
        {
            IsJumping = false;
            _isJumpFalling = true;
        }

        // otros checks 

        if (LastOnGroundTime > 0 && !IsJumping) //añadir aqui en caso de mas checks
        {
            _isJumpFalling = false;
            _isJumpCut = false;
        }
        #endregion

        #region GRAVITY
        SetGravityScale(0);
        //variaciones gravedad
        #endregion


    }

    //fixed Update? Run(1);

    #region INPUT
    public void OnJumpInput()
    {
        LastPressedJumpTime = _data.jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut())
            _isJumpCut = true;
    }

    #endregion

    #region GRAVITY
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }

    private void Sleep(float duration)
    {
        StartCoroutine(nameof(PerformSleep), duration);
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
    #endregion

    #region RUN
    private void Run(float lerpAmount)
    {
        float targetSpeed = _moveInput.x * _data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

        #region Calculate AccelRate
        float accelRate;

        if (LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _data.runAccelAmount : _data.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _data.runAccelAmount * _data.accelInAir : _data.runDeccelAmount * _data.deccelInAir;
        #endregion

        #region Bonus Jump Apex Accel
        if ((IsJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < _data.jumpHangTimeThreshold)
        {
            accelRate *= _data.jumpHangAccelerationMult;
            targetSpeed *= _data.jumpHangMaxSpeedMult;
        }
        #endregion

        #region Momentum
        if (_data.doConserveMomentum &&
            Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) &&
            Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) &&
            Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            accelRate = 0;
        }
        #endregion

        float speedDif = targetSpeed - RB.velocity.x + (standingPlatform ? standingPlatform.velocity.x : 0.0f);

        float movement = speedDif * accelRate;

        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }
    private void Turn()
    {
        //stores scale and flips the player along the x axis, 
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region JUMP
    private void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        #region Perform Jump
        float force = _data.jumpForce;
        if (RB.velocity.y < 0)
            force -= RB.velocity.y;

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion
    }
    #endregion

    #region CHECKS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }
    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.velocity.y > 0;
    }
    #endregion

    /*****************************************************/
    #region PROFILE METHODS 
    //NIVEL DE IRA
    private void ChangeCurrentProfile()
    {
        if (_data == NormalProfile)
            _data = BigProfile;
        else
            _data = NormalProfile;
    }

    public bool IsOnBigElement()
    {
        return _data.dataType == DataMovementType.BIG;
    }

    //NIVEL DE TRISTEZA
    //private void ChangeCurrentProfile()
    //{
    //    if (_data == NormalProfile)
    //        _data = LittleProfile;
    //    else
    //        _data = NormalProfile;
    //}

    //public bool IsOnLittleElement()
    //{
    //    return _data.dataType == DataMovementType.LITTLE;
    //}

    /*****************************************************/
    #endregion

}
