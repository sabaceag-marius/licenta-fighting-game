using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LocalCharacterSelectManager : BaseCharacterSelectManager
{
    public override void TryStartMatch()
    {
        if (canStartMatch)
        {
            canStartMatch = false;

            SceneManager.LoadScene("CombatScene");
        }
    }
}