using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ArmyUnitAuthoring : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float radius = 5f;
}

public class ArmyUnitBaker : Baker<ArmyUnitAuthoring>
{
    public override void Bake(ArmyUnitAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new LocalTransform
        {
            Position = float3.zero,
            Rotation = quaternion.identity,
            Scale = 1f
        });

        AddComponent(entity, new MoveSpeed { Value = authoring.moveSpeed });
        AddComponent(entity, new Radius { Value = authoring.radius });
        AddComponent(entity, new CenterPosition { Value = float3.zero });
        AddComponent(entity, new InitialPosition { Value = float3.zero });
        AddComponent<ArmyTag>(entity);
    }
}