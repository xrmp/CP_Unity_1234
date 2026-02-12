using Unity.Entities;
using Unity.Mathematics;

public struct ZigzagComponent : IComponentData
{
    public float3 StartPosition;
    public float Speed;        // Скорость вперед
    public float Amplitude;    // Амплитуда зигзага
    public float Frequency;    // Частота колебаний
    public float TimeOffset;   // Смещение по времени
}