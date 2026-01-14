using UnityEngine;
using CozyTown.Build;

namespace CozyTown.Sim
{
    public class MonsterHive : MonoBehaviour
    {
        [Header("Spawn")]
        public GameObject monsterPrefab;
        public float spawnEverySeconds = 6f;
        public int spawnGold = 0;

        private float _t;

        private void Update()
        {
            if (monsterPrefab == null) return;

            _t += Time.deltaTime;
            if (_t >= spawnEverySeconds)
            {
                _t = 0f;
                SpawnMonster();
            }
        }

        private void SpawnMonster()
        {
            Vector3 p = transform.position + new Vector3(Random.Range(-1.2f, 1.2f), 0f, Random.Range(-1.2f, 1.2f));
            Instantiate(monsterPrefab, p, Quaternion.identity);
        }
    }
}
