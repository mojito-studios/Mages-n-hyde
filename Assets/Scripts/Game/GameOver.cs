using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winners;
    [SerializeField] private TextMeshProUGUI stats;
    [SerializeField] private TextMeshProUGUI mvpStats;
    [SerializeField] private SpriteRenderer mvpSprite;
    public bool win;
    private Player player;
    private GameObject mvp;

    private void Start()
    {
    }
    public void OnRestart()
    {
        SceneManager.LoadScene(0);
        NetworkManager.Singleton.Shutdown();
        //NetworkManager.Destroy(NetworkManager.Singleton);
    }
    private void OnEnable()
    {
        player = this.GetComponentInParent<Player>();
        mvp = player.gameObject;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.GetComponent<Player>().killCount.Value > mvp.GetComponent<Player>().killCount.Value) mvp = p;
            else if (p.GetComponent<Player>().killCount.Value == mvp.GetComponent<Player>().killCount.Value)
            {
                if (p.GetComponent<Player>().assistCount.Value > mvp.GetComponent<Player>().assistCount.Value) mvp = p;
                else if (p.GetComponent<Player>().assistCount.Value == mvp.GetComponent<Player>().assistCount.Value)
                {
                    if (p.GetComponent<Player>().deathCount.Value < mvp.GetComponent<Player>().deathCount.Value) mvp = p;
                }
            }
        }

        if (win) { winners.text = "You win"; }
        else { winners.text = "You lose"; }

        stats.text = "You got " + player.killCount.Value + " kills, " + player.assistCount.Value + " assists and died " + player.deathCount.Value + " times";

        mvpSprite.sprite = mvp.GetComponent<SpriteRenderer>().sprite;
        mvpSprite.color = mvp.GetComponent<SpriteRenderer>().color;
        mvpStats.text = mvp.GetComponent<Player>().killCount.Value + " kills, " + mvp.GetComponent<Player>().assistCount.Value + " assists and " + mvp.GetComponent<Player>().deathCount.Value + " deaths";
    }
}
