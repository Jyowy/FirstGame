using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // public

    [HideInInspector]
    public GameObject m_target;
    [HideInInspector]
    public BoxCollider2D m_boundaries;

    public float m_cameraSpeed = 0.2f;

    // private

    private Camera m_camera;
    private Vector2 m_position = new Vector2();
    private Vector2 m_targetPosition = new Vector2();
    private Vector2 m_movement = new Vector2();
    private Vector3 m_newPosition = new Vector3();
    private Vector2 m_size = new Vector2();
    private float m_leftLimit;
    private float m_rightLimit;
    private float m_upLimit;
    private float m_botLimit;
    private float m_cameraDistance;

    //private const float k_cameraSpeed = 10.0f;

    void Awake()
    {
        m_camera = GetComponent<Camera>();
        if (m_camera == null)
        {
            throw new System.Exception("CameraController script should only be attached to a camera object.");
        }
        //SetUp();
    }

    public void SetUp()
    {
        m_size.y = m_camera.orthographicSize * 2.0f;
        m_size.x = m_size.y * m_camera.aspect;

        float x = m_boundaries.transform.position.x;
        float y = m_boundaries.transform.position.y;
        float w = m_boundaries.transform.localScale.x;
        float h = m_boundaries.transform.localScale.y;
        m_leftLimit = x - w * 0.5f + m_size.x * 0.5f;
        m_rightLimit = x + w * 0.5f - m_size.x * 0.5f;
        m_upLimit = y + h * 0.5f - m_size.y * 0.5f;
        m_botLimit = y - h * 0.5f + m_size.y * 0.5f;

        //Debug.Log("Bounds position: " + m_boundaries.transform.position + "; bounds size: " + m_boundaries.transform.localScale);
        //Debug.Log("Limits: " + m_leftLimit + ", " + m_rightLimit + ", " + m_upLimit + ", " + m_botLimit);
        
        m_position = transform.position;
        m_cameraDistance = transform.position.z;
        m_targetPosition = m_target.transform.position;
    }

    void Update()
    {
        // Retrieve player's current position
        m_position = transform.position;
        m_targetPosition = m_target.transform.position;

        // Calculate new position for camera
        CheckCameraLimits();

        // Move the camera smoothly to the target
        //m_movement = m_targetPosition - m_position;
        //float distance = Vector2.Distance(m_position, m_targetPosition);
        //float relDistance = distance > m_dampTime ? 1.0f : distance / m_dampTime;
        //m_newPosition = m_position + m_movement * relDistance;
        //m_newPosition.z = m_cameraDistance;
        //transform.position = m_newPosition;

        m_movement = m_targetPosition - m_position;
        float distance = Vector2.Distance(m_position, m_targetPosition);
        float cameraDistance = m_cameraSpeed * Time.deltaTime;
        float distanceRate = cameraDistance > distance ? 1.0f : cameraDistance / distance;
        m_newPosition = m_position + m_movement * distanceRate;
        m_newPosition.z = m_cameraDistance;

        transform.position = m_newPosition;
    }

    void CheckCameraLimits()
    {
        // Check 4 borders
        // Left
        if (m_targetPosition.x < m_leftLimit)
        {
            m_targetPosition.x = m_leftLimit;
        }
        // Right
        if (m_targetPosition.x > m_rightLimit)
        {
            m_targetPosition.x = m_rightLimit;
        }
        // Top
        if (m_targetPosition.y > m_upLimit)
        {
            m_targetPosition.y = m_upLimit;
        }
        // Down
        if (m_targetPosition.y < m_botLimit)
        {
            m_targetPosition.y = m_botLimit;
        }
    }

}
