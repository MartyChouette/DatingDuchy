using System.Collections.Generic;
using UnityEngine;
using CozyTown.Grid;

namespace CozyTown.Build
{
    [CreateAssetMenu(menuName = "CozyTown/Building Definition")]
    public class BuildingDefinition : ScriptableObject
    {
        public string id = "house";
        public string displayName = "House";

        [Header("Kind (core)")]
        public BuildingKind kind = BuildingKind.Other;

        [Header("Footprint (axial offsets)")]
        public List<HexCoordOffset> footprint = new List<HexCoordOffset> { new HexCoordOffset(0, 0) };

        [Header("Tags / Hooks")]
        public bool isRoad = false;
        public bool isShop = false;
        public bool isSocialNode = false;

        [Header("Tavern Hooks")]
        public int tavernSpendMin = 1;
        public int tavernSpendMax = 5;


        [Header("Simulation Hooks (use now)")]
        [Tooltip("If > 0, this building creates job demand. Player places jobs; jobs pull people.")]
        public int jobSlots = 0;

        [Tooltip("If > 0, this building provides housing capacity. Houses are auto-spawned by the sim.")]
        public int housingBeds = 0;

        [Tooltip("How much tax a collector receives when visiting this building (toy Majesty model).")]
        public int taxValue = 2;

        [Tooltip("Optional: later, beauty can influence 'pretty path' and desirability.")]
        public float beauty = 0f;

        [Tooltip("Optional: later, utility can influence 'speedy path' and industry attraction.")]
        public float utility = 0f;

        [Header("Spawner Hooks (use now for guild/hive)")]
        public GameObject heroPrefab;          // KnightsGuild
        public GameObject monsterPrefab;       // MonsterHive
        public float spawnEverySeconds = 10f;

        [Header("Prefab / Visuals")]
        public GameObject prefab;                  // optional, can be null for primitives
        public Vector3 visualOffset = Vector3.zero;
        public Vector3 visualScale = Vector3.one;
    }

    [System.Serializable]
    public struct HexCoordOffset
    {
        public int dq;
        public int dr;
        public HexCoordOffset(int dq, int dr) { this.dq = dq; this.dr = dr; }
        public HexCoord ToHex() => new HexCoord(dq, dr);
    }
}
