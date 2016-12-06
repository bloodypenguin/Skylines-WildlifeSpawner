using ColossalFramework;
using UnityEngine;
using WildlifeSpawner.Redirection;

namespace WildlifeSpawner.Detours
{
    [TargetType(typeof(LivestockAI))]
    public class LivestockAIDetour : LivestockAI
    {

        public static bool isFreeAnimal(CitizenInstance data)
        {
            return WildlifeSpawnManager.instance.SpawnMap.ContainsKey(data.m_targetBuilding);
        }


        [RedirectMethod]
        public override void SimulationStep(ushort instanceID, ref CitizenInstance citizenData, ref CitizenInstance.Frame frameData, bool lodPhysics)
        {
            float sqrMagnitude1 = frameData.m_velocity.sqrMagnitude;
            if ((double)sqrMagnitude1 > 0.00999999977648258)
                frameData.m_position += frameData.m_velocity * 0.5f;
            Vector3 vector3_1 = (Vector3)citizenData.m_targetPos - frameData.m_position;
            float sqrMagnitude2 = vector3_1.sqrMagnitude;
            float num1 = Mathf.Max(sqrMagnitude1 * 3f, 6f);
            if ((double)sqrMagnitude2 < (double)num1 && Singleton<SimulationManager>.instance.m_randomizer.Int32(20U) == 0 && (int)citizenData.m_targetBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                Vector3 position;
                Vector3 target;
                Vector2 direction;
                CitizenInstance.Flags specialFlags;
                instance.m_buildings.m_buffer[(int)citizenData.m_targetBuilding].Info.m_buildingAI.CalculateUnspawnPosition(citizenData.m_targetBuilding, ref instance.m_buildings.m_buffer[(int)citizenData.m_targetBuilding], ref Singleton<SimulationManager>.instance.m_randomizer, this.m_info, instanceID, out position, out target, out direction, out specialFlags);
                position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                citizenData.m_targetPos = (Vector4)position;
                vector3_1 = (Vector3)citizenData.m_targetPos - frameData.m_position;
                sqrMagnitude2 = vector3_1.sqrMagnitude;
            }
            float a = this.m_info.m_walkSpeed;
            float b = 2f;
            if ((double)sqrMagnitude2 < 4.0)
            {
                vector3_1 = Vector3.zero;
            }
            else
            {
                float num2 = Mathf.Sqrt(sqrMagnitude2);
                //begin mod
                if (isFreeAnimal(citizenData))
                {
                    float maxLength = Mathf.Min(a, Mathf.Sqrt(num2 * b));
                    float num3_ = Mathf.Max(1f, 0.5f * sqrMagnitude1 / b);
                    float num4 = Mathf.Max(8f, 0.5f * sqrMagnitude1 / b + maxLength);
                    Vector3 position = frameData.m_position + vector3_1 * (num3_ / num2);
                    var stubAi = new WildlifeAI { m_info = this.m_info };

                    if (WildlifeAIDetour.IsFreePosition(stubAi, frameData.m_position + vector3_1 * (num4 / num2)))
                    {
                        vector3_1 = Quaternion.Inverse(frameData.m_rotation) * vector3_1;
                        if ((double)vector3_1.z < (double)Mathf.Abs(vector3_1.x) * 1.70000004768372)
                        {
                            vector3_1.x = (double)vector3_1.x < 0.0 ? Mathf.Min(-1f, vector3_1.x) : Mathf.Max(1f, vector3_1.x);
                            vector3_1.z = Mathf.Abs(vector3_1.x) * 1.7f;
                            maxLength = Mathf.Min(1.5f, num2 * 0.1f);
                        }
                        vector3_1 = Vector3.ClampMagnitude(frameData.m_rotation * vector3_1, maxLength);
                    }
                    else if (WildlifeAIDetour.IsFreePosition(stubAi, position))
                    {
                        citizenData.m_targetPos = (Vector4)position;
                        vector3_1 = Vector3.zero;
                    }
                    else if (WildlifeAIDetour.IsFreePosition(stubAi, frameData.m_position))
                    {
                        citizenData.m_targetPos = (Vector4)frameData.m_position;
                        vector3_1 = Vector3.zero;
                    }
                    else
                    {
                        citizenData.Unspawn(instanceID);
                        return;
                    }
                }
                else
                {
                    //end mod
                    float maxLength = Mathf.Min(a, num2 * 0.5f);
                    vector3_1 = Quaternion.Inverse(frameData.m_rotation) * vector3_1;
                    if ((double)vector3_1.z < (double)Mathf.Abs(vector3_1.x) * 5.0)
                    {
                        vector3_1.x = (double)vector3_1.x < 0.0 ? Mathf.Min(-1f, vector3_1.x) : Mathf.Max(1f, vector3_1.x);
                        vector3_1.z = Mathf.Abs(vector3_1.x) * 5f;
                        maxLength = Mathf.Min(0.5f, num2 * 0.1f);
                    }
                    vector3_1 = Vector3.ClampMagnitude(frameData.m_rotation * vector3_1, maxLength);
                    //begin mod
                }
                //end mod

            }
            Vector3 vector3_2 = vector3_1 - frameData.m_velocity;
            float magnitude = vector3_2.magnitude;
            Vector3 vector3_3 = vector3_2 * (b / Mathf.Max(magnitude, b));
            frameData.m_velocity += vector3_3;
            citizenData.m_targetPos.y = Singleton<TerrainManager>.instance.SampleDetailHeight(frameData.m_position + frameData.m_velocity);
            frameData.m_velocity.y = citizenData.m_targetPos.y - frameData.m_position.y;
            float sqrMagnitude3 = frameData.m_velocity.sqrMagnitude;
            if ((double)sqrMagnitude3 > 0.00999999977648258)
            {
                Vector3 forward = frameData.m_velocity;
                if (!lodPhysics)
                {
                    Vector3 pushAmount = Vector3.zero;
                    float pushDivider = 0.0f;
                    //begin mod
                    this.CheckCollisions(instanceID, ref citizenData, frameData.m_position, frameData.m_position + frameData.m_velocity, isFreeAnimal(citizenData) ? (ushort)0 : citizenData.m_targetBuilding, ref pushAmount, ref pushDivider);
                    //end mod
                    if ((double)pushDivider > 0.00999999977648258)
                    {
                        pushAmount *= 1f / pushDivider;
                        pushAmount = Vector3.ClampMagnitude(pushAmount, Mathf.Sqrt(sqrMagnitude3) * 0.5f);
                        frameData.m_velocity += pushAmount;
                        forward += pushAmount * 0.25f;
                    }
                }
                frameData.m_position += frameData.m_velocity * 0.5f;
                if ((double)forward.sqrMagnitude > 0.00999999977648258)
                    frameData.m_rotation = Quaternion.LookRotation(forward);
            }
            if (this.m_randomEffect == null || Singleton<SimulationManager>.instance.m_randomizer.Int32(40U) != 0)
                return;
            InstanceID instance1 = new InstanceID();
            instance1.CitizenInstance = instanceID;
            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(frameData.m_position, Vector3.up, 0.0f);
            float num3 = 3.75f;
            Singleton<EffectManager>.instance.DispatchEffect(this.m_randomEffect, instance1, spawnArea, frameData.m_velocity * num3, 0.0f, 1f, Singleton<CitizenManager>.instance.m_audioGroup);
        }
    }
}