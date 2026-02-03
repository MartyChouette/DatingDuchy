using System.Collections.Generic;
using UnityEngine;

namespace CozyTown.Build
{
    public static class BuildingWorldRegistry
    {
        private static readonly List<BuildingInstance> _all = new List<BuildingInstance>(256);
        private static readonly HashSet<BuildingInstance> _set = new HashSet<BuildingInstance>(256);
        public static IReadOnlyList<BuildingInstance> All => _all;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { _all.Clear(); _set.Clear(); }

        public static void Register(BuildingInstance b)
        {
            if (b == null) return;
            if (_set.Add(b)) _all.Add(b);
        }

        public static void Unregister(BuildingInstance b)
        {
            if (b == null) return;
            if (_set.Remove(b)) _all.Remove(b);
        }

        public static BuildingInstance FindNearest(BuildingKind kind, Vector3 pos)
        {
            BuildingInstance best = null;
            float bestSqr = float.PositiveInfinity;

            for (int i = 0; i < _all.Count; i++)
            {
                var inst = _all[i];
                if (inst == null || inst.Def == null) continue;
                if (inst.Def.kind != kind) continue;

                float d = (inst.transform.position - pos).sqrMagnitude;
                if (d < bestSqr)
                {
                    bestSqr = d;
                    best = inst;
                }
            }
            return best;
        }
    }
}
