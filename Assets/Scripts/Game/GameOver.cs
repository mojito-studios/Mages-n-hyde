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
    [SerializeField] public Canvas canvas;
    public bool win;
    private Player player;
    private GameObject mvp;

    public void OnRestart()
    {
        SoundManager.Instance.PlaySound("button");
        if (IsServer) { RestartRpc(); }
        Destroy(OptionsChosen.Instance.gameObject);
        Destroy(NetworkManager.Singleton.gameObject);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
    }


    public void OnEnd()
    {
        //GetComponent<Animator>().SetBool("isFirst", false);
        player = this.GetComponent<Player>();
        mvp = player.gameObject;
        GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerParent");
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
        mvpSprite.sprite = mvp.GetComponent<GameOver>().canvas.GetComponent<Image>().sprite;
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
