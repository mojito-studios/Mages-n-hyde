using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winners;
    [SerializeField] private Judge judge;
    public void OnRestart()
    {
        SceneManager.LoadScene(0);
        //NetworkManager.Destroy(NetworkManager.Singleton);
    }
    private void Awake()
    {
        judge = GameObject.FindGameObjectWithTag("Judge").GetComponent<Judge>();
        winners.text = "Losers:" + FindObjectOfType<Judge>().winningTeam.ToString();
    }
}
