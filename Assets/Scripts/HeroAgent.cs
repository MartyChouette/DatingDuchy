using UnityEngine;
using CozyTown.Build;
using CozyTown.Core;

namespace CozyTown.Sim
{
    public class HeroAgent : AgentBase
    {
        public int bravery = 5;     // 0..10
        public int greed = 5;       // 0..10
        public int damage = 3;

        public float attackRange = 0.9f;
        public float attackCooldown = 1.0f;

        private float _atkT;
        private float _bountyCheckT;

        private enum State { IdleAtTavern, SeekingBounty, Fighting, Celebrating }
        private State _state;

        private BountySystem.Bounty _bounty;
        private MonsterAgent _targetMonster;

        private const float BountyCheckInterval = 0.5f;

        protected override void Awake()
        {
            base.Awake();
            EmitSpawn(GameEvent.KindHero);
            _state = State.IdleAtTavern;

            // Randomize a bit
            bravery = Mathf.Clamp(bravery + Random.Range(-2, 3), 0, 10);
            greed = Mathf.Clamp(greed + Random.Range(-2, 3), 0, 10);

            // Stagger bounty checks so heroes don't all check on the same frame
            _bountyCheckT = Random.Range(0f, BountyCheckInterval);
        }

        private void Update()
        {
            switch (_state)
            {
                case State.IdleAtTavern:
                    GoToTavern();
                    _bountyCheckT -= Time.deltaTime;
                    if (BountySystem.Instance != null && _bountyCheckT <= 0f)
                    {
                        _bountyCheckT = BountyCheckInterval;
                        _state = State.SeekingBounty;
                    }
                    break;

                case State.SeekingBounty:
                    if (TryAcceptAnyBounty())
                        _state = State.Fighting;
                    else
                        _state = State.IdleAtTavern;
                    break;

                case State.Fighting:
                    TickFight();
                    break;

                case State.Celebrating:
                    TickCelebrate();
                    break;
            }
        }

        private void GoToTavern()
        {
            var tavern = BuildingWorldRegistry.FindNearest(BuildingKind.Tavern, transform.position);
            if (tavern != null)
            {
                mover.SetTarget(tavern.transform.position);
                mover.TickMove();
            }
        }

        private bool TryAcceptAnyBounty()
        {
            if (BountySystem.Instance == null) return false;

            // Greedy heroes check more often; brave heroes accept even if far (we�ll add distance later).
            if (!BountySystem.Instance.TryAcceptBounty(pid.id, out _bounty))
                return false;

            // Find target monster by ID (cheap search for prototype)
            _targetMonster = FindMonsterById(_bounty.targetMonsterId);
            if (_targetMonster == null)
            {
                BountySystem.Instance.FailBounty(_bounty);
                _bounty = null;
                return false;
            }

            return true;
        }

        private MonsterAgent FindMonsterById(int id)
        {
            return FindAgentById<MonsterAgent>(id);
        }

        private void TickFight()
        {
            if (_bounty == null)
            {
                _state = State.IdleAtTavern;
                return;
            }

            if (_targetMonster == null)
            {
                // Someone else killed it or it despawned
                if (BountySystem.Instance != null) BountySystem.Instance.FailBounty(_bounty);
                _bounty = null;
                _state = State.IdleAtTavern;
                return;
            }

            mover.SetTarget(_targetMonster.transform.position);
            bool arrived = mover.TickMove();

            _atkT -= Time.deltaTime;

            if (arrived && _atkT <= 0f)
            {
                float d = Vector3.Distance(transform.position, _targetMonster.transform.position);
                if (d <= attackRange)
                {
                    _atkT = attackCooldown;
                    _targetMonster.TakeDamage(damage);

                    if (_targetMonster == null)
                    {
                        GameEventBus.Emit(GameEvent.Make(GameEventType.MonsterKilled, aId: pid.id, world: transform.position));
                        if (BountySystem.Instance != null) BountySystem.Instance.CompleteBounty(_bounty);

                        // Reward paid from treasury via ledger (MetricsLedger will subtract)
                        gold += _bounty.reward;

                        _bounty = null;
                        _state = State.Celebrating;
                    }
                }
            }
        }

        private void TickCelebrate()
        {
            var tavern = BuildingWorldRegistry.FindNearest(BuildingKind.Tavern, transform.position);
            if (tavern == null)
            {
                _state = State.IdleAtTavern;
                return;
            }

            mover.SetTarget(tavern.transform.position);
            bool arrived = mover.TickMove();
            if (!arrived) return;

            var tavernRules = tavern.GetComponent<Tavern>();
            int min = tavernRules != null ? tavernRules.spendMin : 1;
            int max = tavernRules != null ? tavernRules.spendMax : 5;

            int spend = Random.Range(min, max + 1);

            spend = Mathf.Min(spend, gold);
            if (spend > 0)
            {
                gold -= spend;
                GameEventBus.Emit(GameEvent.Make(GameEventType.GoldSpent, amount: spend, aId: pid.id, world: tavern.transform.position, text: "Tavern"));
            }

            // after a brief �celebration�
            _state = State.IdleAtTavern;
        }

        protected override string GetAgentKindName() => GameEvent.KindHero;
    }
}
