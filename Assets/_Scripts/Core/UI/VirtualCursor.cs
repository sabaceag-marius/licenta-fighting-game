using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class VirtualCursor : MonoBehaviour
{
    private CharacterSlot[] characterSlots;

    [SerializeField]
    private float cursorSpeed = 1000f;
    private RectTransform cursorRect;
    private Vector2 moveInput;
    
    private TMP_Text tmpText;

    public PlayerHandlerBase PlayerHandler;

    private int playerIndex;
    private RectTransform canvasRect;
    private Canvas parentCanvas;

    public int PlayerIndex
    {
        get => playerIndex;
        set
        {
            playerIndex = value;
            tmpText.text = $"P{playerIndex + 1}";
        }
    }

    private bool isCharacterSelected;

    private PlayerInput myInput;
    private BaseCharacterSelectManager selectManager;
    private InputAction moveAction;
    private InputAction selectAction;
    private InputAction startAction;

    private void Awake()
    {
        cursorRect = GetComponent<RectTransform>();
        tmpText = GetComponentInChildren<TMP_Text>();

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        characterSlots = FindObjectsOfType<CharacterSlot>();
    }

    // Called by the CharacterSelectManager right after this cursor is instantiated
    public void Initialize(PlayerInput input, BaseCharacterSelectManager manager)
    {
        myInput = input;
        selectManager = manager;

        // Cache the specific actions for this player's controller
        moveAction = myInput.actions["LeftStick"];
        selectAction = myInput.actions["Select"];
        startAction = myInput.actions["Start"];
    }

    void Update()
    {
        // Safety check to ensure Initialize was called
        if (myInput == null) return;

        // 1. Read Inputs
        moveInput = moveAction.ReadValue<Vector2>();

        // 2. Handle Movement
        // Move the cursor based on thumbstick input (only if they haven't locked in)
        if (moveInput.sqrMagnitude > 0.1f && !isCharacterSelected)
        {
            Vector2 newPosition = cursorRect.anchoredPosition + moveInput * cursorSpeed * Time.deltaTime;

            // Clamp the position if we have a Canvas reference
            if (canvasRect != null)
            {
                newPosition = ClampToCanvas(newPosition);
            }

            cursorRect.anchoredPosition = newPosition;
        }

        // 3. Handle Selection
        if (selectAction.WasPressedThisFrame())
        {
            HandleSelection();
        }

        // 4. Handle Match Start
        if (startAction.WasPressedThisFrame())
        {
            selectManager.TryStartMatch();
        }
    }

    // Extracted from your previous OnSelect logic
    private void HandleSelection()
    {
        Camera uiCamera = null;
        if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = parentCanvas.worldCamera;
        }

        // Convert the cursor's World Space position into Screen Space (Pixels)
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, cursorRect.position);

        foreach (CharacterSlot slot in characterSlots)
        {
            // Check if the center of our cursor is inside the bounds of this character slot
            bool isHovering = RectTransformUtility.RectangleContainsScreenPoint(
                slot.rectTransform, 
                screenPosition, 
                uiCamera
            );

            if (isHovering)
            {
                if (isCharacterSelected)
                {
                    PlayerHandler.SetCharacter(null); // Deselect
                }
                else
                {
                    Debug.Log($"Player {playerIndex + 1} selected {slot.CharacterName}!");
                    PlayerHandler.SetCharacter(slot.CharacterPrefab); // Select
                }
                
                isCharacterSelected = !isCharacterSelected;
                
                // Notify the manager to update the "Start" label visibility
                selectManager.CheckStartCondition();

                break;
            }
        }
    }

    private Vector2 ClampToCanvas(Vector2 targetPosition)
    {
        float minX = (-canvasRect.rect.width / 2f) + (cursorRect.rect.width / 2f);
        float maxX = (canvasRect.rect.width / 2f) - (cursorRect.rect.width / 2f);
        
        float minY = (-canvasRect.rect.height / 2f) + (cursorRect.rect.height / 2f);
        float maxY = (canvasRect.rect.height / 2f) - (cursorRect.rect.height / 2f);

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        return targetPosition;
    }
}