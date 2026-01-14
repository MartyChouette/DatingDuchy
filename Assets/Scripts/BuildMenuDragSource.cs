using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CozyTown.Build;

namespace CozyTown.UI
{
    public class BuildMenuDragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Refs")]
        public BuildingDefinition building;
        public BuildToolController buildTool;
        public UnityEngine.Camera worldCamera;
        public RectTransform canvasRoot;

        [Header("Drag Visual")]
        public Sprite dragSprite; // optional, else uses button image sprite
        public float dragIconScale = 1.0f;

        private GameObject _dragIconGO;
        private RectTransform _dragIconRT;
        private CanvasGroup _dragIconCG;
        private Image _dragIconImg;

        private void EnsureCamera()
        {
            if (worldCamera == null) worldCamera = UnityEngine.Camera.main;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (building == null || buildTool == null) return;

            EnsureCamera();

            // Start placement mode immediately (ghost should follow cursor/hex)
            buildTool.BeginPlacement(building);

            // Create a small UI icon that follows the mouse while dragging (optional but feels great)
            if (canvasRoot != null)
            {
                _dragIconGO = new GameObject("DragIcon");
                _dragIconGO.transform.SetParent(canvasRoot, false);

                _dragIconRT = _dragIconGO.AddComponent<RectTransform>();
                _dragIconCG = _dragIconGO.AddComponent<CanvasGroup>();
                _dragIconImg = _dragIconGO.AddComponent<Image>();

                _dragIconCG.blocksRaycasts = false;
                _dragIconCG.interactable = false;
                _dragIconCG.alpha = 0.85f;

                var img = GetComponent<Image>();
                _dragIconImg.sprite = dragSprite != null ? dragSprite : (img != null ? img.sprite : null);
                _dragIconImg.raycastTarget = false;

                _dragIconRT.sizeDelta = new Vector2(64, 64) * dragIconScale;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Move icon
            if (_dragIconRT != null)
                _dragIconRT.position = eventData.position;

            // Optional: if you want drag itself to steer the world cursor hex,
            // your existing TownInputRouter mouse hover already does that via Point action.
            // So we don't need anything here unless you want special behavior.
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Remove drag icon
            if (_dragIconGO != null) Destroy(_dragIconGO);

            // If we released over UI, don't place (player probably dragged within menu)
            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                // Still keep placement active after drag ends?
                // I recommend: keep placement active (they can move and click to place)
                // If you prefer: cancel placement here.
                return;
            }

            // Released over world → attempt place immediately
            bool placed = buildTool.TryPlaceAtCursor();

            // If it failed, keep placement active so they can move and place again.
            // If it succeeded, you can either keep placing (like paint mode) or exit.
            // Cozy city builder default: stay in placement mode until cancel.
            // If you want “one-and-done” for drag, uncomment:
            // if (placed) buildTool.CancelPlacement();
        }
    }
}
