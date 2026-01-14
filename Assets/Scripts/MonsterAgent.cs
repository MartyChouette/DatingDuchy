using UnityEngine;
using CozyTown.Build;
using CozyTown.Core;

namespace CozyTown.Sim
{
    public class MonsterAgent : AgentBase
    {
        public int damage = 2;
        public float attackRange = 0.7f;
        public float attackCooldown = 1.25f;

        private float _atkT;

        protected override void Awake()
        {
            base.Awake();
            EmitSpawn("Monster");
            GameEventBus.Emit(GameEvent.Make(GameEventType.MonsterSpawned, aId: pid.id, world: transform.position));
        }

        private void Update()
        {
            // Simple: wander toward nearest TownHall/House/Market if exists
            var townHall = BuildingWorldRegistry.FindNearest(BuildingKind.TownHall, transform.position);
            var house = BuildingWorldRegistry.FindNearest(BuildingKind.House, transform.position);
            var market = BuildingWorldRegistry.FindNearest(BuildingKind.Market, transform.position);

            BuildingInstance target = townHall ?? house ?? market;
            if (target != null)
                mover.SetTarget(target.transform.position);

            bool arrived = mover.TickMove();

            _atkT -= Time.deltaTime;
            if (arrived && target != null && _atkT <= 0f)
            {
                _atkT = attackCooldown;

                // For now: “damage building” is a ledger event hook
                GameEventBus.Emit(GameEvent.Make(GameEventType.Note, aId: pid.id, world: transform.position, text: $"Monster hit {target.kind}"));
            }
        }

        protected override string GetAgentKindName() => "Monster";
    }
}
