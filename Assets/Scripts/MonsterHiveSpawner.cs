using UnityEngine;

namespace CozyTown.Sim
{
    public class MonsterHiveSpawner : MonoBehaviour
    {
        public GameObject monsterPrefab;
        public float spawnEverySeconds = 6f;

        float _t;

        void Update()
        {
            if (monsterPrefab == null) return;

            _t += Time.deltaTime;
            if (_t < spawnEverySeconds) return;
            _t = 0f;

            Vector3 p = transform.position + new Vector3(Random.Range(-1.2f, 1.2f), 0f, Random.Range(-1.2f, 1.2f));
            Instantiate(monsterPrefab, p, Quaternion.identity);
        }
    }
}
