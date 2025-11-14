using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InstructionRunner : MonoBehaviour
{
    public RectTransform[] pipelineStages;         // IF, ID, EX, MEM, WB
    public float activeHeatTime = 7f;              
    public float idleCoolTime = 3f;                 
    private string[] instructions = { "ADD", "SUB", "LW", "SW", "BEQ", "BNE", "J" };

    public GameObject mcqPanel;
    public TMPro.TextMeshProUGUI option1Text;
    public TMPro.TextMeshProUGUI option2Text;
    public TMPro.TextMeshProUGUI option3Text;
    public TMPro.TextMeshProUGUI option4Text;

    public Button option1Button;
    public Button option2Button;
    public Button option3Button;
    public Button option4Button;

    public TMPro.TextMeshProUGUI feedbackText;
    private string correctCategory;

    
    public TMPro.TextMeshProUGUI scoreText;
    //Group the instructions into their category so that they can be used for MCQ...
    private Dictionary<string, string> instructionCategory = new Dictionary<string, string>()
    {
        {"ADD", "Arithmetic"},
        {"SUB", "Arithmetic"},
        {"AND", "Arithmetic"},
        {"OR", "Arithmetic"},
        {"LW", "Memory"},
        {"SW", "Memory"},
        {"BEQ", "Branch"},
        {"BNE", "Branch"},
        {"J", "Jump"}
    };


    private void Start()
    {
        StartCoroutine(InstructionLoop());
    }

    IEnumerator InstructionLoop()
    {
        while (true)
        {
            string instr = instructions[Random.Range(0, instructions.Length)];
            Debug.Log("Auto-running instruction: " + instr);
            RunInstruction(instr);

            ShowMCQ(instr);

            yield return new WaitForSeconds(activeHeatTime);

            ResetAllStages();
            Debug.Log("Pipeline stages reset (cool down)");

            yield return new WaitForSeconds(idleCoolTime);
        }
    }

    void RunInstruction(string instr)
    {
        instr = instr.ToUpper().Trim();

        switch (instr)
        {
            case "ADD":
            case "SUB":
            case "AND":
            case "OR":
                ApplyHeatToStage(2, Random.Range(0.6f, 1.0f)); // EX
                ApplyHeatToStage(4, Random.Range(0.3f, 0.6f)); // WB
                break;

            case "LW":
                ApplyHeatToStage(0, Random.Range(0.2f, 0.4f)); // IF
                ApplyHeatToStage(1, Random.Range(0.2f, 0.4f)); // ID
                ApplyHeatToStage(2, Random.Range(0.4f, 0.7f)); // EX
                ApplyHeatToStage(3, Random.Range(0.8f, 1.0f)); // MEM
                ApplyHeatToStage(4, Random.Range(0.4f, 0.6f)); // WB
                break;

            case "SW":
                ApplyHeatToStage(0, Random.Range(0.2f, 0.4f)); //IF
                ApplyHeatToStage(1, Random.Range(0.2f, 0.4f)); //ID
                ApplyHeatToStage(2, Random.Range(0.4f, 0.7f)); //EX
                ApplyHeatToStage(3, Random.Range(0.8f, 1.0f)); //WB
                break;

            case "BEQ":
            case "BNE":
                ApplyHeatToStage(0, Random.Range(0.2f, 0.4f)); //IF
                ApplyHeatToStage(1, Random.Range(0.2f, 0.4f)); //ID
                ApplyHeatToStage(2, Random.Range(0.5f, 0.9f)); //EX
                break;

            case "J":
                ApplyHeatToStage(0, Random.Range(0.2f, 0.4f)); //IF
                ApplyHeatToStage(1, Random.Range(0.2f, 0.4f)); //ID
                break;

            default:
                Debug.LogWarning("Unknown instruction: " + instr);
                break;
        }
    }

    void ApplyHeatToStage(int index, float intensity)
    {
        if (index < 0 || index >= pipelineStages.Length) return;

        var heat = pipelineStages[index].GetComponent<PipelineHeatMap>();
        if (heat != null)
        {
            heat.ApplyHeat(intensity);
        }
        else
        {
            Debug.LogWarning($"No PipelineHeatMap script found on stage {index}");
        }
    }

    void ResetAllStages()
    {
        foreach (var stage in pipelineStages)
        {
            var heat = stage.GetComponent<PipelineHeatMap>();
            if (heat != null)
            {
                heat.ResetHeat();
            }
        }
    }

    
    void ShowMCQ(string instruction)
    {
        mcqPanel.SetActive(true);
        feedbackText.text = "";
        correctCategory = instructionCategory[instruction];

        List<string> allCategories = new List<string> { "Arithmetic","Branch","Jump","Memory"};

        List<string> wrongOptions = new List<string>(allCategories);
        wrongOptions.Remove(correctCategory);

        List<string> options = new List<string>();
        options.Add(correctCategory);
        while(options.Count<4)
        {
            string wrong = wrongOptions[Random.Range(0,wrongOptions.Count)];
            if(!options.Contains(wrong))
                options.Add(wrong);
        }
        for (int i = 0; i < options.Count; i++)
        {
            string temp = options[i];
            int randIndex = Random.Range(i, options.Count);
            options[i] = options[randIndex];
            options[randIndex] = temp;
        }
        option1Text.text = options[0];
        option2Text.text = options[1];
        option3Text.text = options[2];
        option4Text.text = options[3];
    }

    public void CheckAnswer(string selectedCategory)
    {
        option1Button.interactable = false;
        option2Button.interactable = false;
        option3Button.interactable = false; 
        option4Button.interactable = false;

        GameState.QuestionsAnswered++;

        if (selectedCategory == correctCategory)
        {
            GameState.CorrectAnswers+=10;
            feedbackText.text = "Correct!";
        }
        else
        {
            feedbackText.text = "Wrong! The correct answer is: " + correctCategory;
        }

        UpdateScoreUI();

        if (GameState.QuestionsAnswered == 5)
        {
            if (GameState.CorrectAnswers < 30)
            {
                Debug.Log("Resetting score here since person didn't score more than 2");
                feedbackText.text += "\n Please try again!! Score atleast 3 out of 5 answers to proceed!";
                GameState.CorrectAnswers = 0;
                GameState.QuestionsAnswered = 0;
                UpdateScoreUI();
            }
            else if(GameState.CorrectAnswers >= 50 || GameState.CorrectAnswers>=30) 
            {
                feedbackText.text += "\nWell done! Let's move to part2 !";
                StartCoroutine(loadNextScene(2f));
            }
        }
        // Hide MCQ after a few seconds
        StartCoroutine(HideMCQAfterDelay());
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"Score:{GameState.CorrectAnswers}";
    }


    IEnumerator HideMCQAfterDelay()
    {
        yield return new WaitForSeconds(4f);
        mcqPanel.SetActive(false);

        option1Button.interactable = true;
        option2Button.interactable = true;
        option3Button.interactable = true;
        option4Button.interactable = true;

    }

    IEnumerator loadNextScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(1);
    }

    public void SelectOption1()
    {
        CheckAnswer(option1Text.text);
    }

    public void SelectOption2()
    {
        CheckAnswer(option2Text.text);
    }
    public void SelectOption3()
    {
        CheckAnswer(option3Text.text);
    }
    public void SelectOption4()
    {
        CheckAnswer(option4Text.text);
    }



}
