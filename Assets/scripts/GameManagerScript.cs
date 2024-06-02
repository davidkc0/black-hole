using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{
    public GameObject gameOverUI;
    private BlackHoleController blackHoleController;

    private void Start()
    {
        blackHoleController = FindObjectOfType<BlackHoleController>();
    }

    private void Update()
    {
        // No need for any code here in this example
    }

    public void gameOver()
    {
        gameOverUI.SetActive(true);
    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        blackHoleController.isChangeColorActive = false;
        blackHoleController.isFreezeActive = false;
    }

    public void mainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void startGame()
    {
        SceneManager.LoadScene("Game");
    }
}