using System.Collections.Generic;
using UnityEngine;
using CozyTown.Build;
using CozyTown.Core;

namespace CozyTown.Sim
{
    public class TaxCollectorAgent : AgentBase
    {
        public float collectEverySeconds = 8f;
        public float dwellSeconds = 0.6f;

        private readonly List<BuildingInstance> _route = new List<BuildingInstance>(64);
        private int _i;
        private float _t;
        private float _dwell;

        protected override void Awake()
        {
            base.Awake();
            EmitSpawn("TaxCollector");
            gold = 0;
        }

        private void Start()
        {
            RebuildRoute();
        }

        private void Update()
        {
            _t += Time.deltaTime;
            if (_t >= collectEverySeconds)
            {
                _t = 0f;
                RebuildRoute();
            }

            if (_route.Count == 0) return;

            // Remove destroyed buildings from the route
            while (_route.Count > 0 && _i < _route.Count && _route[_i] == null)
            {
                _route.RemoveAt(_i);
                if (_route.Count > 0 && _i >= _route.Count)
                    _i = 0;
            }
            if (_route.Count == 0) return;

            var b = _route[_i];
            mover.SetTarget(b.transform.position);
            bool arrived = mover.TickMove();

            if (!arrived) return;

            _dwell -= Time.deltaTime;
            if (_dwell > 0f) return;

            _dwell = dwellSeconds;

            if (b.taxValue > 0)
            {
                GameEventBus.Emit(GameEvent.Make(GameEventType.TaxCollected, amount: b.taxValue, aId: pid.id, world: b.transform.position, text: b.kind.ToString()));
            }

            _i++;
            if (_i >= _route.Count)
            {
                // Finished route — wait for next rebuild timer before collecting again
                _route.Clear();
                _i = 0;
            }
        }

        private void RebuildRoute()
        {
            _route.Clear();

            // Collect from Market + Tavern + Houses for now
            foreach (var b in BuildingWorldRegistry.All)
            {
                if (b == null) continue;
                if (b.kind == BuildingKind.Market || b.kind == BuildingKind.Tavern || b.kind == BuildingKind.House)
                    _route.Add(b);
            }

            _i = 0;
        }

        protected override string GetAgentKindName() => "TaxCollector";
    }
}
