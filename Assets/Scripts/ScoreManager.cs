using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField] private float timeToRestart = 5f;
    private GameObject[] dots;
    private GameObject[] players;
    private ControllerGamer[] score;
    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        dots = GameObject.FindGameObjectsWithTag("Respawn");
        ControllerGamer.winCall.AddListener(Restart);
    }
    IEnumerator restartScene()
    {
        Debug.Log("CorutineScoreManagerTrue");
        yield return new WaitForSeconds(timeToRestart);
        for (int i = 0; i < dots.Length; i++)
        {
            int rnd = (int)UnityEngine.Random.Range(0f, 4f);
            players[i].transform.position = dots[rnd].transform.position;
            if (i != 0)
            {
                if (players[i].transform.position == players[i - 1].transform.position)
                {
                    rnd = (int)UnityEngine.Random.Range(0f, 4f);
                    players[i].transform.position = dots[rnd].transform.position;
                }
            }
        }


    }
    [ClientRpc]
    private void RpcRespawnServerPosition()
    {
        StartCoroutine(restartScene());

    }
    [Command(requiresAuthority = false)]
    private void CmdRespawnServerPosition()
    {
        RpcRespawnServerPosition();
        Debug.Log("RpcRespawnTrue");
    }

    private void Restart()
    {
        CmdRespawnServerPosition();
        Debug.Log("CmdPositionTrue");
    }
}
