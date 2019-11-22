using Unity.Entities;
using Unity.Mathematics;

public struct Bullet : IComponentData {
    public float3 velocity;
    public Entity Prefab;
}