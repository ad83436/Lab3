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

    struct SpawnJob : IJobForEachWithEntity<Bullet, PlayerInput, LocalToWorld> {
        public EntityCommandBuffer CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref Bullet bullet, [ReadOnly] ref PlayerInput pInput, [ReadOnly] ref LocalToWorld location) {
            if (pInput.RightClick) {
                var instance = CommandBuffer.Instantiate(bullet.Prefab);
                var position = math.transform(location.Value, new float3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));

                //TODO: Eventually switch to the new Unity.Physics AABB 
                var aabb = new AABB {
                    //0.5f will represent halfwidth for now
                    max = position + 0.5f,
                    min = position - 0.5f,
                };
                CommandBuffer.AddComponent(instance, aabb);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        // Schedule the job that will add Instantiate commands to the EntityCommandBuffer.
        var job = new SpawnJob {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer()
        }.Schedule(this, inputDeps);

        // SpawnJob runs in parallel with no sync point until the barrier system executes.
        // When the barrier system executes we want to complete the SpawnJob and then play back the commands (Creating the entities and placing them).
        // We need to tell the barrier system which job it needs to complete before it can play back the commands.
        m_EntityCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}
