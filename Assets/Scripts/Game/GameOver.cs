using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winners;
    [SerializeField] private TextMeshProUGUI stats;
    [SerializeField] private TextMeshProUGUI mvpStats;
    public bool win;
    private Player player;
    private Player mvp;

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
        mvp = player;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.GetComponent<Player>().killCount.Value > mvp.killCount.Value) mvp = p.GetComponent<Player>();
        }

        if (win) { winners.text = "You win"; }
        else { winners.text = "You lose"; }

        stats.text = "You got " + player.killCount.Value + " kills and died " + player.deathCount.Value + " times";
        mvpStats.text = mvp.killCount.Value + " kills and " + mvp.deathCount.Value + " deaths";
    }
}
