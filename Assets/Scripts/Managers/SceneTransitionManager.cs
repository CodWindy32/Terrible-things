using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Синглтон для управления переходами между сценами с fade эффектами.
/// Обеспечивает плавные переходы и асинхронную загрузку.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;
    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("SceneTransitionManager");
                instance = go.AddComponent<SceneTransitionManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    private UIDocument fadeDocument;
    private VisualElement fadeOverlay;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFadeOverlay();
    }

    private void InitializeFadeOverlay()
    {
        fadeDocument = gameObject.AddComponent<UIDocument>();
        
        PanelSettings panelSettings = null;
        
#if UNITY_EDITOR
        // В редакторе загружаем существующий PanelSettings
        string[] guids = AssetDatabase.FindAssets("t:PanelSettings");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var existingSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            
            if (existingSettings != null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.themeStyleSheet = existingSettings.themeStyleSheet;
                panelSettings.targetTexture = null;
                panelSettings.scaleMode = existingSettings.scaleMode;
                panelSettings.scale = existingSettings.scale;
                panelSettings.sortingOrder = 9999;
            }
        }
#endif
        
        // Если не удалось загрузить, создаем минимальный
        if (panelSettings == null)
        {
            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.targetTexture = null;
            panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
            panelSettings.scale = 1.0f;
            panelSettings.sortingOrder = 9999;
        }
        
        fadeDocument.panelSettings = panelSettings;
        fadeDocument.sortingOrder = 9999;
        
        // Ждем один кадр, чтобы UIDocument инициализировался
        StartCoroutine(InitializeOverlayDelayed());
    }

    private IEnumerator InitializeOverlayDelayed()
    {
        yield return null; // Ждем один кадр
        
        var root = fadeDocument.rootVisualElement;
        
        fadeOverlay = new VisualElement();
        fadeOverlay.name = "FadeOverlay";
        
        // Используем inline стили для гарантированного позиционирования
        fadeOverlay.style.position = Position.Absolute;
        fadeOverlay.style.left = 0;
        fadeOverlay.style.top = 0;
        fadeOverlay.style.right = 0;
        fadeOverlay.style.bottom = 0;
        fadeOverlay.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        fadeOverlay.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        fadeOverlay.style.backgroundColor = new Color(0, 0, 0, 0);
        
        // Убеждаемся, что элемент будет отображаться поверх всего
        fadeOverlay.pickingMode = PickingMode.Ignore;
        
        root.Add(fadeOverlay);
        
        fadeOverlay.style.display = DisplayStyle.None;
    }

    public void LoadScene(string sceneName)
    {
        if (!isTransitioning)
            StartCoroutine(TransitionToScene(sceneName));
    }

    public void LoadScene(int sceneIndex)
    {
        if (!isTransitioning)
            StartCoroutine(TransitionToScene(sceneIndex));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeOut());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    private IEnumerator TransitionToScene(int sceneIndex)
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeOut());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(FadeIn());

        isTransitioning = false;
    }

    private IEnumerator FadeOut()
    {
        while (fadeOverlay == null)
        {
            yield return null;
        }

        fadeOverlay.style.display = DisplayStyle.Flex;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float alpha = EaseInOutCubic(t);
            fadeOverlay.style.backgroundColor = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeOverlay.style.backgroundColor = new Color(0, 0, 0, 1);
    }

    private IEnumerator FadeIn()
    {
        if (fadeOverlay == null)
            yield break;

        fadeOverlay.style.backgroundColor = new Color(0, 0, 0, 1);
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float alpha = 1f - EaseInOutCubic(t);
            fadeOverlay.style.backgroundColor = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeOverlay.style.backgroundColor = new Color(0, 0, 0, 0);
        fadeOverlay.style.display = DisplayStyle.None;
    }

    public void SetFadeDuration(float duration)
    {
        fadeDuration = duration;
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}
