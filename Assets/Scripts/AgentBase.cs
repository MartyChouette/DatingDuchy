using UnityEngine;
using CozyTown.Core;

namespace CozyTown.Sim
{
    [RequireComponent(typeof(PersonId))]
    [RequireComponent(typeof(AgentMover))]
    public abstract class AgentBase : MonoBehaviour
    {
        public PersonId pid { get; private set; }
        public AgentMover mover { get; private set; }

        [Header("Stats")]
        public int maxHP = 10;
        public int hp = 10;

        public int gold = 5;

        protected virtual void Awake()
        {
            pid = GetComponent<PersonId>();
            mover = GetComponent<AgentMover>();
            hp = Mathf.Clamp(hp, 1, maxHP);
        }

        protected void EmitSpawn(string kind)
        {
            GameEventBus.Emit(GameEvent.Make(GameEventType.PersonSpawned, aId: pid.id, world: transform.position, text: kind));
        }

        protected void EmitDeath(string kind)
        {
            GameEventBus.Emit(GameEvent.Make(GameEventType.PersonDied, aId: pid.id, world: transform.position, text: kind));
        }

        public virtual void TakeDamage(int dmg)
        {
            hp -= dmg;
            if (hp <= 0) Die();
        }

        protected virtual void Die()
        {
            EmitDeath(GetAgentKindName());
            Destroy(gameObject);
        }

        protected abstract string GetAgentKindName();
    }
}
