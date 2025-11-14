using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class DisclaimerScript : MonoBehaviour
{
    public Button continueButton;
    public string nextScene = "MainMenu";
    void Start()
    {
        continueButton.onClick.AddListener(()=>
        {
            SceneManager.LoadScene(nextScene);
        });
    }

}
