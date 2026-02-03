using UnityEngine;

namespace CozyTown.Sim
{
    public class ContactSensor : MonoBehaviour
    {
        public AgentBase agent;
        public float minContactSpeed = 0f; // later: ignore grazing

        void Awake()
        {
            if (agent == null) agent = GetComponentInParent<AgentBase>();
        }

        void OnCollisionStay(Collision c)
        {
            var other = c.collider.GetComponentInParent<AgentBase>();
            if (agent == null || other == null || other == agent) return;

            // pick a representative contact point
            Vector3 point = c.contactCount > 0 ? c.GetContact(0).point : transform.position;
            RelationshipSystem.Instance?.RegisterContact(agent, other, point, Time.deltaTime);
        }

        void OnTriggerStay(Collider otherCol)
        {
            var other = otherCol.GetComponentInParent<AgentBase>();
            if (agent == null || other == null || other == agent) return;

            // Triggers have no contact points; approximate
            Vector3 point = otherCol.ClosestPoint(transform.position);
            RelationshipSystem.Instance?.RegisterContact(agent, other, point, Time.deltaTime);
        }
    }
}
