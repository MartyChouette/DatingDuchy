using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CozyTown.Sim
{
    public class BountyReportDebug : MonoBehaviour
    {
        public Key reportKey = Key.P;

        readonly StringBuilder _sb = new StringBuilder(2048);

        void Update()
        {
            if (Keyboard.current == null) return;
            if (!Keyboard.current[reportKey].wasPressedThisFrame) return;

            PrintReport();
        }

        void PrintReport()
        {
            var bs = BountySystem.Instance;
            if (bs == null)
            {
                Debug.Log("[BountyReport] No BountySystem in scene.");
                return;
            }

            _sb.Clear();
            _sb.AppendLine("=== BOUNTY REPORT ===");

            var list = bs.ActiveBounties;
            if (list == null || list.Count == 0)
            {
                _sb.AppendLine("No active bounties.");
                Debug.Log(_sb.ToString());
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                var b = list[i];

                var monster = FindMonsterById(b.targetMonsterId);
                var hero = b.accepted ? FindHeroById(b.acceptedByHeroId) : null;

                _sb.AppendLine($"Bounty #{b.bountyId} reward={b.reward} targetMonsterId={b.targetMonsterId} accepted={b.accepted}");

                if (monster != null)
                    _sb.AppendLine($"  Monster: HP {monster.hp}/{monster.maxHP}  DMG {monster.damage}  SPD {monster.mover.moveSpeed:0.0}");
                else
                    _sb.AppendLine($"  Monster: (missing/dead)");

                if (hero != null)
                    _sb.AppendLine($"  Hero:    HP {hero.hp}/{hero.maxHP}  DMG {hero.damage}  BRV {hero.bravery}  GRD {hero.greed}  SPD {hero.mover.moveSpeed:0.0}");
                else
                    _sb.AppendLine($"  Hero:    (not accepted)");

                if (hero != null && monster != null)
                {
                    // tiny �who wins� heuristic
                    float heroDps = hero.damage / Mathf.Max(0.1f, hero.attackCooldown);
                    float monDps = monster.damage / Mathf.Max(0.1f, monster.attackCooldown);
                    float heroTTK = monster.hp / Mathf.Max(0.1f, heroDps);
                    float monTTK = hero.hp / Mathf.Max(0.1f, monDps);

                    _sb.AppendLine($"  Compare: Hero TTK={heroTTK:0.0}s  Monster TTK={monTTK:0.0}s  => {(heroTTK < monTTK ? "Hero favored" : "Monster favored")}");
                }
            }

            Debug.Log(_sb.ToString());
        }

        static MonsterAgent FindMonsterById(int id) => AgentBase.FindAgentById<MonsterAgent>(id);
        static HeroAgent FindHeroById(int id) => AgentBase.FindAgentById<HeroAgent>(id);
    }
}
