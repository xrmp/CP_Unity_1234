using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ZigzagAuthoring : MonoBehaviour
{
    public float speed = 5f;
    public float amplitude = 2f;
    public float frequency = 3f;
}

public class ZigzagBaker : Baker<ZigzagAuthoring>
{
    public override void Bake(ZigzagAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new LocalTransform
        {
            Position = authoring.transform.position,
            Rotation = quaternion.identity,
            Scale = 1f
        });

        AddComponent(entity, new ZigzagComponent
        {
            StartPosition = authoring.transform.position,
            Speed = authoring.speed,
            Amplitude = authoring.amplitude,
            Frequency = authoring.frequency,
            TimeOffset = UnityEngine.Random.Range(0f, 100f)
        });

        AddComponent<ZigzagTag>(entity);
    }
}