using UnityEngine;

namespace CozyTown.Core
{
    /// <summary>
    /// Festival event emitter. Delegates day/year tracking to GameTime (single source of truth).
    /// </summary>
    public class GameCalendar : MonoBehaviour
    {
        public static GameCalendar Instance { get; private set; }

        [Header("Festivals")]
        public int festivalDayA = 60;   // biannual
        public int festivalDayB = 120;  // year end / big one

        int _lastDay = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            var t = GameTime.Instance;
            if (t == null) return;

            if (t.day == _lastDay) return;
            _lastDay = t.day;

            if (t.day == festivalDayA)
                GameEventBus.Emit(GameEvent.Make(GameEventType.Note, text: $"Festival (Midyear) Day {festivalDayA}"));

            if (t.day == festivalDayB)
                GameEventBus.Emit(GameEvent.Make(GameEventType.Note, text: $"Festival (Year-End) Day {festivalDayB}"));

            if (t.day == 1)
                GameEventBus.Emit(GameEvent.Make(GameEventType.Note, text: $"New Year: Year {t.year}"));
        }
    }
}
