using UnityEngine;
using UnityEngine.InputSystem;

// Bindings belong to the input adapter; the motor only receives values and edges.
public sealed class CatInput : MonoBehaviour
{
    public CatController controller;
    InputActionMap actions;
    InputAction move, jump, restart, menu;
    public InputActionMap Actions => actions;
    public System.Action BackToMenu;

    void Awake()
    {
        actions = new InputActionMap("Gameplay");
        move = actions.AddAction("Move", InputActionType.Value, expectedControlLayout: "Axis");
        move.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/a").With("Positive", "<Keyboard>/d");
        move.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
        move.AddBinding("<Gamepad>/leftStick/x");
        move.AddBinding("<Gamepad>/dpad/x");
        jump = actions.AddAction("Jump", InputActionType.Button);
        foreach (var key in new[] { "space", "w", "upArrow" }) jump.AddBinding("<Keyboard>/" + key);
        jump.AddBinding("<Gamepad>/buttonSouth");
        restart = actions.AddAction("Restart", InputActionType.Button, "<Keyboard>/r");
        restart.AddBinding("<Gamepad>/select");
        menu = actions.AddAction("Menu", InputActionType.Button, "<Keyboard>/escape");
        menu.AddBinding("<Gamepad>/start");
        move.performed += c => controller.Motion.SetMove(c.ReadValue<float>());
        move.canceled += c => controller.Motion.SetMove(0);
        jump.performed += c => controller.Motion.PressJump();
        jump.canceled += c => controller.Motion.ReleaseJump();
        restart.performed += c => controller.ReturnToStart();
        menu.performed += c => BackToMenu?.Invoke();
    }
    void OnEnable() => actions.Enable();
    void OnDisable() { actions.Disable(); controller.Motion.Reset(); }
    void OnApplicationFocus(bool focused) { if (focused && isActiveAndEnabled) actions.Enable(); else { actions.Disable(); controller.Motion.Reset(); } }
    void OnDestroy() => actions.Dispose();
}
