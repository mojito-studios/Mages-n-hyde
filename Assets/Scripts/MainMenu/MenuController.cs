using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControlller : MonoBehaviour
{
    [SerializeField] private AudioClip currentClip;
    private enum States
    {
        MainMenu,
        HostClient,
        CreditsMenu,
        Tutorial
    }
    private States _states;
   [SerializeField] private List<GameObject> menus = new List<GameObject> ();
   [SerializeField] private List<GameObject> tutorialTexts = new List<GameObject> ();
   int actualText;
    [SerializeField] GameObject animationImage;
    void Start()
    {
        actualText = 0;
        BackgroundMusicController.instance.SetCurrentClip(currentClip);
    }

    // Update is called once per frame
    void Update()
    {
        switch (_states)
        {
      
            case States.MainMenu:
                MainMenuState();
                break;
            case States.HostClient:
                StartCoroutine(HostClientState());
                break;
            case States.CreditsMenu:
                CreditsState();
                break;
            case States.Tutorial:
                Tutorial();
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
                _states = States.Tutorial;
                break;
        }
    }

    public void ChangeText()
    {
        tutorialTexts[actualText].SetActive(false);
        actualText++;
        if(actualText >= tutorialTexts.Count) actualText = 0;
        tutorialTexts[actualText].SetActive(true);

    }

    public void PrevText()
    {
        tutorialTexts[actualText].SetActive(false);
        actualText--;
        if (actualText < 0) actualText = tutorialTexts.Count-1;
        tutorialTexts[actualText].SetActive(true);

    }
    private void MainMenuState()
    {
        menus[1].SetActive(false);
        menus[2].SetActive(false);
       if(!menus[0].activeSelf) menus[0].SetActive(true);
    }


    private IEnumerator HostClientState()
    {
        // menus[0].SetActive(false);
        animationImage.SetActive(true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(3);
    }

   
    private void CreditsState()
    {
        
        menus[0].SetActive(false);
        menus[2].SetActive(false) ;
        menus[1].SetActive(true);
    }
    private void Tutorial()
    {
        menus[0].SetActive(false);
        menus[1].SetActive(false);
        menus[2].SetActive(true);
    }
}
