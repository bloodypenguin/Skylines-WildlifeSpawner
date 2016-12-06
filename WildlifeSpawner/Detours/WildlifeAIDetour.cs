using System.Runtime.CompilerServices;
using UnityEngine;
using WildlifeSpawner.Redirection;

namespace WildlifeSpawner.Detours
{
    [TargetType(typeof(WildlifeAI))]
    public class WildlifeAIDetour
    {
        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsFreePosition(WildlifeAI ai, Vector3 position)
        {
            UnityEngine.Debug.Log("IsFreePosition");
            return false;
        }
    }
}