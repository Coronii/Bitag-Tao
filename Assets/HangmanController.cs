using System.Collections;
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
    [SerializeField] TextAsset possibleWord;

    private string word;
    private int incorrectGuesses, correctGuesses;
    private int totalLettersToGuess;

    // Dictionary to map uppercase letter strings to their UI Button components
    private Dictionary<string, Button> runtimeButtons = new Dictionary<string, Button>();

    void Start()
    {
        InitializeButtons();
        InitialiseGame();
    }

    // Listens for physical keyboard entries every frame
    void Update()
    {
        // Check if any key was pressed down and a character exists
        if (Input.anyKeyDown && !string.IsNullOrEmpty(Input.inputString))
        {
            // Convert physical key press to uppercase to match the virtual keyboard buttons
            string physicalKeyInput = Input.inputString.ToUpper();

            // If the typed letter exists in our dictionary and the button is still interactable (not guessed yet)
            if (runtimeButtons.ContainsKey(physicalKeyInput))
            {
                Button targetButton = runtimeButtons[physicalKeyInput];

                if (targetButton.interactable)
                {
                    // Mechanically trigger the button click logic
                    targetButton.onClick.Invoke();
                }
            }
        }
    }

    private bool IsGuessableChar(char c)
    {
        return char.IsLetter(c);
    }

    private void InitializeButtons()
    {
        runtimeButtons.Clear(); // Clear dictionary safety check before rebuilding

        for (int i = 65; i <= 90; i++)
        {
            CreateButtons(i);
        }
    }

    private void InitialiseGame()
    {
        incorrectGuesses = 0;
        correctGuesses = 0;
        totalLettersToGuess = 0;

        foreach (Button child in keyboardContainer.GetComponentsInChildren<Button>())
        {
            child.interactable = true;
        }

        foreach (Transform child in wordContainer.GetComponentInChildren<Transform>())
        {
            Destroy(child.gameObject);
        }

        foreach (GameObject stage in hangmanStages)
        {
            stage.SetActive(false);
        }

        word = generateWord().ToUpper();

        // Count total guessable letters
        foreach (char letter in word)
        {
            if (IsGuessableChar(letter))
                totalLettersToGuess++;
        }

        // Instantiate slots for ALL characters
        foreach (char letter in word)
        {
            var temp = Instantiate(letterContainer, wordContainer.transform);
            TextMeshProUGUI textComponent = temp.GetComponentInChildren<TextMeshProUGUI>();
            Transform underline = temp.transform.Find("Underline");

            if (IsGuessableChar(letter))
            {
                // Guessable letter - show as underscore
                textComponent.text = " ";
                if (underline != null)
                    underline.gameObject.SetActive(true);
            }
            else
            {
                // Auto-revealed - show immediately
                textComponent.text = letter.ToString();
                textComponent.color = Color.white;
                if (underline != null)
                    underline.gameObject.SetActive(false);
            }
        }
    }

    private void CreateButtons(int i)
    {
        string letterStr = ((char)i).ToString(); 

        GameObject temp = Instantiate(letterButton, keyboardContainer.transform);
        temp.name = letterStr; // Rename GameObject for clarity in Hierarchy
        temp.GetComponentInChildren<TextMeshProUGUI>().text = letterStr;

        Button btnComponent = temp.GetComponent<Button>();

        // When clicked (via mouse OR physical key), lock the button out and check letter rules
        btnComponent.onClick.AddListener(delegate {
            btnComponent.interactable = false; // Grays out the button automatically
            CheckLetter(letterStr);
        });

        // Add this runtime button into our quick-lookup mapping tracking
        runtimeButtons.Add(letterStr, btnComponent);
    }

    private string generateWord()
    {
        string[] wordList = possibleWord.text.Split('\n');
        string line;
        int attempts = 0;

        do
        {
            line = wordList[Random.Range(0, wordList.Length)].Trim();
            attempts++;
        } while (string.IsNullOrEmpty(line) && attempts < wordList.Length * 2);

        return line;
    }

    private void CheckLetter(string inputLetter)
    {
        bool letterInWord = false;
        for (int i = 0; i < word.Length; i++)
        {
            if (inputLetter == word[i].ToString())
            {
                letterInWord = true;
                correctGuesses++;
                wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i].text = inputLetter;
            }
        }

        if (!letterInWord)
        {
            incorrectGuesses++;
            if (incorrectGuesses <= hangmanStages.Length)
                hangmanStages[incorrectGuesses - 1].SetActive(true);
        }

        CheckOutcome();
    }

    private void CheckOutcome()
    {
        if (correctGuesses == totalLettersToGuess)
        {
            // Word won
            for (int i = 0; i < word.Length; i++)
            {
                if (IsGuessableChar(word[i]))
                    wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i].color = Color.green;
            }
            Invoke("InitialiseGame", 3f);
        }

        if (incorrectGuesses == hangmanStages.Length)
        {
            // Word lost - reveal all letters
            for (int i = 0; i < word.Length; i++)
            {
                TextMeshProUGUI textComponent = wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i];
                textComponent.text = word[i].ToString();

                if (IsGuessableChar(word[i]))
                    textComponent.color = Color.red;
            }
            Invoke("InitialiseGame", 3f);
        }
    }
}
