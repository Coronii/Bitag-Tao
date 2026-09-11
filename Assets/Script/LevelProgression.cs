using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgression : MonoBehaviour
{
    public static int CurrentLevel { get; private set; } = 1;
    private static LevelProgression instance;
    public const int TotalLevels = 10;


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

        CurrentLevel = 1; // fresh scene load = fresh game
    }

    public static BankTier GetTierForLevel(int level)
    {
        if (level <= 3) return BankTier.Easy;
        if (level <= 6) return BankTier.Medium;
        return BankTier.Hard;
    }

    public static void NextLevel()
    {
        if (CurrentLevel < 10)
            CurrentLevel++;
    }

    public static bool IsFinalLevel()
    {
        return CurrentLevel >= 10;
    }
}
