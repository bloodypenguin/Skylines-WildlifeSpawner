using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using WildlifeSpawner.Redirection;

namespace WildlifeSpawner.Detours
{
    [TargetType(typeof(PetAI))]
    public class PetAIDetour : AnimalAI
    {
        public static bool isCaged = false;

        private static bool IsCagedAnimal(CitizenInstance data)
        {
            try
            {
                return BuildingManager.instance.m_buildings.m_buffer[data.m_targetBuilding].Info.name ==
                       "dog-park-fence" || isCaged;
            }
            catch
            {
                return false;
            }
        }

        [RedirectMethod]
        public override void LoadInstance(ushort instanceID, ref CitizenInstance data)
        {
            if (!(data.Info?.m_citizenAI is PetAI) || (!IsCagedAnimal(data) && !LivestockAIDetour.isFreeAnimal(data)))
            {
                return;
            }
            if ((int)data.m_targetBuilding == 0)
                return;
            Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
        }

        [RedirectMethod]
        public override Color GetColor(ushort instanceID, ref CitizenInstance data, InfoManager.InfoMode infoMode)
        {
            //begin mod
            if (!IsCagedAnimal(data) && !LivestockAIDetour.isFreeAnimal(data))
            {
                if (infoMode == InfoManager.InfoMode.Transport)
                {
                    ushort instanceID1 = data.m_targetBuilding;
                    if ((int)instanceID1 != 0)
                    {
                        CitizenManager instance = Singleton<CitizenManager>.instance;
                        return instance.m_instances.m_buffer[(int)instanceID1].Info.m_citizenAI.GetColor(instanceID1, ref instance.m_instances.m_buffer[(int)instanceID1], infoMode);
                    }
                }
            }
            //end mod
            return base.GetColor(instanceID, ref data, infoMode);
        }

        [RedirectMethod]
        public override void SetSource(ushort instanceID, ref CitizenInstance data, ushort sourceBuilding)
        {
            //begin mod
            if (IsCagedAnimal(data) || LivestockAIDetour.isFreeAnimal(data))
            {
                SetSourceCaged(instanceID, ref data, sourceBuilding);
                return;
            }
            //end mod

            if ((int)sourceBuilding == 0)
                return;
            CitizenManager instance = Singleton<CitizenManager>.instance;
            data.Unspawn(instanceID);
            Randomizer randomizer = new Randomizer((int)instanceID);
            CitizenInstance.Frame lastFrameData = instance.m_instances.m_buffer[(int)sourceBuilding].GetLastFrameData();
            Quaternion quaternion = lastFrameData.m_rotation;
            Vector3 vector3_1 = quaternion * new Vector3(0.5f, 0.0f, 0.0f);
            if (randomizer.Int32(2U) == 0)
                vector3_1 = -vector3_1;
            Vector3 vector3_2 = lastFrameData.m_position + vector3_1;
            Vector4 vector4 = instance.m_instances.m_buffer[(int)sourceBuilding].m_targetPos + (Vector4)vector3_1;
            data.m_frame0.m_velocity = Vector3.zero;
            data.m_frame0.m_position = vector3_2;
            data.m_frame0.m_rotation = quaternion;
            data.m_frame1 = data.m_frame0;
            data.m_frame2 = data.m_frame0;
            data.m_frame3 = data.m_frame0;
            data.m_targetPos = vector4;
            data.Spawn(instanceID);
        }

        [RedirectMethod]
        public static void SetTarget(PetAI ai, ushort instanceID, ref CitizenInstance data, ushort targetBuilding)
        {
            //begin mod
            if (IsCagedAnimal(data) || LivestockAIDetour.isFreeAnimal(data))
            {
                SetTargetCaged(instanceID, ref data, targetBuilding);
                return;
            }
            //end mod
            data.m_targetBuilding = targetBuilding;
        }

        [RedirectMethod]
        public override void SimulationStep(ushort instanceID, ref CitizenInstance citizenData, ref CitizenInstance.Frame frameData, bool lodPhysics)
        {
            if (IsCagedAnimal(citizenData))
            {
                SimulationStepCaged(instanceID, ref citizenData, ref frameData, lodPhysics);
                return;
            } else if (LivestockAIDetour.isFreeAnimal(citizenData))
            {
                SimulationStepWild(instanceID, ref citizenData, ref frameData, lodPhysics);
                return;
            }

            float sqrMagnitude1 = frameData.m_velocity.sqrMagnitude;
            if ((double)sqrMagnitude1 > 0.00999999977648258)
                frameData.m_position += frameData.m_velocity * 0.5f;
            CitizenInstance.Flags flags1 = CitizenInstance.Flags.None;
            if ((int)citizenData.m_targetBuilding != 0)
            {
                CitizenManager instance = Singleton<CitizenManager>.instance;
                CitizenInstance.Flags flags2 = instance.m_instances.m_buffer[(int)citizenData.m_targetBuilding].m_flags;
                if ((flags2 & CitizenInstance.Flags.Character) != CitizenInstance.Flags.None)
                {
                    Randomizer randomizer = new Randomizer((int)instanceID);
                    CitizenInstance.Frame lastFrameData = instance.m_instances.m_buffer[(int)citizenData.m_targetBuilding].GetLastFrameData();
                    Vector3 vector3 = lastFrameData.m_rotation * new Vector3(0.5f, 0.0f, 0.0f);
                    if (randomizer.Int32(2U) == 0)
                        vector3 = -vector3;
                    Vector4 vector4 = instance.m_instances.m_buffer[(int)citizenData.m_targetBuilding].m_targetPos + (Vector4)vector3;
                    if ((double)Vector3.SqrMagnitude(lastFrameData.m_position - frameData.m_position) > 10000.0)
                    {
                        citizenData.m_targetBuilding = (ushort)0;
                    }
                    else
                    {
                        flags1 = flags2 & (CitizenInstance.Flags.Underground | CitizenInstance.Flags.InsideBuilding | CitizenInstance.Flags.Transition);
                        citizenData.m_targetPos = vector4;
                    }
                }
                else
                    citizenData.m_targetBuilding = (ushort)0;
            }
            citizenData.m_flags = citizenData.m_flags & ~(CitizenInstance.Flags.Underground | CitizenInstance.Flags.InsideBuilding | CitizenInstance.Flags.Transition) | flags1;
            Vector3 v = (Vector3)citizenData.m_targetPos - frameData.m_position;
            float f = lodPhysics || (double)citizenData.m_targetPos.w <= 1.0 / 1000.0 ? v.sqrMagnitude : VectorUtils.LengthSqrXZ(v);
            float a = this.m_info.m_walkSpeed;
            float b = 2f;
            if ((double)f < 1.0)
            {
                v = Vector3.zero;
            }
            else
            {
                float num = Mathf.Sqrt(f);
                float maxLength = Mathf.Min(a, num * 0.75f);
                v = Quaternion.Inverse(frameData.m_rotation) * v;
                if ((double)v.z < (double)Mathf.Abs(v.x))
                {
                    v.x = (double)v.x < 0.0 ? Mathf.Min(-1f, v.x) : Mathf.Max(1f, v.x);
                    v.z = Mathf.Abs(v.x);
                    maxLength = Mathf.Min(0.5f, num * 0.1f);
                }
                v = Vector3.ClampMagnitude(frameData.m_rotation * v, maxLength);
            }
            Vector3 vector3_1 = v - frameData.m_velocity;
            float magnitude = vector3_1.magnitude;
            Vector3 vector3_2 = vector3_1 * (b / Mathf.Max(magnitude, b));
            frameData.m_velocity += vector3_2;
            float sqrMagnitude2 = frameData.m_velocity.sqrMagnitude;
            bool flag = !lodPhysics && (double)citizenData.m_targetPos.w > 1.0 / 1000.0 && ((double)sqrMagnitude2 > 0.00999999977648258 || (double)sqrMagnitude1 > 0.00999999977648258);
            ushort buildingID = !flag ? (ushort)0 : Singleton<BuildingManager>.instance.GetWalkingBuilding(frameData.m_position + frameData.m_velocity * 0.5f);
            if ((double)sqrMagnitude2 > 0.00999999977648258)
            {
                Vector3 forward = frameData.m_velocity;
                if (!lodPhysics)
                {
                    Vector3 pushAmount = Vector3.zero;
                    float pushDivider = 0.0f;
                    this.CheckCollisions(instanceID, ref citizenData, frameData.m_position, frameData.m_position + frameData.m_velocity, buildingID, ref pushAmount, ref pushDivider);
                    if ((double)pushDivider > 0.00999999977648258)
                    {
                        pushAmount *= 1f / pushDivider;
                        pushAmount = Vector3.ClampMagnitude(pushAmount, Mathf.Sqrt(sqrMagnitude2) * 0.5f);
                        frameData.m_velocity += pushAmount;
                        forward += pushAmount * 0.25f;
                    }
                }
                frameData.m_position += frameData.m_velocity * 0.5f;
                if ((double)forward.sqrMagnitude > 0.00999999977648258)
                    frameData.m_rotation = Quaternion.LookRotation(forward);
            }
            if (flag)
            {
                Vector3 worldPos = frameData.m_position;
                float terrainHeight = Singleton<TerrainManager>.instance.SampleDetailHeight(worldPos);
                if ((int)buildingID != 0)
                {
                    float num = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)buildingID].SampleWalkingHeight(worldPos, terrainHeight);
                    worldPos.y = worldPos.y + (num - worldPos.y) * Mathf.Min(1f, citizenData.m_targetPos.w * 4f);
                    frameData.m_position.y = worldPos.y;
                }
                else if ((double)Mathf.Abs(terrainHeight - worldPos.y) < 2.0)
                {
                    worldPos.y = worldPos.y + (terrainHeight - worldPos.y) * Mathf.Min(1f, citizenData.m_targetPos.w * 4f);
                    frameData.m_position.y = worldPos.y;
                }
            }
            frameData.m_underground = (citizenData.m_flags & CitizenInstance.Flags.Underground) != CitizenInstance.Flags.None;
            frameData.m_insideBuilding = (citizenData.m_flags & CitizenInstance.Flags.InsideBuilding) != CitizenInstance.Flags.None;
            frameData.m_transition = (citizenData.m_flags & CitizenInstance.Flags.Transition) != CitizenInstance.Flags.None;
            var mRandomEffect = ((PetAI)Convert.ChangeType(this, typeof(PetAI))).m_randomEffect;
            if (mRandomEffect == null || Singleton<SimulationManager>.instance.m_randomizer.Int32(40U) != 0)
                return;
            InstanceID instance1 = new InstanceID();
            instance1.CitizenInstance = instanceID;
            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(frameData.m_position, Vector3.up, 0.0f);
            float num1 = 3.75f;
            Singleton<EffectManager>.instance.DispatchEffect(mRandomEffect, instance1, spawnArea, frameData.m_velocity * num1, 0.0f, 1f, Singleton<CitizenManager>.instance.m_audioGroup);
        }

        public static void SetTargetCaged(ushort instanceID, ref CitizenInstance data, ushort targetBuilding)
        {
            if ((int)targetBuilding == (int)data.m_targetBuilding)
                return;
            if ((int)data.m_targetBuilding != 0)
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
            data.m_targetBuilding = targetBuilding;
            if ((int)data.m_targetBuilding == 0)
                return;
            Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
        }

        public void SetSourceCaged(ushort instanceID, ref CitizenInstance data, ushort sourceBuilding)
        {
            if ((int)sourceBuilding == 0)
                return;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[(int)sourceBuilding].Info;
            data.Unspawn(instanceID);
            Vector3 position;
            Vector3 target;
            info.m_buildingAI.CalculateSpawnPosition(sourceBuilding, ref instance.m_buildings.m_buffer[(int)sourceBuilding], ref Singleton<SimulationManager>.instance.m_randomizer, this.m_info, out position, out target);
            Quaternion quaternion = Quaternion.identity;
            Vector3 forward = target - position;
            if ((double)forward.sqrMagnitude > 0.00999999977648258)
                quaternion = Quaternion.LookRotation(forward);
            position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
            data.m_frame0.m_velocity = Vector3.zero;
            data.m_frame0.m_position = position;
            data.m_frame0.m_rotation = quaternion;
            data.m_frame1 = data.m_frame0;
            data.m_frame2 = data.m_frame0;
            data.m_frame3 = data.m_frame0;
            data.m_targetPos = (Vector4)target;
            data.Spawn(instanceID);
        }

        public void SimulationStepCaged(ushort instanceID, ref CitizenInstance citizenData, ref CitizenInstance.Frame frameData, bool lodPhysics)
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
                float maxLength = Mathf.Min(a, num2 * 0.5f);
                vector3_1 = Quaternion.Inverse(frameData.m_rotation) * vector3_1;
                if ((double)vector3_1.z < (double)Mathf.Abs(vector3_1.x) * 5.0)
                {
                    vector3_1.x = (double)vector3_1.x < 0.0 ? Mathf.Min(-1f, vector3_1.x) : Mathf.Max(1f, vector3_1.x);
                    vector3_1.z = Mathf.Abs(vector3_1.x) * 5f;
                    maxLength = Mathf.Min(0.5f, num2 * 0.1f);
                }
                vector3_1 = Vector3.ClampMagnitude(frameData.m_rotation * vector3_1, maxLength);
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
                    this.CheckCollisions(instanceID, ref citizenData, frameData.m_position, frameData.m_position + frameData.m_velocity, citizenData.m_targetBuilding, ref pushAmount, ref pushDivider);
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
            var m_randomEffect = ((PetAI)Convert.ChangeType(this, typeof(PetAI))).m_randomEffect;
            if (m_randomEffect == null || Singleton<SimulationManager>.instance.m_randomizer.Int32(40U) != 0)
                return;
            InstanceID instance1 = new InstanceID();
            instance1.CitizenInstance = instanceID;
            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(frameData.m_position, Vector3.up, 0.0f);
            float num3 = 3.75f;
            Singleton<EffectManager>.instance.DispatchEffect(m_randomEffect, instance1, spawnArea, frameData.m_velocity * num3, 0.0f, 1f, Singleton<CitizenManager>.instance.m_audioGroup);
        }

        public void SimulationStepWild(ushort instanceID, ref CitizenInstance citizenData, ref CitizenInstance.Frame frameData, bool lodPhysics)
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
                if (LivestockAIDetour.isFreeAnimal(citizenData))
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
                    this.CheckCollisions(instanceID, ref citizenData, frameData.m_position, frameData.m_position + frameData.m_velocity, LivestockAIDetour.isFreeAnimal(citizenData) ? (ushort)0 : citizenData.m_targetBuilding, ref pushAmount, ref pushDivider);
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
            var m_randomEffect = ((PetAI)Convert.ChangeType(this, typeof(PetAI))).m_randomEffect;
            if (m_randomEffect == null || Singleton<SimulationManager>.instance.m_randomizer.Int32(40U) != 0)
                return;
            InstanceID instance1 = new InstanceID();
            instance1.CitizenInstance = instanceID;
            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(frameData.m_position, Vector3.up, 0.0f);
            float num3 = 3.75f;
            Singleton<EffectManager>.instance.DispatchEffect(m_randomEffect, instance1, spawnArea, frameData.m_velocity * num3, 0.0f, 1f, Singleton<CitizenManager>.instance.m_audioGroup);
        }
    }
}