using UnityEngine;

namespace CozyTown.Sim
{
    /// <summary>
    /// Tiny mover: goes straight toward a target point. (We’ll replace with pathfinding later.)
    /// </summary>
    public class AgentMover : MonoBehaviour
    {
        public float moveSpeed = 3.5f;
        public float arriveDistance = 0.25f;

        public Vector3 Target { get; private set; }
        public bool HasTarget { get; private set; }

        public void SetTarget(Vector3 world)
        {
            Target = world;
            HasTarget = true;
        }

        public void ClearTarget()
        {
            HasTarget = false;
        }

        public bool TickMove()
        {
            if (!HasTarget) return true;

            Vector3 p = transform.position;
            Vector3 t = Target;
            t.y = p.y;

            float d = Vector3.Distance(p, t);
            if (d <= arriveDistance) return true;

            Vector3 dir = (t - p).normalized;
            transform.position = p + dir * (moveSpeed * Time.deltaTime);

            if (dir.sqrMagnitude > 0.001f)
                transform.forward = Vector3.Slerp(transform.forward, dir, 10f * Time.deltaTime);

            return false;
        }
    }
}
