using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    // public
    public GameObject m_playerPrefab;
    public BoxCollider2D m_cameraBoundaries;
    public Transform m_spawnPoint;
    public BoxCollider2D m_endPoint;
    public Camera m_levelCamera;

    public bool m_isDebugModeOn = false;

    // private
    private SceneMaster m_sceneMaster;
    private CameraController m_cameraController;
    private PlayerController m_playerController;
    private GameObject m_playerObject;

    private bool m_isActive;
    
    void Start()
    {
        m_isActive = true;

        if (!m_isDebugModeOn)
        {
            m_sceneMaster = FindObjectOfType<SceneMaster>();
            if (m_sceneMaster == null)
            {
                throw new System.Exception("Scene Master not found.");
            }
        }

        // Spawn Point
        if (m_spawnPoint == null)
        {
            throw new System.Exception("Spawn Point not set.");
        }

        // End Point
        if (m_endPoint == null)
        {
            throw new System.Exception("End Point not set.");
        }
        if (!m_endPoint.isTrigger)
        {
            throw new System.Exception("End Point's BoxCollider2D is not set as trigger.");
        }

        // Player
        if (m_playerPrefab == null)
        {
            throw new System.Exception("Player Prefab not set.");
        }
        m_playerObject = Instantiate(m_playerPrefab, m_spawnPoint);

        if (m_playerObject == null)
        {
            throw new System.Exception("Player Prefab couldn't be instantiated.");
        }
        m_playerController = m_playerObject.GetComponent<PlayerController>();
        if (m_playerController == null)
        {
            throw new System.Exception("PlayerController not found in Player Prefab instance.");
        }
        m_playerController.m_levelManager = this;
        
        // Camera
        if (m_levelCamera == null)
        {
            throw new System.Exception("Level Camera not found.");
        }
        m_cameraController = m_levelCamera.GetComponent<CameraController>();
        if (m_cameraController == null)
        {
            throw new System.Exception("Camera Controller not found in Level Camera.");
        }

        // Boundaries
        if (m_cameraBoundaries == null)
        {
            throw new System.Exception("Camera Boundaries not set.");
        }

        // Set up Level Camera with Player gameObject and Camera Boundaries
        m_cameraController.m_target = m_playerObject;
        m_cameraController.m_boundaries = m_cameraBoundaries;
        m_cameraController.SetUp();
    }

    public void PlayerEnteredInAHole()
    {
        m_playerController.ChangePosition(m_spawnPoint.position, true);
    }

    public void PlayerEnteredEndPoint()
    {
        if (!m_isActive || m_isDebugModeOn)
        {
            return;
        }

        m_sceneMaster.ExitLevelAndFade();
        m_isActive = false;
    }

}
	
