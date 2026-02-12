using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Army.ECS
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CircleMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            float elapsedTime = (float)SystemAPI.Time.ElapsedTime;

            new CircleMovementJob
            {
                DeltaTime = deltaTime,
                ElapsedTime = elapsedTime
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct CircleMovementJob : IJobEntity
    {
        public float DeltaTime;
        public float ElapsedTime;

        public void Execute(ref LocalTransform transform,
                            ref CenterPosition center,
                            in InitialPosition initial,
                            in MoveSpeed speed,
                            in Radius radius)
        {
            // Индивидуальное движение вокруг своей точки спавна
            float angle = ElapsedTime * speed.Value + initial.Value.x * 0.1f + initial.Value.z * 0.1f;

            float x = math.sin(angle) * radius.Value;
            float z = math.cos(angle) * radius.Value;

            // Позиция = центр + смещение по кругу
            transform.Position = center.Value + new float3(x, 0, z);

            // Вращение объекта по направлению движения
            float3 forward = math.normalizesafe(new float3(x, 0, z));
            if (math.lengthsq(forward) > 0.001f)
            {
                transform.Rotation = quaternion.LookRotationSafe(forward, new float3(0, 1, 0));
            }
        }
    }
}