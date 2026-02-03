using System.Collections.Generic;
using UnityEngine;

namespace CozyTown.Core
{
    /// <summary>
    /// Live counters derived from events. Fast to query for UI.
    /// </summary>
    public class MetricsLedger : MonoBehaviour
    {
        public static MetricsLedger Instance { get; private set; }

        [Header("Economy")]
        public int treasury;
        public int totalTaxesCollected;
        public int totalBountiesPaid;
        public int totalGoldSpentAtTavern;

        [Header("Population")]
        public int peasants;
        public int heroes;
        public int monsters;

        [Header("Bounties")]
        public int bountiesPosted;
        public int bountiesCompleted;

        private readonly Queue<GameEvent> _recent = new Queue<GameEvent>();
        public int recentEventLimit = 25;

        public IEnumerable<GameEvent> RecentEvents => _recent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable() => GameEventBus.OnEvent += OnGameEvent;
        private void OnDisable() => GameEventBus.OnEvent -= OnGameEvent;

        private void OnGameEvent(GameEvent e)
        {
            switch (e.type)
            {
                case GameEventType.GoldAdded:
                    treasury += e.amount;
                    break;

                case GameEventType.GoldSpent:
                    treasury -= e.amount;
                    if (!string.IsNullOrEmpty(e.text) && e.text.Contains("Tavern"))
                        totalGoldSpentAtTavern += e.amount;
                    break;

                case GameEventType.TaxCollected:
                    totalTaxesCollected += e.amount;
                    treasury += e.amount;
                    break;

                case GameEventType.BountyPosted:
                    bountiesPosted++;
                    break;

                case GameEventType.BountyCompleted:
                    bountiesCompleted++;
                    totalBountiesPaid += e.amount;
                    treasury -= e.amount;
                    break;

                case GameEventType.PersonSpawned:
                    if (e.text == GameEvent.KindPeasant) peasants++;
                    else if (e.text == GameEvent.KindHero) heroes++;
                    else if (e.text == GameEvent.KindMonster) monsters++;
                    break;

                case GameEventType.PersonDied:
                    if (e.text == GameEvent.KindPeasant) peasants = Mathf.Max(0, peasants - 1);
                    else if (e.text == GameEvent.KindHero) heroes = Mathf.Max(0, heroes - 1);
                    else if (e.text == GameEvent.KindMonster) monsters = Mathf.Max(0, monsters - 1);
                    break;

                case GameEventType.MonsterSpawned:
                    // also increments via PersonSpawned; keep for hooks
                    break;
            }

            _recent.Enqueue(e);
            while (_recent.Count > recentEventLimit) _recent.Dequeue();
        }
    }
}
