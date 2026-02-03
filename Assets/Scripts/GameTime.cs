using UnityEngine;

namespace CozyTown.Core
{
    public enum TimeSpeed { Paused, Half, Normal, Fast1, Fast2, Fast3 }

    public enum DayPhase
    {
        EarlyMorning,
        Morning,
        Noon,
        EarlyAfternoon,
        Afternoon,
        LateAfternoon,
        Night,
        Midnight,
        LateNight
    }

    public class GameTime : MonoBehaviour
    {
        public static GameTime Instance { get; private set; }

        [Header("Day Length")]
        [Tooltip("Real-time minutes for one full day at Normal speed.")]
        public float minutesPerDayAtNormal = 25f;

        [Header("Speed Multipliers")]
        public float halfMult = 0.5f;
        public float normalMult = 1f;
        public float fast1Mult = 2f;
        public float fast2Mult = 4f;
        public float fast3Mult = 8f;

        [Header("State")]
        public TimeSpeed speed = TimeSpeed.Normal;
        [Range(0f, 1f)] public float dayTime01 = 0f; // 0..1 wraps

        public int day = 1;
        public int year = 1;
        public int daysPerYear = 120;

        public DayPhase CurrentPhase => PhaseFrom(dayTime01);

        float _realSecondsPerDay;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _realSecondsPerDay = Mathf.Max(1f, minutesPerDayAtNormal * 60f);
        }

        void Update()
        {
            float mult = SpeedMultiplier(speed);
            if (mult <= 0f) return;

            // Advance game-day clock; day length scales with speed multiplier
            float dt = Time.deltaTime * mult;
            dayTime01 += dt / _realSecondsPerDay;

            // Guard against large deltaTime spikes skipping multiple days
            while (dayTime01 >= 1f)
            {
                dayTime01 -= 1f;
                AdvanceDay();
            }
        }

        public float SpeedMultiplier(TimeSpeed s) => s switch
        {
            TimeSpeed.Paused => 0f,
            TimeSpeed.Half => halfMult,
            TimeSpeed.Normal => normalMult,
            TimeSpeed.Fast1 => fast1Mult,
            TimeSpeed.Fast2 => fast2Mult,
            TimeSpeed.Fast3 => fast3Mult,
            _ => 1f
        };

        public void SetSpeed(TimeSpeed s) => speed = s;

        void AdvanceDay()
        {
            day++;
            if (day > daysPerYear) { day = 1; year++; }
        }

        public static DayPhase PhaseFrom(float t01)
        {
            // 9 segments across the day
            // (t01 in [0..1))
            float seg = Mathf.Repeat(t01, 1f) * 9f;
            int i = Mathf.FloorToInt(seg);

            return (DayPhase)Mathf.Clamp(i, 0, 8);
        }
    }
}
