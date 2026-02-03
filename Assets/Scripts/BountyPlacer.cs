using UnityEngine.InputSystem;
using CozyTown.Core;

namespace CozyTown.Sim
{
    public class BountyPlacer : UnityEngine.MonoBehaviour
    {
        [UnityEngine.Header("Refs")]
        public UnityEngine.Camera cam;
        public UnityEngine.LayerMask monsterLayer;
        public BountyFlagVisual bountyFlagPrefab;

        [UnityEngine.Header("Settings")]
        public int reward = 25;
        public int rewardIncreaseOnRepeat = 10;
        public float maxRayDistance = 200f;

        public void TryPlaceBountyOnMonsterUnderCursor()
        {
            if (BountySystem.Instance == null) return;

            if (cam == null) cam = UnityEngine.Camera.main;
            if (cam == null) return;

            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse == null)
            {
                GameEventBus.Emit(GameEvent.Make(GameEventType.Note, text: "No mouse detected"));
                return;
            }

            UnityEngine.Vector2 screen = mouse.position.ReadValue();
            UnityEngine.Ray ray = cam.ScreenPointToRay(screen);

            if (!UnityEngine.Physics.Raycast(ray, out UnityEngine.RaycastHit hit, maxRayDistance, monsterLayer, UnityEngine.QueryTriggerInteraction.Ignore))
            {
                GameEventBus.Emit(GameEvent.Make(GameEventType.Note, text: "No monster under cursor"));
                return;
            }

            var monster = hit.collider.GetComponentInParent<MonsterAgent>();
            if (monster == null || monster.pid == null)
            {
                GameEventBus.Emit(GameEvent.Make(GameEventType.Note, text: "Hit something, but not a MonsterAgent"));
                return;
            }

            PlaceOrUpdateBounty(monster);
        }

        public void PlaceOrUpdateBounty(MonsterAgent monster)
        {
            if (monster == null || monster.pid == null) return;

            var b = BountySystem.Instance.PostBounty(monster.pid.id, monster.transform.position, reward);
            b.targetTransform = monster.transform;
            b.targetPos = monster.transform.position;

            if (b.flag != null)
            {
                BountySystem.Instance.IncreaseBountyReward(b, rewardIncreaseOnRepeat);
                return;
            }

            if (bountyFlagPrefab != null)
            {
                var flag = UnityEngine.Object.Instantiate(bountyFlagPrefab);
                flag.followTarget = monster.transform;
                b.flag = flag;
            }

            GameEventBus.Emit(GameEvent.Make(GameEventType.Note, text: $"Bounty placed: {b.reward} on monster {monster.pid.id}"));
        }
    }
}
