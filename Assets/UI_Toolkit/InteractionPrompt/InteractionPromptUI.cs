using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Система подсказок для взаимодействия на UI Toolkit.
/// Отображает подсказки типа "Нажмите [F] чтобы..." в хоррор стиле.
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

    private VisualElement promptContainer;
    private Label keyLabel;
    private Label actionLabel;
    private bool isVisible;

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("UIDocument не найден! Добавьте компонент UIDocument к объекту.");
            return;
        }

        InitializeUI();
    }

    private void InitializeUI()
    {
        var root = uiDocument.rootVisualElement;

        promptContainer = root.Q<VisualElement>("PromptContainer");
        keyLabel = root.Q<Label>("KeyLabel");
        actionLabel = root.Q<Label>("ActionLabel");

        Hide();
    }

    /// <summary>
    /// Показывает подсказку с указанным текстом.
    /// </summary>
    /// <param name="message">Текст подсказки (например, "Взобраться по лестнице")</param>
    /// <param name="key">Клавиша для отображения (по умолчанию "F")</param>
    public void Show(string message, string key = "F")
    {
        if (promptContainer == null) return;

        if (keyLabel != null)
            keyLabel.text = $"[{key}]";

        if (actionLabel != null)
            actionLabel.text = message;

        promptContainer.RemoveFromClassList("hidden");
        isVisible = true;
    }

    /// <summary>
    /// Скрывает подсказку.
    /// </summary>
    public void Hide()
    {
        if (promptContainer == null) return;

        promptContainer.AddToClassList("hidden");
        isVisible = false;
    }

    /// <summary>
    /// Проверяет, видна ли подсказка в данный момент.
    /// </summary>
    public bool IsVisible => isVisible;
}
