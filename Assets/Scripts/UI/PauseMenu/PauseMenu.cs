using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    public bool IsPaused { get; private set; } = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1;
        IsPaused = false;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Resume audio
        // FindObjectOfType<AudioManager>().ResumeAudio();
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0;
        IsPaused = true;
        
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        // Pause or lower volume for audio
        // FindObjectOfType<AudioManager>().PauseAudio();
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenSettings()
    {
        Debug.Log("Opening Settings");
        // Implement settings menu functionality
    }

    public void QuitToMainMenu()
    {
        Debug.Log("Quitting To Main Menu");
        //Time.timeScale = 1;
        //SceneManager.LoadScene("MainMenu"); // Replace "MainMenu" with main menu scene name
        
        /*
         * For Quitting the game Application.Quit() needs to be used
         */
    }
}
