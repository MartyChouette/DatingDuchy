using UnityEngine;
using CozyTown.Core;
using CozyTown.Grid;

namespace CozyTown.Build
{
    public class BuildingInstance : MonoBehaviour
    {
        public BuildingDefinition Def { get; private set; }
        public HexCoord Origin { get; private set; }
        public int RotationSteps { get; private set; }
        public BuildingKind kind => Def != null ? Def.kind : BuildingKind.Other;
        public int taxValue => (Def != null ? Def.taxValue : 0) + Mathf.FloorToInt(beautyBonus * 0.5f);

        // Adjacency bonuses (summed from neighboring buildings)
        public float beautyBonus;
        public float utilityBonus;

        // Future runtime state hooks:
        public float owedTax = 0f;

        // HP system
        public int hp;
        public int maxHP;
        public BuildingRegistry Registry { get; set; }

        public void Initialize(BuildingDefinition def, HexCoord origin, int rotationSteps)
        {

            Def = def;
            Origin = origin;
            RotationSteps = ((rotationSteps % 6) + 6) % 6;

            maxHP = def.maxHP;
            hp = maxHP;

            // Clear old children if any
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            GameObject visualGo;

            if (def.prefab != null)
            {
                visualGo = Instantiate(def.prefab, transform);

            }
            else
            {
                visualGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visualGo.transform.SetParent(transform, false);

            }

            visualGo.name = "Visual";
            visualGo.transform.localPosition = def.visualOffset;
            visualGo.transform.localScale = def.visualScale;
            BuildingSimInstaller.EnsureBehaviors(this);


        }

        public void RecalculateAdjacencyBonuses()
        {
            if (Registry == null) return;
            var neighbors = Registry.GetAdjacentBuildings(this);
            beautyBonus = 0f;
            utilityBonus = 0f;
            foreach (var n in neighbors)
            {
                if (n.Def != null)
                {
                    beautyBonus += n.Def.beauty;
                    utilityBonus += n.Def.utility;
                }
            }
            // Utility bonus adds to maxHP (rebuild from base)
            int baseHP = Def != null ? Def.maxHP : 0;
            if (baseHP > 0)
            {
                int bonusHP = Mathf.FloorToInt(utilityBonus * 2f);
                int newMax = baseHP + bonusHP;
                if (newMax != maxHP)
                {
                    maxHP = newMax;
                    hp = Mathf.Min(hp, maxHP);
                }
            }

            if (beautyBonus > 0 || utilityBonus > 0)
            {
                TownLog.Instance?.Push(LogCategory.Building,
                    $"{(Def != null ? Def.displayName : "Building")} benefits from nearby buildings (beauty +{beautyBonus:F0}, utility +{utilityBonus:F0}).");
            }
        }

        public void TakeDamage(int dmg)
        {
            if (maxHP == 0) return; // indestructible

            hp -= dmg;
            GameEventBus.Emit(GameEvent.Make(GameEventType.BuildingDamaged,
                amount: dmg, world: transform.position,
                text: Def != null ? Def.displayName : "Building"));

            if (hp <= 0)
                DestroyBuilding();
        }

        void DestroyBuilding()
        {
            GameEventBus.Emit(GameEvent.Make(GameEventType.BuildingDestroyed,
                world: transform.position,
                text: Def != null ? Def.displayName : "Building"));

            if (Registry != null)
            {
                var neighbors = Registry.GetAdjacentBuildings(this);
                Registry.Unregister(this);
                foreach (var n in neighbors)
                    n.RecalculateAdjacencyBonuses();
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            BuildingWorldRegistry.Unregister(this);
        }

    }


}
