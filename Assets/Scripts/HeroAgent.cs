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

        [SerializeField] private float distanceNormalizer = 20f;

        private float _bountyCheckInterval;

        protected override void Awake()
        {
            base.Awake();
            EmitSpawn(GameEvent.KindHero);
            _state = State.IdleAtTavern;

            // Randomize a bit
            bravery = Mathf.Clamp(bravery + Random.Range(-2, 3), 0, 10);
            greed = Mathf.Clamp(greed + Random.Range(-2, 3), 0, 10);

            // Greedier heroes check for bounties more eagerly
            _bountyCheckInterval = 0.7f - greed * 0.04f;

            // Stagger bounty checks so heroes don't all check on the same frame
            _bountyCheckT = Random.Range(0f, _bountyCheckInterval);
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
                        _bountyCheckT = _bountyCheckInterval;
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

            var bounties = BountySystem.Instance.ActiveBounties;
            BountySystem.Bounty best = null;
            float bestScore = 0f;

            for (int i = 0; i < bounties.Count; i++)
            {
                var b = bounties[i];
                if (b == null || b.accepted) continue;

                float distance = Vector3.Distance(transform.position, b.targetPos);
                float rewardScore = b.reward * (1f + greed * 0.1f);
                float distancePenalty = (distance / distanceNormalizer) * (1f - bravery * 0.08f);
                float score = rewardScore - distancePenalty;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = b;
                }
            }

            if (best == null) return false;

            BountySystem.Instance.AcceptBounty(pid.id, best);
            _bounty = best;

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

                    // Check hp instead of null — Destroy is deferred so null check fails same-frame
                    if (_targetMonster != null && _targetMonster.hp <= 0)
                    {
                        GameEventBus.Emit(GameEvent.Make(GameEventType.MonsterKilled, aId: pid.id, world: transform.position));
                        if (BountySystem.Instance != null) BountySystem.Instance.CompleteBounty(_bounty);

                        // Reward paid from treasury via ledger (MetricsLedger will subtract)
                        gold += _bounty.reward;

                        _bounty = null;
                        _targetMonster = null;
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
