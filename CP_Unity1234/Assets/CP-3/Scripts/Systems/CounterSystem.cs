using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public partial struct CounterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CounterComponent>();

        // Создаем сущность со счетчиком
        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(entity, new CounterComponent { Value = 0 });
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var counter in SystemAPI.Query<RefRW<CounterComponent>>())
        {
            counter.ValueRW.Value++;

            if (counter.ValueRO.Value % 60 == 0)
            {
                int frameValue = counter.ValueRO.Value;
                float timeValue = (float)SystemAPI.Time.ElapsedTime;

                Debug.Log($"[ECS Counter] Frame: {frameValue}, Time: {timeValue}s");
            }
        }
    }
}