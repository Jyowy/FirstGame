using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBasicActions : PlayerActions {

    // private
    
    public Vector2 m_walkImpulse = new Vector2(1400.0f, 0.0f);
    public Vector2 m_runImpulse  = new Vector2(2000.0f, 0.0f);
    public Vector2 m_jumpImpulse = new Vector2(0.0f, 1200.0f);
    public Vector2 m_moveInAirImpulse = new Vector2(100.0f, 0.0f);

    private Vector2 m_newVelocity = new Vector2();

    private float m_jumpTimer = 0.0f;
    private const float k_jumpCD = 0.4f;

    private const float k_jumpInputThreshold = 0.0f;
    private const float k_horizontalInputThreshold = 0.1f;
    private const float k_runInputThreshold = 0.1f;
    private const float k_movingThreshold = 0.2f;
    private const float k_minSpeedAscending = 0.4f;

    private int k_animationWalkParameter = Animator.StringToHash("IsWalking");
    private int k_animationRunParameter = Animator.StringToHash("IsRunning");
    private int k_animationFallingParameter = Animator.StringToHash("IsFalling");
    private int k_animationAscendingParameter = Animator.StringToHash("IsAscending");

    public PlayerBasicActions(PlayerController playerController,
        Rigidbody2D rigidbody,
        Animator animator,
        SpriteRenderer spriteRenderer)
        : base(playerController, rigidbody, animator, spriteRenderer)
    {
    }

    // Actions

    private void ResetInnerStatus()
    {
        m_isWalking = false;
        m_isRunning = false;
    }

    private void UpdateCurrentPlayerStatus()
    {
        m_playerController.SetIsWalking(m_isWalking);
        m_playerController.SetIsRunning(m_isRunning);
    }

    private void GetCurrentPlayerStatus()
    {
        m_isInAir = m_playerController.GetIsInAir();
        m_isStanding = m_playerController.GetIsStanding();
    }

    override public bool UpdateActions()
    {
        bool singleAnimation = false;

        float horizontalInput = Input.GetAxis("Horizontal");
        float jumpInput = Input.GetAxis("Jump");
        float runInput = Input.GetAxis("Run");

        ResetInnerStatus();
        GetCurrentPlayerStatus();
        if (horizontalInput < -k_horizontalInputThreshold)
        {
            m_isGoingToRight = false;
        }
        else if (horizontalInput > k_horizontalInputThreshold)
        {
            m_isGoingToRight = true;
        }

        UpdateTimers();

        if (!m_isInAir && m_isStanding)
        {
            // Jump
            if (jumpInput > k_jumpInputThreshold &&
                m_jumpTimer <= 0.0f)
            {
                Jump();
            }
            else if (Mathf.Abs(horizontalInput) > k_horizontalInputThreshold)
            {
                m_isGoingToRight = horizontalInput > 0.0f;
                if (runInput > k_runInputThreshold)
                {
                    Run();
                }
                else
                {
                    Walk();
                }
            }
        }
        else if (m_isInAir)
        {
            if (Mathf.Abs(horizontalInput) > k_horizontalInputThreshold)
            {
                MoveInAir();
            }
        }

        if (m_isFalling && (!m_isInAir || m_rigidbody.velocity.y >= 0.0f))
        {
            m_isFalling = false;
            m_playerController.SetIsFalling(m_isFalling);
        }
        else if (!m_isFalling && (m_isInAir && m_rigidbody.velocity.y < 0.0f))
        {
            m_isFalling = true;
            m_playerController.SetIsFalling(m_isFalling);
        }

        UpdateCurrentPlayerStatus();
        
        return singleAnimation;
    }

    void UpdateTimers()
    {
        if (m_jumpTimer > 0.0f)
        {
            m_jumpTimer -= Time.fixedDeltaTime;
        }
    }

    void Jump()
    {
        m_newVelocity = m_rigidbody.velocity;
        m_newVelocity.y = 0.0f;
        m_rigidbody.velocity = m_newVelocity;
        m_rigidbody.AddForce(m_jumpImpulse);
        m_isJumping = true;
        m_jumpTimer = k_jumpCD;
        m_playerController.SetIsJumping(m_isJumping);
    }
    
    void Run()
    {
        m_isWalking = true;
        m_isRunning = true;
        if (m_isGoingToRight)
        {
            m_rigidbody.AddForce(m_runImpulse * Time.fixedDeltaTime);
        }
        else
        {
            m_rigidbody.AddForce(-m_runImpulse * Time.fixedDeltaTime);
        }
    }

    void Walk()
    {
        m_isWalking = true;
        if (m_isGoingToRight)
        {
            m_rigidbody.AddForce(m_walkImpulse * Time.fixedDeltaTime);
        }
        else
        {
            m_rigidbody.AddForce(-m_walkImpulse * Time.fixedDeltaTime);
        }
    }

    void MoveInAir()
    {
        if (m_isGoingToRight)
        {
            m_rigidbody.AddForce(m_moveInAirImpulse * Time.fixedDeltaTime);
        }
        else
        {
            m_rigidbody.AddForce(-m_moveInAirImpulse * Time.fixedDeltaTime);
        }
    }

    // Animations

    public override void UpdateAnimations()
    {
        if (m_isJumping)
        {
            m_isJumping = false;
            m_playerController.SetIsJumping(m_isJumping);
        }
        m_animator.SetBool(k_animationWalkParameter, m_isWalking);
        m_animator.SetBool(k_animationRunParameter, m_isRunning);
        
        if (m_isInAir)
        {
            if (m_rigidbody.velocity.y > k_minSpeedAscending)
            {
                m_animator.SetBool(k_animationFallingParameter, false);
                m_animator.SetBool(k_animationAscendingParameter, true);
            }
            else
            {
                m_animator.SetBool(k_animationFallingParameter, true);
                m_animator.SetBool(k_animationAscendingParameter, false);
            }
        }

        m_spriteRenderer.flipX = !m_isGoingToRight;
    }

}
