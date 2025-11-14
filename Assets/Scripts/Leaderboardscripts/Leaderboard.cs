using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Dan.Main;
public class Leaderboard : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> names;
    [SerializeField] private List<TextMeshProUGUI> scores;
    private string publicLeaderBoardKey = "8535b1d8b39f42492f78efdd13857fbd3217d1091cf49147be3233ce659a5a7c";

    private void Start()
    {
        GetLeaderboard();
    }

    public void GetLeaderboard()
    {
        LeaderboardCreator.GetLeaderboard(publicLeaderBoardKey, ((msg) => {
            int loopLength = (msg.Length < names.Count) ? msg.Length : names.Count;
            for(int i =0; i < loopLength; i++)
            {
                names[i].text = msg[i].Username;
                scores[i].text = msg[i].Score.ToString();
            }
        }));
    }

    public void setLeaderboardEntry(string username, int score)
    {
        LeaderboardCreator.UploadNewEntry(publicLeaderBoardKey, username, score,((msg) => {
            GetLeaderboard();
        }));
    }


}
