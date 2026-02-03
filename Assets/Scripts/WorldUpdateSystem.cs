using UnityEngine;
using CozyTown.Core;

namespace CozyTown.UI
{
    public enum WorldUpdateKind { FestivalMidyear, FestivalYearEnd, NewYear }

    /// <summary>
    /// Pops the world-update panel on specific days (biannual festivals + new year),
    /// driven by GameTime (the single source of truth for day/year).
    /// </summary>
    public class WorldUpdateSystem : MonoBehaviour
    {
        public WorldUpdatePanel panel;

        [Header("Schedule (days in year)")]
        public int festivalDayA = 60;
        public int festivalDayB = 120;

        int _lastDay = -1;
        int _lastYear = -1;

        void Update()
        {
            var t = GameTime.Instance;
            if (t == null) return;

            // fire once per day transition
            if (t.day == _lastDay && t.year == _lastYear) return;
            _lastDay = t.day;
            _lastYear = t.year;

            if (t.day == 1) Show(WorldUpdateKind.NewYear);
            if (t.day == festivalDayA) Show(WorldUpdateKind.FestivalMidyear);
            if (t.day == festivalDayB) Show(WorldUpdateKind.FestivalYearEnd);
        }

        void Show(WorldUpdateKind kind)
        {
            if (panel == null) return;
            panel.Show(WorldUpdateReportBuilder.Build(kind));
        }
    }
}
