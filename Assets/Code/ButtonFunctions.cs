using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu = null;
    public void Resume()
    {
        Time.timeScale = 1.0f;
        pauseMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("is_paused", 0.0f);
    }

    public void Restart() {
        //Restarts the current level
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level 1") {
            ToLevelOne();
        }
        /**
        else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level 2") {
            ToLevelTwo();
        }
        else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level 3") {
            ToLevelThree();
        }
        **/
        Resume();
    }

    public void ToLevelOne()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Level 1");
    }
    public void ToLevelTwo()
    {
        //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Level 1");
    }
    public void ToLevelThree()
    {
        //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Level 1");
    }

    public void QuitToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenu");
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("is_paused", 0.0f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnPause()
    {
        var t = pauseMenu.activeSelf;
        pauseMenu.SetActive(!t);
        if (!t)
        {
            Time.timeScale = 0.0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("is_paused", 1.0f);
        }
        else
            Resume();
    }
}
