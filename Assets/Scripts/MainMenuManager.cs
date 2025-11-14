using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private const string PIPELINE_SCENE = "BasicPipeline";
    private const string HEATGRID_SCENE = "EasyHeatmapGraph";
    private const string CREDIT_SCENE = "Credits";
    private const string MAINMENU_SCENE = "MainMenu";

    public void LoadPipelineGame()
    {
        SceneManager.LoadScene(PIPELINE_SCENE);
    }

    public void LoadHeatmapGridGame()
    {
        SceneManager.LoadScene(HEATGRID_SCENE);
    }
    public void LoadCreditsScene()
    {
        SceneManager.LoadScene(CREDIT_SCENE);
    }
    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(MAINMENU_SCENE);
    }

}

