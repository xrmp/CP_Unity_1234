using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public partial class ArmyPerformanceSystem : SystemBase
{
    [SerializeField] private Text performanceText;
    private float deltaTime = 0.0f;
    private float fps = 0.0f;
    private int frameCount = 0;
    private EntityQuery armyQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        armyQuery = GetEntityQuery(ComponentType.ReadOnly<ArmyTag>());
        CreatePerformanceDisplay();
    }

    private void CreatePerformanceDisplay()
    {

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

        int entityCount = armyQuery.CalculateEntityCount();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>=== ECS ARMY PERFORMANCE ===</b>");
        sb.AppendLine($"FPS: {fps:F1} | Frame: {deltaTime * 1000:F2} ms");
        sb.AppendLine($"Entities: {entityCount:N0}");
        sb.AppendLine($"System: Unity Entities 1.0.16+");
        sb.AppendLine($"Movement: IJobEntity + Burst");
        sb.AppendLine($"<b>===========================</b>");

        performanceText.text = sb.ToString();
    }
}