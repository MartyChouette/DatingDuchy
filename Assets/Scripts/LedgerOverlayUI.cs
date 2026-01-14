using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using CozyTown.Core;

namespace CozyTown.UI
{
    /// <summary>
    /// Simple overlay: press L to toggle a live ledger panel.
    /// </summary>
    public class LedgerOverlayUI : MonoBehaviour
    {
        public TMP_Text text;
        public GameObject root;
        public Key toggleKey = Key.L;

        private readonly StringBuilder _sb = new StringBuilder(2048);

        private void Start()
        {
            if (root != null) root.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
            {
                if (root != null) root.SetActive(!root.activeSelf);
            }

            if (root == null || !root.activeSelf) return;
            if (text == null) return;
            if (MetricsLedger.Instance == null) return;

            var m = MetricsLedger.Instance;

            _sb.Clear();
            _sb.AppendLine("<b>Kingdom Ledger</b>");
            _sb.AppendLine($"Treasury: {m.treasury}");
            _sb.AppendLine($"Taxes Collected: {m.totalTaxesCollected}");
            _sb.AppendLine($"Bounties: posted {m.bountiesPosted} | completed {m.bountiesCompleted} | paid {m.totalBountiesPaid}");
            _sb.AppendLine($"Spent at Tavern: {m.totalGoldSpentAtTavern}");
            _sb.AppendLine();
            _sb.AppendLine($"Peasants: {m.peasants} | Heroes: {m.heroes} | Monsters: {m.monsters}");
            _sb.AppendLine();
            _sb.AppendLine("<b>Recent</b>");

            foreach (var e in m.RecentEvents)
                _sb.AppendLine(e.ToString());

            text.text = _sb.ToString();
        }
    }
}
