using TMPro;
using UnityEngine;

namespace CozyTown.UI
{
    public class WorldUpdatePanel : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text text;

        void Awake()
        {
            if (root != null) root.SetActive(false);
        }

        public void Show(string report)
        {
            if (root != null) root.SetActive(true);
            if (text != null) text.text = report;
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }
    }
}
