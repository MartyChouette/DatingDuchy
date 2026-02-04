using System.Text;
using CozyTown.Core;

namespace CozyTown.UI
{
    public static class WorldUpdateReportBuilder
    {
        public static string Build(WorldUpdateKind kind)
        {
            var sb = new StringBuilder(4096);

            var t = GameTime.Instance;
            var m = MetricsLedger.Instance;
            var log = TownLog.Instance;

            sb.AppendLine("<b>World Update</b>");
            if (t != null) sb.AppendLine($"Year {t.year}, Day {t.day}");
            sb.AppendLine();

            // City summary
            if (m != null)
            {
                sb.AppendLine("<b>City</b>");
                sb.AppendLine($"Treasury: {m.treasury}");
                sb.AppendLine($"Population: Peasants {m.peasants} | Heroes {m.heroes} | Monsters {m.monsters}");
                sb.AppendLine($"Bounties: Posted {m.bountiesPosted} | Completed {m.bountiesCompleted}");
                sb.AppendLine($"Taxes Collected: {m.totalTaxesCollected}");
                sb.AppendLine($"Spent at Tavern: {m.totalGoldSpentAtTavern}");
                sb.AppendLine();
            }

            // Recent highlights from all systems
            sb.AppendLine("<b>Recent Highlights</b>");
            if (log != null)
            {
                int count = kind == WorldUpdateKind.FestivalMidyear ? 6 : 10;
                log.AppendRecent(sb, count);
            }
            else
            {
                sb.AppendLine("(TownLog not in scene yet.)");
            }
            sb.AppendLine();

            // Flavor
            sb.AppendLine(kind switch
            {
                WorldUpdateKind.FestivalMidyear => "<i>The city celebrates. Bonds shift in the crowd.</i>",
                WorldUpdateKind.FestivalYearEnd => "<i>A year closes. Some hearts harden. Some open.</i>",
                WorldUpdateKind.NewYear => "<i>A new year begins. Old debts remain. New chances appear.</i>",
                _ => ""
            });

            return sb.ToString();
        }
    }
}
