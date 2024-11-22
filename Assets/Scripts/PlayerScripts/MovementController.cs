using GoodbyeBuddy;
using System;
using System.Collections.Generic;
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
    public Transform transform;
    public Rigidbody2D rb;
    public CapsuleCollider2D collider;
    public float counterShrink = 1;

    //Movement Variables
    protected bool isGrounded;


    public virtual bool IsFinished() { return false; }
    public virtual IState NextState() { return null; }
    public virtual void Start() { }
    public virtual void Finish() { }
    public virtual void Update(float dt) { }
}

public class GrowState : BaseState
{

    public override void Start()
    {
        Debug.LogError("Growing");
        transform.localScale = new Vector3(2, 2, 2);
    }



    public GrowState(Transform transform, PlayerStats stats, Rigidbody2D rb, CapsuleCollider2D collider) :
        base(transform, stats, rb, collider)
    {
        this.transform = transform;
        this.Stats = stats;
        this.rb = rb;
        this.collider = collider;
    }

    public override bool IsFinished()
    {
        if (!growinput)
        {

            return true;
        }
        return false;
    }



    public override IState NextState()
    {
        if (!growinput)
        {
            return new BaseState(transform, Stats, rb, collider);

        }

        return base.NextState();
    }
    public override void Finish()
    {
        base.Finish();
    }
}

public class ShrinkState : BaseState
{

    public override void Start()
    {
        counterShrink++;
        Debug.LogError("Shrink" + counterShrink);

        counterShrink = Math.Clamp(counterShrink, 0, 4);
        transform.localScale = new Vector3(1 / counterShrink, 1 / counterShrink, 1 / counterShrink);
    }



    public ShrinkState(Transform transform, PlayerStats stats, Rigidbody2D rb, CapsuleCollider2D collider, float counterShrink) :
        base(transform, stats, rb, collider)
    {
        this.transform = transform;
        this.Stats = stats;
        this.rb = rb;
        this.collider = collider;
        this.counterShrink = counterShrink;
    }

    public override bool IsFinished()
    {
        if (shrinkinput)
        {

            return true;
        }
        return false;
    }



    public override IState NextState()
    {
        if (shrinkinput)
        {
            return new ShrinkState(transform, Stats, rb, collider, counterShrink);

        }

        return base.NextState();
    }
    public override void Finish()
    {
        base.Finish();
    }
}

public class BaseState : State
{
    public bool growinput;
    public bool shrinkinput;

    Vector2 targetMovementVector;
    protected float jumpForce;
    protected bool jumped;
    private RaycastHit2D _groundHit;

    float _acceleration = 0;

    public BaseState(Transform transform, PlayerStats stats, Rigidbody2D rb, CapsuleCollider2D collider)
    {
        this.transform = transform;
        this.Stats = stats;
        this.rb = rb;
        this.collider = collider;
    }

    public override void Start()
    {
        Debug.LogError("Normal");
        transform.localScale = new Vector3(1, 1, 1);


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
        growinput = PlayerInputs._instance.myInputs.grow;
        shrinkinput = PlayerInputs._instance.myInputs.shrink;

        targetMovementVector = Vector2.zero;

        //Movement
        float movementValue = PlayerInputs._instance.myInputs.move.x;

        targetMovementVector = Vector2.right * movementValue * Stats.BaseSpeed * 3;

        //Grounded
        isGrounded = PerformRay(rb.position);

        if (!jumped && PlayerInputs._instance.myInputs.jump)
        {
            //rb.velocity = targetMovementVector += Vector2.up * Stats.JumpPower;
            rb.AddForce(Vector2.up * Stats.JumpPower * transform.localScale.x, ForceMode2D.Impulse);
            jumped = true;
        }
        jumped = !isGrounded;

        if (Math.Abs(rb.velocity.x) <= Stats.BaseSpeed)
        {
            rb.AddForce(targetMovementVector * transform.localScale.x);

        }
        if (movementValue != 0)
        {
            PhysicsMaterial2D frictionLow = new PhysicsMaterial2D(); frictionLow.friction = 0.1f; frictionLow.bounciness = 0f;

            rb.sharedMaterial = frictionLow;


        }
        else
        {
            PhysicsMaterial2D frictionHigh = new PhysicsMaterial2D(); frictionHigh.friction = 0.4f; frictionHigh.bounciness = 0f;

            rb.sharedMaterial = frictionHigh;

        }
    }


    public override bool IsFinished()
    {
        if (growinput)
        {

            return true;
        }
        if (shrinkinput)
        {
            return true;

        }
        return false;
    }


    public override IState NextState()
    {
        if (growinput)
        {
            return new GrowState(transform, Stats, rb, collider);

        }
        if (shrinkinput)
        {
            return new ShrinkState(transform, Stats, rb, collider, counterShrink);


        }

        return base.NextState();
    }
    public override void Finish()
    {
        base.Finish();
    }


    public bool PerformRay(Vector2 point)
    {

        _groundHit = Physics2D.Raycast(point, Vector2.down, (transform.localScale.x * collider.size.y) / 2 + 0.1f, Stats.CollisionLayers);



        if (!_groundHit) return false;
        if (_groundHit.collider.isTrigger) return false;

        if (Vector2.Angle(_groundHit.normal, Vector2.up) > Stats.MaxWalkableSlope)
        {
            return false;
        }

        return true;
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

        currentState = new BaseState(
            GameObject.FindAnyObjectByType<MovementController>().transform,
            Stats,
            GameObject.FindAnyObjectByType<MovementController>().gameObject.GetComponent<Rigidbody2D>(),
            GameObject.FindAnyObjectByType<MovementController>().gameObject.GetComponent<CapsuleCollider2D>());
        currentState.Start();

    }
    private void FixedUpdate()
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

    void Update()
    {


    }



}
