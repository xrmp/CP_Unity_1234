using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Default)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ArmyInspectorSystem : SystemBase
{
    private float moveSpeed = 2f;
    private float radius = 5f;
    private int entityCount = 50000;
    private Vector2 scrollPosition;
    private bool showSettings = true;
    private bool showStats = true;

    protected override void OnUpdate()
    {
        entityCount = SystemAPI.QueryBuilder().WithAll<ArmyTag>().Build().CalculateEntityCount();
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUI.skin.box.fontSize = 12;
        GUI.skin.label.fontSize = 12;
        GUI.skin.button.fontSize = 12;

        // Правая панель инспектора
        GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 300, 400), GUI.skin.box);

        // Заголовок
        GUI.color = Color.cyan;
        GUILayout.Label("<b>⚡ ECS ARMY INSPECTOR ⚡</b>", GUI.skin.label);
        GUI.color = Color.white;
        GUILayout.Space(10);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // Настройки движения
        showSettings = GUILayout.Toggle(showSettings, "MOVEMENT SETTINGS", GUI.skin.button);
        if (showSettings)
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("Move Speed:");
            moveSpeed = GUILayout.HorizontalSlider(moveSpeed, 0.5f, 5f);
            GUILayout.Label($"Current: {moveSpeed:F2}");

            GUILayout.Space(5);

            GUILayout.Label("Rotation Radius:");
            radius = GUILayout.HorizontalSlider(radius, 2f, 10f);
            GUILayout.Label($"Current: {radius:F2}");

            GUILayout.Space(10);

            if (GUILayout.Button("APPLY SETTINGS"))
            {
                ApplySettingsToAll();
            }

            GUILayout.EndVertical();
        }

        GUILayout.Space(10);

        // Статистика
        showStats = GUILayout.Toggle(showStats, "PERFORMANCE STATS", GUI.skin.button);
        if (showStats)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Total Entities: {entityCount:N0}");
            GUILayout.Label($"Active Systems: 3");
            GUILayout.Label($"Memory: ~{(entityCount * 128) / 1048576:F1} MB");
            GUILayout.Label($"Draw Calls: ~{entityCount / 5000 * 10}");
            GUILayout.EndVertical();
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void ApplySettingsToAll()
    {
        foreach (var (speed, radiusComponent, entity) in
                 SystemAPI.Query<RefRW<MoveSpeed>, RefRW<Radius>>()
                 .WithEntityAccess())
        {
            speed.ValueRW.Value = moveSpeed;
            radiusComponent.ValueRW.Value = this.radius;
        }
    }
#endif
}