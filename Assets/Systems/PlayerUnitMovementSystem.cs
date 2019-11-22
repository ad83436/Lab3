﻿using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

public class PlayerUnitMovementSystem : JobComponentSystem {
    public struct PlayerUnitMovementJob : IJobForEach<PlayerInput, UnitNavAgent, PlayerUnitSelect, Rotation,Translation> {
        public float dT;
        public float3 mousePos;

        public void Execute([ReadOnly] ref PlayerInput pInput, ref UnitNavAgent navAgent, [ReadOnly] ref PlayerUnitSelect selected, ref Rotation rotation,[ReadOnly] ref Translation position) {
           if (pInput.RightClick) {
                navAgent.finalDestination = mousePos;
                navAgent.agentStatus = NavAgentStatus.Moving;
                float3 direction = math.normalize(navAgent.finalDestination - position.Value);
                rotation.Value = Quaternion.LookRotation(new Vector3(direction.x,direction.y,direction.z));
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
        var job = new PlayerUnitMovementJob {
            mousePos = mousePos
        };
        return job.Schedule(this, inputDeps);
    }
}
