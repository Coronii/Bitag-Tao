using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    private void Awake()
    {
        // !Placeholder listeners - replaced with real logic 
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void RetryGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    public void SetStats(float totalTimeSeconds, float averageTimePerLevelSeconds,
        int levelsCompleted, int totalLevels, int correctGuesses)
    {
        if (statsText == null) return;

        statsText.text =
            $"Time spent: {FormatTime(totalTimeSeconds)}\n" +
            $"Average time per level: {FormatTime(averageTimePerLevelSeconds)}\n" +
            $"Total Words Guessed: {levelsCompleted}/{totalLevels}\n" +
            $"Correct Letter Guesses: {correctGuesses}";
    }

    private string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;
        return $"{minutes}:{secs:00}";
    }
}
