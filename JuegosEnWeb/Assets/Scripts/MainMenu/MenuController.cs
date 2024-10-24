using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControlller : MonoBehaviour
{
    public enum States
    {
        MainMenu,
        HostClient,
        CreditsMenu,
        Quit
    }
    public States _states;
    public List<GameObject> menus = new List<GameObject> ();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (_states)
        {
      
            case States.MainMenu:
                MainMenuState();
                Debug.Log("MainMenu");
                break;
            case States.HostClient:
                HostClientState();
                Debug.Log("HostClient");
                break;
            case States.CreditsMenu:
                CreditsState();
                Debug.Log("Credits");
                break;
            case States.Quit:
                Debug.Log("Quitting");
                Quit();
                break;
        }
    }

    public void ChangeState(int number)
    {
        switch (number)
        {
            case 0:
                _states = States.MainMenu;
                break;
            case 1:
                _states = States.HostClient;
                break;
            case 2:
                _states = States.CreditsMenu;
                break;
            case 3:
                _states = States.Quit;
                break;
        }
    }

    private void MainMenuState()
    {
        menus[1].SetActive(false);
        menus[2].SetActive(false);
       if(!menus[0].activeSelf) menus[0].SetActive(true);
    }

    private void HostClientState() //Mi idea esq aqui los jugadores eligieran si son host o clients y luego pasasen directamente a la escena de  lobby de personalización, pero como me da cosa liarla para Pablo cuando haga el lobby mejor que lo mire él
    {
         /*menus[0].SetActive(false);
         menus[2].SetActive(false);
         menus[1].SetActive(true);*/
       SceneManager.LoadScene(1); //Carga el juego actual. En principio va bien
    }

    private void CreditsState()
    {
        menus[0].SetActive(false);
        menus[1].SetActive(false);
        menus[2].SetActive(true);
    }
    private void Quit()
    {
        Application.Quit(0);
    }
}
