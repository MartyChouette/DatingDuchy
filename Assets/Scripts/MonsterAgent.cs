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
        private BuildingInstance _cachedTarget;
        private float _targetRefreshT;
        private const float TargetRefreshInterval = 2f;

        protected override void Awake()
        {
            base.Awake();
            EmitSpawn("Monster");
            GameEventBus.Emit(GameEvent.Make(GameEventType.MonsterSpawned, aId: pid.id, world: transform.position));
        }

        private void Update()
        {
            _targetRefreshT -= Time.deltaTime;
            if (_cachedTarget == null || _targetRefreshT <= 0f)
            {
                _targetRefreshT = TargetRefreshInterval;
                _cachedTarget = BuildingWorldRegistry.FindNearest(BuildingKind.TownHall, transform.position)
                             ?? BuildingWorldRegistry.FindNearest(BuildingKind.House, transform.position)
                             ?? BuildingWorldRegistry.FindNearest(BuildingKind.Market, transform.position);
            }

            if (_cachedTarget != null)
                mover.SetTarget(_cachedTarget.transform.position);

            bool arrived = mover.TickMove();

            _atkT -= Time.deltaTime;
            if (arrived && _cachedTarget != null && _atkT <= 0f)
            {
                _atkT = attackCooldown;

                // For now: �damage building� is a ledger event hook
                GameEventBus.Emit(GameEvent.Make(GameEventType.Note, aId: pid.id, world: transform.position, text: $"Monster hit {_cachedTarget.kind}"));
            }
        }

        protected override string GetAgentKindName() => "Monster";
    }
}
