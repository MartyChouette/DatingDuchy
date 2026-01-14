using UnityEngine;
using UnityEngine.InputSystem;
using CozyTown.Camera;
using CozyTown.Build;
using CozyTown.Input;
using CozyTown.UI;
using UnityEngine.EventSystems;



namespace CozyTown.Input
{
    public class TownInputRouter : MonoBehaviour
    {
        public enum ActiveDeviceMode { Gamepad, KeyboardMouse }
        

        private InputAction _toggleBuildMenu;
        public CozyTown.UI.BuildMenuController buildMenu;


        [Header("Refs")]
        public TownCameraController cameraController;
        public HexCursorController cursorController;
        public BuildToolController buildTool;

        [Header("Mouse -> World")]
        public UnityEngine.Camera worldCamera;
        public float groundY = 0f;

        [Header("Device Switching")]
        [Tooltip("How long after mouse movement we keep mouse as the active cursor device.")]
        public float mouseActiveSeconds = 1.25f;

        [Tooltip("How long after gamepad stick movement we keep gamepad as the active cursor device.")]
        public float gamepadActiveSeconds = 1.25f;

        public ActiveDeviceMode ActiveMode { get; private set; } = ActiveDeviceMode.KeyboardMouse;

        private PlayerInput _playerInput;

        // Actions
        private InputAction _navigateCursor;
        private InputAction _moveCamera;
        private InputAction _rotateCamera;
        private InputAction _zoom;

        private InputAction _confirm;
        private InputAction _cancel;
        private InputAction _rotateBuilding;

        private InputAction _point;
        private InputAction _mouseMove;

        private float _lastMouseTime = -999f;
        private float _lastGamepadTime = -999f;

        private void Awake()
        {


            _playerInput = GetComponent<PlayerInput>();
            var map = _playerInput.actions.FindActionMap("Gameplay", true);

            _navigateCursor = map.FindAction("NavigateCursor", true);
            _moveCamera = map.FindAction("MoveCamera", true);
            _rotateCamera = map.FindAction("RotateCamera", true);
            _zoom = map.FindAction("Zoom", true);

            _confirm = map.FindAction("Confirm", true);
            _cancel = map.FindAction("Cancel", true);
            _rotateBuilding = map.FindAction("RotateBuilding", true);

            _point = map.FindAction("Point", false);
            _mouseMove = map.FindAction("MouseMove", false);

            _confirm.performed += ctx =>
            {
                // If mouse is clicking UI, do NOT place in the world.
                if (ctx.control.device is Mouse)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                        return;
                }

                if (buildTool != null && buildTool.IsPlacing)
                    buildTool.TryPlaceAtCursor();
            };

            _rotateBuilding.performed += _ => buildTool?.RotateBuilding();
            _toggleBuildMenu = map.FindAction("ToggleBuildMenu", false);
            if (_toggleBuildMenu != null)
                _toggleBuildMenu.performed += _ => buildMenu?.Toggle();

            _cancel.performed += _ => buildMenu?.CancelPlacementOrCloseMenu();



            if (worldCamera == null) worldCamera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            // --- Detect recent device activity ---
            // Mouse activity: delta or pointer moved
            if (_mouseMove != null)
            {
                Vector2 md = _mouseMove.ReadValue<Vector2>();
                if (md.sqrMagnitude > 0.5f) _lastMouseTime = Time.unscaledTime;
            }

            // Gamepad activity: cursor stick
            Vector2 stick = _navigateCursor.ReadValue<Vector2>();
            if (stick.sqrMagnitude > 0.25f) _lastGamepadTime = Time.unscaledTime;

            // Decide active mode (last input wins)
            bool mouseActive = (Time.unscaledTime - _lastMouseTime) <= mouseActiveSeconds;
            bool gamepadActive = (Time.unscaledTime - _lastGamepadTime) <= gamepadActiveSeconds;

            if (gamepadActive && !mouseActive) ActiveMode = ActiveDeviceMode.Gamepad;
            else if (mouseActive && !gamepadActive) ActiveMode = ActiveDeviceMode.KeyboardMouse;
            else
            {
                // If both recently active, prefer the more recent one:
                ActiveMode = (_lastGamepadTime > _lastMouseTime) ? ActiveDeviceMode.Gamepad : ActiveDeviceMode.KeyboardMouse;
            }

            // --- Cursor ---
            if (cursorController != null)
            {
                if (ActiveMode == ActiveDeviceMode.Gamepad)
                {
                    cursorController.NudgeFromStick(stick);
                }
                else
                {
                    // Mouse hover sets cursor hex
                    if (_point != null && worldCamera != null)
                    {
                        Vector2 screen = _point.ReadValue<Vector2>();
                        if (MouseWorldPicker.TryGetPointOnGroundPlane(worldCamera, screen, groundY, out var world))
                            cursorController.SetFromWorld(world);

                    }
                }
            }

            // --- Camera ---
            if (cameraController != null)
            {
                // WASD (keyboard) OR right stick (gamepad) will both feed MoveCamera.
                Vector2 pan = _moveCamera.ReadValue<Vector2>();
                cameraController.AddPanInput(pan);

                float yaw = _rotateCamera.ReadValue<float>();
                cameraController.AddYawInput(yaw);

                float z = _zoom.ReadValue<float>();
                cameraController.AddZoomInput(z);
            }
        }
    }
}
