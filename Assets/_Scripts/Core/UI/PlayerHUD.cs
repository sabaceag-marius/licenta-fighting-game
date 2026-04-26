using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI percentageText;
    public Image[] stockImages;

    [Header("Heart Sprites")]
    public Sprite stockSprite;

    public void UpdateDamage(float currentDamage)
    {
        percentageText.text = currentDamage.ToString("0") + "%"; 
    }

    public void UpdateStocks(int currentHealth)
    {
        for (int i = 0; i < stockImages.Length; i++)
        {
            if (i < currentHealth)
            {
                stockImages[i].sprite = stockSprite;
                stockImages[i].enabled = true;
            }
            else
            {
                stockImages[i].enabled = false;
            }
        }
    }
}