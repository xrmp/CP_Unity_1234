using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CounterSystem))]
public partial struct ZigzagMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        float elapsedTime = (float)SystemAPI.Time.ElapsedTime;

        new ZigzagMovementJob
        {
            DeltaTime = deltaTime,
            ElapsedTime = elapsedTime
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct ZigzagMovementJob : IJobEntity
{
    public float DeltaTime;
    public float ElapsedTime;

    public void Execute(ref LocalTransform transform, ref ZigzagComponent zigzag)
    {
        // Движение вперед (по Z)
        float forwardMovement = zigzag.Speed * ElapsedTime;

        // Зигзаг влево-вправо (по X)
        float zigzagOffset = math.sin((ElapsedTime + zigzag.TimeOffset) * zigzag.Frequency) * zigzag.Amplitude;

        // Новая позиция
        transform.Position = new float3(
            zigzag.StartPosition.x + zigzagOffset,
            zigzag.StartPosition.y,
            zigzag.StartPosition.z + forwardMovement
        );

        // Поворот в направлении движения
        float3 direction = new float3(
            math.cos((ElapsedTime + zigzag.TimeOffset) * zigzag.Frequency) * zigzag.Amplitude * zigzag.Frequency,
            0,
            zigzag.Speed
        );

        if (math.lengthsq(direction) > 0.001f)
        {
            transform.Rotation = quaternion.LookRotationSafe(math.normalize(direction), new float3(0, 1, 0));
        }
    }
}