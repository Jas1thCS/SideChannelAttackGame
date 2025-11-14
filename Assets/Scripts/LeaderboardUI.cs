using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    public Transform content;      // ScrollView/Viewport/Content
    public GameObject rowPrefab;   // The LBRow prefab
    [SerializeField] string gameKey = GameKeys.GRID; // default which board to show

    void OnEnable() => Refresh();

    public void SetGame(string key)
    {
        gameKey = key;
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform c in content) Destroy(c.gameObject);
        List<ScoreEntry> list = LocalLeaderboard.Load(gameKey);
        for (int i = 0; i < list.Count; i++)
        {
            var go = GameObject.Instantiate(rowPrefab, content);
            var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            // Expect order: Rank, Name, Score
            texts[0].text = (i + 1).ToString();
            texts[1].text = list[i].name;
            texts[2].text = list[i].score.ToString();
            
        }
    }

    // Optional button hook
    public void ClearCurrent()
    {
        LocalLeaderboard.Clear(gameKey);
        Refresh();
    }
}
