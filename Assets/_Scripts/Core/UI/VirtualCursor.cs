using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.AI;
using System;

public class VirtualCursor : MonoBehaviour
{
    private CharacterSlot[] characterSlots;


    [SerializeField]
    private float cursorSpeed = 1000f;
    private RectTransform cursorRect;
    private Vector2 moveInput;
    
    private TMP_Text tmpText;

    public PlayerHandler PlayerHandler;

    private int playerIndex;
    private RectTransform canvasRect;
    private Canvas parentCanvas;

    public int PlayerIndex
    {
        get => playerIndex;

        set
        {
            playerIndex = value;

            tmpText.text = $"P{playerIndex+1}";
        }
    }

    private bool isCharacterSelected;

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

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

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
                    PlayerHandler.SetCharacter(null);
                }
                else
                {
                    Debug.Log($"Player {playerIndex+1} selected {slot.CharacterName}!");
                
                    PlayerHandler.SetCharacter(slot.CharacterPrefab);
                }
                
                isCharacterSelected = !isCharacterSelected;

                break;
            }
        }
    }
    void Update()
    {
        // Move the cursor based on thumbstick input
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