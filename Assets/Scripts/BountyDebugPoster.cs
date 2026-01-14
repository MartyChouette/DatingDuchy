using UnityEngine;
using UnityEngine.InputSystem;
using CozyTown.Core;

namespace CozyTown.Sim
{
    public class BountyDebugPoster : MonoBehaviour
    {
        public int reward = 25;
        public Key postKey = Key.P;

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (!Keyboard.current[postKey].wasPressedThisFrame) return;
            if (BountySystem.Instance == null) return;

            var monsters = GameObject.FindObjectsByType<MonsterAgent>(FindObjectsSortMode.None);
            if (monsters.Length == 0)
            {
                GameEventBus.Emit(GameEvent.Make(GameEventType.Note, text: "No monsters to bounty"));
                return;
            }

            // pick nearest to origin for now
            MonsterAgent best = null;
            float bestD = float.PositiveInfinity;
            foreach (var m in monsters)
            {
                if (m == null) continue;
                float d = (m.transform.position - Vector3.zero).sqrMagnitude;
                if (d < bestD) { bestD = d; best = m; }
            }

            if (best == null) return;
            BountySystem.Instance.PostBounty(best.pid.id, best.transform.position, reward);
        }
    }
}
