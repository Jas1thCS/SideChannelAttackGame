using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitToMenu : MonoBehaviour
{
    [SerializeField] private GameObject QuitPanel; 
    private const string MAIN_MENU_SCENE = "MainMenu";

    // Called when player clicks "Quit to Menu" button
    public void ShowConfirmPanel()
    {
        if (QuitPanel != null)
            QuitPanel.SetActive(true);
    }

    // Called when player confirms "Yes"
    public void ConfirmReturn()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }

    // Called when player cancels "No"
    public void CancelReturn()
    {
        if (QuitPanel != null)
            QuitPanel.SetActive(false);
    }
}
