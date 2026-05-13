using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [Header("Scene settings")]

    [SerializeField]
    private Transform SpawnPointsTransform;

    [Header("Camera Setup")]
    
    [SerializeField]
    private CinemachineTargetGroup TargetGroup;

    void Awake()
    {
        if (SpawnPointsTransform == null)
            return;
            
        PlayerHandler[] players = FindObjectsOfType<PlayerHandler>();

        Transform[] spawnpoints = SpawnPointsTransform.GetComponentsInChildren<Transform>().Where(t => t != SpawnPointsTransform).ToArray();

        for (int i = 0; i < Math.Min(players.Length, spawnpoints.Length); i++)
        {
            GameObject character = players[i].SpawnCharacter(spawnpoints[i]);

            if (character != null)
            {
                TargetGroup.AddMember(character.transform, 1f, 2f);
            }
        }
    }
}
