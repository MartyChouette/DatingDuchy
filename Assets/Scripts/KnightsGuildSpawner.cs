using UnityEngine;

namespace CozyTown.Sim
{
    public class KnightsGuildSpawner : MonoBehaviour
    {
        public GameObject heroPrefab;
        public float spawnEverySeconds = 12f;
        public int maxHeroes = 6;

        float _t;

        void Update()
        {
            if (heroPrefab == null) return;

            _t += Time.deltaTime;
            if (_t < spawnEverySeconds) return;
            _t = 0f;

            int heroes = Object.FindObjectsByType<HeroAgent>(FindObjectsSortMode.None).Length;
            if (heroes >= maxHeroes) return;

            Vector3 p = transform.position + new Vector3(Random.Range(-0.8f, 0.8f), 0f, Random.Range(-0.8f, 0.8f));
            Instantiate(heroPrefab, p, Quaternion.identity);
        }
    }
}
