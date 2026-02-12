using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Default)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ArmyGizmosSystem : SystemBase
{
    private bool showGizmos = true;
    private int maxGizmosToShow = 1000;

    protected override void OnCreate()
    {
        Enabled = false;

#if UNITY_EDITOR
        Enabled = true;
#endif
    }

    protected override void OnUpdate()
    {
        if (!showGizmos) return;

#if UNITY_EDITOR
        int maxCount = maxGizmosToShow; // копируем в локальную переменную

        Entities
            .WithAll<ArmyTag>()
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, false) // false = отключаем Burst
            .ForEach((in LocalTransform transform,
                     in CenterPosition center,
                     in Entity entity) =>
            {
                // Используем локальную переменную, не поле класса
                if (entity.Index >= maxCount) return;

                var position = transform.Position;
                var centerPos = center.Value;
                var radius = math.length(position - centerPos);

                // Путь движения (круг)
                UnityEditor.Handles.color = new Color(0, 1, 0, 0.2f);
                UnityEditor.Handles.DrawWireDisc(centerPos, Vector3.up, radius);

                // Центр вращения
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.SphereHandleCap(0, centerPos, Quaternion.identity, 0.2f, EventType.Repaint);

                // Текущая позиция
                UnityEditor.Handles.color = Color.blue;
                UnityEditor.Handles.SphereHandleCap(0, position, Quaternion.identity, 0.15f, EventType.Repaint);

                // Линия от центра к объекту
                UnityEditor.Handles.color = new Color(1, 1, 0, 0.3f);
                UnityEditor.Handles.DrawLine(centerPos, position);

            }).Run(); // Run() для выполнения в главном потоке
#endif
    }
}