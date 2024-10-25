using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Numerics;
using System;
using UnityEngine.Events;
using Vector2 = UnityEngine.Vector2;

public class PlayerMovementTest : MonoBehaviour
{
    public Rigidbody2D rb;
    private Vector2 _inputDirection;

    //faltan por implementar----------------------------
    private bool _isJumpCut;
    private bool _isJumpFalling;


    //movimiento---------------------------------------
    public float direHorizontal;

    //checks-------------------------------
    public GameObject _bottomCenterCheck;
    private bool bottomCenterCheck;
    public bool BottomCenterCheck { set => bottomCenterCheck = value; }

    //layers----------------------------------------------
    [SerializeField] private LayerMask _groundLayer;

    [Header("Gravity Values")]
    public float gravityStrength;
    public float gravityScale;

    [Header("Jump Values")]
    public float jumpingPower;

    [Header("Movement Values")]
    public float speed;



    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        IsGrounded();
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded()) 
            Jump();
        
        
    }
    void FixedUpdate()
    {
        direHorizontal = Input.GetAxis("Horizontal");


        Movimiento();
    }
    public bool IsGrounded() //Temporarily public for the grip script
    {
        bottomCenterCheck = Physics2D.OverlapCircle(_bottomCenterCheck.transform.position, 0.2f, _groundLayer);
        if (bottomCenterCheck)
        {
            Debug.Log("UWU CHeck?");
            return true;
        }
        return false;
    }
    
    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpingPower);
    }

    public void Movimiento()
    {
        rb.position += direHorizontal * speed * Time.deltaTime * Vector2.right;

        //rb.velocity = new Vector2(direHorizontal * speed, rb.velocity.y);

        //rb.position += new Vector2(direHorizontal * speed * Time.deltaTime, 0);

        //rb.velocity = new Vector2(direHorizontal*speed, rb.velocity.y);
        
    }

}

