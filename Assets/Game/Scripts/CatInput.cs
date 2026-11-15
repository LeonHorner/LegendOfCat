using UnityEngine;
using UnityEngine.InputSystem;

// Bindings belong to the input adapter; the motor only receives values and edges.
public sealed class CatInput : MonoBehaviour
{
    public CatController controller;
    InputActionMap actions;
    InputAction move, jump, restart, fullRestart, pause, menu, pulse, lob;
    public System.Action PulsePressed, LobPressed, LobReleased, CombatCanceled;
    public System.Action<float> FacingChanged;
    bool canceling;
    public InputActionMap Actions => actions;
    public System.Action BackToMenu;
    public System.Action RetryCheckpoint;
    public System.Action RestartRun;
    public System.Action TogglePause;

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
        fullRestart = actions.AddAction("FullRestart", InputActionType.Button, "<Keyboard>/backspace");
        fullRestart.AddBinding("<Gamepad>/leftShoulder");
        pause = actions.AddAction("Pause", InputActionType.Button, "<Keyboard>/p");
        pause.AddBinding("<Gamepad>/rightShoulder");
        menu = actions.AddAction("Menu", InputActionType.Button, "<Keyboard>/escape");
        menu.AddBinding("<Gamepad>/start");
        pulse = actions.AddAction("Pulse", InputActionType.Button, "<Keyboard>/j");
        pulse.AddBinding("<Gamepad>/buttonWest");
        lob = actions.AddAction("Lob", InputActionType.Button, "<Keyboard>/k");
        lob.AddBinding("<Gamepad>/rightTrigger");
        pulse.performed += c => PulsePressed?.Invoke();
        lob.performed += c => LobPressed?.Invoke();
        lob.canceled += c => { if (!canceling) LobReleased?.Invoke(); };
        move.performed += c => {
            float value = c.ReadValue<float>();
            controller.Motion.SetMove(value);
            if (Mathf.Abs(value) > .1f) FacingChanged?.Invoke(Mathf.Sign(value));
        };
        move.canceled += c => controller.Motion.SetMove(0);
        jump.performed += c => controller.PressJump();
        jump.canceled += c => controller.Motion.ReleaseJump();
        restart.performed += c => { if (RetryCheckpoint != null) RetryCheckpoint(); else controller.ReturnToStart(); };
        fullRestart.performed += c => RestartRun?.Invoke();
        pause.performed += c => TogglePause?.Invoke();
        menu.performed += c => BackToMenu?.Invoke();
    }
    void OnEnable() => actions.Enable();
    void CancelActions()
    {
        canceling = true;
        CombatCanceled?.Invoke();
        actions.Disable();
        controller.Motion.Reset();
        canceling = false;
    }
    void OnDisable() => CancelActions();
    void OnApplicationFocus(bool focused) { if (focused && isActiveAndEnabled) actions.Enable(); else CancelActions(); }
    void OnDestroy() => actions.Dispose();
}
