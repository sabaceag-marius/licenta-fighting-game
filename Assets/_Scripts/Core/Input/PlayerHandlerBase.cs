using System;
using UnityEngine;

public abstract class PlayerHandlerBase : MonoBehaviour
{
    public int PlayerIndex { get; set; }
    
    // Using nullable annotation as you had in your original code
    public GameObject? SelectedCharacterPrefab { get; private set; }

    protected virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Called during the Character Select screen
    public void SetCharacter(GameObject? characterPrefab)
    {
        SelectedCharacterPrefab = characterPrefab;
    }

    // Called by the Game Manager in the Combat scene
    public GameObject SpawnCharacter(Transform spawnPoint)
    {
        if (SelectedCharacterPrefab == null)
            return null;

        // Spawn the character
        GameObject character = Instantiate(SelectedCharacterPrefab, spawnPoint.position, spawnPoint.rotation);

        // Fix: Use the instantiated character's original Y/Z scale, not the Handler's scale
        character.transform.localScale = new Vector3(
            spawnPoint.localScale.x,
            character.transform.localScale.y,
            character.transform.localScale.z
        );

        if (PlayerIndex != 0)
        {
            var spriteRenderer = character.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null) 
            {
                spriteRenderer.color = Color.grey;
            }
        }

        var characterScript = character.GetComponent<Character>();

        if (characterScript != null)
        {
            characterScript.Index = PlayerIndex;
        }

        // Delegate the input initialization to the specific child classes
        InitializeCharacterInput(character);

        return character;
    }

    // Abstract method that Local and Remote handlers MUST implement
    protected abstract void InitializeCharacterInput(GameObject spawnedCharacter);
}