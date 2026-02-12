using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public partial struct ZigzagSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ZigzagAuthoring>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Спавним только один мяч
        var prefabQuery = SystemAPI.QueryBuilder().WithAll<ZigzagAuthoring>().Build();
        var prefab = prefabQuery.GetSingletonEntity();

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        var entity = ecb.Instantiate(prefab);
        ecb.SetComponent(entity, LocalTransform.FromPosition(new float3(0, 1, -10)));
        ecb.SetComponent(entity, new ZigzagComponent
        {
            StartPosition = new float3(0, 1, -10),
            Speed = 5f,
            Amplitude = 2f,
            Frequency = 3f,
            TimeOffset = 0
        });

        ecb.Playback(state.EntityManager);

        // Отключаем систему после спавна
        state.Enabled = false;
    }
}