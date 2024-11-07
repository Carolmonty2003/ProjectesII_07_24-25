using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    public float timer;

    public LayerMask playerLayer;

    [SerializeField] public MovementState movementState;

    public Walking walking;
    public Jump jump;
    public FreeFall freeFall;

    public int jumpsLeft = 2;
    //public int LittleJumps = 3; //implementar----------------------------------------
    public int NormalJumps = 2;
    public int BigJumps = 1;

    private float horizontal;
    [SerializeField] private float speed = 6f;
    public float Speed { get => speed; }
    [SerializeField] private float jumpingPower = 28f;
    public float JumpingPower { get => jumpingPower; }

    //public LittleMovement littleMovement; //implementar----------------------------------------
    public NormalMovement normalMovement;
    public BigMovement bigMovement;
    //Static instances of the state scripts
    //Get rid of the public access

    public MovementScript state_; //Contains the current state script

    public Rigidbody2D rb2D; //Temporarily public for the grip script
    public SpriteRenderer sr; //Temporarily public


    [Header("Touching Values")]
    [SerializeField] private bool _isFacingRight = true;
    public bool IsFacingRight { get => _isFacingRight; set => _isFacingRight = value; }
    private bool grounded;
    public bool Grounded { get => bottomLeftCheck || bottomCenterCheck || bottomRightCheck; }

    public GameObject _leftCheck;
    public GameObject _rightCheck;
    public GameObject _bottomLeftCheck;
    public GameObject _bottomCenterCheck;
    public GameObject _bottomRightCheck;

    private bool leftCheck;
    private bool rightCheck;
    private bool bottomLeftCheck;
    private bool bottomCenterCheck;
    private bool bottomRightCheck;

    public bool LeftCheck { get => leftCheck; set => leftCheck = value; }
    public bool RightCheck { get => rightCheck; set => rightCheck = value; }
    public bool BottomLeftCheck { set => bottomLeftCheck = value; }
    public bool BottomCenterCheck { set => bottomCenterCheck = value; }
    public bool BottomRightCheck { set => bottomRightCheck = value; }

    public GameObject _groundCheck; //Temporarily public for the grip script
    private LayerMask _groundLayer;

    private bool _element; //Variable that saves the current element of the player
    public bool Element { get => _element; set => _element = value; }

    // Start is called before the first frame update
    void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        //Initialize all relevant components
        rb2D = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        _element = true;

        _groundLayer = 6; //cambiar la layer de ground-----------------------------------------------------------------------------------------------

        //littleMovement = new LittleMovement(this); //implementar----------------------------------------
        normalMovement = new NormalMovement(this);
        bigMovement = new BigMovement(this);
        state_ = normalMovement;

        walking = new Walking(this);
        jump = new Jump(this);
        freeFall = new FreeFall(this);

        movementState = walking;
        movementState.enterState();

    }

    void Update()
    {
        IsGrounded();
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0) movementState.timerDone();
        }
    }

    private void FixedUpdate()
    {

    }

    public bool IsGrounded() //Temporarily public for the grip script
    {
        if (bottomLeftCheck || bottomCenterCheck || bottomRightCheck)
        {
            movementState.ground();
            return true;
        }
        return false;
    }
    public void ChangeElement()
    {
        _element = !_element;
        sr.color = _element ? Color.green : Color.red;
    }
    public void HorizontalMovement(float horizontal) => movementState.horizontal(horizontal);
    public void Jump() => movementState.jump();

}

public class MovementState
{
    public PlayerMovement pMovement;

    public virtual void enterState() { }
    public virtual void exitState() { }
    public virtual void horizontal(float horizontal) { }
    public virtual void ground() { }
    public virtual void jump() { }
    public virtual void timerDone() { }
}

public class Walking : MovementState
{
    public Walking(PlayerMovement pMovement)
    {
        this.pMovement = pMovement;
    }
    public override void horizontal(float horizontal)
    {
        pMovement.rb2D.position += horizontal * pMovement.Speed * Time.deltaTime * Vector2.right;
        if (horizontal != 0) pMovement.sr.flipX = (horizontal < 0);
    }
    public override void enterState()
    {
        base.enterState();
        pMovement.jumpsLeft = pMovement.Element ? pMovement.NormalJumps : pMovement.BigJumps; //falta little state---------------------------------------------------
    }
    public override void jump()
    {
        if (pMovement.jumpsLeft > 0)
        {
            pMovement.movementState = pMovement.jump;
            pMovement.movementState.enterState();
        }
    }
}

public class FreeFall : MovementState
{
    public FreeFall(PlayerMovement pMovement)
    {
        this.pMovement = pMovement;
    }
    public override void ground()
    {
        pMovement.movementState = pMovement.walking;
        pMovement.movementState.enterState();
    }
    public override void horizontal(float horizontal)
    {
        pMovement.rb2D.position += horizontal * pMovement.Speed * Time.deltaTime * Vector2.right;
        if (horizontal != 0) pMovement.sr.flipX = (horizontal < 0);
    }
    public override void jump()
    {
        if (pMovement.jumpsLeft > 0)
        {
            pMovement.movementState = pMovement.jump;
            pMovement.movementState.enterState();
        }
    }
}

public class Jump : MovementState
{
    public Jump(PlayerMovement pMovement)
    {
        this.pMovement = pMovement;
    }
    override public void enterState()
    {
        base.enterState();
        pMovement.jumpsLeft--;
        pMovement.rb2D.AddForce(Vector2.up * pMovement.JumpingPower);
        pMovement.movementState = pMovement.freeFall;
        pMovement.movementState.enterState();
    }
}

public class MovementScript
{
    //State class for the movement with all its virtual functions

    public virtual void enterState() { }
    public virtual void exitState() { }
    public virtual void jump() { }
    public virtual void dash() { }
    public virtual void flip() { }
    public virtual void horizontalMovement(float horizontal) { }
}

//public class LittleMovement : MovementScript //implementar----------------------------------------
//{
//    private PlayerMovement player;
//    private float horizontal;

//    public LittleMovement(PlayerMovement player)
//    {
//        this.player = player;
//    }

//    public override void enterState()
//    {
//        player.sr.color = Color.blue;
//        //Temporary to see the state change
//    }

//    public override void jump()
//    {
//        if (!player.IsGrounded()) return;
//        player.rb2D.AddForce(new Vector2(0, player.JumpingPower));
//    }

//    public override void flip()
//    {
//        player.sr.flipX = horizontal <= 0;
//    }

//    public override void exitState()
//    {
//        player.Element = !player.Element;
//        player.state_ = player.normalMovement;
//        player.state_.enterState();
//    }

//    public override void horizontalMovement(float horizontal)
//    {
//        player.rb2D.position += new Vector2(horizontal * player.Speed * Time.deltaTime, 0);
//        if (horizontal != 0) flip();
//    }

//}

public class NormalMovement : MovementScript
{
    private PlayerMovement player;
    private float horizontal;

    public NormalMovement(PlayerMovement player)
    {
        this.player = player;
    }

    public override void enterState()
    {
        player.sr.color = Color.green;
        //Temporary to see the state change
    }

    public override void jump()
    {
        if (!player.IsGrounded()) return;
        player.rb2D.AddForce(new Vector2(0, player.JumpingPower));
    }

    public override void flip()
    {
        player.sr.flipX = horizontal <= 0;
    }

    public override void exitState()
    {
        player.Element = !player.Element;
        player.state_ = player.normalMovement;
        player.state_.enterState();
    }

    public override void horizontalMovement(float horizontal)
    {
        player.rb2D.position += new Vector2(horizontal * player.Speed * Time.deltaTime, 0);
        if (horizontal != 0) flip();
    }

}

public class BigMovement : MovementScript
{
    private PlayerMovement player;
    private float horizontal;

    public BigMovement(PlayerMovement player)
    {
        this.player = player;
    }

    public override void enterState()
    {
        player.sr.color = Color.red;
        //Temporary to see the state change
    }

    public override void jump()
    {
        if (!player.IsGrounded()) return;
        player.rb2D.AddForce(new Vector2(0, player.JumpingPower));
    }

    public override void flip()
    {
        player.sr.flipX = horizontal <= 0;
    }

    public override void exitState()
    {
        player.Element = !player.Element;
        player.state_ = player.normalMovement;
        player.state_.enterState();
    }

    public override void horizontalMovement(float horizontal)
    {
        player.rb2D.velocity += new Vector2(horizontal * player.Speed * Time.deltaTime, 0);
        //Different movement control to compare with the original
        if (horizontal != 0) flip();
    }

}
