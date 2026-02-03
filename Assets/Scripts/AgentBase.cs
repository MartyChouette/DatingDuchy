using System.Collections.Generic;
using UnityEngine;
using CozyTown.Core;

namespace CozyTown.Sim
{
    [RequireComponent(typeof(PersonId))]
    [RequireComponent(typeof(AgentMover))]
    public abstract class AgentBase : MonoBehaviour
    {
        // ── Static agent registry ──────────────────────────────────
        private static readonly Dictionary<int, AgentBase> _registry = new(128);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRegistry() => _registry.Clear();

        public static T FindAgentById<T>(int id) where T : AgentBase
        {
            return _registry.TryGetValue(id, out var a) ? a as T : null;
        }

        public static int CountAgentsOfType<T>() where T : AgentBase
        {
            int count = 0;
            foreach (var kvp in _registry)
                if (kvp.Value is T) count++;
            return count;
        }

        // ── Instance ───────────────────────────────────────────────
        public PersonId pid { get; private set; }
        public AgentMover mover { get; private set; }

        [Header("Stats")]
        public int maxHP = 10;
        public int hp = 10;

        public int gold = 5;

        protected virtual void Awake()
        {
            pid = GetComponent<PersonId>();
            pid.EnsureId();
            mover = GetComponent<AgentMover>();
            hp = Mathf.Clamp(hp, 1, maxHP);
            _registry[pid.id] = this;
        }

        protected virtual void OnDestroy()
        {
            if (pid != null)
                _registry.Remove(pid.id);
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
