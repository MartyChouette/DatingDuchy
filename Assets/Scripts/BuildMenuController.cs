using UnityEngine;
using CozyTown.Build;

namespace CozyTown.UI
{
    public class BuildMenuController : MonoBehaviour
    {
        [Header("UI")]
        public GameObject panel;              // the menu panel GameObject to show/hide

        [Header("Placement")]
        public BuildToolController buildTool; // drag your BuildToolController here

        public bool IsOpen => panel != null && panel.activeSelf;

        public void Toggle()
        {
            if (panel == null) return;
            panel.SetActive(!panel.activeSelf);
        }

        public void Open()
        {
            if (panel == null) return;
            panel.SetActive(true);
        }

        public void Close()
        {
            if (panel == null) return;
            panel.SetActive(false);
        }

        /// <summary>
        /// If we are currently placing a building, cancel that.
        /// Otherwise close the menu.
        /// </summary>
        public void CancelPlacementOrCloseMenu()
        {
            if (buildTool != null && buildTool.IsPlacing)
                buildTool.CancelPlacement();
            else
                Close();
        }
    }
}
