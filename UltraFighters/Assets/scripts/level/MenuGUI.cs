using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuGUI : MonoBehaviour
{


    void Start()
    {
        Debug.ClearDeveloperConsole();
        dataManager.gameData.LastScene = "idklol";
    }
    public void toPlayOffline()
    {
        SceneManager.LoadScene("SinglePlayer");
    }
    public void toPlayMultiplayer()
    {
        SceneManager.LoadScene("Demolition");
    }
    public void toSettingsMenu()
    {
        SceneManager.LoadScene("Settings");
    }

    public void toExit()
    {
        Application.Quit();
    }

}
