using TMPro;
using UnityEngine;

public class Result : MonoBehaviour 
{
    public TextMeshProUGUI finalScoreText;
    void Start()
    {
        finalScoreText.text = $"Your Score: {GameState.CorrectAnswers}";
    }
}
