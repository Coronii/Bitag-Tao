using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BankTier { Easy, Medium, Hard }

[System.Serializable]
public class Theme
{
    public string themeName;
    public TextAsset easyBank;   // levels 1-3, single words
    public TextAsset mediumBank; // levels 4-6, phrases
    public TextAsset hardBank;   // levels 7-10, phrases

    public TextAsset GetBank(BankTier tier)
    {
        switch (tier)
        {
            case BankTier.Easy: return easyBank;
            case BankTier.Medium: return mediumBank;
            default: return hardBank;
        }
    }
}
public class ThemeManager : MonoBehaviour
{
    [SerializeField] private Theme[] themes;
    private static ThemeManager instance;
    public static int SelectedTheme { get; private set; } = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SelectedTheme = 0; // fresh scene load = fresh game
    }

    public static void RandomizeTheme()
    {
        if (instance == null || instance.themes.Length == 0) return;
        SelectedTheme = Random.Range(0, instance.themes.Length);
    }

    public static Theme GetCurrentTheme()
    {
        if (instance == null || instance.themes.Length == 0) return null;
        int index = Mathf.Clamp(SelectedTheme, 0, instance.themes.Length - 1);
        return instance.themes[index];
    }

    public static TextAsset GetBankForCurrentLevel()
    {
        Theme theme = GetCurrentTheme();
        if (theme == null) return null;
        return theme.GetBank(LevelProgression.GetTierForLevel(LevelProgression.CurrentLevel));
    }
}
