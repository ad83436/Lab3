using Unity.Entities;
using Unity.Mathematics;

public struct BulletSpawner : IComponentData {
    public float3 velocity;
    public Entity Prefab;
}