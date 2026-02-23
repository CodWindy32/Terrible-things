using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Эффект дыма/газа вдоль плоскости. Настраиваемый цвет, плотность, скорость и другие параметры.
/// Добавьте на Plane или объект с MeshRenderer — эффект автоматически подстроится под размер.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class PlaneSmokeEffect : MonoBehaviour
{
    [Header("Внешний вид")]
    [Tooltip("Цвет дыма/газа")]
    [SerializeField] private Color smokeColor = new Color(0.6f, 0.6f, 0.65f, 0.4f);
    [Tooltip("Размер частиц")]
    [SerializeField] private Vector2 particleSize = new Vector2(0.8f, 2.2f);
    [Tooltip("Материал для частиц (оставьте пустым для автосоздания)")]
    [SerializeField] private Material particleMaterial;
    [Tooltip("Текстура дыма (опционально, по умолчанию — мягкий круг)")]
    [SerializeField] private Texture2D smokeTexture;

    [Header("Движение")]
    [Tooltip("Скорость течения дыма вдоль плоскости")]
    [SerializeField] private float flowSpeed = 1.5f;
    [Tooltip("Направление потока (локальные оси)")]
    [SerializeField] private Vector3 flowDirection = Vector3.forward;
    [Tooltip("Случайное отклонение движения")]
    [SerializeField] private float turbulence = 0.5f;

    [Header("Эмиссия")]
    [Tooltip("Количество частиц в секунду")]
    [SerializeField] private float emissionRate = 120f;
    [Tooltip("Время жизни частицы")]
    [SerializeField] private float particleLifetime = 6f;
    [Tooltip("Высота подъёма дыма над плоскостью")]
    [SerializeField] private float riseHeight = 0.5f;

    [Header("Прозрачность")]
    [Tooltip("Начальная прозрачность")]
    [Range(0f, 1f)] [SerializeField] private float startAlpha = 0.65f;
    [Tooltip("Конечная прозрачность (при исчезновении)")]
    [Range(0f, 1f)] [SerializeField] private float endAlpha = 0f;

    private ParticleSystem particleSystem;
    private ParticleSystemRenderer particleRenderer;
    private Texture2D proceduralSmokeTexture;

    private void Awake()
    {
        SetupParticleSystem();
    }

    private void SetupParticleSystem()
    {
        var psTransform = transform.Find("SmokeEffect");
        if (psTransform != null)
        {
            particleSystem = psTransform.GetComponent<ParticleSystem>();
            particleRenderer = psTransform.GetComponent<ParticleSystemRenderer>();
        }

        if (particleSystem == null)
        {
            var go = new GameObject("SmokeEffect");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            particleSystem = go.AddComponent<ParticleSystem>();
            particleRenderer = go.GetComponent<ParticleSystemRenderer>();
        }

        ApplySettings();
    }

    private void ApplySettings()
    {
        if (particleSystem == null) return;

        var main = particleSystem.main;
        main.loop = true;
        main.startLifetime = particleLifetime;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize.x, particleSize.y);
        main.startColor = smokeColor;
        main.gravityModifier = 0f;
        main.maxParticles = 2000;

        var emission = particleSystem.emission;
        emission.rateOverTime = emissionRate;
        emission.enabled = true;

        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        Bounds bounds = GetPlaneBounds();
        shape.scale = new Vector3(bounds.size.x, 0.01f, bounds.size.z);
        shape.position = new Vector3(0, 0.01f, 0);

        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        Vector3 dir = flowDirection.normalized * flowSpeed;
        float riseSpeed = riseHeight / particleLifetime;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(dir.x - turbulence, dir.x + turbulence);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(riseSpeed - turbulence * 0.5f, riseSpeed + turbulence);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(dir.z - turbulence, dir.z + turbulence);

        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(smokeColor, 0f), new GradientColorKey(smokeColor, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(startAlpha * 0.5f, 0.5f),
                new GradientAlphaKey(endAlpha, 1f)
            });
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.6f),
            new Keyframe(0.5f, 1.2f),
            new Keyframe(1f, 2f)));

        var rendererModule = particleSystem.GetComponent<ParticleSystemRenderer>();
        rendererModule.renderMode = ParticleSystemRenderMode.Billboard;
        rendererModule.material = GetOrCreateMaterial();
        rendererModule.sortingFudge = 0.5f;

        particleSystem.Clear();
        particleSystem.Play();
    }

    private Bounds GetPlaneBounds()
    {
        var rend = GetComponent<Renderer>();
        if (rend != null && rend.bounds.size.sqrMagnitude > 0.01f)
        {
            Vector3 localSize = new Vector3(
                rend.bounds.size.x / Mathf.Max(0.001f, transform.lossyScale.x),
                rend.bounds.size.y / Mathf.Max(0.001f, transform.lossyScale.y),
                rend.bounds.size.z / Mathf.Max(0.001f, transform.lossyScale.z));
            return new Bounds(Vector3.zero, localSize);
        }

        var filter = GetComponent<MeshFilter>();
        if (filter != null && filter.sharedMesh != null)
            return filter.sharedMesh.bounds;

        return new Bounds(Vector3.zero, new Vector3(5f, 0.1f, 5f));
    }

    private Texture2D CreateSoftSmokeTexture()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDist = center.magnitude;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                // Мягкий градиент: центр белый, края прозрачные
                float alpha = 1f - Mathf.SmoothStep(0.3f, 1f, dist);
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    private Material GetOrCreateMaterial()
    {
        if (particleMaterial != null) return particleMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
            ?? Shader.Find("Particles/Standard Unlit")
            ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        if (shader == null)
        {
            Debug.LogWarning("PlaneSmokeEffect: Не найден подходящий шейдер для частиц. Используется Sprites-Default.");
            shader = Shader.Find("Sprites/Default");
        }

        if (shader != null)
        {
            particleMaterial = new Material(shader);
            particleMaterial.SetColor("_BaseColor", smokeColor);
            particleMaterial.SetColor("_Color", smokeColor);

            Texture2D tex = smokeTexture;
            if (tex == null)
            {
                proceduralSmokeTexture = CreateSoftSmokeTexture();
                tex = proceduralSmokeTexture;
            }
            if (tex != null)
            {
                particleMaterial.SetTexture("_BaseMap", tex);
                particleMaterial.SetTexture("_BaseColorMap", tex);
            }

            particleMaterial.renderQueue = 3000;
            particleMaterial.SetInt("_Surface", 1);
            particleMaterial.SetInt("_Blend", 0);
            particleMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            particleMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            particleMaterial.SetInt("_ZWrite", 0);
            particleMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

            return particleMaterial;
        }

        return null;
    }

    private void OnDestroy()
    {
        if (proceduralSmokeTexture != null)
        {
            Destroy(proceduralSmokeTexture);
            proceduralSmokeTexture = null;
        }
    }

    private void OnValidate()
    {
        if (particleSystem != null && Application.isPlaying)
            ApplySettings();
    }

#if UNITY_EDITOR
    [ContextMenu("Обновить настройки")]
    private void RefreshInEditor()
    {
        if (particleSystem != null)
            ApplySettings();
    }
#endif
}
