using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Experimental;

public class Player : NetworkBehaviour
{
    #region Variables
    [SerializeField] private string playerName = "BJ"; //player nickname not implemented atm
    [SerializeField] public int attackPower = 10; 
    [SerializeField] public float critrate = .20f; //critrate from 0 to 1
    [SerializeField] public int maxHp = 50;
    [SerializeField][SyncVar] public int hp;    
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    private Vector2 movement;    
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GameObject arrowPrefab;    
    public delegate void HealthChangedDelegate(int hp, int maxHp);    
    public event HealthChangedDelegate EventHealthChanged; //event called on server and all clients when health of a player/enemy changes
    public string currentLocation;
    private float attackCooldown = 1f; //number of seconds cooldown for attack presses
    private float lastAttackTime = 0f; //stores the time of the last attack for cooldown purposes   
    [SyncVar] private float horizontal = 1f; //stores the horizontal direction the player is facing
    [SyncVar] private float vertical = 0f; //stores the vertical direction the player is facing
    #endregion

    //function is called when player is enabled
    private void OnEnable()
    {        
        GameObject[] otherObjects = GameObject.FindGameObjectsWithTag("Player");  //find other players     
        healthBar.SetMaxHp(hp, maxHp); //set healthbar
        SyncHealthBars(); //sync healthbars with most rescent network variables
        currentLocation = "Location A"; //set current location on client
        SetLocation(currentLocation); //set current location on server
        CmdPlayerConnected(); // call playerconnected command on the server
        foreach (GameObject obj in otherObjects) //ignore collisions with other players
        {
            Physics2D.IgnoreCollision(obj.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }        
    }
    public override void OnStartLocalPlayer()
    {
        Camera.main.GetComponent<CameraFollow>().setTarget(gameObject.transform); //set main cam to follow player        
    }
    //handles movement and movement animations
    private void HandleMovement()
    {
        if (!this.animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) //dont move while attacking
        {
            //check if player is local
            if (isLocalPlayer)
            {
                //animations
                movement.x = Input.GetAxis("Horizontal");
                movement.y = Input.GetAxis("Vertical");
                animator.SetFloat("Horizontal", movement.x);
                animator.SetFloat("Vertical", movement.y);
                animator.SetFloat("Speed", movement.sqrMagnitude);

                if(Input.GetAxisRaw("Horizontal")== 1 || Input.GetAxisRaw("Horizontal") == -1 || Input.GetAxisRaw("Vertical") == 1 || Input.GetAxisRaw("Vertical") == -1)
                {
                    horizontal = Input.GetAxisRaw("Horizontal"); //set horizontal direction
                    vertical = Input.GetAxisRaw("Vertical"); //set vertical direction
                    animator.SetFloat("LastHorizontal", horizontal);
                    animator.SetFloat("LastVertical", vertical);
                }   
                if(movementSpeed > 5) //check if player movement speed exceeds max
                {
                    CmdSpeedLimitExceeded(movementSpeed);
                }
                //move player
                playerRB.MovePosition(playerRB.position + movement * movementSpeed * Time.fixedDeltaTime); 
            }
        }        
    }
    //handles attack animations
    private void HandleAttack()
    {        
        if (!this.animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) //check if not already attacking
        {
            playerRB.velocity = Vector2.zero; // stop player from moving
            networkAnimator.SetTrigger("Attack"); //set animation            
        }                           
    }   

    void Update()
    {
        HandleMovement();

        if(Input.GetKeyDown(KeyCode.X)) //when x is pressed
        {
            if (isLocalPlayer)
            {
                float currentTime = Time.time; //get current time
                float diffSecs = currentTime - lastAttackTime; //compare current time to time of last attack
                if(diffSecs >= attackCooldown) //if there is no cooldown attack
                {
                    lastAttackTime = currentTime; //set attack time
                    CmdSpawnArrow(this, horizontal, vertical); //ask server to spwan arrow
                    HandleAttack(); //animations
                }                
            }               
            
        }
        else if (Input.GetKeyDown(KeyCode.Z)) //take damage on z press
        {
            if(isLocalPlayer)
                CmdTakeDamage(5, .2f); //ask server to handle damage calc
        }        
    }   
      
    //spawns arrow serverside according to player position
    [Command]
    public void CmdSpawnArrow(Player player, float horizontal, float vertical)
    {
        if (horizontal > 0) //if player is facing right
        {
            GameObject arrow = Instantiate(arrowPrefab, transform.position + new Vector3(1f, -0.3f), transform.rotation * Quaternion.Euler(0f, 0f, 270f));
            Projectile projectile = arrow.GetComponent<Projectile>();
            projectile.direction = 1; //set arrow direction
            projectile.player = player; //set arrow player for dmg calc
            NetworkServer.Spawn(arrow);
        }
        else if (horizontal < 0) //if player is facing left
        {
            GameObject arrow = Instantiate(arrowPrefab, transform.position + new Vector3(-1f, -0.3f), transform.rotation * Quaternion.Euler(0f, 0f, 90f));
            Projectile projectile = arrow.GetComponent<Projectile>();
            projectile.direction = 2;
            projectile.player = player;
            NetworkServer.Spawn(arrow);
        }
        else if (vertical > 0) //if player is facing up
        {
            GameObject arrow = Instantiate(arrowPrefab, transform.position + new Vector3(0f, 1.2f), transform.rotation * Quaternion.Euler(0f, 0f, 0f));
            Projectile projectile = arrow.GetComponent<Projectile>();
            projectile.direction = 3;
            projectile.player = player;
            NetworkServer.Spawn(arrow);
        }
        else if (vertical < 0) //if player is facing down
        {
            GameObject arrow = Instantiate(arrowPrefab, transform.position + new Vector3(0f, -1.2f), transform.rotation * Quaternion.Euler(0f, 0f, 180f));
            Projectile projectile = arrow.GetComponent<Projectile>();
            projectile.direction = 4;
            projectile.player = player;
            NetworkServer.Spawn(arrow);
        }
    }

    //server side dmg calc on taking dmg
    [Command]
    public void CmdTakeDamage(int attackPower, float critrate)
    {
        int damage;
        float randValue = Random.value;
        if (randValue < 1f - critrate) // 100% minus critrate 
        {
            // Do Normal Attack
            damage = Mathf.RoundToInt(attackPower * Random.Range(1f, 1.3f));
            Debug.Log("Player got hit with" + damage);
        }        
        else 
        {
            // Do Crit Attack
            damage = Mathf.RoundToInt(attackPower * 2);
            Debug.Log("Player got crit with " + damage);
        }
        
        hp -= damage;
        if(hp <= 0)
        {
            hp = 0;
            //handle dead player
        }
        SetPlayerHealth(hp);
    }

    //travel function transports player
    void Travel(Transform spawn)
    {
        transform.position = new Vector3(spawn.position.x, spawn.position.y);        
    }

    //collision with teleporter
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Teleporter"))
        {
            if (isLocalPlayer)
            {                
                Teleporter teleporter = collision.GetComponent<Teleporter>();
                Travel(teleporter.spawnPoint); //call travel function with spawn location
                currentLocation = teleporter.location; //set current location on client
                SetLocation(currentLocation); //set current location on server
                CmdPlayerTravel(teleporter.location); //call load and distribution for print
            }            
        }        
    }
    //invokes healtyh changed event on all clients
    [ClientRpc]
    private void RpcSetPlayerHealth(int hp , int maxHp)
    {
        this.EventHealthChanged?.Invoke(hp, maxHp);
    }
    //calls load and distribution when player travels
    [Command]
    public void CmdPlayerTravel(string location)
    {
        Object.FindObjectOfType<LoadAndDistributionManager>().PrintPlayerLocations();
    }
    //set location server side
    [Command]
    public void SetLocation(string location)
    {
        currentLocation = location;
    }
    //calls load and distribution when player connects
    [Server]
    public void CmdPlayerConnected()
    {        
        Object.FindObjectOfType<LoadAndDistributionManager>().PrintPlayerLocations();
    }
    //gets called when a player exceeds the speed limit
    [Command]
    public void CmdSpeedLimitExceeded(float speed)
    {
        RpcResetPlayerSpeed(speed);
    }
    //resets the speed limit of the client that exceeded it
    [TargetRpc]
    private void RpcResetPlayerSpeed(float speed)
    {
        Debug.Log("You've exeeded the speed limit! Are you hacking?");
        Debug.Log("Your speed was " + speed);
        movementSpeed = 5f;
    }
    //syncs the all the health bars on the client
    public void SyncHealthBars()
    {        
        HealthBar[] healthBars = Object.FindObjectsOfType<HealthBar>();
        foreach (HealthBar healthBar in healthBars)
        {
            healthBar.SyncHealthBar();
        }
    }
    //invokes the health changed event on server

    [Server]
    public void SetPlayerHealth(int newHp)
    {
        hp = newHp;
        this.EventHealthChanged?.Invoke(hp, maxHp);
        RpcSetPlayerHealth(hp, maxHp);
    }       
}
