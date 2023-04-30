using UnityEngine;
public class InGameMenuManager : MenuManager
{
    [SerializeField] private GameObject pauseScreen;

    private void OnEnable()
    {
        pauseScreen.SetActive(false);
        loadingScreen.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseScreen.activeSelf)
            {
                ResumeGame();
            }
            else
            {
                LoadPauseMenu();
            }
        }
    }

    private void LoadPauseMenu()
    {
        pauseScreen.SetActive(true);
        GameManager.GetInstance().controlable = false;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        pauseScreen.SetActive(false);
        GameManager.GetInstance().controlable = true;
        Time.timeScale = 1f;
    }

}