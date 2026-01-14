using UnityEngine;

namespace CozyTown.Build
{
    public static class BuildingSimInstaller
    {
        public static void EnsureBehaviors(BuildingInstance inst)
        {
            if (inst == null || inst.Def == null) return;

            // Clear old sim components when reinitializing (optional)
            RemoveIfExists<Sim.JobProvider>(inst);
            RemoveIfExists<Sim.HouseCapacity>(inst);
            RemoveIfExists<Sim.KnightsGuildSpawner>(inst);
            RemoveIfExists<Sim.MonsterHiveSpawner>(inst);
            RemoveIfExists<Sim.Tavern>(inst);

            // Install based on kind
            switch (inst.Def.kind)
            {
                case BuildingKind.Market:
                case BuildingKind.Temple:
                    {
                        var jp = inst.gameObject.AddComponent<Sim.JobProvider>();
                        jp.jobSlots = Mathf.Max(0, inst.Def.jobSlots);
                        break;
                    }

                case BuildingKind.House:
                    {
                        var hc = inst.gameObject.AddComponent<Sim.HouseCapacity>();
                        hc.beds = Mathf.Max(1, inst.Def.housingBeds);
                        break;
                    }

                case BuildingKind.KnightsGuild:
                    {
                        var kg = inst.gameObject.AddComponent<Sim.KnightsGuildSpawner>();
                        kg.heroPrefab = inst.Def.heroPrefab;
                        kg.spawnEverySeconds = inst.Def.spawnEverySeconds;
                        break;
                    }

                case BuildingKind.MonsterHive:
                    {
                        var mh = inst.gameObject.AddComponent<Sim.MonsterHiveSpawner>();
                        mh.monsterPrefab = inst.Def.monsterPrefab;
                        mh.spawnEverySeconds = inst.Def.spawnEverySeconds;
                        break;
                    }

                case BuildingKind.Tavern:
                    {
                        var jp = inst.gameObject.AddComponent<Sim.JobProvider>();
                        jp.jobSlots = Mathf.Max(0, inst.Def.jobSlots);

                        var t = inst.gameObject.AddComponent<Sim.Tavern>();
                        t.spendMin = Mathf.Max(0, inst.Def.tavernSpendMin);
                        t.spendMax = Mathf.Max(t.spendMin, inst.Def.tavernSpendMax);

                        break;
                    }
            }
        }

        private static void RemoveIfExists<T>(BuildingInstance inst) where T : Component
        {
            var c = inst.GetComponent<T>();
            if (c != null) Object.Destroy(c);
        }
    }
}
