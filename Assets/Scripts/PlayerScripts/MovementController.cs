using GoodbyeBuddy;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;


public interface IState
{
    public void Start();
    public void Finish();
    public void Update(float dt);
    public bool IsFinished();
    public IState NextState();
}

public class State : IState
{
    public PlayerStats Stats;
    protected Transform transform;
    protected Rigidbody2D rb;
    protected CapsuleCollider2D collider;

    //Movement Variables
    protected bool isGrounded;


    public virtual bool IsFinished() { return false; }
    public virtual IState NextState() { return null; }
    public virtual void Start() { }
    public virtual void Finish() { }
    public virtual void Update(float dt) { }
}


public class IdleWalkingState : State
{

    Vector2 targetMovementVector;
    float _acceleration = 0;

    public IdleWalkingState(Transform transform, PlayerStats stats, Rigidbody2D rb, CapsuleCollider2D collider)
    {
        this.transform = transform;
        this.Stats = stats;
        this.rb = rb;
        this.collider = collider;
    }

    public override void Start()
    {
        if (rb == null)
        {
            //esto no se como cogerlo
            rb = GameObject.FindAnyObjectByType<MovementController>().gameObject.GetComponent<Rigidbody2D>();
            transform = GameObject.FindAnyObjectByType<MovementController>().transform;
            collider = GameObject.FindAnyObjectByType<MovementController>().gameObject.GetComponent<CapsuleCollider2D>();

        }
    }
    public override void Update(float dt)
    {

        targetMovementVector = Vector2.zero;

        //Movement
        float movementValue = PlayerInputs._instance.myInputs.move.x;
        if (PlayerInputs._instance.myInputs.move.x != 0)
        {
            if (_acceleration <= Stats.BaseSpeed)
            {
                _acceleration += Stats.Acceleration;
            }
        }
        else { _acceleration = Stats.BaseSpeed / 4; }
        targetMovementVector = Vector2.right * movementValue * Stats.Acceleration;

        Debug.LogError(movementValue);
        //Grounded
        Vector2 checkPoint = rb.position + collider.offset + collider.size.y * Vector2.down * transform.localScale.y;
        isGrounded = Physics2D.OverlapPoint(checkPoint - 0.05f * Vector2.down, 6);

        //Gravity
        if (!isGrounded)
            targetMovementVector += Vector2.down * Stats.Gravity;
        else
        {
            //Jump code
        }
        //rb.AddForce(targetMovementVector);
        rb.velocity = targetMovementVector;
        //isGrounded = true;
    }

    public override bool IsFinished()
    {
        if (!isGrounded)
        {
            //nextState = JumpFallState;
            return false;
        }
        return false;
    }
}

public class JumpFallState : State
{
    private float coyoteTime;
    private float coyoteTimeCounter;

    public override void Start()
    {
        if (rb == null)
        {
            rb = GameObject.FindAnyObjectByType<MovementController>().gameObject.GetComponent<Rigidbody2D>();
            transform = GameObject.FindAnyObjectByType<MovementController>().transform;
            collider = GameObject.FindAnyObjectByType<MovementController>().gameObject.GetComponent<CapsuleCollider2D>();
        }
        coyoteTimeCounter = coyoteTime;
    }
    public override void Update(float dt)
    {
        //coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTime -= dt;
        }

        //salto
        if (PlayerInputs._instance.myInputs.jump && coyoteTimeCounter > 0)
        {
            rb.AddForce(Vector2.up * Stats.JumpPower);
        }

    }

    public override bool IsFinished()
    {
        if (isGrounded)
        {
            //nextState = JumpFallState
            return false;
        }
        return false;
    }
}

public class MovementController : MonoBehaviour
{
    [field: SerializeField] public PlayerStats Stats { get; private set; }

    HashSet<IState> states = new HashSet<IState>();
    IState currentState;

    void Start()
    {
        // Inicializa el estado inicial= 

        currentState = new IdleWalkingState( //pasar a forma optima
            GameObject.FindAnyObjectByType<MovementController>().transform,
            Stats,
            GameObject.FindAnyObjectByType<MovementController>().gameObject.GetComponent<Rigidbody2D>(),
            GameObject.FindAnyObjectByType<MovementController>().gameObject.GetComponent<CapsuleCollider2D>());
        currentState.Start();

    }


    void Update()
    {

        if (currentState != null)
        {
            currentState.Update(Time.deltaTime);
            if (currentState.IsFinished())
            {
                currentState.Finish();
                currentState = currentState.NextState();
                currentState.Start();
            }
        }
        else
        {
            Debug.LogError("CurrentState = null");
        }
    }



}
