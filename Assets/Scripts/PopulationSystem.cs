using UnityEngine;
using CozyTown.Build;

namespace CozyTown.Sim
{
    public class PopulationSystem : MonoBehaviour
    {
        public BuildingRegistry registry;

        [Header("Prefabs")]
        public GameObject peasantPrefab;

        [Header("House Growth")]
        public BuildingDefinition houseDefinition;  // kind=House, beds>0, visual prefab set
        public Transform buildingsParent;          // same parent you use for placed buildings

        [Header("Rates")]
        public float immigrationCheckEvery = 3f;
        public float housingCheckEvery = 3f;

        [Header("Rules")]
        public int peasantsPerJobSlot = 1;  // simplest: 1 peasant per job slot
        public float spawnRadius = 3f;

        float _immT, _houseT;

        void Update()
        {
            if (registry == null) return;

            _immT += Time.deltaTime;
            _houseT += Time.deltaTime;

            if (_immT >= immigrationCheckEvery)
            {
                _immT = 0f;
                TryImmigrate();
            }

            if (_houseT >= housingCheckEvery)
            {
                _houseT = 0f;
                TryGrowHousing();
            }
        }

        int CountJobSlots()
        {
            int total = 0;
            foreach (var b in registry.Buildings)
            {
                if (b == null || b.Def == null) continue;
                var jp = b.GetComponent<JobProvider>();
                if (jp != null) total += Mathf.Max(0, jp.jobSlots);
            }
            return total;
        }

        int CountBeds()
        {
            int total = 0;
            foreach (var b in registry.Buildings)
            {
                if (b == null || b.Def == null) continue;
                if (b.Def.kind != BuildingKind.House) continue;

                var hc = b.GetComponent<HouseCapacity>();
                if (hc != null) total += Mathf.Max(1, hc.beds);
            }
            return total;
        }

        int CountPeasants() => FindObjectsOfType<PeasantAgent>().Length;

        Vector3 TownAnchor()
        {
            // Prefer TownHall if exists
            foreach (var b in registry.Buildings)
                if (b != null && b.Def != null && b.Def.kind == BuildingKind.TownHall)
                    return b.transform.position;

            // Else any building
            foreach (var b in registry.Buildings)
                if (b != null) return b.transform.position;

            return Vector3.zero;
        }

        void TryImmigrate()
        {
            if (peasantPrefab == null) return;

            int jobSlots = CountJobSlots();
            int desiredPeasants = jobSlots * peasantsPerJobSlot;
            int peasants = CountPeasants();

            if (peasants >= desiredPeasants) return;

            Vector3 basePos = TownAnchor();
            Vector3 p = basePos + new Vector3(Random.Range(-spawnRadius, spawnRadius), 0f, Random.Range(-spawnRadius, spawnRadius));
            Instantiate(peasantPrefab, p, Quaternion.identity);
        }

        void TryGrowHousing()
        {
            if (houseDefinition == null) return;

            int peasants = CountPeasants();
            int beds = CountBeds();
            if (beds >= peasants) return;

            // Spawn a new house building instance automatically.
            // We place it “freeform” for now (not snapping to hex yet).
            // Later we’ll pick an empty hex and call registry.Place(def, hex, rot,...)
            Vector3 basePos = TownAnchor();
            Vector3 p = basePos + new Vector3(Random.Range(-spawnRadius * 1.5f, spawnRadius * 1.5f), 0f, Random.Range(-spawnRadius * 1.5f, spawnRadius * 1.5f));

            // Create a building instance object using same pattern as registry does
            var go = new GameObject($"AutoHouse_{houseDefinition.id}");
            if (buildingsParent != null) go.transform.SetParent(buildingsParent, true);

            var inst = go.AddComponent<BuildingInstance>();
            inst.Initialize(houseDefinition, origin: new CozyTown.Grid.HexCoord(0, 0), rotationSteps: 0); // origin is not meaningful yet for freeform
            go.transform.position = p;
        }
    }
}
