using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using WildlifeSpawner.Detours;

namespace WildlifeSpawner
{
    public class DogParkAI : ParkAI
    {
        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            this.ReleaseAnimals(buildingID, ref data);
            base.ReleaseBuilding(buildingID, ref data);
        }

        public override void BuildingDeactivated(ushort buildingID, ref Building data)
        {
            this.ReleaseAnimals(buildingID, ref data);
            base.BuildingDeactivated(buildingID, ref data);
        }

        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            int num = this.TargetAnimals(buildingID, ref buildingData);
            if (this.CountAnimals(buildingID, ref buildingData) < num)
                this.CreateAnimal(buildingID, ref buildingData);
            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);
        }

        public override void CalculateSpawnPosition(ushort buildingID, ref Building data, ref Randomizer randomizer, CitizenInfo info, out Vector3 position, out Vector3 target)
        {
            if (info.m_citizenAI.IsAnimal() && info.m_class.m_service == ItemClass.Service.Residential && info.m_class.m_subService == ItemClass.SubService.ResidentialLow)
            {
                int max1 = data.Width * 30;
                int max2 = data.Length * 30;
                position.x = (float)randomizer.Int32(-max1, max1) * 0.1f;
                position.y = 0.0f;
                position.z = (float)randomizer.Int32(-max2, max2) * 0.1f;
                position = data.CalculatePosition(position);
                float f = (float)randomizer.Int32(360U) * ((float)System.Math.PI / 180f);
                target = position;
                target.x += Mathf.Cos(f);
                target.z += Mathf.Sin(f);
            }
            else
                base.CalculateSpawnPosition(buildingID, ref data, ref randomizer, info, out position, out target);
        }

        public override void CalculateUnspawnPosition(ushort buildingID, ref Building data, ref Randomizer randomizer, CitizenInfo info, ushort ignoreInstance, out Vector3 position, out Vector3 target, out Vector2 direction, out CitizenInstance.Flags specialFlags)
        {
            if (info.m_citizenAI.IsAnimal() && info.m_class.m_service == ItemClass.Service.Residential && info.m_class.m_subService == ItemClass.SubService.ResidentialLow)
            {
                int max1 = data.Width * 30;
                int max2 = data.Length * 30;
                position.x = (float)randomizer.Int32(-max1, max1) * 0.1f;
                position.y = 0.0f;
                position.z = (float)randomizer.Int32(-max2, max2) * 0.1f;
                position = data.CalculatePosition(position);
                target = position;
                direction = Vector2.zero;
                specialFlags = CitizenInstance.Flags.HangAround;
            }
            else
                base.CalculateUnspawnPosition(buildingID, ref data, ref randomizer, info, ignoreInstance, out position, out target, out direction, out specialFlags);
        }


        private void CreateAnimal(ushort buildingID, ref Building data)
        {
            CitizenManager instance1 = Singleton<CitizenManager>.instance;
            Randomizer r = new Randomizer((int)buildingID);
            CitizenInfo groupAnimalInfo = PrefabCollection<CitizenInfo>.FindLoaded("Dog");//instance1.GetGroupAnimalInfo(ref r, ItemClass.Service.Residential, ItemClass.SubService.ResidentialLow);
            ushort instance2;
            if (groupAnimalInfo == null || !instance1.CreateCitizenInstance(out instance2, ref Singleton<SimulationManager>.instance.m_randomizer, groupAnimalInfo, 0U))
                return;
            PetAIDetour.isCaged = true;
            groupAnimalInfo.m_citizenAI.SetSource(instance2, ref instance1.m_instances.m_buffer[(int)instance2], buildingID);
            PetAIDetour.SetTarget((PetAI)groupAnimalInfo.m_citizenAI, instance2, ref instance1.m_instances.m_buffer[(int)instance2], buildingID);
            PetAIDetour.isCaged = false;
        }

        private void ReleaseAnimals(ushort buildingID, ref Building data)
        {
            CitizenManager instance1 = Singleton<CitizenManager>.instance;
            ushort instance2 = data.m_targetCitizens;
            int num1 = 0;
            while ((int)instance2 != 0)
            {
                ushort num2 = instance1.m_instances.m_buffer[(int)instance2].m_nextTargetInstance;
                if (instance1.m_instances.m_buffer[(int)instance2].Info.m_citizenAI.IsAnimal())
                    instance1.ReleaseCitizenInstance(instance2);
                instance2 = num2;
                if (++num1 > 65536)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
        }

        private int CountAnimals(ushort buildingID, ref Building data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort num1 = data.m_targetCitizens;
            int num2 = 0;
            int num3 = 0;
            while ((int)num1 != 0)
            {
                ushort num4 = instance.m_instances.m_buffer[(int)num1].m_nextTargetInstance;
                if (instance.m_instances.m_buffer[(int)num1].Info.m_citizenAI.IsAnimal())
                    ++num2;
                num1 = num4;
                if (++num3 > 65536)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                    break;
                }
            }
            return num2;
        }

        private int TargetAnimals(ushort buildingID, ref Building data)
        {
            Randomizer randomizer = new Randomizer((int)buildingID);
            return Mathf.Max(100, data.Width * data.Length * 30 + randomizer.Int32(100U)) / 100;
        }
    }
}