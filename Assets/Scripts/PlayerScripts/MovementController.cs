using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;


public interface IState
{
    public void Start();
    public void Finish();
    public void Update(float dt);
    public bool IsFinished();
    public IState NextState();
}

public class State: IState
{
    protected Transform transform;
    protected Rigidbody2D rb;
    protected CapsuleCollider2D collider;

    //Movement Variables
    protected bool isGrounded;
    protected float speed;

    protected float gravity;

    public virtual bool IsFinished(){ return false;}
    public virtual IState NextState() { return null;}
    public virtual void Start() { }
    public virtual void Finish() { }
    public virtual void Update(float dt) { }
}

public class IdleWalkingState : State
{
    Vector2 targetMovementVector;
    protected float jumpForce;
    public override void Update(float dt) {

        targetMovementVector = Vector2.zero;

        //Movement
        float movementValue = Input.GetAxis("Horizontal");
        targetMovementVector = Vector2.right * movementValue * speed;

        //Grounded
        Vector2 checkPoint = rb.position + collider.offset + collider.size.y * Vector2.down * transform.localScale.y;
        isGrounded = Physics2D.OverlapPoint(checkPoint - 0.05f * Vector2.down);

        //Gravity
        if (!isGrounded)
            targetMovementVector += Vector2.down * gravity;
        else
        {
            //Jump code
        }
    }

    public override bool IsFinished() {
        if (!isGrounded)
        {
            //nextState = JumpFallState
            return true;
        }
        return true;
    }
}

public class JumpFallState : State
{

}

public class GrowState : State
{

}

public class ShrinkState : State
{

}

/*public class PushingState : State
{

}*/

public class MovementController : MonoBehaviour
{
    HashSet<IState> states = new HashSet<IState>();
    IState currentState;

    void Update()
    {
        currentState.Update(Time.deltaTime);
        if (currentState.IsFinished())
        {
            currentState.Finish();
            currentState = currentState.NextState();
            currentState.Start();
        }
    }
}
