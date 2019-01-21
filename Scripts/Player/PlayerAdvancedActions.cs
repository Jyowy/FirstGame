using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAdvancedActions : PlayerActions
{
    private Vector2 k_hangImpulse = new Vector2(0.0f, 1200.0f);
    private Vector2 k_twoSideClimbingImpulse = new Vector2(0.0f, 1400.0f);
    private Vector2 k_climbSpeed = new Vector2(0.0f, 10.0f);
    private Vector2 k_jumpFromLeftWallImpulse = new Vector2(500.0f, 800.0f);
    private Vector2 k_jumpFromRightWallImpulse = new Vector2(-500.0f, 800.0f);
    private Vector2 k_croachJumpImpulse = new Vector2(0.0f, 2000.0f);

    private Vector2 m_newVelocity = new Vector2();

    private Vector2 m_hangPosition = new Vector2();
    private bool m_isHangingToRight;
    private Vector2 m_climbPosition = new Vector2();
    private bool m_isClimbingToRight;
    private bool m_wasClimbing;
    private bool m_wasHanging;
    private bool m_isClimbingMoving;

    private const float k_jumpInputThreshold = 0.1f;
    private const float k_horizontalInputThreshold = 0.1f;
    private const float k_verticalInputThreshold = 0.1f;
    private const float k_runInputThreshold = 0.1f;
    private const float k_movingThreshold = 0.1f;
    private const float k_croachThreshold = -0.4f;

    private float m_hangTimer = 0.0f;
    private const float k_hangCD = 0.5f;
    private float m_climbTimer = 0.0f;
    private const float k_climbCD = 0.2f;
    private float m_jumpTimer = 0.0f;
    private const float k_jumpCD = 0.2f;

    private int k_animationHangParameter = Animator.StringToHash("Hang");
    private int k_animationIsHangingParameter = Animator.StringToHash("IsHanging");
    private int k_animationClimbParameter = Animator.StringToHash("Climb");
    private int k_animationIsClimbingParameter = Animator.StringToHash("IsClimbing");
    private int k_animationIsClimbingMovingParameter = Animator.StringToHash("IsClimbingMoving");
    private int k_animationIsCroachingParameter = Animator.StringToHash("IsCroaching");
    private int k_animationJumpFromWallParameter = Animator.StringToHash("JumpFromWall");

    public PlayerAdvancedActions(PlayerController playerController,
        Rigidbody2D rigidbody,
        Animator animator,
        SpriteRenderer spriteRenderer)
        : base(playerController, rigidbody, animator, spriteRenderer)
    {
    }

    private void ResetInnerStatus()
    {
        m_isTwoSideClimbing = false;
        m_isClimbing = false;
        m_isCroaching = false;
        m_isHanging = false;
        m_isClimbingMoving = false;
    }

    private void UpdateCurrentPlayerStatus()
    {
        m_playerController.SetIsHanging(m_isHanging);
        m_playerController.SetIsClimbing(m_isClimbing);
        m_playerController.SetIsCroaching(m_isCroaching);
    }

    private void SetPreviousStatus()
    {
        m_wasInAir = m_isInAir;
        m_wasHanging = m_isHanging;
        m_wasClimbing = m_isClimbing;
    }

    private void GetCurrentPlayerStatus()
    {
        m_isInAir = m_playerController.GetIsInAir();
        m_isGoingToRight = m_playerController.GetIsGoingToRight();
        m_canHang = m_playerController.GetCanHang();
        m_isHangingToRight = m_playerController.GetIsHangingToRight();
        m_isFalling = m_playerController.GetIsFalling();
        m_isAWallOnLeftSide = m_playerController.GetIsAWallOnLeftSide();
        m_isAWallOnRightSide = m_playerController.GetIsAWallOnRightSide();
        m_canClimb = m_playerController.GetCanClimb();
        m_isClimbingToRight = m_playerController.GetIsClimbingToRight();
    }

    private void ResetCDs()
    {
        m_jumpTimer = k_jumpCD;
        m_hangTimer = k_hangCD;
        m_climbTimer = k_climbCD;
    }

    // Update Actions

    public override bool UpdateActions()
    {
        bool singleAnimation = false;
        
        ResetInnerStatus();
        GetCurrentPlayerStatus();
        UpdateTimers();

        if (m_isInAir)
        {
            singleAnimation = UpdateActionsInAir();
        }
        else
        {
            singleAnimation = UpdateOnFloor();
        }

        SetTimers();
        UpdateCurrentPlayerStatus();

        return singleAnimation;
    }

    private void SetTimers()
    {
        if (m_wasClimbing && !m_isClimbing)
        {
            m_climbTimer = k_climbCD;
        }
    }

    private void UpdateTimers()
    {
        if (m_jumpTimer > 0.0f)
        {
            m_jumpTimer -= Time.fixedDeltaTime;
        }
        if (m_hangTimer > 0.0f)
        {
            m_hangTimer -= Time.fixedDeltaTime;
        }
        if (m_climbTimer > 0.0f)
        {
            m_climbTimer -= Time.fixedDeltaTime;
        }
    }

    // Air Actions

    public bool UpdateActionsInAir()
    {
        bool singleAnimation = false;

        if (!m_wasInAir && m_isInAir)
        {
            m_jumpTimer = k_jumpCD;
        }
        
        if (m_canHang && m_hangTimer <= 0.0f)
        {
            Hang();
            singleAnimation = m_isHanging;
        }
        else if (m_isAWallOnLeftSide && m_isAWallOnRightSide)
        {
            TwoSideClimb();
        }
        else if (m_canClimb && m_climbTimer <= 0.0f)
        {
            Climb();
            singleAnimation = m_isClimbing;
        }
        else if (m_isAWallOnLeftSide || m_isAWallOnRightSide)
        {
            float jumpInput = Input.GetAxis("Jump");
            if (jumpInput > k_jumpInputThreshold
                && m_jumpTimer <= 0.0f)
            {
                JumpFromWall();
            }
        }

        return singleAnimation;
    }
    
    private void AddImpulse(ref Vector2 jumpImpulse)
    {
        m_newVelocity = m_rigidbody.velocity;
        m_newVelocity.y = 0.0f;
        m_rigidbody.velocity = m_newVelocity;
        m_rigidbody.AddForce(jumpImpulse);
    }

    private void StopHanging()
    {
        m_isHanging = false;
        ResetCDs();
    }

    void Hang()
    {
        float jumpInput = Input.GetAxis("Jump");
        float verticalInput = Input.GetAxis("Vertical");

        m_rigidbody.velocity = Vector2.zero;

        if (jumpInput > k_jumpInputThreshold)
        {
            float horizontalInput = Input.GetAxis("Horizontal");

            if (m_isHangingToRight && horizontalInput < -k_horizontalInputThreshold)
            {
                JumpFromWall();
            }
            else if (!m_isHangingToRight && horizontalInput > k_horizontalInputThreshold)
            {
                JumpFromWall();
            }
            else if (!(verticalInput < -k_verticalInputThreshold))
            {
                AddImpulse(ref k_hangImpulse);
            }

            StopHanging();
        }
        else
        {
            if (!m_wasHanging)
            {
                m_playerController.GetHangPosition(ref m_hangPosition);
            }
            m_isHanging = true;
            m_rigidbody.MovePosition(m_hangPosition);
        }
        
        m_playerController.SetIsHanging(m_isHanging);
    }

    void TwoSideClimb()
    {
        float verticalInput = Input.GetAxis("Vertical");

        m_rigidbody.velocity = Vector2.zero;

        if (verticalInput > k_verticalInputThreshold)
        {
            m_rigidbody.AddForce(k_twoSideClimbingImpulse * Time.fixedDeltaTime);
        }
        else if (verticalInput < -k_verticalInputThreshold)
        {
            m_rigidbody.AddForce(-k_twoSideClimbingImpulse * Time.fixedDeltaTime);
        }

        m_isTwoSideClimbing = true;
    }

    void Climb()
    {
        float jumpInput = Input.GetAxis("Jump");
        float verticalInput = Input.GetAxis("Vertical");

        if (m_isInAir)
        {
            m_rigidbody.velocity = Vector2.zero;
        }

        if (!m_wasClimbing)
        {
            m_climbPosition = m_rigidbody.position;
        }

        if (jumpInput > k_jumpInputThreshold)
        {
            if (m_isInAir)
            {
                JumpFromWall();
            }
            m_isClimbing = false;
            return;
        }
        else if (Mathf.Abs(verticalInput) > k_verticalInputThreshold)
        {
            m_climbPosition += Time.fixedDeltaTime * (verticalInput > 0.0f ? k_climbSpeed : -k_climbSpeed);
            m_rigidbody.MovePosition(m_climbPosition);
            m_isClimbingMoving = true;
        }
        else if (m_isInAir)
        {
            m_rigidbody.MovePosition(m_climbPosition);
        }
        m_isClimbing = true;

    }

    void JumpFromWall()
    {
        m_rigidbody.velocity = Vector2.zero;

        if (m_isAWallOnLeftSide)
        {
            AddImpulse(ref k_jumpFromLeftWallImpulse);
        }
        else
        {
            AddImpulse(ref k_jumpFromRightWallImpulse);
        }
        ResetCDs();

        m_jumpTimer = k_jumpCD;

        m_animator.SetTrigger(k_animationJumpFromWallParameter);
    }
    
    // Floor Actions

    public bool UpdateOnFloor()
    {
        bool singleAnimation = false;

        float verticalInput = Input.GetAxis("Vertical");

        if (m_canClimb && m_climbTimer <= 0.0f)
        {
            Climb();
        }
        else if (verticalInput < k_croachThreshold)
        {
            Croach();
        }
        
        return singleAnimation;
    }

    private void Croach()
    {
        float jumpInput = Input.GetAxis("Jump");

        m_isCroaching = true;

        if (jumpInput > k_jumpInputThreshold)
        {
            AddImpulse(ref k_croachJumpImpulse);
            m_jumpTimer = k_jumpCD;
            m_isCroaching = false;
        }
    }

    // Update Animations

    public override void UpdateAnimations()
    {
        m_animator.SetBool(k_animationIsHangingParameter, m_isHanging);
        if (m_isHanging)
        {
            if (!m_wasHanging)
            {
                m_animator.SetTrigger(k_animationHangParameter);
            }
            m_spriteRenderer.flipX = !m_isHangingToRight;
        }
        m_animator.SetBool(k_animationIsClimbingParameter, m_isClimbing);
        if (m_isClimbing)
        {
            if (!m_wasClimbing)
            {
                m_animator.SetTrigger(k_animationClimbParameter);
            }
            m_spriteRenderer.flipX = !m_isClimbingToRight;
        }
        m_animator.SetBool(k_animationIsCroachingParameter, m_isCroaching);
        m_animator.SetBool(k_animationIsClimbingMovingParameter, m_isClimbingMoving);
        
        SetPreviousStatus();
    }
}
