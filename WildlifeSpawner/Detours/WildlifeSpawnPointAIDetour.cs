using System;
using System.Runtime.CompilerServices;
using ColossalFramework;
using ColossalFramework.Math;
using WildlifeSpawner.Redirection;

namespace WildlifeSpawner.Detours
{
    [TargetType(typeof(WildlifeSpawnPointAI))]
    public class WildlifeSpawnPointAIDetour : BuildingAI
    {
        public static int maxAnimalCount = 200; //TODO(earalov): specify per building

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int CountAnimals(WildlifeSpawnPointAI ai, ushort buildingID, ref Building data)
        {
            UnityEngine.Debug.LogError($"{ai}-{buildingID}-{data}");
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ReleaseAnimals(ushort buildingID, ref Building data)
        {
            UnityEngine.Debug.LogError($"{buildingID}-{data}");
        }

        [RedirectMethod]
        public void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            var ai = (WildlifeSpawnPointAI)Convert.ChangeType(this, typeof(WildlifeSpawnPointAI));
            int num = new Randomizer((int)buildingID).Int32(1, maxAnimalCount);
            if (CountAnimals(ai, buildingID, ref buildingData) < num)
                this.CreateAnimal(buildingID, ref buildingData);
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
        }

        [RedirectMethod]
        private void CreateAnimal(ushort buildingID, ref Building data)
        {
            SimulationManager instance1 = Singleton<SimulationManager>.instance;
            CitizenManager instance2 = Singleton<CitizenManager>.instance;
            CitizenInfo groupAnimalInfo = instance2.GetGroupAnimalInfo(ref instance1.m_randomizer, this.m_info.m_class.m_service, this.m_info.m_class.m_subService);
            //begin mod
            var animalIndex = WildlifeSpawnManager.instance.SpawnMap.ContainsKey(buildingID) ? WildlifeSpawnManager.instance.SpawnMap[buildingID] : 0;
            animalIndex--;
            groupAnimalInfo = WildlifeSpawnManager.instance.GetGroupAnimalInfo(ref instance1.m_randomizer, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, animalIndex);
            //end mod
            ushort instance3;
            if (groupAnimalInfo == null || !instance2.CreateCitizenInstance(out instance3, ref instance1.m_randomizer, groupAnimalInfo, 0U))
                return;
            //begin mod
            PetAIDetour.isCaged = true;
            //end mod
            groupAnimalInfo.m_citizenAI.SetSource(instance3, ref instance2.m_instances.m_buffer[(int)instance3], buildingID);
            groupAnimalInfo.m_citizenAI.SetTarget(instance3, ref instance2.m_instances.m_buffer[(int)instance3], buildingID);
            //begin mod
            PetAIDetour.isCaged = false;
            //end mod
        }

        [RedirectMethod]
        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            WildlifeSpawnManager.instance.SpawnMap.Add(buildingID, ModUI.SelectedIndex);
        }

        [RedirectMethod]
        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            this.ReleaseAnimals(buildingID, ref data);
            base.ReleaseBuilding(buildingID, ref data);
            WildlifeSpawnManager.instance.SpawnMap.Remove(buildingID);
        }
    }
}