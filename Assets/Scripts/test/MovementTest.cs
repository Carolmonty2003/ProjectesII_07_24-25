using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using Vector2 = UnityEngine.Vector2;

public class MovementTest : MonoBehaviour
{
    public Rigidbody2D RB { get; private set; }
    private PlayerMovementDataTest _data;
    public PlayerMovementDataTest NormalProfile;
    private Vector2 _inputDirection;


    //jump
    private bool _isJumpCut;
    private bool _isJumpFalling;

    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize;

    public bool IsJumping { get; private set; }

    //timers
    public float LastOnGroundTime { get; private set; }
    public float LastPressedJumpTime { get; private set; }

    //layers
    [SerializeField] private LayerMask _groundLayer;


    //movimiento---------------------------------------
    public float direHorizontal;

    //checks-------------------------------
    public GameObject _bottomCenterCheck;
    private bool bottomCenterCheck;
    public bool BottomCenterCheck { set => bottomCenterCheck = value; }

    

    [Header("Gravity Values")]
    public float gravityStrength;

    [Header("Jump Values")]
    public float jumpingPower;

    [Header("Movement Values")]
    public float speed;


    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        _data = NormalProfile;
    }
    
    private void Start()
    {
        SetGravityScale(_data.gravityScale);
    }
    private void OnEnable()
    {
        InputManagerTest.jumped += OnJumpPerform;
        InputManagerTest.jumping += OnJumpingPerform;
    }

    private void OnDisable()
    {
        InputManagerTest.jumped -= OnJumpPerform;
        InputManagerTest.jumping -= OnJumpingPerform;

    }
    void Update()
    {
        //Timers
        LastPressedJumpTime -= Time.deltaTime;

        if (!IsJumping)
        {
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping)
            {
                LastOnGroundTime = _data.coyoteTime;
            }
        }

        //Checks
        if (IsJumping && RB.velocity.y < 0)
        {
            IsJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping)
        {
            _isJumpCut = false;

            if (!IsJumping)
                _isJumpFalling = false;
        }

        //Jumps
        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            _isJumpCut = false;
            _isJumpFalling = false;
            Jump();
        }

        //Gravity
        if (RB.velocity.y < 0 && _inputDirection.y < 0)
        {
            SetGravityScale(_data.gravityScale * _data.fastFallGravityMult);

            RB.velocity = new Vector2(RB.velocity.x,
                Mathf.Max(RB.velocity.y, -_data.maxFastFallSpeed));
        }

        else if (_isJumpCut)
        {
            SetGravityScale(_data.gravityScale * _data.jumpCutGravityMult);

            RB.velocity = new Vector2(RB.velocity.x,
                Mathf.Max(RB.velocity.y, -_data.maxFallSpeed));
        }

        else if ((IsJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) <
                 _data.jumpHangTimeThreshold)
        {
            SetGravityScale(_data.gravityScale * _data.jumpHangGravityMult);
        }

        else if (RB.velocity.y < 0)
        {
            SetGravityScale(RB.gravityScale * _data.fallGravityMult);
        }

        else
        {
            SetGravityScale(_data.gravityScale);
        }

        //_--------------------------------------------------------------
        //IsGrounded();
        //if ( Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        //    Jump();

        _inputDirection.x = Input.GetAxis("Horizontal");
        _inputDirection.y = Input.GetAxisRaw("Vertical");
    }
    void FixedUpdate()
    {
        Movimiento();
    }
    //public bool IsGrounded() //Temporarily public for the grip script
    //{
    //    bottomCenterCheck = Physics2D.OverlapCircle(_bottomCenterCheck.transform.position, 0.2f, _groundLayer);
    //    if (bottomCenterCheck)
    //    {
    //        Debug.Log("UWU CHeck?");
    //        return true;
    //    }
    //    return false;
    //}

    private void Jump()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        // Jumping Actions
        float force = _data.jumpForce;
        if (RB.velocity.y < 0)
            force -= RB.velocity.y;

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        //RB.AddForce(Vector2.up * jumpingPower, ForceMode2D.Impulse);
    }

    public void Movimiento()
    {
        RB.position += _inputDirection.x * speed * Time.deltaTime * Vector2.right;

        //rb.velocity = new Vector2(direHorizontal * speed, rb.velocity.y);

        //rb.position += new Vector2(direHorizontal * speed * Time.deltaTime, 0);

        //rb.velocity = new Vector2(direHorizontal*speed, rb.velocity.y);

    }

    #region INPUT
    public void OnJumpPerform()
    {
        LastPressedJumpTime = _data.jumpInputBufferTime;       
    }

    public void OnJumpingPerform()
    {
        if (CanJumpCut()) _isJumpCut = true;
        //Debug.Log(_isJumpCut);
    }
    #endregion

    //Checks
    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanJumpCut()
    {
        IsJumping = true;
        Debug.Log(IsJumping);
        //Debug.Log(RB.velocity);
        return (IsJumping) && RB.velocity.y > 0;
    }

    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }
    public bool IsOnAir()
    {
        return IsJumping || _isJumpFalling;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.color = Color.blue;
    }
}
