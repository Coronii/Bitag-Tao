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

    // A dictionary to track created UI buttons by their letter string
    private Dictionary<string, Button> runtimeButtons = new Dictionary<string, Button>();
    void Start()
    {
        InitializeButtons();
        InitialiseGame();
    }

   
    void Update()
    {
        // Check if any key was pressed and if an input string is available
        if (Input.anyKeyDown && !string.IsNullOrEmpty(Input.inputString))
        {
            // Convert physical key press to uppercase (e.g., 'a' becomes "A")
            string physicalKeyInput = Input.inputString.ToUpper();

           
            if (runtimeButtons.ContainsKey(physicalKeyInput))
            {
                Button targetButton = runtimeButtons[physicalKeyInput];

                if (targetButton.interactable)
                {
                   
                    targetButton.onClick.Invoke();
                }
            }
        }
    }

    private void InitializeButtons()
    {
        runtimeButtons.Clear(); // Reset the tracking dictionary

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
        temp.name = letterStr; // Rename the GameObject
        temp.GetComponentInChildren<TextMeshProUGUI>().text = letterStr;

        Button btnComponent = temp.GetComponent<Button>();

       
        btnComponent.onClick.AddListener(delegate {
            btnComponent.interactable = false; 
            CheckLetter(letterStr);
        });

      
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
        if (correctGuesses == word.Length)
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

