﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collisions))]
public class CharacterBehaviour : MonoBehaviour
{
    public enum State { Default, Dead, GodMode }
    public State state;
    public CameraBehaviour cameraBehaviour;
    [Header("State")]
    public bool canMove = true;
    public bool canJump = true;
    public bool isFacingRight = true;
    public bool isJumping = false;
    public bool isRunning = false;
    public bool crouch = false;
    //public bool isLookingUp = false;
    //public bool isLookingDown = false;
    public bool canDoubleJump = false;
    public bool onLadder = false;
    public bool canWallJump = false;
    public bool isWallJumping = false;
    [Header("Physics")]
    public Rigidbody2D rb;
    public Collisions collisions;
    public BoxCollider2D collider2d;
    [Header("Speed")]
    public float walkSpeed;
    public float runSpeed;
    public float movementSpeed;
    public float horizontalSpeed;
    public float onLadderSpeed;
    public float verticalSpeed;
    public Vector2 axis;
    [Header("Forces")]
    public float jumpWalkForce;
    public float jumpRunForce;
    public float jumpForce;
    [Header("Graphics")]
    public SpriteRenderer rend;
    [Header("Collider Values")]
    public float standYSize;
    public float crouchYSize;
    public float standYOffset;
    public float crouchYOffset;
    
    void Start ()
    {
        collisions = GetComponent<Collisions>();
        rb = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<BoxCollider2D>();
        standYSize = collider2d.size.y;
        crouchYSize = collider2d.size.y / 2;
        standYOffset = collider2d.offset.y;
        crouchYOffset = collider2d.offset.y / 2 + 0.065f;
	}
	
	void Update ()
    {
        switch(state)
        {
            case State.Default:
                DefaultUpdate();
                break;
            case State.Dead:
                break;
            case State.GodMode:
                break;
            default:
                break;
        }
    }

    private void FixedUpdate()
    {
        collisions.MyFixedUpdate();

        if(isJumping)
        {
            isJumping = false;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (isWallJumping)
        {
            isWallJumping = false;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        rb.velocity = new Vector2(horizontalSpeed, rb.velocity.y);

        if(onLadder)
        {
            rb.velocity = new Vector2(rb.velocity.x, verticalSpeed);
        }

        if (crouch)
        {
            canMove = false;
            collider2d.size = new Vector2(collider2d.size.x, crouchYSize);
            collider2d.offset = new Vector2(collider2d.offset.x, crouchYOffset);
        }
        else
        {
            canMove = true;
            crouch = false;
            collider2d.size = new Vector2(collider2d.size.x, standYSize);
            collider2d.offset = new Vector2(collider2d.offset.x, standYOffset);
        }
    }

    protected virtual void DefaultUpdate()
    {
        // Calcular el movimiento horizontal
        HorizontalMovement();
        // Calcular el movimiento vertical
        VerticalMovement();
    }

    void HorizontalMovement()
    {
        if(!canMove)
        {
            horizontalSpeed = 0;
            return;
        }
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        //Punto muerto
        if(-0.1f < axis.x && axis.x < 0.1f)
        {
            if(collisions.isGrounded)
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }
            horizontalSpeed = 0;
            return;
        }
        //Si toca la pared
        if(collisions.isWalled && collisions.isFalling)
        {
            canWallJump = true;
            canDoubleJump = false;
        }
        else canWallJump = false;

        if (collisions.isWalled)
        {
            if ((isFacingRight && axis.x > 0) || (!isFacingRight && axis.x < 0))
            {
                horizontalSpeed = 0;
                return;
            }
        }

        if(isFacingRight && axis.x < 0) Flip();
        if(!isFacingRight && axis.x > 0) Flip();

        if(isRunning) movementSpeed = runSpeed;
        else movementSpeed = walkSpeed;

        horizontalSpeed = axis.x * movementSpeed;
    }
    void VerticalMovement()
    {
        crouch = false;
        //isLookingDown = false;
        //isLookingUp = false;

        if(onLadder)
        {
            canDoubleJump = false;
            verticalSpeed = axis.y * onLadderSpeed;
        }
    }
    void Jump()
    {
        isJumping = true;
    }
    void DoubleJump()
    {
        isJumping = true;
        canDoubleJump = false;
    }
    void WallJump()
    {
        isWallJumping = true;
    }
    void Flip()
    {
        isFacingRight = !isFacingRight;
        rend.flipX = !rend.flipX;
        collider2d.offset = new Vector2(collider2d.offset.x * -1, collider2d.offset.y);
        collisions.Flip(isFacingRight);
        cameraBehaviour.offSet.x *= -1;
    }
    void Crouching()
    {
        crouch = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("ladder"))
        {
            onLadder = true;
            rb.gravityScale = 0;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("ladder"))
        {
            onLadder = false;
            rb.gravityScale = 1;
        }
    }
    #region Public
    public void SetAxis(Vector2 inputAxis)
    {
        axis = inputAxis;
    }
    public void JumpStart() //Decidir como será el salto
    {
        if(!canJump) return;

        if(collisions.isGrounded)
        {
            /*if(isLookingDown)
            {
                Debug.Log("bajar plataforma");
            }*/

            if(isRunning) jumpForce = jumpRunForce;
            else jumpForce = jumpWalkForce;
            Jump();
        }

        if(collisions.isFalling && canDoubleJump)
        {
            if(isRunning) jumpForce = jumpRunForce;
            else jumpForce = jumpWalkForce;
            DoubleJump();
        }

        if(collisions.isFalling && canWallJump)
        {
            if (isRunning) jumpForce = jumpRunForce;
            else jumpForce = jumpWalkForce;
            WallJump();
        }
    }
    public void Crouch()
    {
        if (collisions.isGrounded)
        {
            Crouching();
        }
    }
    #endregion
}
