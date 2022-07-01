using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadAndDistributionManager : NetworkBehaviour
{
    public List<Player> players; //keeps track of connected players

    //prints player ids and locations server side
    [Server]
    public void PrintPlayerLocations()
    {          
        players = Object.FindObjectsOfType<Player>().ToList();
        foreach (Player player in players)
        {
            Debug.Log(player.netIdentity + " Location: " + player.currentLocation);
        }        
    }

    //[Server]
    //public void PlayerTravel(NetworkIdentity id, string loc)
    //{
    //    PlayerData playerData = players.Find(a => a.networkId == id);        
    //    foreach(PlayerData player in players)
    //    {
    //        if(player.location == loc)
    //        {
    //            SendTravelMessage(player.networkId, playerData.nickname);
    //        }
    //    }
    //    playerData.location = loc;
    //}

    //[ClientRpc]
    //public void SendTravelMessage(NetworkIdentity connection, string nickname)
    //{
    //    Debug.Log(nickname + " traveled to your location");
    //}
}
