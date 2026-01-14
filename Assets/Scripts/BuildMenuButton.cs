using UnityEngine;
using UnityEngine.UI;
using CozyTown.Build;

namespace CozyTown.UI
{
    [RequireComponent(typeof(Button))]
    public class BuildMenuButton : MonoBehaviour
    {
        public BuildingDefinition building;
        public BuildToolController buildTool;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (building == null || buildTool == null) return;
            buildTool.BeginPlacement(building);
        }
    }
}
