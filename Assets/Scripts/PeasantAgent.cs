using UnityEngine;
using CozyTown.Build;
using CozyTown.Core;

namespace CozyTown.Sim
{
    public class PeasantAgent : AgentBase
    {
        public float waitAtNodeSeconds = 2.0f;

        private enum State { GoingHome, GoingMarket, GoingTavern, Waiting }
        private State _state;

        private BuildingInstance _home;
        private BuildingInstance _market;
        private BuildingInstance _tavern;

        private float _waitT;

        protected override void Awake()
        {
            base.Awake();
            EmitSpawn(GameEvent.KindPeasant);
            gold = Random.Range(1, 8);
        }

        private void Start()
        {
            _home = BuildingWorldRegistry.FindNearest(BuildingKind.House, transform.position);
            _market = BuildingWorldRegistry.FindNearest(BuildingKind.Market, transform.position);
            _tavern = BuildingWorldRegistry.FindNearest(BuildingKind.Tavern, transform.position);

            _state = State.GoingMarket;
        }

        private void Update()
        {
            if (_home == null) _home = BuildingWorldRegistry.FindNearest(BuildingKind.House, transform.position);
            if (_market == null) _market = BuildingWorldRegistry.FindNearest(BuildingKind.Market, transform.position);
            if (_tavern == null) _tavern = BuildingWorldRegistry.FindNearest(BuildingKind.Tavern, transform.position);

            switch (_state)
            {
                case State.GoingMarket:
                    GoTo(_market, State.Waiting);
                    break;

                case State.GoingTavern:
                    GoTo(_tavern, State.Waiting);
                    break;

                case State.GoingHome:
                    GoTo(_home, State.Waiting);
                    break;

                case State.Waiting:
                    _waitT -= Time.deltaTime;
                    if (_waitT <= 0f)
                        ChooseNext();
                    break;
            }
        }

        private void GoTo(BuildingInstance b, State arrivedState)
        {
            if (b == null)
            {
                _state = State.Waiting;
                _waitT = 1f;
                return;
            }

            mover.SetTarget(b.transform.position);
            bool arrived = mover.TickMove();
            if (!arrived) return;

            _state = arrivedState;
            _waitT = waitAtNodeSeconds;

            // Earn a small wage when returning home
            if (b.kind == BuildingKind.House)
            {
                int wage = Random.Range(2, 5);
                gold += wage;
            }

            // �Buy goods� at market (toy economy)
            if (b.kind == BuildingKind.Market && gold > 0)
            {
                int spend = Mathf.Min(gold, Random.Range(1, 3));
                gold -= spend;
                GameEventBus.Emit(GameEvent.Make(GameEventType.GoldSpent, amount: spend, aId: pid.id, world: b.transform.position, text: "Market"));
            }

            // Sometimes �hang out� at tavern too
            if (b.kind == BuildingKind.Tavern && gold > 0 && Random.value < 0.35f)
            {
                int spend = Mathf.Min(gold, Random.Range(1, 4));
                gold -= spend;
                GameEventBus.Emit(GameEvent.Make(GameEventType.GoldSpent, amount: spend, aId: pid.id, world: b.transform.position, text: "Tavern"));
            }
        }

        private void ChooseNext()
        {
            float r = Random.value;
            var gt = GameTime.Instance;

            if (gt != null && gt.IsNight)
            {
                // Late night: mostly go home, rarely tavern
                if (r < 0.05f) _state = State.GoingTavern;
                else _state = State.GoingHome;
            }
            else if (gt != null && gt.IsEvening)
            {
                // Evening: tavern-heavy
                if (r < 0.10f) _state = State.GoingMarket;
                else if (r < 0.70f) _state = State.GoingTavern;
                else _state = State.GoingHome;
            }
            else
            {
                // Daytime: market-heavy
                if (r < 0.50f) _state = State.GoingMarket;
                else if (r < 0.65f) _state = State.GoingTavern;
                else _state = State.GoingHome;
            }
        }

        protected override string GetAgentKindName() => GameEvent.KindPeasant;
    }
}
