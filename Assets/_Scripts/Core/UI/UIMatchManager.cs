using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Core.UI
{
    
    public class UIMatchManager : MonoBehaviour
    {
        public static UIMatchManager Instance { get; private set; }

        [Header("UI objects")]

        [SerializeField]
        private TMP_Text TimerLabel;

        [SerializeField]
        private PlayerHUD[] playerHUDs;

        private char[] timerChars = new char[] { '0', '0', ':', '0', '0', '.', '0', '0' };

        private void OnEnable()
        {
            MatchEventBus.OnTimerUpdated += UpdateTimer;
            MatchEventBus.OnCharacterStocksChanged += UpdateCharacterStocks;
            MatchEventBus.OnCharacterDamageChanged += UpdateCharacterDamage;
        }

        private void OnDisable()
        {
            MatchEventBus.OnTimerUpdated -= UpdateTimer;
            MatchEventBus.OnCharacterStocksChanged -= UpdateCharacterStocks;
            MatchEventBus.OnCharacterDamageChanged -= UpdateCharacterDamage;
        }

        private void UpdateTimer(long framesRemaining, int gameFPS)
        {
            if (TimerLabel == null)
            {
                Debug.LogWarning("Timer label was not set in the scene!");
                return;
            }

            if (framesRemaining < 0)
            {
                framesRemaining = 0;
            }

            // 1. Convert frames to total raw seconds (with decimals)
            double totalSecondsRemaining = (double)framesRemaining / gameFPS;

            // Extract Minutes, Seconds, and Milliseconds
            int minutes = (int)(totalSecondsRemaining / 60);
            int seconds = (int)(totalSecondsRemaining % 60);
            int milliseconds = (int)((totalSecondsRemaining - Mathf.Floor((float)totalSecondsRemaining)) * 100);

            timerChars[0] = (char)('0' + (minutes / 10));
            timerChars[1] = (char)('0' + (minutes % 10));

            timerChars[3] = (char)('0' + (seconds / 10));
            timerChars[4] = (char)('0' + (seconds % 10));
            
            timerChars[6] = (char)('0' + (milliseconds / 10));
            timerChars[7] = (char)('0' + (milliseconds % 10));

            // Display the format MM:SS:ms
            // {0:00} ensures 2 digits (e.g., 01 instead of 1)
            TimerLabel.SetText("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }

        private void UpdateCharacterStocks(int i, int stockCount)
        {
            if (playerHUDs == null || i >= playerHUDs.Length)
                return;

            playerHUDs[i].UpdateStocks(stockCount);
        }

        private void UpdateCharacterDamage(int i, FixedFloat damagePercentage)
        {
            if (playerHUDs == null || i >= playerHUDs.Length)
                return;

            playerHUDs[i].UpdateDamage(damagePercentage);
        }
    }
}