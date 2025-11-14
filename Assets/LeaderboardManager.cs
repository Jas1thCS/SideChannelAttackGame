using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerScore
{
    public string playerName;
    public int score;
}

[System.Serializable]
public class LeaderboardData
{
    public List<PlayerScore> scores = new List<PlayerScore>();
}

public class LeaderboardManager : MonoBehaviour
{
    public Transform leaderboardContainer;
    public GameObject leaderboardEntryPrefab;
    public int maxEntries = 10;

    private string filePath;
    private LeaderboardData leaderboard = new LeaderboardData();

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");
        LoadLeaderboard();
        DisplayLeaderboard();
    }

    public void AddScore(string name, int score)
    {
        leaderboard.scores.Add(new PlayerScore { playerName = name, score = score });
        leaderboard.scores = leaderboard.scores
            .OrderByDescending(s => s.score)
            .Take(maxEntries)
            .ToList();
        SaveLeaderboard();
        DisplayLeaderboard();
    }

    void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(leaderboard, true);
        File.WriteAllText(filePath, json);
    }

    void LoadLeaderboard()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            leaderboard = JsonUtility.FromJson<LeaderboardData>(json);
        }
    }

    public void DisplayLeaderboard()
    {
        foreach (Transform child in leaderboardContainer)
            Destroy(child.gameObject);

        foreach (var entry in leaderboard.scores)
        {
            GameObject go = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            go.transform.Find("NameText").GetComponent<Text>().text = entry.playerName;
            go.transform.Find("ScoreText").GetComponent<Text>().text = entry.score.ToString();
        }
    }
}
