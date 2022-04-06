using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu = null;
    
    [Tooltip("First overlay MUST be the Pause Menu Options")]
    [SerializeField] GameObject[] pauseOverlays = null;

    public void Resume()
    {
        Time.timeScale = 1.0f;
        ResetPause();
        pauseMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("is_paused", 0.0f);
    }

    //Fixes bug with Esc and reopening leaving sub-menus open
    void ResetPause() {
        pauseOverlays[0].SetActive(true);
        for (int i = 1; i < pauseOverlays.Length; i++) {
            if (pauseOverlays[i].activeSelf) 
                pauseOverlays[i].SetActive(false);
        }
    }

    public void Restart() {
        //Restarts the current level
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level 1") {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("level_section", 0);
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
        Time.timeScale = 1.0f;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("is_paused", 0.0f);
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenu");
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
