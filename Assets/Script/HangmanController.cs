using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HangmanController : MonoBehaviour
{
    [SerializeField] GameObject wordContainer;
    [SerializeField] GameObject keyboardContainer;
    [SerializeField] GameObject letterContainer;
    [SerializeField] GameObject[] hangmanStages;
    [SerializeField] GameObject letterButton;
    [SerializeField] GameOverPanelUI winPanelUI;
    [SerializeField] GameOverPanelUI losePanelUI;
    [SerializeField] TextMeshProUGUI themeLabel;
    [SerializeField] GameObject[] levelStages;
    [SerializeField] TextMeshProUGUI levelLabel;  


    private Dictionary<char, Button> letterButtons = new Dictionary<char, Button>();
    private string currentPhrase;
    private int incorrectGuesses, correctGuesses;
    private int totalLettersToGuess;
    private bool isGameOver;
    private bool isTransitioning;

    private float gameStartTime;
    private int levelsCompleted;
    private int totalCorrectGuesses;

    void Start()
    {
        gameStartTime = Time.time;
        foreach (GameObject stage in hangmanStages)
            stage.SetActive(false); // life pool starts fresh only here, once per game

        if (winPanelUI != null) winPanelUI.gameObject.SetActive(false);
        if (losePanelUI != null) losePanelUI.gameObject.SetActive(false);

        BuildKeyboard();
        LoadLevel();
    }

    void Update()
    {
        if (isGameOver || isTransitioning) return;

        foreach (char c in Input.inputString)
        {
            char upper = char.ToUpperInvariant(c);

            if (letterButtons.TryGetValue(upper, out Button button) && button.interactable)
            {
                CheckLetter(upper.ToString(), button);
            }
        }
    }

    private bool IsGuessableChar(char c)
    {
        return char.IsLetter(c);
    }

    private void BuildKeyboard()
    {
        for (int i = 65; i <= 90; i++)
        {
            CreateButton(i);
        }
    }

    private void CreateButton(int i)
    {
        GameObject temp = Instantiate(letterButton, keyboardContainer.transform);
        Button button = temp.GetComponent<Button>();
        char letter = (char)i;

        temp.GetComponentInChildren<TextMeshProUGUI>().text = letter.ToString();
        button.onClick.AddListener(() => CheckLetter(letter.ToString(), button));
        letterButtons[letter] = button;
    }

    private void ResetKeyboard()
    {
        Debug.Log("Resetting keyboard");
        foreach (Button child in keyboardContainer.GetComponentsInChildren<Button>())
        {
            child.interactable = true;
        }
    }
    private void DisableKeyboard()
    {
        foreach (Button child in keyboardContainer.GetComponentsInChildren<Button>())
            child.interactable = false;
    }

    private void LoadLevel()
    {
        isGameOver = false;
        isTransitioning = false;
        ThemeManager.RandomizeTheme();

        TextAsset bank = ThemeManager.GetBankForCurrentLevel();
        if (bank == null)
        {
            Debug.LogError("No word/phrase bank found for the current theme and level. " +
                "Check that ThemeManager exists in the scene, its Themes array is populated, " +
                "and every tier bank (Easy/Medium/Hard) is assigned for the selected theme.");
            DisableKeyboard();
            return;
        }

        currentPhrase = PickRandomLine(bank).ToUpper();
        ResetKeyboard();
        BuildPhraseDisplay();
        UpdateHud();
        UpdateLevelStages();
    }

    private void UpdateHud()
    {
        if (themeLabel != null)
        {
            Theme theme = ThemeManager.GetCurrentTheme();
            themeLabel.text = theme != null ? theme.themeName : "";
        }

        if (levelLabel != null)
            levelLabel.text = LevelProgression.CurrentLevel.ToString();
    }

    private string PickRandomLine(TextAsset bank)
    {
        string[] lines = bank.text.Split('\n');
        string line;
        int attempts = 0;

        do
        {
            line = lines[Random.Range(0, lines.Length)].Trim();
            attempts++;
        } while (string.IsNullOrEmpty(line) && attempts < lines.Length * 2);

        return line;
    }

    private void BuildPhraseDisplay()
    {
        correctGuesses = 0;
        totalLettersToGuess = 0;

        for (int i = wordContainer.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(wordContainer.transform.GetChild(i).gameObject);
        }

        foreach (char letter in currentPhrase)
        {
            if (IsGuessableChar(letter))
                totalLettersToGuess++;
        }

        foreach (char letter in currentPhrase)
        {
            var temp = Instantiate(letterContainer, wordContainer.transform);
            TextMeshProUGUI textComponent = temp.GetComponentInChildren<TextMeshProUGUI>();
            Transform underline = temp.transform.Find("Underline");

            if (IsGuessableChar(letter))
            {
                textComponent.text = " ";
                textComponent.color = Color.white;
                if (underline != null) underline.gameObject.SetActive(true);
            }
            else
            {
                textComponent.text = letter.ToString();
                textComponent.color = Color.white;
                if (underline != null) underline.gameObject.SetActive(false);
            }
        }
    }

    private void CheckLetter(string inputLetter, Button clickedButton)
    {
        if (isGameOver || isTransitioning || string.IsNullOrEmpty(currentPhrase)) return;

        clickedButton.interactable = false; // disable on click, correct or wrong, no matter what

        bool letterInWord = false;

        for (int i = 0; i < currentPhrase.Length; i++)
        {
            if (inputLetter == currentPhrase[i].ToString())
            {
                letterInWord = true;
                correctGuesses++;
                wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i].text = inputLetter;
            }
        }

        if (letterInWord)
        {
            totalCorrectGuesses++;
            if (correctGuesses == totalLettersToGuess)
                HandlePhraseWon();
            return;
        }


        incorrectGuesses++;
        if (incorrectGuesses <= hangmanStages.Length)
            hangmanStages[incorrectGuesses - 1].SetActive(true);

        if (incorrectGuesses >= hangmanStages.Length)
            HandleGameOver();
    }

    private void HandlePhraseWon()
    {
        isTransitioning = true;
        levelsCompleted++;
        for (int i = 0; i < currentPhrase.Length; i++)
        {
            if (IsGuessableChar(currentPhrase[i]))
                wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i].color = Color.green;
        }

        if (LevelProgression.IsFinalLevel())
            Invoke(nameof(HandleGameWon), 2f);
        else
            Invoke(nameof(AdvanceLevel), 2f);
    }

    private void UpdateLevelStages()
    {
        if (levelStages == null) return;

        for (int i = 0; i < levelStages.Length; i++)
        {
            levelStages[i].SetActive(i < LevelProgression.CurrentLevel);
        }
    }
    private void AdvanceLevel()
    {
        LevelProgression.NextLevel();
        LoadLevel();
    }

    private void HandleGameWon()
    {
        isGameOver = true;
        //if (winPanel != null) winPanel.SetActive(true);
        ShowGameOverPanel(winPanelUI, "YOU WIN!");
    }

    private void HandleGameOver()
    {
        isGameOver = true;

        for (int i = 0; i < currentPhrase.Length; i++)
        {
            TextMeshProUGUI textComponent = wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i];
            textComponent.text = currentPhrase[i].ToString();
            if (IsGuessableChar(currentPhrase[i]))
                textComponent.color = Color.red;
        }

        Invoke(nameof(ShowLosePanel), 2f);
    }

    private void ShowLosePanel()
    {
        ShowGameOverPanel(losePanelUI, "YOU LOSE!");
    }

    private void ShowGameOverPanel(GameOverPanelUI panel, string title)
    {
        if (panel == null) return;

        float elapsed = Time.time - gameStartTime;
        float average = levelsCompleted > 0 ? elapsed / levelsCompleted : 0f;

        panel.SetTitle(title);
        panel.SetStats(elapsed, average, levelsCompleted, LevelProgression.TotalLevels, totalCorrectGuesses);
        panel.gameObject.SetActive(true);
    }
}
