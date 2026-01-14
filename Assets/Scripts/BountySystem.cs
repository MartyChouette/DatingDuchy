using System.Collections.Generic;
using UnityEngine;
using CozyTown.Core;

namespace CozyTown.Sim
{
    public class BountySystem : MonoBehaviour
    {
        public static BountySystem Instance { get; private set; }

        public class Bounty
        {
            public int bountyId;
            public int targetMonsterId;
            public int reward;
            public Vector3 targetPos;
            public bool accepted;
            public int acceptedByHeroId;
        }

        private int _nextId = 1;
        private readonly List<Bounty> _bounties = new List<Bounty>();

        public IReadOnlyList<Bounty> ActiveBounties => _bounties;

        private void Awake()
        {
            Instance = this;
        }

        public Bounty PostBounty(int targetMonsterId, Vector3 targetPos, int reward)
        {
            var b = new Bounty
            {
                bountyId = _nextId++,
                targetMonsterId = targetMonsterId,
                reward = reward,
                targetPos = targetPos,
                accepted = false
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
                if (b.accepted) continue;

                b.accepted = true;
                b.acceptedByHeroId = heroId;
                bounty = b;

                GameEventBus.Emit(GameEvent.Make(GameEventType.BountyAccepted, amount: b.reward, aId: heroId, bId: b.targetMonsterId, world: b.targetPos));
                return true;
            }

            bounty = null;
            return false;
        }

        public void CompleteBounty(Bounty b)
        {
            if (b == null) return;

            GameEventBus.Emit(GameEvent.Make(GameEventType.BountyCompleted, amount: b.reward, aId: b.acceptedByHeroId, bId: b.targetMonsterId, world: b.targetPos));
            _bounties.Remove(b);
        }

        public void FailBounty(Bounty b)
        {
            if (b == null) return;

            GameEventBus.Emit(GameEvent.Make(GameEventType.BountyFailed, amount: b.reward, aId: b.acceptedByHeroId, bId: b.targetMonsterId, world: b.targetPos));
            _bounties.Remove(b);
        }
    }
}
