using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CozyTown.Core
{
    public enum LogCategory { Romance, Social, Economy, Combat, Building }

    public struct LogEntry
    {
        public float time;
        public LogCategory category;
        public string text;
    }

    public class TownLog : MonoBehaviour
    {
        public static TownLog Instance { get; private set; }

        readonly List<LogEntry> _entries = new(512);
        public int maxEntries = 200;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Instance = null;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable() => GameEventBus.OnEvent += OnGameEvent;
        void OnDisable() => GameEventBus.OnEvent -= OnGameEvent;

        public void Push(LogCategory cat, string text)
        {
            _entries.Add(new LogEntry
            {
                time = Time.time,
                category = cat,
                text = text
            });
            while (_entries.Count > maxEntries)
                _entries.RemoveAt(0);
        }

        void OnGameEvent(GameEvent e)
        {
            switch (e.type)
            {
                case GameEventType.BountyPosted:
                    Push(LogCategory.Combat, "A bounty was posted on a monster.");
                    break;
                case GameEventType.BountyCompleted:
                    Push(LogCategory.Combat, "A hero completed a bounty.");
                    break;
                case GameEventType.MonsterKilled:
                    Push(LogCategory.Combat, "A monster was slain.");
                    break;
                case GameEventType.BuildingDestroyed:
                    Push(LogCategory.Building, $"{e.text} was destroyed!");
                    break;
                case GameEventType.PersonDied:
                    Push(LogCategory.Combat, $"Someone has perished.");
                    break;
                // Romance/Social handled by RelationshipSystem calling Push() directly
                // BuildingDamaged skipped (too spammy)
            }
        }

        public void AppendRecent(StringBuilder sb, int count)
        {
            int start = Mathf.Max(0, _entries.Count - count);
            for (int i = start; i < _entries.Count; i++)
                sb.AppendLine("- " + _entries[i].text);
        }

        public IReadOnlyList<LogEntry> Entries => _entries;
    }
}
