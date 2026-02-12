using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public partial class PerformanceAnalysisSystem : SystemBase
{
    private Text performanceText;
    private float deltaTime = 0.0f;
    private float fps = 0.0f;
    private int frameCount = 0;
    private EntityQuery zigzagQuery;

    private ProfilerRecorder systemTimeRecorder;
    private ProfilerRecorder jobTimeRecorder;
    private ProfilerRecorder gcMemoryRecorder;

    protected override void OnCreate()
    {
        zigzagQuery = GetEntityQuery(ComponentType.ReadOnly<ZigzagTag>());
        CreatePerformanceDisplay();

        systemTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "ZigzagMovementSystem", 30);
        jobTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "ZigzagMovementJob", 30);
        gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
    }

    protected override void OnDestroy()
    {
        systemTimeRecorder.Dispose();
        jobTimeRecorder.Dispose();
        gcMemoryRecorder.Dispose();
    }

    private void CreatePerformanceDisplay()
    {
        var canvasGO = new GameObject("PerformanceCanvas");
        canvasGO.transform.position = Vector3.zero;

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var textGO = new GameObject("PerformanceText");
        textGO.transform.SetParent(canvasGO.transform);

        performanceText = textGO.AddComponent<Text>();
        performanceText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        performanceText.fontSize = 18;
        performanceText.color = Color.white;
        performanceText.alignment = TextAnchor.UpperLeft;

        var rect = performanceText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(500, 300);
    }

    protected override void OnUpdate()
    {
        deltaTime += (UnityEngine.Time.unscaledDeltaTime - deltaTime) * 0.1f;
        frameCount++;

        if (frameCount % 30 == 0)
        {
            fps = 1.0f / deltaTime;
            UpdatePerformanceInfo();
            frameCount = 0;
        }
    }

    private void UpdatePerformanceInfo()
    {
        if (performanceText == null) return;

        int entityCount = zigzagQuery.CalculateEntityCount();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>⚡ ECS ZIGZAG PERFORMANCE ⚡</b>");
        sb.AppendLine($"<color=#00FF00>FPS: {fps:F1}</color> | Frame: {deltaTime * 1000:F2} ms");
        sb.AppendLine($"Entities: {entityCount} / 1000");
        sb.AppendLine();

        sb.AppendLine("<b>📊 JOB SYSTEM:</b>");

        if (systemTimeRecorder.Valid)
        {
            float systemTime = systemTimeRecorder.LastValue / 1e6f;
            sb.AppendLine($"System Time: {systemTime:F3} ms");
        }

        if (jobTimeRecorder.Valid)
        {
            float jobTime = 0;
            for (int i = 0; i < jobTimeRecorder.Capacity; i++)
            {
                jobTime += jobTimeRecorder.GetSample(i).Value / 1e6f;
            }
            jobTime /= jobTimeRecorder.Capacity;
            sb.AppendLine($"Job Time: {jobTime:F3} ms (avg)");
        }

        if (gcMemoryRecorder.Valid)
        {
            sb.AppendLine($"GC Memory: {gcMemoryRecorder.LastValue / 1048576:F1} MB");
        }

        sb.AppendLine();
        sb.AppendLine("<b>🎮 ZIGZAG SETTINGS:</b>");
        sb.AppendLine($"Speed: 3-7 | Amp: 1-4 | Freq: 2-5");
        sb.AppendLine($"Burst: Enabled | Parallel: Yes");

        sb.AppendLine();
        sb.AppendLine("<b>📈 BUILD ANALYSIS:</b>");
        sb.AppendLine($"Platform: {Application.platform}");
        sb.AppendLine($"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");

        performanceText.text = sb.ToString();
    }
}