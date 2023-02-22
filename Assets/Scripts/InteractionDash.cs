using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDash : NetworkBehaviour
{
    [SerializeField] private float changeColorTime = 1f;
    private float nextChangeColorTime;
    public Color playerColor = Color.white;
    [ClientRpc]
    public void RpcCollorChanger()
    {
        if (Time.time > nextChangeColorTime)
        {
            nextChangeColorTime = Time.time + changeColorTime;
            playerColor = Color.red;

            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                renderer.material.color = playerColor;
            }
        }
    }
    [ClientRpc]
    private void RpcChangerUpdate()
    {
        if (Time.time > nextChangeColorTime)
        {
            playerColor = Color.white;


            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                renderer.material.color = playerColor;
            }

        }
    }
    private void Update()
    {
        CmdUpdatePlayer();
        //RpcChangerUpdate();
    }
    [Command(requiresAuthority = false)]
    public void CmdChangeColorPlayer()
    {
        RpcCollorChanger();
    }
    [Command(requiresAuthority = false)]
    private void CmdUpdatePlayer()
    {
        RpcChangerUpdate();
    }
}
