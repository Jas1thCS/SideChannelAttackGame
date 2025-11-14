using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameKeys
{
    public const string CPU = "cpu";   // CPU Phase game
    public const string GRID = "grid";  // Heatmap Grid game
}

[Serializable]
public class ScoreEntry
{
    public string name;
    public int score;
    public string game;  // "cpu" or "grid"
}

[Serializable]
class EntryList { public List<ScoreEntry> items = new(); }

public static class LocalLeaderboard
{
    const int MaxEntries = 10;
    static string Key(string game) => $"LB_{game}";

    public static List<ScoreEntry> Load(string game)
    {
        var json = PlayerPrefs.GetString(Key(game), "");
        if (string.IsNullOrEmpty(json)) return new List<ScoreEntry>();
        try { return JsonUtility.FromJson<EntryList>(json).items; }
        catch { return new List<ScoreEntry>(); }
    }

    public static void Save(string game, List<ScoreEntry> list)
    {
        list.Sort((a, b) => b.score.CompareTo(a.score));
        if (list.Count > MaxEntries) list = list.GetRange(0, MaxEntries);
        var wrap = new EntryList { items = list };
        PlayerPrefs.SetString(Key(game), JsonUtility.ToJson(wrap));
        PlayerPrefs.Save();
    }

    public static void Add(string game, string name, int score)
    {
        var list = Load(game);
        list.Add(new ScoreEntry
        {
            name = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim(),
            score = score,
            game = game
        });
        Save(game, list);
    }

    public static void Clear(string game)
    {
        PlayerPrefs.DeleteKey(Key(game));
        PlayerPrefs.Save();
    }
}
