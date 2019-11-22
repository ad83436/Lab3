using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[RequiresEntityConversion]
public class BulletSpawnerProxy : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity {
    public GameObject Prefab;
    public float3 Velocity;

    public void DeclareReferencedPrefabs(List<GameObject> gameObjects) {
        gameObjects.Add(Prefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var spawnerData = new BulletSpawner {
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            velocity = Velocity,
        };
        dstManager.AddComponentData(entity, spawnerData);
    }
}