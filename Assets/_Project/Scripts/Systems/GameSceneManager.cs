using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    
    public static GameSceneManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string tutorialScene = "Tutorial";
    [SerializeField] private string[] missionScenes;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    private bool _isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (fadeCanvasGroup == null)
        {
            GameObject fadePanel = GameObject.Find("FadePanel");
            if (fadePanel != null) fadeCanvasGroup = fadePanel.GetComponent<CanvasGroup>();
        }
    }

    public void LoadTutorial()
    {
        LoadScene(tutorialScene);
    }

    public void LoadMission(int missionIndex)
    {
        if (missionIndex < missionScenes.Length) LoadScene(missionScenes[missionIndex]);
        else Debug.LogError($"Mission {missionIndex} does not exist!");
    }

    public void LoadMainMenu()
    {
        LoadScene(mainMenuScene);
    }

    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings) LoadSceneByIndex(nextIndex);
        else Debug.LogError("No more scenes to load!");
    }

    private void LoadScene(string sceneName)
    {
        if (!_isTransitioning) StartCoroutine(TransitionToScene(sceneName));
    }

    private void LoadSceneByIndex(int index)
    {
        if (!_isTransitioning) StartCoroutine(TransitionToSceneByIndex(index));
    }

    private IEnumerator TransitionToScene(String sceneName)
    {
        _isTransitioning = true;

        // Fade out
        yield return StartCoroutine(Fade(1f));

        // Reset checkpoint for new level
        if (GameManager.Instance != null) GameManager.Instance.ResetCheckpoint();

        SceneManager.LoadScene(sceneName);

        // Fade in 
        yield return StartCoroutine(Fade(0f));

        _isTransitioning = false;
    }

    private IEnumerator TransitionToSceneByIndex(int index)
    {
        _isTransitioning = true;

        yield return StartCoroutine(Fade(1f));

        if (GameManager.Instance != null) GameManager.Instance.ResetCheckpoint();

        SceneManager.LoadScene(index);

        yield return StartCoroutine(Fade(0f));

        _isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / transitionDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
