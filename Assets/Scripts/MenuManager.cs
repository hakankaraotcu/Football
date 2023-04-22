using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject controlsScreen;
    [SerializeField] private GameObject mainMenuScreen;

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;

    public void LoadScene(int sceneIndex)
    {
        mainMenuScreen.SetActive(false);
        loadingScreen.SetActive(true);

        StartCoroutine(LoadGame(sceneIndex));
    }

    private void OnEnable()
    {
        mainMenuScreen.SetActive(true);
        loadingScreen.SetActive(false);
        controlsScreen.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadControls()
    {
        controlsScreen.SetActive(true);
        mainMenuScreen.SetActive(false);
    }

    public void LoadMainMenu()
    {
        controlsScreen.SetActive(false);
        mainMenuScreen.SetActive(true);
    }

    private IEnumerator LoadGame(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = progress;

            yield return null;
        }
    }
}
