using System.Collections.Generic;
using UnityEngine;
using CozyTown.Core;

namespace CozyTown.Sim
{
    public class BountySystem : MonoBehaviour
    {
        public static BountySystem Instance { get; private set; }

        [System.Serializable]
        public class Bounty
        {
            public int bountyId;
            public int targetMonsterId;
            public int reward;
            public Vector3 targetPos;

            public bool accepted;
            public int acceptedByHeroId;

            public float createdAt;

            // ✅ Visual / runtime links
            public Transform targetTransform;     // monster transform (optional but useful)
            public BountyFlagVisual flag;         // spawned flag instance (optional)
        }

        public float bountyExpirationSeconds = 120f;

        private int _nextId = 1;
        private readonly List<Bounty> _bounties = new List<Bounty>();

        public IReadOnlyList<Bounty> ActiveBounties => _bounties;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void LateUpdate()
        {
            // Expire unaccepted bounties that have lived too long (target likely dead)
            for (int i = _bounties.Count - 1; i >= 0; i--)
            {
                var b = _bounties[i];
                if (!b.accepted && Time.time - b.createdAt > bountyExpirationSeconds)
                {
                    CleanupVisuals(b);
                    GameEventBus.Emit(GameEvent.Make(GameEventType.BountyFailed, amount: b.reward, bId: b.targetMonsterId, world: b.targetPos, text: "Expired"));
                    _bounties.RemoveAt(i);
                }
            }
        }

        public Bounty PostBounty(int targetMonsterId, Vector3 targetPos, int reward)
        {
            // ✅ Prevent duplicate bounties for the same monster
            for (int i = 0; i < _bounties.Count; i++)
            {
                var existing = _bounties[i];
                if (existing != null && existing.targetMonsterId == targetMonsterId)
                    return existing;
            }

            var b = new Bounty
            {
                bountyId = _nextId++,
                targetMonsterId = targetMonsterId,
                reward = reward,
                targetPos = targetPos,
                accepted = false,
                createdAt = Time.time
            };
            _bounties.Add(b);

            GameEventBus.Emit(GameEvent.Make(GameEventType.BountyPosted, amount: reward, aId: targetMonsterId, world: targetPos));
            return b;
        }

        public bool TryAcceptBounty(int heroId, out Bounty bounty)
        {
            for (int i = 0; i < _bounties.Count; i++)
            {
                var b = _bounties[i];
                if (b == null || b.accepted) continue;

                b.accepted = true;
                b.acceptedByHeroId = heroId;
                bounty = b;

                GameEventBus.Emit(GameEvent.Make(GameEventType.BountyAccepted, amount: b.reward, aId: heroId, bId: b.targetMonsterId, world: b.targetPos));
                return true;
            }

            bounty = null;
            return false;
        }

        public void IncreaseBountyReward(Bounty b, int increase)
        {
            if (b == null || increase <= 0) return;
            b.reward += increase;
            GameEventBus.Emit(GameEvent.Make(GameEventType.Note, amount: increase, bId: b.targetMonsterId, world: b.targetPos, text: $"Bounty raised to {b.reward}"));
        }

        public void CompleteBounty(Bounty b)
        {
            if (b == null) return;

            CleanupVisuals(b);

            GameEventBus.Emit(GameEvent.Make(GameEventType.BountyCompleted, amount: b.reward, aId: b.acceptedByHeroId, bId: b.targetMonsterId, world: b.targetPos));
            _bounties.Remove(b);
        }

        public void FailBounty(Bounty b)
        {
            if (b == null) return;

            CleanupVisuals(b);

            GameEventBus.Emit(GameEvent.Make(GameEventType.BountyFailed, amount: b.reward, aId: b.acceptedByHeroId, bId: b.targetMonsterId, world: b.targetPos));
            _bounties.Remove(b);
        }

        void CleanupVisuals(Bounty b)
        {
            if (b.flag != null)
            {
                Destroy(b.flag.gameObject);
                b.flag = null;
            }
            b.targetTransform = null;
        }
    }
}
