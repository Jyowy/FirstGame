using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerActions {

    // protected
    
    protected PlayerController m_playerController;
    protected Rigidbody2D m_rigidbody;
    protected Animator m_animator;
    protected SpriteRenderer m_spriteRenderer;

    // Current State
    // Basic
    protected bool m_isStanding;
    protected bool m_isInAir;
    protected bool m_wasInAir;
    protected bool m_isFalling;
    protected bool m_isGoingToRight;
    // Advanced
    protected bool m_canHang;
    protected bool m_canClimb;
    protected bool m_isAWallOnLeftSide;
    protected bool m_isAWallOnRightSide;
    protected bool m_canHold;

    // Current Actions
    // Basic
    protected bool m_isWalking;
    protected bool m_isJumping;
    protected bool m_isRunning;
    // Advanced
    protected bool m_isHanging;
    protected bool m_isTwoSideClimbing;
    protected bool m_isClimbing;
    protected bool m_isCroaching;

    public PlayerActions(PlayerController playerController,
        Rigidbody2D rigidbody,
        Animator animator,
        SpriteRenderer spriteRenderer)
    {
        m_playerController = playerController;
        m_rigidbody = rigidbody;
        m_animator = animator;
        m_spriteRenderer = spriteRenderer;

        ResetGeneralStatus();
    }
    
    public abstract bool UpdateActions();
    public abstract void UpdateAnimations();

    protected void ResetGeneralStatus()
    {
        m_isStanding = true;
        m_isInAir = false;
        m_wasInAir = false;
        m_isFalling = false;
        m_isTwoSideClimbing = false;
        m_isGoingToRight = true;

        m_canHang = false;
        m_canClimb = false;
        m_isAWallOnLeftSide = false;
        m_isAWallOnRightSide = false;
    }
    
}
