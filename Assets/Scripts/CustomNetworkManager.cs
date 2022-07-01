using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] private MonsterSpawner spawner;
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server Started");
        spawner.SpawnTreant();        
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Server Stopped");
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();        
        Debug.Log("Client Connected");        
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("Client Disconnected");
    }     

}
