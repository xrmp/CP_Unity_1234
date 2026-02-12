using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ArmySettings", menuName = "ECS/Army Settings")]
public class ArmySettings : ScriptableObject
{
    public GameObject unitPrefab;
    public int gridWidth = 250;
    public int gridHeight = 200;
    public float spacing = 1.2f;
    public float moveSpeed = 2f;
    public float radius = 5f;
}

public struct ArmySettingsComponent : IComponentData
{
    public Entity Prefab;
    public int GridWidth;
    public int GridHeight;
    public float Spacing;
    public float MoveSpeed;
    public float Radius;
}