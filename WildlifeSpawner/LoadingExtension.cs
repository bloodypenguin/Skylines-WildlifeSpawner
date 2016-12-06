using System;
using System.Linq;
using System.Reflection;
using ICities;
using UnityEngine;
using WildlifeSpawner.Detours;
using WildlifeSpawner.Redirection;

namespace WildlifeSpawner
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            Redirector<WildlifeSpawnPointAIDetour>.Deploy();
            Redirector<LivestockAIDetour>.Deploy();
            Redirector<WildlifeAIDetour>.Deploy();
            Redirector<PetAIDetour>.Deploy();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            Redirector<WildlifeSpawnPointAIDetour>.Revert();
            Redirector<LivestockAIDetour>.Revert();
            Redirector<WildlifeAIDetour>.Revert();
            Redirector<PetAIDetour>.Revert();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            WildlifeSpawnerMonitor.Initialize();
            ModUI.Initialize();

            PatchDogAndPark();
        }

        private static void PatchDogAndPark()
        {
            var dogPark = PrefabCollection<BuildingInfo>.FindLoaded("dog-park-fence");
            if (dogPark != null)
            {
                var ai = new DogParkAI();
                var fields =
                    typeof(ParkAI).GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance |
                                             BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    field.SetValue(ai, field.GetValue(dogPark.m_buildingAI));
                }
                dogPark.m_buildingAI = ai;

                var avenueLight = PrefabCollection<PropInfo>.FindLoaded("Avenue Light");
                if (avenueLight != null)
                {
                    var props = dogPark.m_props.ToList();
                    props.Add(
                       new BuildingInfo.Prop()
                       {
                           m_finalProp = avenueLight,
                           m_position = Vector3.zero,
                           m_probability = Int32.MaxValue,
                           m_prop = avenueLight
                       }
                   );
                    dogPark.m_props = props.ToArray();
                }
            }

            var dog = PrefabCollection<CitizenInfo>.FindLoaded("Dog");
            if (dog != null)
            {
                dog.m_color1 = Color.black;
                dog.m_color2 = new Color(0.15f, 0.15f, 0.15f);
                dog.m_color3 = new Color(0.4f, 0.35f, 0.07f);
            }
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            WildlifeSpawnerMonitor.Dispose();
            ModUI.Dispose();
        }
    }
}