using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
public class SnippetQuizManager : MonoBehaviour
{
    public RectTransform[] pipelineStages;
    public List<InstructionSnippet> allSnippets;

    public GameObject mcqPanel;
    public TextMeshProUGUI option1Text;
    public TextMeshProUGUI option2Text;
    public TextMeshProUGUI option3Text;
    public TextMeshProUGUI option4Text;

    public Button option1Button;
    public Button option2Button;
    public Button option3Button;
    public Button option4Button;

    public TextMeshProUGUI feedbackText;
    public TMPro.TextMeshProUGUI scoreText;
    private InstructionSnippet correctSnipet;


    void Start()
    {
        showSnippetQuestion();
    }

    void showSnippetQuestion()
    {
        //Choose a snippet randomly to be the answer
        correctSnipet = allSnippets[Random.Range(0, allSnippets.Count)];

        //Heat Animation
        ApplyHeatPattern(correctSnipet.heatPattern);

        //For the wrong 3 snippet options
        var wrongSnippets = allSnippets
            .Where(s => !IsPatternMatch(s.heatPattern, correctSnipet.heatPattern))
            .OrderBy(_ => Random.value).Take(3).ToList();

        List<InstructionSnippet> options = new List<InstructionSnippet>(wrongSnippets) { correctSnipet };
        options = options.OrderBy(s => Random.value).ToList();

        option1Text.text = options[0].codeSnippet;
        option2Text.text = options[1].codeSnippet;
        option3Text.text = options[2].codeSnippet;
        option4Text.text = options[3].codeSnippet;

        option1Button.onClick.RemoveAllListeners();
        option2Button.onClick.RemoveAllListeners();
        option3Button.onClick.RemoveAllListeners();
        option4Button.onClick.RemoveAllListeners();

        option1Button.onClick.AddListener(() => CheckAnswer(options[0]));
        option2Button.onClick.AddListener(() => CheckAnswer(options[1]));
        option3Button.onClick.AddListener(() => CheckAnswer(options[2]));
        option4Button.onClick.AddListener(() => CheckAnswer(options[3]));


    }

    void ApplyHeatPattern(bool[] heatPattern)
    {
        for(int i = 0; i < heatPattern.Length; i++)
        {
            if(heatPattern[i])
            {
                var heat = pipelineStages[i].GetComponent<PipelineHeatMap>();
                if (heat != null)
                    heat.ApplyHeat(Random.Range(0.5f, 1.0f));
            }
        }

    }


    bool IsPatternMatch(bool[] a, bool[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if(a[i] != b[i]) return false;
        }
        return true;
    }

    void CheckAnswer(InstructionSnippet selectedAnswer)
    {
        GameState.QuestionsAnswered++;
        bool isCorrect = selectedAnswer == correctSnipet;

            if (isCorrect) {
            feedbackText.text = "Correct!";
            GameState.CorrectAnswers += 10;
        }
        else {
            feedbackText.text = "Wrong! The right answer is " + correctSnipet.codeSnippet;
        }
        UpdateScoreUI();

        option1Button.interactable = false;
        option2Button.interactable = false;
        option3Button.interactable = false;
        option4Button.interactable = false;

        if (GameState.QuestionsAnswered == 10)
        {
            if (GameState.CorrectAnswers >= 60)
            {
                feedbackText.text += "\nScore" + GameState.CorrectAnswers;
                UpdateScoreUI();
            }
        } else
        { Invoke("ResetAndGoNext", 2f); }
            
    }
    void UpdateScoreUI()
    {
        scoreText.text = $"Score:{GameState.CorrectAnswers}";
    }


    void ResetAndGoNext()
    {
        foreach (var stage in pipelineStages)
        {
            var heat = stage.GetComponent<PipelineHeatMap>();
            if (heat != null) heat.ResetHeat();
        }
        option1Button.interactable = true;
        option2Button.interactable = true;
        option3Button.interactable = true;
        option4Button.interactable = true;

        feedbackText.text = "";
        showSnippetQuestion();

    }

}
