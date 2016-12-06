using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Math;

namespace WildlifeSpawner
{
    public class WildlifeSpawnManager : Singleton<WildlifeSpawnManager>
    {

        private FastList<ushort> m_animals;

        public FastList<ushort> Animals => m_animals ?? (m_animals = RefreshAnimals());
        public Dictionary<ushort, int> SpawnMap = new Dictionary<ushort, int>();

        public void Awake()
        {
            RefreshAnimals();
        }

        //based on CitizenManager#GetGroupAnimalInfo()
        public CitizenInfo GetGroupAnimalInfo(ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, int animalIndex)
        {

            var index = r.Int32((uint)Animals.m_size);
            if (animalIndex > -1)
            {
                index = animalIndex;
            }
            return PrefabCollection<CitizenInfo>.GetPrefab((uint)Animals.m_buffer[index]);
        }

        //based on CitizenManager#RefreshGroupCitizens()
        private static FastList<ushort> RefreshAnimals()
        {
            var animals = new FastList<ushort>();
            var num1 = PrefabCollection<CitizenInfo>.PrefabCount();
            for (var index = 0; index < num1; ++index)
            {
                var prefab = PrefabCollection<CitizenInfo>.GetPrefab((uint)index);
                if (prefab == null)
                {
                    continue;
                }
                if (prefab.m_citizenAI.IsAnimal())
                {
                    animals.Add((ushort)index);
                }
            }
            return animals;
        }
    }
}
