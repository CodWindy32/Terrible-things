using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Компонент для создания эффекта размытия фона при открытии меню паузы.
/// Использует URP Post-Processing с Depth of Field.
/// ВАЖНО: Вручную добавьте Depth of Field override в Volume Profile!
/// </summary>
[RequireComponent(typeof(Volume))]
public class BlurEffect : MonoBehaviour
{
    [Header("Blur Settings")]
    [SerializeField] private float transitionSpeed = 20f;
    
    [Header("Depth of Field Settings")]
    [Tooltip("Максимальная апертура для blur эффекта (рекомендуется 5-15)")]
    [SerializeField] private float maxAperture = 10f;

    private Volume volume;
    private DepthOfField depthOfField;
    private float currentWeight = 0f;
    private float targetWeight = 0f;

    private void Awake()
    {
        volume = GetComponent<Volume>();

        if (volume == null)
        {
            Debug.LogError("Volume компонент не найден!");
            return;
        }

        var profile = volume.profile != null ? volume.profile : volume.sharedProfile;
        if (profile != null && profile.TryGet(out depthOfField))
        {
            depthOfField.active = false;
        }
        else
        {
            Debug.LogWarning("Depth of Field не найден в Volume Profile! Добавьте его вручную через Inspector.");
        }

        volume.weight = 0f;
    }

    private void Update()
    {
        if (Mathf.Abs(currentWeight - targetWeight) > 0.001f)
        {
            currentWeight = Mathf.Lerp(currentWeight, targetWeight, Time.unscaledDeltaTime * transitionSpeed);
            
            if (volume != null)
                volume.weight = currentWeight;

            if (depthOfField != null && depthOfField.active)
            {
                float apertureValue = currentWeight * maxAperture;
                depthOfField.aperture.value = apertureValue;
            }
        }
        else if (Mathf.Abs(currentWeight - targetWeight) > 0f)
        {
            currentWeight = targetWeight;
            
            if (volume != null)
                volume.weight = currentWeight;

            if (depthOfField != null && depthOfField.active)
                depthOfField.aperture.value = currentWeight * maxAperture;
        }
    }

    public void EnableBlur()
    {
        if (depthOfField != null)
            depthOfField.active = true;
        
        targetWeight = 1f;
    }

    public void DisableBlur()
    {
        targetWeight = 0f;
        
        if (currentWeight < 0.01f && depthOfField != null)
            depthOfField.active = false;
    }
}

