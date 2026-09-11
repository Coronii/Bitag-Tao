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

    // A dictionary to track created UI buttons by their letter string
    private Dictionary<string, Button> runtimeButtons = new Dictionary<string, Button>();

    // Start is called before the first frame update
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
        string[] wordList = possibleWord.text.Split("\n");
        string line = wordList[Random.Range(0, wordList.Length - 1)];
        return line.Substring(0, line.Length - 1);
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
        if (letterInWord == false)
        {
            incorrectGuesses++;
            hangmanStages[incorrectGuesses - 1].SetActive(true);
        }
        CheckOutcome();
    }

    private void CheckOutcome()
    {
        if (correctGuesses == word.Length)
        {
            for (int i = 0; i < word.Length; i++)
            {
                wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i].color = Color.green;
            }
            Invoke("InitialiseGame", 3f);
        }

        if (incorrectGuesses == hangmanStages.Length)
        {
            for (int i = 0; i < word.Length; i++)
            {
                wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i].color = Color.red;
                wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i].text = word[i].ToString();
            }
            Invoke("InitialiseGame", 3f);
        }
    }
}

