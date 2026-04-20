using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GBInputDisplay : MonoBehaviour
{
    private InputActions inputActions;

    #region InputActions

    private InputAction leftStickIA;
    private InputAction cStickIA;
    private InputAction aButtonIA;
    private InputAction bButtonIA;
    private InputAction xButtonIA;
    private InputAction yButtonIA;
    private InputAction zButtonIA;
    private InputAction lButtonIA;
    private InputAction rButtonIA;

    #endregion

    #region LeftStick

    [SerializeField]
    private RectTransform leftStickUI;

    [SerializeField]
    private float leftStickRadius = 50f;

    #endregion

    #region CStick

    [SerializeField]
    private RectTransform cStickUI;

    [SerializeField]
    private float cStickRadius = 20f;

    #endregion

    #region ZButton

    [SerializeField]
    private Image zButton;

    [SerializeField]
    private Sprite zButtonInactive;

    [SerializeField]
    private Sprite zButtonActive;

    #endregion

    #region xButton

    [SerializeField]
    private Image xButton;

    [SerializeField]
    private Sprite xButtonInactive;

    [SerializeField]
    private Sprite xButtonActive;

    #endregion

    #region yButton

    [SerializeField]
    private Image yButton;

    [SerializeField]
    private Sprite yButtonInactive;

    [SerializeField]
    private Sprite yButtonActive;

    #endregion

    #region aButton

    [SerializeField]
    private Image aButton;

    [SerializeField]
    private Sprite aButtonInactive;

    [SerializeField]
    private Sprite aButtonActive;

    #endregion

    #region bButton

    [SerializeField]
    private Image bButton;

    [SerializeField]
    private Sprite bButtonInactive;

    [SerializeField]
    private Sprite bButtonActive;

    #endregion

    #region lButton

    [SerializeField]
    private Image lButton;

    [SerializeField]
    private Sprite lButtonInactive;

    [SerializeField]
    private Sprite lButtonActive;

    #endregion

    #region rButton

    [SerializeField]
    private Image rButton;

    [SerializeField]
    private Sprite rButtonInactive;

    [SerializeField]
    private Sprite rButtonActive;

    #endregion

    private void Awake()
    {
        inputActions = new InputActions();

        leftStickIA = inputActions.UIOverlay.LeftStick;
        cStickIA = inputActions.UIOverlay.CStick;
        aButtonIA = inputActions.UIOverlay.AButton;
        bButtonIA = inputActions.UIOverlay.BButton;
        xButtonIA = inputActions.UIOverlay.XButton;
        yButtonIA = inputActions.UIOverlay.YButton;
        zButtonIA = inputActions.UIOverlay.ZButton;
        lButtonIA = inputActions.UIOverlay.LTrigger;
        rButtonIA = inputActions.UIOverlay.RTrigger;
    }

    // Update is called once per frame
    void Update()
    {
        // Left stick
        Vector2 leftStickinput = leftStickIA.ReadValue<Vector2>();
        leftStickUI.anchoredPosition = leftStickinput * leftStickRadius;

        // CStick

        Vector2 cStickInput = cStickIA.ReadValue<Vector2>();
        cStickUI.anchoredPosition = cStickInput * cStickRadius;

        zButton.sprite = zButtonIA.IsPressed() ? zButtonActive : zButtonInactive;

        xButton.sprite = xButtonIA.IsPressed() ? xButtonActive : xButtonInactive;

        yButton.sprite = yButtonIA.IsPressed() ? yButtonActive : yButtonInactive;

        aButton.sprite = aButtonIA.IsPressed() ? aButtonActive : aButtonInactive;

        bButton.sprite = bButtonIA.IsPressed() ? bButtonActive : bButtonInactive;

        lButton.sprite = lButtonIA.IsPressed() ? lButtonActive : lButtonInactive;

        rButton.sprite = rButtonIA.IsPressed() ? rButtonActive : rButtonInactive;

    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
}
