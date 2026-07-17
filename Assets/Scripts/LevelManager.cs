using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private const string LevelKey = "CurrentLevel";
    public const int MaxLevel = 50;

    public int CurrentLevel { get; private set; }

    public LevelConfig CurrentConfig => LevelConfig.ForLevel(CurrentLevel);

    private void Awake()
    {
        CurrentLevel = Mathf.Clamp(PlayerPrefs.GetInt(LevelKey, 1), 1, MaxLevel);
    }


    public LayoutData.Layout GetCurrentLayout()
    {
        return ProceduralLayout.Generate(CurrentConfig);
    }


    public void AdvanceLevel()
    {
        if (CurrentLevel < MaxLevel)
        {
            CurrentLevel++;
            Save();
        }
    }


    public void ResetProgress()
    {
        CurrentLevel = 1;
        Save();
    }

    private void Save()
    {
        PlayerPrefs.SetInt(LevelKey, CurrentLevel);
        PlayerPrefs.Save();
    }
}
