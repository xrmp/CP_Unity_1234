using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ArmyBootstrap : MonoBehaviour
{
    [SerializeField] private ArmySettings settings;

    private void Awake()
    {
        if (settings == null)
        {
            settings = Resources.Load<ArmySettings>("ArmySettings");
        }

        if (settings == null)
        {
            Debug.LogError("ArmySettings not found! Create one from Assets → Create → ECS → Army Settings");
            return;
        }

        var world = World.DefaultGameObjectInjectionWorld;

        // Создаем сущность с настройками
        var settingsEntity = world.EntityManager.CreateEntity();

        // Конвертируем GameObject в Entity через Baker (правильный способ)
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(settings.unitPrefab, world);

        world.EntityManager.AddComponentData(settingsEntity, new ArmySettingsComponent
        {
            Prefab = prefab,
            GridWidth = settings.gridWidth,
            GridHeight = settings.gridHeight,
            Spacing = settings.spacing,
            MoveSpeed = settings.moveSpeed,
            Radius = settings.radius
        });
    }
}

public static class GameObjectConversionUtility
{
    public static Entity ConvertGameObjectHierarchy(GameObject go, World world)
    {
        // Создаем временную сущность для бейкинга
        var entity = world.EntityManager.CreateEntity();

        // Добавляем компонент для бейкинга
        world.EntityManager.AddComponent<Prefab>(entity);

        // В реальном проекте используйте Baker, это временное решение
        var authoring = go.GetComponent<ArmyUnitAuthoring>();
        if (authoring != null)
        {
            world.EntityManager.AddComponentData(entity, new MoveSpeed { Value = authoring.moveSpeed });
            world.EntityManager.AddComponentData(entity, new Radius { Value = authoring.radius });
            world.EntityManager.AddComponentData(entity, new CenterPosition { Value = float3.zero });
            world.EntityManager.AddComponentData(entity, new InitialPosition { Value = float3.zero });
            world.EntityManager.AddComponent<ArmyTag>(entity);
            world.EntityManager.AddComponent<LocalTransform>(entity);
        }

        return entity;
    }
}