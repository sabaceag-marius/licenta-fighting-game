using System;
using UnityEngine;

public abstract class PlayerHandlerBase : MonoBehaviour
{
    public int PlayerIndex { get; set; }
    
    public GameObject? SelectedCharacterPrefab { get; private set; }

    protected virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SetCharacter(GameObject? characterPrefab)
    {
        SelectedCharacterPrefab = characterPrefab;
    }

    public GameObject SpawnCharacter(Transform spawnPoint)
    {
        if (SelectedCharacterPrefab == null)
            return null;

        // Spawn the character
        GameObject character = Instantiate(SelectedCharacterPrefab, spawnPoint.position, spawnPoint.rotation);

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

        InitializeCharacterInput(character);

        return character;
    }

    protected abstract void InitializeCharacterInput(GameObject spawnedCharacter);
}