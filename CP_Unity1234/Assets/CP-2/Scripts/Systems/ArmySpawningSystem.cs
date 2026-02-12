using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public partial struct ArmySpawningSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<ArmySettingsComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var settings = SystemAPI.GetSingleton<ArmySettingsComponent>();

        int totalCount = settings.GridWidth * settings.GridHeight;
        var instances = new NativeArray<Entity>(totalCount, Allocator.Temp);
        ecb.Instantiate(settings.Prefab, instances);

        // Центр сетки
        float3 gridCenter = new float3(
            -settings.GridWidth * settings.Spacing / 2,
            0,
            -settings.GridHeight * settings.Spacing / 2
        );

        // Устанавливаем позиции
        int index = 0;
        for (int x = 0; x < settings.GridWidth; x++)
        {
            for (int y = 0; y < settings.GridHeight; y++)
            {
                if (index >= instances.Length) break;

                var entity = instances[index];
                float3 position = gridCenter + new float3(x * settings.Spacing, 0, y * settings.Spacing);

                ecb.SetComponent(entity, LocalTransform.FromPosition(position));
                ecb.SetComponent(entity, new CenterPosition { Value = position });
                ecb.SetComponent(entity, new InitialPosition { Value = position });

                index++;
            }
        }

        state.Enabled = false;
    }
}