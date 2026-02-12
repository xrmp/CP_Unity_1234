using Unity.Entities;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Default)]
public partial struct ArmySettingsSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.Enabled = false; // Отключаем, используем ArmyBootstrap
    }

    public void OnUpdate(ref SystemState state) { }
}