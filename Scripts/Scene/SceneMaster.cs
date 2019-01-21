using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMaster : MonoBehaviour {

    // public
    public string m_titleSceneName;

    public CanvasGroup m_faderLayer;
    public float m_fadeInDuration;
    public float m_fadeOutDuration;

    public Camera m_masterCamera;

    // private
    //private Scene m_masterScene;
    private bool m_isFading;

    void Awake()
    {
        //m_masterScene = SceneManager.GetActiveScene();
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(LoadTitleScene());
    }
    
    public void LoadLevelAndFade(string levelName)
    {
        StartCoroutine(SwitchSceneAndFade(levelName));
    }

    public void ExitLevelAndFade()
    {
        StartCoroutine(SwitchSceneAndFade(m_titleSceneName));
    }
    
    private IEnumerator LoadTitleScene()
    {
        yield return StartCoroutine(Fade(true, true));
        yield return LoadSceneAndSetActive(m_titleSceneName);
        yield return StartCoroutine(Fade(false));
    }

    private IEnumerator SwitchSceneAndFade(string sceneName)
    {
        yield return StartCoroutine(Fade(true));
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        yield return LoadSceneAndSetActive(sceneName);
        yield return StartCoroutine(Fade(false));
    }

    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        if (!m_isFading)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            Scene newSceneLoaded = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(newSceneLoaded);
        }
    }

    private IEnumerator Fade(bool active, bool immediately = false)
    {
        m_isFading = true;
        m_faderLayer.blocksRaycasts = true;

        if (!active)
        {
            if (m_fadeOutDuration == 0.0f || immediately)
            {
                m_faderLayer.alpha = 0.0f;
            }
            else
            {
                float fadeOutSpeed = (m_faderLayer.alpha) / m_fadeOutDuration;
                while (!Mathf.Approximately(m_faderLayer.alpha, 0.0f))
                {
                    m_faderLayer.alpha = Mathf.MoveTowards(m_faderLayer.alpha, 0.0f, fadeOutSpeed * Time.deltaTime);
                    yield return null;
                }
            }
        }
        else
        {
            if (m_fadeInDuration == 0.0f || immediately)
            {
                m_faderLayer.alpha = 1.0f;
            }
            else
            {
                float fadeInSpeed = (1.0f - m_faderLayer.alpha) / m_fadeInDuration;
                while (!Mathf.Approximately(m_faderLayer.alpha, 1.0f))
                {
                    m_faderLayer.alpha = Mathf.MoveTowards(m_faderLayer.alpha, 1.0f, fadeInSpeed * Time.deltaTime);
                    yield return null;
                }
            }
        }

        m_isFading = false;
        m_faderLayer.blocksRaycasts = false;
    }

}
