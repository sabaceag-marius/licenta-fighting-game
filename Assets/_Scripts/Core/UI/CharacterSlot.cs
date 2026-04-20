using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterSlot : MonoBehaviour
{
    public GameObject CharacterPrefab;

    public string CharacterName;

    public RectTransform rectTransform { get; private set; }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        TMP_Text tmpText = GetComponentInChildren<TMP_Text>();

        tmpText.text = CharacterName;
    }
}
