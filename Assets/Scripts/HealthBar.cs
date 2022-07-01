using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMPro.TMP_Text Healthtext;
    [SerializeField]private Player player; //player the bar is attached to
    [SerializeField] private Enemy enemy; //enemy the bar is attached to

    //subscribe to heatlhchanged event for corresponding player or enemy
    private void OnEnable()
    {        
        if(player != null)
            player.EventHealthChanged += EventHealthChangedHandler;
        else if (enemy != null)
            enemy.EventHealthChanged += EventHealthChangedHandler;
    }
    //unsubscribe to heatlhchanged event for corresponding player or enemy
    private void OnDisable()
    {
        if (player != null)
            player.EventHealthChanged -= EventHealthChangedHandler;
        else if (enemy != null)
            enemy.EventHealthChanged -= EventHealthChangedHandler;
    }
    //set hp when event is called
    private void EventHealthChangedHandler(int hp, int maxHp)
    {        
        SetHp(hp, maxHp);
    }
    //set maxhp of bar
    public void SetMaxHp(int hp, int maxhp)
    {
        slider.maxValue = maxhp;
        slider.value = hp;
        Healthtext.SetText(hp + "/" + maxhp);
    }
    //update hp values
    public void SetHp(int hp, int maxHp)
    {
        slider.value = hp;
        Healthtext.SetText(hp + "/" + maxHp);
    }    
    //syncs healthbars with latest values
    public void SyncHealthBar()
    {
        if (player != null)
            SetHp(player.hp, player.maxHp);
        else if (enemy != null)
            SetHp(enemy.hp, enemy.maxHp);
    }
}
