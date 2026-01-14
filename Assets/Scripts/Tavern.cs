using UnityEngine;

namespace CozyTown.Sim
{
    public class Tavern : MonoBehaviour
    {
        [Min(0)] public int spendMin = 1;
        [Min(0)] public int spendMax = 5;

        private void OnValidate()
        {
            if (spendMax < spendMin) spendMax = spendMin;
        }
    }
}
