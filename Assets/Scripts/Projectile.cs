using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField][SyncVar] public Player player; //player that shot the projectile
    [SerializeField] private float speed = 10f; //projectile speed
    [SyncVar] public int direction; //projectile direction 1 = right, 2 = left, 3 = up and 4 = down
    // Update is called once per frame
        
    //moves prjectile according to direction
    void Update()
    {
        if (direction == 1)
        {
            transform.position += transform.up * Time.deltaTime * speed;
        }
        else if (direction == 2)
        {
            transform.position += transform.up * Time.deltaTime * speed;
        }
        else if (direction == 3)
        {
            transform.position += transform.up * Time.deltaTime * speed;
        }
        else if (direction == 4)
        {
            transform.position += transform.up * Time.deltaTime * speed;
        }

    }

    //calculate projectile dmg server side
    [Server]
    public void CalculateDmg(Enemy enemy)
    {
        int damage;
        float randValue = Random.value;        
        if (randValue < 1f - player.critrate) // 100% minus critrate 
        {
            // Do Normal Attack
            damage = Mathf.RoundToInt(player.attackPower * Random.Range(1f, 1.3f));
            Debug.Log("Monster got hit with" + damage);
        }
        else
        {
            // Do Crit Attack
            damage = Mathf.RoundToInt(player.attackPower * 2);
            Debug.Log("Monster got crit with " + damage);
        }
        enemy.ApplyDamage(damage);
    }

    //handles collisions with terrain or enemies
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Teleporter") || collision.gameObject.CompareTag("Terrain"))
        {            
            NetworkServer.Destroy(this.gameObject); //destroy projectile when it hits terrain
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {            
            Enemy enemy = collision.GetComponent<Enemy>();
            CalculateDmg(enemy); //calculate dmg and apply it on the enemy
            NetworkServer.Destroy(this.gameObject); //destroy projectile after hit
        }
    }       
}
