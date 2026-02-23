using UnityEngine;

/// <summary>
/// Небольшая плавная тряска камеры для главного меню.
/// </summary>
public class MainMenuCameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float intensity = 0.08f;
    [SerializeField] private float speed = 2.5f;

    private Vector3 basePosition;

    private void Awake()
    {
        basePosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        float time = Time.unscaledTime * speed;
        float x = (Mathf.PerlinNoise(time, 0) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(0, time) - 0.5f) * 2f;

        transform.localPosition = basePosition + new Vector3(x, y, 0) * intensity;
    }
}
