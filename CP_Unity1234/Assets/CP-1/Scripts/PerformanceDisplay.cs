using UnityEngine;
using UnityEngine.UI;
using Unity.Jobs;
using System.Text;

public class PerformanceDisplay : MonoBehaviour
{
    [SerializeField] private Text performanceText;
    private SpawnManager spawnManager;
    private float deltaTime = 0.0f;
    private int frameCount = 0;
    private float fps = 0.0f;
    private float updateRate = 0.5f; // обновление 2 раза в секунду

    private void Start()
    {
        spawnManager = FindAnyObjectByType<SpawnManager>(FindObjectsInactive.Exclude);
        if (performanceText == null)
        {
            // Создаем Canvas и Text автоматически
            CreatePerformanceDisplay();
        }
    }

    private void CreatePerformanceDisplay()
    {
        GameObject canvasGO = new GameObject("PerformanceCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject textGO = new GameObject("PerformanceText");
        textGO.transform.SetParent(canvasGO.transform);

        Text text = textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;

        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -10);
        rect.sizeDelta = new Vector2(600, 200);

        performanceText = text;
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        frameCount++;

        if (Time.time % updateRate < Time.deltaTime)
        {
            fps = 1.0f / deltaTime;
            UpdatePerformanceInfo();
            frameCount = 0;
        }
    }

    private void UpdatePerformanceInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<b>=== PERFORMANCE METRICS ===</b>");
        sb.AppendLine($"FPS: {fps:F1}");
        sb.AppendLine($"Frame Time: {deltaTime * 1000:F2} ms");
        sb.AppendLine($"Objects Count: {spawnManager?.GetComponent<SpawnManager>()?.GetObjectCount() ?? 0}");
        sb.AppendLine($"System: Unity 2022.3 LTS");
        sb.AppendLine($"Job System: IJobParallelForTransform Active");
        sb.AppendLine($"<b>===========================</b>");

        if (performanceText != null)
            performanceText.text = sb.ToString();
    }
}