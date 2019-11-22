using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BulletSpawningSystem : JobComponentSystem {
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate() {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    struct SpawnJob : IJobForEachWithEntity<BulletSpawner, PlayerInput, LocalToWorld> {
        public EntityCommandBuffer CommandBuffer;
        public Vector3 mousePosition;

        public void Execute(Entity entity, int index, [ReadOnly] ref BulletSpawner bullet, [ReadOnly] ref PlayerInput pInput, [ReadOnly] ref LocalToWorld location) {
            if (pInput.RightClick) {
                var instance = CommandBuffer.Instantiate(bullet.Prefab);
                var position = math.transform(location.Value, mousePosition);
                //TODO: Eventually switch to the new Unity.Physics AABB 
                var aabb = new AABB {
                    //0.5f will represent halfwidth for now
                    max = position + 0.5f,
                    min = position - 0.5f,
                };
                CommandBuffer.AddComponent(instance, aabb);
                CommandBuffer.SetComponent(instance, new Translation { Value = position });
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        var mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            if (hit.collider != null) {
                mousePos = new float3(hit.point.x, 0, hit.point.z);
            }
        }
        // Schedule the job that will add Instantiate commands to the EntityCommandBuffer.
        var job = new SpawnJob {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
            mousePosition = mousePos

        }.Schedule(this, inputDeps);

        return job;
    }
}
