using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Enemy : NetworkBehaviour
{
    [SerializeField] public int maxHp;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private int respawnTimer;
    public delegate void HealthChangedDelegate(int hp, int maxHp);
    public event HealthChangedDelegate EventHealthChanged; //event when monster hp changes    
    [SerializeField][SyncVar]public int hp;

    [SerializeField] private int attackPower;

    //set maxhp on spawn
    private void OnEnable()
    {
        SetMaxHp();
    }
    //calls healthbar function
    public void SetMaxHp()
    {        
        healthBar.SetMaxHp(hp, maxHp);        
    }
        
    // Update function for enemy behaviour
    void Update()
    {
        
    }     
    
    //calls health changed event on clients
    [ClientRpc]
    private void RpcSetEnemyHealth(int hp, int maxHp)
    {
        this.EventHealthChanged?.Invoke(hp, maxHp);
    }    
    //applies dmg on monster serverside end calls client rpc
    [Server]
    public void ApplyDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            //calls respawner when monster dies
            GameObject.Find("MonsterSpawner").GetComponent<MonsterSpawner>().RespawnTreant(respawnTimer); 
            Debug.Log("monster death");
            NetworkServer.UnSpawn(this.gameObject);
            NetworkServer.Destroy(this.gameObject);
        }
        else
        {
            //invokes health changed event server side
            this.EventHealthChanged?.Invoke(hp, maxHp);
            RpcSetEnemyHealth(hp, maxHp); //invoke client side
        }        
    }
}
