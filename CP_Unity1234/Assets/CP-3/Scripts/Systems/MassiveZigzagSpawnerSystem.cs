using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public partial struct MassiveZigzagSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<ZigzagAuthoring>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var prefabQuery = SystemAPI.QueryBuilder().WithAll<ZigzagAuthoring>().Build();
        var prefab = prefabQuery.GetSingletonEntity();

        // Спавним 1000 мячей
        int count = 1000;
        var instances = new NativeArray<Entity>(count, Allocator.Temp);
        ecb.Instantiate(prefab, instances);

        // Располагаем их сеткой 40x25
        int gridWidth = 40;
        int gridHeight = 25;
        float spacing = 2.5f;
        float3 startCenter = new float3(-gridWidth * spacing / 2, 1, -20);

        int index = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (index >= instances.Length) break;

                var entity = instances[index];
                float3 position = startCenter + new float3(x * spacing, 0, y * spacing * 0.5f);

                ecb.SetComponent(entity, LocalTransform.FromPosition(position));
                ecb.SetComponent(entity, new ZigzagComponent
                {
                    StartPosition = position,
                    Speed = UnityEngine.Random.Range(3f, 7f),
                    Amplitude = UnityEngine.Random.Range(1f, 4f),
                    Frequency = UnityEngine.Random.Range(2f, 5f),
                    TimeOffset = UnityEngine.Random.Range(0f, 100f)
                });

                index++;
            }
        }

        // Логируем результат
        Debug.Log($"[ECS] Spawned {count} zigzag balls");

        state.Enabled = false;
    }
}