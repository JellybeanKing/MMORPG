using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject TreantPrefab;
    [SerializeField] private Transform SpawnPoint;       
    
    //respawns treant with timer in seconds
    [Server]
    public void RespawnTreant(int respawnTimer)
    {
        StartCoroutine(RespawnTreantTimer(respawnTimer));
    }

    //waits for timer then spwans treant
    [Server]
    private IEnumerator RespawnTreantTimer(int timer)
    {
        yield return new WaitForSeconds(timer);
        SpawnTreant();
    }

    //spawn treant at spwanpoint
    [Server]
    public void SpawnTreant()
    {
        GameObject treant = Instantiate(TreantPrefab, new Vector3(SpawnPoint.position.x, SpawnPoint.position.y), Quaternion.identity);        
        NetworkServer.Spawn(treant);                      
    }
}
