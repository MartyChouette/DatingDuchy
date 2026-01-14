using UnityEngine;
using CozyTown.Grid;

namespace CozyTown.Build
{
    public class HexCursorController : MonoBehaviour
    {
        public HexGridManager grid;
        public Transform cursorVisual;

        [Header("Gamepad Cursor")]
        public float deadzone = 0.35f;
        public float initialRepeatDelay = 0.25f;
        public float repeatRate = 0.10f;

        public HexCoord CurrentHex { get; private set; } = new HexCoord(0, 0);

        private Vector2 _heldDir;
        private float _repeatTimer;
        private bool _hasRepeated;

        public void SetHex(HexCoord h)
        {
            if (grid != null && !grid.IsInBounds(h)) return;
            CurrentHex = h;
            UpdateVisual();
        }

        public void NudgeFromStick(Vector2 stick)
        {
            // Quantize to 6 directions. If stick is small, do nothing.
            if (stick.magnitude < deadzone)
            {
                _heldDir = Vector2.zero;
                _repeatTimer = 0f;
                _hasRepeated = false;
                return;
            }

            Vector2 dir = stick.normalized;
            if (Vector2.Dot(dir, _heldDir) < 0.8f)
            {
                // new direction
                _heldDir = dir;
                _repeatTimer = 0f;
                _hasRepeated = false;
                StepOnce(dir);
                return;
            }

            _repeatTimer += Time.deltaTime;
            float delay = _hasRepeated ? repeatRate : initialRepeatDelay;
            if (_repeatTimer >= delay)
            {
                _repeatTimer = 0f;
                _hasRepeated = true;
                StepOnce(dir);
            }
        }

        private void StepOnce(Vector2 dir)
        {
            int neighborDir = DirectionToNeighborIndex(dir);
            HexCoord next = CurrentHex.Neighbor(neighborDir);
            if (grid != null && !grid.IsInBounds(next)) return;
            CurrentHex = next;
            UpdateVisual();
        }

        private int DirectionToNeighborIndex(Vector2 dir)
        {
            // Map 2D direction to one of 6 axial neighbors.
            // This is "good enough" for prototype and feels consistent.
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            angle = (angle + 360f) % 360f;

            // 0° = right, 90° = up
            // We'll slice into 6 wedges of 60°
            int wedge = Mathf.RoundToInt(angle / 60f) % 6;

            // Convert wedge to our Neighbor(1..6) order:
            // Neighbor order in HexCoord: E, NE, NW, W, SW, SE
            return wedge switch
            {
                0 => 1, // E
                1 => 2, // NE
                2 => 3, // NW
                3 => 4, // W
                4 => 5, // SW
                _ => 6, // SE
            };
        }
        public void SetFromWorld(Vector3 worldPos)
        {
            if (grid == null) return;
            var h = grid.WorldToHex(worldPos);
            if (!grid.IsInBounds(h)) return;
            CurrentHex = h;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (grid == null || cursorVisual == null) return;
            cursorVisual.position = grid.HexToWorld(CurrentHex) + new Vector3(0f, 0.05f, 0f);
        }
    }
}
