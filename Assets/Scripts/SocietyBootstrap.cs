using UnityEngine;
using CozyTown.Build;
using CozyTown.Core;

namespace CozyTown.Sim
{
    public class SocietyBootstrap : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject peasantPrefab;
        public GameObject taxCollectorPrefab;

        [Header("Spawn")]
        public int startPeasants = 6;

        [Header("Economy")]
        public int startTreasury = 200;

        private bool _did;

        private void Start()
        {
            if (_did) return;
            _did = true;

            // Seed treasury
            GameEventBus.Emit(GameEvent.Make(GameEventType.GoldAdded, amount: startTreasury, text: "StartTreasury"));

            // Spawn peasants near houses (or origin if none)
            for (int i = 0; i < startPeasants; i++)
                SpawnPeasant();

            // Spawn tax collector near TownHall
            if (taxCollectorPrefab != null)
            {
                var hall = BuildingWorldRegistry.FindNearest(BuildingKind.TownHall, Vector3.zero);
                Vector3 p = hall != null ? hall.transform.position : Vector3.zero;
                p += new Vector3(Random.Range(-1.0f, 1.0f), 0f, Random.Range(-1.0f, 1.0f));
                Instantiate(taxCollectorPrefab, p, Quaternion.identity);
            }

            GameEventBus.Emit(GameEvent.Make(GameEventType.Note, text: "SocietyBootstrap complete"));
        }

        private void SpawnPeasant()
        {
            if (peasantPrefab == null) return;

            var house = BuildingWorldRegistry.FindNearest(BuildingKind.House, Vector3.zero);
            Vector3 p = house != null ? house.transform.position : Vector3.zero;
            p += new Vector3(Random.Range(-2.0f, 2.0f), 0f, Random.Range(-2.0f, 2.0f));
            Instantiate(peasantPrefab, p, Quaternion.identity);
        }
    }
}
