using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // public

    // Player main components

    public bool m_includeBasicActions = true;
    public bool m_includeAdvancedActions = true;
    
    // Movement Actions

    // Level Manager
    [HideInInspector]
    public LevelManager m_levelManager;

    // private

    // Player main components
    private Collider2D m_collider;
    private Rigidbody2D m_rigidbody;
    private Animator m_animator;
    private SpriteRenderer m_spriteRenderer;
    
    // Player Actions
    private PlayerActions[] m_playerActionsSet;

    //private PlayerBasicActions m_playerBasicActions;

    // Movement Actions
    // General

    // Current Status
    // Basic
    private bool m_isStanding;
    private bool m_isInAir;
    private bool m_isGoingToRight;

    private Vector2 k_checkIsInAirOrigin = new Vector2();
    private float k_checkIsInAirHorizontalMargin = 0.01f;
    private float k_distanceBetweenFeet;
    private int k_scenaryLayerMask;
    // Advanced
    private bool m_canHang;
    private bool m_canClimb;
    private bool m_isAWallOnLeftSide;
    private bool m_isAWallOnRightSide;
    private bool m_canHold;

    private Vector2 m_hangPosition = new Vector2();
    private Vector2 k_checkIsLeftWallOrigin = new Vector2();
    private Vector2 k_checkIsRightWallOrigin = new Vector2();
    private float k_checkIsAWallHeight;
    private float k_checkIsAWallDistance = 0.6f;
    private const float k_minimumDistanceToBeAHanger = 0.1f;
    private const float k_maximumDistanceToBeAHanger = 0.8f;

    private Vector2 m_climbPosition = new Vector2();
    // Current Actions
    // Basic
    private bool m_isWalking;
    private bool m_isJumping;
    private bool m_isRunning;
    private bool m_isFalling;
    // Advanced
    private bool m_isHanging;
    private bool m_isHangingToRight;
    private bool m_isTwoSideClimbing;
    private bool m_isClimbing;
    private bool m_isClimbingToRight;
    private bool m_isCroaching;

    // Animations
    protected int k_animationInAirParameter = Animator.StringToHash("InAir");
    protected int k_animationOnFloorParameter = Animator.StringToHash("OnFloor");
    protected int k_animationIsStandingParameter = Animator.StringToHash("IsStanding");

    // Start and SetUp

    void Start()
    {
        // Player main components
        Collider2D[] colliders2D = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders2D)
        {
            if (collider.isTrigger == false)
            {
                m_collider = collider;
                break;
            }
        }
        if (m_collider == false)
        {
            throw new System.Exception("Collider2D for physics collisions not found in Player.");
        }
        
        m_rigidbody = GetComponent<Rigidbody2D>();
        if (m_rigidbody == null)
        {
            throw new System.Exception("Rigidbody2D not found in Player.");
        }

        m_animator = GetComponent<Animator>();
        if (m_animator == null)
        {
            throw new System.Exception("Animator not found in Player.");
        }

        m_spriteRenderer = GetComponent<SpriteRenderer>();
        if (m_spriteRenderer == null)
        {
            throw new System.Exception("SpriteRenderer not found in Player.");
        }

        // Movement Actions
        // General
        ResetCurrentPlayerStatus();

        int actionSetCount = (m_includeBasicActions ? 1 : 0) + (m_includeAdvancedActions ? 1 : 0);
        m_playerActionsSet = new PlayerActions[actionSetCount];

        int actionSetIndex = 0;
        if (m_includeBasicActions)
        {
            m_playerActionsSet[actionSetIndex++] = new PlayerBasicActions(this, m_rigidbody, m_animator, m_spriteRenderer);
        }
        if (m_includeAdvancedActions)
        {
            m_playerActionsSet[actionSetIndex++] = new PlayerAdvancedActions(this, m_rigidbody, m_animator, m_spriteRenderer);
        }
        
        m_animator.SetBool(k_animationIsStandingParameter, m_isStanding);
        m_animator.SetBool(k_animationInAirParameter, m_isInAir);

        // Basic
        k_checkIsInAirOrigin.x = -m_collider.bounds.size.x / 2.0f - k_checkIsInAirHorizontalMargin;
        k_checkIsInAirOrigin.y = -m_collider.bounds.size.y / 2.0f - 0.05f;
        k_distanceBetweenFeet = m_collider.bounds.size.x + k_checkIsInAirHorizontalMargin * 2.0f;
        k_scenaryLayerMask = LayerMask.GetMask("Scenary");

        // Advanced
        k_checkIsRightWallOrigin.x = m_collider.bounds.size.x / 2.0f + k_checkIsAWallDistance;
        k_checkIsRightWallOrigin.y = m_collider.bounds.size.y / 2.0f + 0.25f;
        k_checkIsLeftWallOrigin.x = -k_checkIsRightWallOrigin.x;
        k_checkIsLeftWallOrigin.y = k_checkIsRightWallOrigin.y;
        k_checkIsAWallHeight = k_checkIsRightWallOrigin.y;
    }

    // Update

    void FixedUpdate()
    {
        UpdateStatus();
        for (int i = 0; i < m_playerActionsSet.Length; ++i)
        {
            m_playerActionsSet[i].UpdateActions();
        }
    }
    
    void Update()
    {
        for (int i = 0; i < m_playerActionsSet.Length; ++i)
        {
            m_playerActionsSet[i].UpdateAnimations();
        }
    }
    
    // Position

    public void GetPosition(out Vector2 position)
    {
        position = m_rigidbody.position;
    }

    public void ChangePosition(Vector2 position, bool initialState = false)
    {
        m_rigidbody.position = position;
        if (initialState)
        {
            m_rigidbody.velocity = Vector2.zero;
            m_rigidbody.angularVelocity = 0.0f;
        }
    }

    public void MovePlayerToPosition(Vector2 position, bool initialState = false)
    {
        m_rigidbody.MovePosition(position);
        if (initialState)
        {
            m_rigidbody.velocity = Vector2.zero;
            m_rigidbody.angularVelocity = 0.0f;
        }
    }

    // Status

    private void ResetCurrentPlayerStatus()
    {
        // Status
        // Basic Status
        m_isStanding = true;
        m_isInAir = false;
        m_isGoingToRight = true;
        
        // Advanced Air Status
        m_canHang = false;
        m_isAWallOnLeftSide = false;
        m_isAWallOnRightSide = false;
        m_canClimb = false;
        m_canHold = false;
        // Advanced Floor Status

        // Actions
        // Basic Actions
        m_isWalking = false;
        m_isJumping = false;
        m_isRunning = false;
        m_isFalling = false;
        // Advanced Actions
        m_isHanging = false;
        m_isHangingToRight = false;
        m_isTwoSideClimbing = false;
        m_isClimbing = false;
        m_isClimbingToRight = false;
    }

    // Check Status

    private void UpdateStatus()
    {
        UpdateIsStanding();
        UpdateInAirStatus();
        UpdateWallsAndHangStatus();
        //Debug.Log("IsInAir = " + m_isInAir);
    }

    private void UpdateIsStanding()
    {
        m_isStanding = !m_isInAir && !m_isCroaching;
    }

    private void UpdateInAirStatus()
    {
        bool wasInAir = m_isInAir;

        // Draws a line checking all bottom part of Player's collider
        RaycastHit2D raycastHit = Physics2D.Raycast(
            m_rigidbody.position + k_checkIsInAirOrigin,
            Vector2.right,
            k_distanceBetweenFeet,
            k_scenaryLayerMask);

        m_isInAir = raycastHit.collider == null;

        if (wasInAir != m_isInAir)
        {
            if (m_isInAir)
            {
                m_animator.SetTrigger(k_animationInAirParameter);
            }
            else
            {
                m_animator.SetTrigger(k_animationOnFloorParameter);
            }
        }
    }

    private void UpdateWallsAndHangStatus()
    {
        RaycastHit2D raycastHit;

        m_canHang = false;

        // Left Wall   
        raycastHit = Physics2D.Raycast(
            m_rigidbody.position + k_checkIsLeftWallOrigin,
            Vector2.down,
            k_checkIsAWallHeight,
            k_scenaryLayerMask
            );
        m_isAWallOnLeftSide = raycastHit.collider != null;
        if (m_isAWallOnLeftSide &&
            raycastHit.distance > k_minimumDistanceToBeAHanger &&
            raycastHit.distance < k_maximumDistanceToBeAHanger)
        {
            m_canHang = true;
            m_isHangingToRight = false;
            if (m_canHang && !m_isHanging)
            {
                //m_hangPosition = m_rigidbody.position + k_checkIsLeftWallOrigin;
                m_hangPosition = m_rigidbody.position;
            }
        }

        // Right Wall
        raycastHit = Physics2D.Raycast(
            m_rigidbody.position + k_checkIsRightWallOrigin,
            Vector2.down,
            k_checkIsAWallHeight,
            k_scenaryLayerMask
            );
        m_isAWallOnRightSide = raycastHit.collider != null;
        if (!m_canHang && m_isAWallOnRightSide &&
            raycastHit.distance > k_minimumDistanceToBeAHanger &&
            raycastHit.distance < k_maximumDistanceToBeAHanger)
        {
            m_canHang = true;
            m_isHangingToRight = true;

            if (m_canHang && !m_isHanging)
            {
                //m_hangPosition = m_rigidbody.position + k_checkIsRightWallOrigin;
                m_hangPosition = m_rigidbody.position;
            }
        }

    }
    
    // Status

    // Basic Status

    public bool GetIsStanding()
    {
        return m_isStanding;
    }
        
    public bool GetIsInAir()
    {
        return m_isInAir;
    }

    public bool GetIsGoingToRight()
    {
        return m_isGoingToRight;
    }

    public void SetIsGoingToRight(bool isGoingToRight)
    {
        m_isGoingToRight = isGoingToRight;
    }

    // Advanced Air Status

    public bool GetCanHang()
    {
        return m_canHang;
    }

    public void SetCanHang(bool isAvailable)
    {
        m_canHang = isAvailable;
    }

    public void GetHangPosition(ref Vector2 hangPosition)
    {
        hangPosition = m_hangPosition;
    }

    public void SetHangPosition(ref Vector2 hangPosition)
    {
        m_hangPosition = hangPosition;
    }

    public void GetClimbPosition(ref Vector2 climbPosition)
    {
        climbPosition = m_climbPosition;
    }

    public void SetClimbPosition(ref Vector2 climbPosition)
    {
        m_climbPosition = climbPosition;
    }

    public bool GetIsAWallOnLeftSide()
    {
        return m_isAWallOnLeftSide;
    }

    public bool GetIsAWallOnRightSide()
    {
        return m_isAWallOnRightSide;
    }

    public bool GetCanClimb()
    {
        return m_canClimb;
    }

    public bool GetCanHold()
    {
        return m_canHold;
    }
    
    public void SetIsHanging(bool isHanging)
    {
        m_isHanging = isHanging;
    }
    
    public bool GetIsHangingToRight()
    {
        return m_isHangingToRight;
    }

    public void SetIsHangingToRight(bool isHangingToRight)
    {
        m_isHangingToRight = isHangingToRight;
    }

    public bool GetIsTwoSideClimbing()
    {
        return m_isTwoSideClimbing;
    }

    public void SetIsTwoSideClimbing(bool isTwoSideClimbing)
    {
        m_isTwoSideClimbing = isTwoSideClimbing;
    }

    public bool GetIsClimbing()
    {
        return m_isClimbing;
    }

    public void SetIsClimbing(bool isClimbing)
    {
        m_isClimbing = isClimbing;
    }

    public bool GetIsClimbingToRight()
    {
        return m_isClimbingToRight;
    }

    public void SetIsCroaching(bool isCroaching)
    {
        m_isCroaching = isCroaching;
    }

    // Advanced Floor Status

    // Actions

    // Basic Actions

    public bool GetIsWalking()
    {
        return m_isWalking;
    }

    public void SetIsWalking(bool isWalking)
    {
        m_isWalking = isWalking;
    }

    public bool GetIsRunning()
    {
        return m_isRunning;
    }

    public void SetIsRunning(bool isRunning)
    {
        m_isRunning = isRunning;
    }

    public bool GetIsJumping()
    {
        return m_isJumping;
    }

    public void SetIsJumping(bool isJumping)
    {
        m_isJumping = isJumping;
    }

    public bool GetIsFalling()
    {
        return m_isFalling;
    }

    public void SetIsFalling(bool isFalling)
    {
        m_isFalling = isFalling;
    }
    
    // Advanced Air Actions

    // Advanced Floor Actions

    // Events

    void OnTriggerEnter2D(Collider2D collider2D)
    {
        // Hole
        if (collider2D.gameObject.CompareTag("Hole"))
        {
            EnterHole();
        }
        else if (collider2D.gameObject.CompareTag("Finish"))
        {
            EndReached();
        }
        else if (collider2D.gameObject.CompareTag("Climb Surface"))
        {
            m_canClimb = true;
            m_climbPosition = m_rigidbody.position;
            m_isClimbingToRight = m_rigidbody.position.x < collider2D.transform.position.x;
        }
    }

    void OnTriggerExit2D(Collider2D collider2D)
    {
        if (collider2D.gameObject.CompareTag("Climb Surface"))
        {
            m_canClimb = false;
            m_climbPosition = m_rigidbody.position;
        }
    }

    
    // HoleEntered
    
    void EnterHole()
    {
        // Notice LevelManager
        if (m_levelManager == null)
        {
            throw new System.Exception("Player has no reference to LevelManager.");
        }
        m_levelManager.PlayerEnteredInAHole();
    }

    // EndReached

    void EndReached()
    {
        if (m_levelManager == null)
        {
            throw new System.Exception("Player has no reference to LevelManager.");
        }
        m_levelManager.PlayerEnteredEndPoint();
    }

}