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
        public int taxValue => Def != null ? Def.taxValue : 0;

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
                Registry.Unregister(this);

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            BuildingWorldRegistry.Unregister(this);
        }

    }


}
