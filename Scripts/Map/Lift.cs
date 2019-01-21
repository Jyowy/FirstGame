using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lift : MonoBehaviour {

    public enum LiftActivationMode
    {
        Always,
        Player,
        Trigger,
    };

    public enum LiftActivationBehavior
    {
        Cycle,
        Go,
    }

    public enum LiftDeactivationBehavior
    {
        Nothing,
        Return,
        CompleteCycle,
        CompleteCycleAndReturn,
    };

    // public

    public float m_speed = 2.0f;
    public float m_timeToActive = 1.0f;
    public bool m_isLoop = true;
    public LiftActivationMode m_activationMode = LiftActivationMode.Always;
    public LiftActivationBehavior m_activationBehavior = LiftActivationBehavior.Cycle;
    public LiftDeactivationBehavior m_deactivationBehavior = LiftDeactivationBehavior.Nothing;
    public GameObject m_pointsContainer;
    public Rigidbody2D m_rigidbody;
    public BoxCollider2D m_playerActivationArea;

    // private

    private Vector2[] m_points;
    private Vector2 m_nextPoint = new Vector2();
    private Vector2 m_movement = new Vector2();
    private Vector2 m_newPosition = new Vector2();
    private float m_activationTimer;
    private int m_prevPointIdx;
    private int m_nextPointIdx;
    private int m_pointsCount;
    private bool m_hasActivationStarted;
    private bool m_hasDeactivationStarted;
    private bool m_isActive;
    private bool m_isMoving;
    private bool m_isReturning;
    private bool m_isLastCycle;

    void Awake()
    {
        //m_rigidbody = GetComponent<Rigidbody2D>();
        if (m_rigidbody == null)
        {
            throw new System.Exception("Rigidbody2D not found in lift " + name);
        }
        // points
        if (m_pointsContainer == null)
        {
            throw new System.Exception("Points not set in lift " + name + ".");
        }
        m_pointsCount = m_pointsContainer.transform.childCount;
        if ( m_pointsCount == 0)
        {
            throw new System.Exception("No points found in Points Container");
        }
        m_points = new Vector2[m_pointsCount];
        int childIdx = 0;
        foreach (Transform child in m_pointsContainer.transform)
        {
            m_points[childIdx++] = child.transform.position;
        }

        if (m_activationMode == LiftActivationMode.Player && m_playerActivationArea == null)
        {
            Debug.LogError("Lift " + name + "'s Activation Mode is set to Player, but it has no BoxCollider2D set to trigger mode.");
            return;
        }
        
        SetPosition(ref m_points[0]);
        m_prevPointIdx = 0;
        m_nextPointIdx = 1;
        m_nextPoint = m_points[m_nextPointIdx];
        if (m_activationMode == LiftActivationMode.Always)
        {
            StartActivation();
        }
        SetMoving(false);
        m_isReturning = false;
        m_isLastCycle = false;
    }

    private void SetPosition(ref Vector2 position)
    {
        m_rigidbody.MovePosition(position);
    }

    void FixedUpdate()
    {
        CheckActivation();
        if (!m_isMoving)
        {
            return;
        }

        float distanceToTravel = m_speed * Time.fixedDeltaTime;
        int iterations = 0;
        while (m_isMoving && distanceToTravel > 0.0f)
        {
            distanceToTravel = TravelForwardCurrentNextPoint(distanceToTravel);
            if (distanceToTravel > 0.0f)
            {
                SetNextPoint();
            }
            if (iterations++ > 1)
            {
                throw new System.Exception("The lift " + name + " has two points too close. Review its speed or its points.");
            }
        }
        SetPosition(ref m_newPosition);
    }

    private bool CheckActivation()
    {
        if (!m_isActive && m_hasActivationStarted)
        {
            m_activationTimer -= Time.fixedDeltaTime;
            if (m_activationTimer <= 0.0f)
            {
                SetActive(true);
            }
        }
        else if (m_isActive && m_hasDeactivationStarted)
        {
            m_activationTimer -= Time.fixedDeltaTime;
            if (m_activationTimer <= 0.0f)
            {
                SetActive(false);
            }
        }

        return m_isActive;
    }
    
    private float TravelForwardCurrentNextPoint(float distanceToTravel)
    {
        float distanceTraveled = 0.0f;

        // Calculate direction and distance from current position to next point positon
        m_movement = (m_nextPoint - m_rigidbody.position);
        float distance = Vector2.Distance(m_nextPoint, m_rigidbody.position);
        // Distance that can travel in this update
        distanceTraveled = Mathf.Min(distance, distanceToTravel);
        float ratio = distance != 0.0f ? distanceTraveled / distance : 1.0f;
        m_newPosition = m_rigidbody.position + m_movement * ratio;

        return distanceToTravel - distanceTraveled;
    }

    private void SetNextPoint()
    {
        int nextPointIdx = -1;
        int lastPoint = m_isReturning ? 0 : m_pointsCount - 1;

        if (m_isActive || m_isLastCycle)
        {
            if (m_activationBehavior == LiftActivationBehavior.Cycle)
            {
                if (m_nextPointIdx == 0)
                {
                    if (!m_isLoop)
                    {
                        m_isLastCycle = false;
                        SetMoving(false);
                    }
                    else
                    {
                        nextPointIdx = 1;
                    }
                }
                else 
                {
                    nextPointIdx = m_nextPointIdx == lastPoint ? 0 : m_nextPointIdx + 1;
                }
            }
            else if (m_activationBehavior == LiftActivationBehavior.Go)
            {
                if (m_nextPointIdx == lastPoint)
                {
                    if (m_isLoop)
                    {
                        m_isReturning = !m_isReturning;
                        nextPointIdx = m_isReturning ? lastPoint - 1 : 1;
                    }
                    else
                    {
                        m_isLastCycle = false;
                        SetMoving(false);
                    }
                }
                else
                {
                    nextPointIdx = m_isReturning ? m_nextPointIdx - 1 : m_nextPointIdx + 1;
                }
            }
        }

        if (nextPointIdx < 0 && !m_isActive && m_isMoving)
        {
            if (m_activationBehavior == LiftActivationBehavior.Cycle)
            {
                if (m_nextPointIdx == 0)
                {
                    SetMoving(false);
                }
                else
                {
                    if (!m_isReturning)
                    {
                        nextPointIdx = m_nextPointIdx == lastPoint ? 0 : m_nextPointIdx + 1;
                    }
                    else
                    {
                        nextPointIdx = m_nextPointIdx - 1;
                    }
                }
            }
            else if (m_deactivationBehavior == LiftDeactivationBehavior.CompleteCycle)
            {

                if (m_nextPointIdx == lastPoint)
                {
                    SetMoving(false);
                }
                else
                {
                    nextPointIdx = m_isReturning ? m_nextPointIdx - 1 : m_nextPointIdx + 1;
                }
            }
            else if (m_deactivationBehavior == LiftDeactivationBehavior.CompleteCycleAndReturn)
            {
                if (m_nextPointIdx == lastPoint)
                {
                    if (!m_isReturning)
                    {
                        m_isReturning = true;
                        nextPointIdx = m_prevPointIdx;
                    }
                    else
                    {
                        SetMoving(false);
                    }
                }
                else
                {
                    nextPointIdx = m_isReturning ? m_nextPointIdx - 1 : m_nextPointIdx + 1;
                }
            }
            else if (m_nextPointIdx == lastPoint)
            {
                SetMoving(false);
                m_isReturning = false;
            }
            else
            {
                nextPointIdx = m_isReturning ? m_nextPointIdx - 1 : m_nextPointIdx + 1;
            }
        }

        if (m_isMoving)
        {
            SetNewPoint(nextPointIdx);
        }
    }

    private void SetNewPoint(int nextPointIdx)
    {
        m_prevPointIdx = m_nextPointIdx;
        m_nextPointIdx = nextPointIdx;
        m_nextPoint = m_points[m_nextPointIdx];
    }

    private void StartActivation()
    {
        m_hasDeactivationStarted = false;
        if (!m_isActive)
        {
            m_hasActivationStarted = true;
            m_activationTimer = m_timeToActive;
        }
    }

    private void StartDeactivation()
    {
        m_hasActivationStarted = false;
        if (m_isActive)
        {
            m_hasDeactivationStarted = true;
            m_activationTimer = m_timeToActive;
        }
    }

    private void SetActive(bool active)
    {
        m_hasActivationStarted = false;
        m_hasDeactivationStarted = false;
        m_isActive = active;
        if (active)
        {
            SetMoving(true);
        }
        else if (m_deactivationBehavior == LiftDeactivationBehavior.Nothing)
        {
            SetMoving(false);
        }
        else if (m_deactivationBehavior == LiftDeactivationBehavior.Return)
        {
            m_isReturning = true;
            SetNewPoint(m_prevPointIdx);
        }
        else if (m_deactivationBehavior == LiftDeactivationBehavior.CompleteCycle
            || m_deactivationBehavior == LiftDeactivationBehavior.CompleteCycleAndReturn)
        {
            m_isLastCycle = true;
        }
    }

    private void SetMoving(bool moving)
    {
        Debug.Log("Moving " + moving);
        m_isMoving = moving;
    }

    // Activation

    public void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (m_activationMode == LiftActivationMode.Player && collider2D.CompareTag("Player")
            && !m_hasActivationStarted && (!m_isActive || m_hasDeactivationStarted)
            && m_playerActivationArea.IsTouching(collider2D))
        {
            StartActivation();
        }
    }

    public void OnTriggerExit2D(Collider2D collider2D)
    {
        if (m_activationMode == LiftActivationMode.Player && collider2D.CompareTag("Player")
            && !m_hasDeactivationStarted && (m_isActive || m_hasActivationStarted)
            && !m_playerActivationArea.IsTouching(collider2D))
        {
            StartDeactivation();
        }
    }

    public void SetActiveByTrigger(bool active)
    {
        if (m_activationMode != LiftActivationMode.Trigger)
        {
            Debug.LogError("Activation Mode of lift " + name + " is not Trigger, so it cannot be activated externaly.");
            return;
        }
        StartActivation();
    }

}
