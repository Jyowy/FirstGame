using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour {

    // public

    public Button[] m_buttons = new Button[0];
    public string[] m_sceneNames = new string[0];

    // private

    private SceneMaster m_sceneMaster;
    private bool m_isActive;

    
    void Start()
    {
        m_isActive = true;

        if (m_buttons.Length != m_sceneNames.Length)
        {
            throw new System.Exception("There are different number of buttons and scenes related.");
        }

        for (int i = 0; i < m_buttons.Length; ++i)
        {
            Button button = m_buttons[i];
            m_buttons[i].onClick.AddListener(() => MapButtonPressed(button));
        }

        m_sceneMaster = FindObjectOfType<SceneMaster>();
        if (m_sceneMaster == null)
        {
            throw new System.Exception("Scene Master not found.");
        }
    }

    void MapButtonPressed(Button button)
    {
        if (!m_isActive)
        {
            return;
        }

        for (int i = 0; i < m_buttons.Length; ++i)
        {
            if (m_buttons[i] == button)
            {
                if (m_sceneNames[i].Length == 0)
                {
                    Debug.Log("No scene related to button " + button.name);
                    return;
                }
                m_sceneMaster.LoadLevelAndFade(m_sceneNames[i]);
                m_isActive = false;
                return;
            }
        }
    }
    
}
