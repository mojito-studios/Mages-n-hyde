using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : NetworkBehaviour
{
    [SerializeField] private Image result;
    [SerializeField] private Sprite winning;
    [SerializeField] private TextMeshProUGUI stats;
    [SerializeField] private TextMeshProUGUI mvpName;
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
        if (IsServer) { RestartRpc(); }
        Destroy(OptionsChosen.Instance.gameObject);
        Destroy(NetworkManager.Singleton.gameObject);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
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

        if (win) { result.sprite = winning; }

        stats.text = player.killCount.Value + " kills, " + player.assistCount.Value + " assists\n" + player.deathCount.Value;
        mvpName.text = mvp.GetComponent<Player>().character;
        mvpSprite.sprite = mvp.GetComponentInChildren<SpriteRenderer>().sprite;
        mvpSprite.color = mvp.GetComponentInChildren<SpriteRenderer>().color;
        mvpStats.text = mvp.GetComponent<Player>().killCount.Value + " kills, " + mvp.GetComponent<Player>().assistCount.Value + " assists\n" + mvp.GetComponent<Player>().deathCount.Value + " deaths";
    }

    [Rpc(SendTo.NotServer)]
    private void RestartRpc()
    {
        Destroy(OptionsChosen.Instance.gameObject);
        Destroy(NetworkManager.Singleton.gameObject);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
    }
}
