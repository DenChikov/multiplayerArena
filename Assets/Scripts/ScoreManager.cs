using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField]private float timeToRestart = 5f;
    [SerializeField] private Text winPlayer;
    private GameObject[] dots;
    private void Awake()
    {
        dots = GetComponentsInChildren<GameObject>();
        ControllerGamer.winCall.AddListener(Restart);
    }
    IEnumerator restartScene()
    {
        yield return new WaitForSeconds(timeToRestart);
       
        CmdRespawnServerPosition();
    }
    [Command]
    private void CmdRespawnServerPosition()
    {
        RpcRespawnServerPosition();
    }
    [ClientRpc]
    private void RpcRespawnServerPosition()
    {

    }
    private void Restart()
    {
        StartCoroutine(restartScene());
    }
}
