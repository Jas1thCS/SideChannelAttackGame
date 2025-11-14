using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public RectTransform target;
        public string tooltipText;
    }
    public Toggle dontShowAgainToggle;          // hook a UI Toggle on your panel
    [SerializeField] private string prefsKey = "tutorial.mainmenu.v1"; // bump suffix when you change content
    [SerializeField] private bool autoShowOnFirstRun = true;

    public List<TutorialStep> steps;
    public RectTransform arrowPointer;
    public TextMeshProUGUI tooltip;
    public TextMeshProUGUI stepCounter; 
    public GameObject tutorialPanel;
    public Button nextButton;
    public Button previousButton; // NEW - Previous button

    [Header("Arrow Animation")]
    public float bounceDistance = 15f;
    public float bounceSpeed = 2f;

    private int currentStep = 0;
    private Coroutine arrowAnimationCoroutine;

    void Start()
    {
        tutorialPanel.SetActive(false);
        arrowPointer.gameObject.SetActive(false);
        if (autoShowOnFirstRun && PlayerPrefs.GetInt(prefsKey, 0) == 0)

            // Setup button listeners
        nextButton.onClick.AddListener(NextStep);
        previousButton.onClick.AddListener(PreviousStep); // NEW

        StartTutorial();
    }

    public void StartTutorial()
    {
        tutorialPanel.SetActive(true);
        arrowPointer.gameObject.SetActive(true);
        currentStep = 0;
        ShowStep();
    }

    public void NextStep()
    {
        // Prevent multiple rapid clicks
        nextButton.interactable = false;

        currentStep++;
        Debug.Log($"Next pressed - moving to step {currentStep}");

        if (currentStep >= steps.Count)
        {
            tutorialPanel.SetActive(false);
            arrowPointer.gameObject.SetActive(false);
            StopArrowAnimation();
            FinishTutorial();
            return;
        }
        ShowStep();

        // Re-enable button after a short delay
        StartCoroutine(ReEnableButton(nextButton));
    }

    // NEW - Previous step functionality  
    public void PreviousStep()
    {
        if (currentStep <= 0) return; // Can't go back from first step

        // Prevent multiple rapid clicks
        previousButton.interactable = false;

        currentStep--;
        Debug.Log($"Previous pressed - moving to step {currentStep}");
        ShowStep();

        // Re-enable button after a short delay
        StartCoroutine(ReEnableButton(previousButton));
    }

    IEnumerator ReEnableButton(Button button)
    {
        yield return new WaitForSeconds(0.2f);
        button.interactable = true;
    }

    public void ResetTutorialFlag()
    {
        PlayerPrefs.DeleteKey(prefsKey);
        PlayerPrefs.Save();
    }
    public void ForceShow()
    {
        tutorialPanel.SetActive(true);
        arrowPointer.gameObject.SetActive(true);
        currentStep = 0;
        ShowStep();
    }

    void ShowStep()
    {
        var step = steps[currentStep];
        tooltip.text = step.tooltipText;

        // Update step counter
        if (stepCounter != null)
        {
            stepCounter.text = $"Step {currentStep + 1} of {steps.Count}";
        }

        // Update button states
        previousButton.gameObject.SetActive(currentStep > 0); // Hide on first step

        // Update next button text
        nextButton.GetComponentInChildren<TextMeshProUGUI>().text =
            (currentStep == steps.Count - 1) ? "Finish" : "Next";

        // Position arrow with smart positioning
        PositionArrowSmart(step.target);

        // Start bouncing animation
        StartArrowAnimation();
    }

    void PositionArrowSmart(RectTransform target)
    {
        if (!target)
        {
            Debug.LogWarning($"Tutorial step {currentStep} has null target!");
            return;
        }

        arrowPointer.gameObject.SetActive(true);

        Vector3 targetPos = target.position;

        // Default to simple positioning first
        Vector2 offset = new Vector2(0, target.rect.height / 2f + 80f); // Above target

        // Only do smart positioning if we have a main camera
        if (Camera.main != null)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, targetPos);
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            float distance = 80f;

            // Simpler logic - just check upper/lower half
            if (screenPos.y > screenCenter.y) // Upper half
            {
                offset = new Vector2(0, -target.rect.height / 2 - distance); // Below target
            }
            else // Lower half  
            {
                offset = new Vector2(0, target.rect.height / 2 + distance); // Above target
            }
        }

        Vector3 arrowPosition = targetPos + (Vector3)offset;
        arrowPointer.position = arrowPosition;

        // Point arrow toward target
        Vector3 dir = (targetPos - arrowPosition).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrowPointer.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log($"Tutorial step {currentStep}: targeting {target.name}");
    }

    void StartArrowAnimation()
    {
        StopArrowAnimation();
        arrowAnimationCoroutine = StartCoroutine(AnimateArrow());
    }

    void StopArrowAnimation()
    {
        if (arrowAnimationCoroutine != null)
        {
            StopCoroutine(arrowAnimationCoroutine);
            arrowAnimationCoroutine = null;
        }
    }

    IEnumerator AnimateArrow()
    {
        Vector3 basePosition = arrowPointer.position;
        Vector3 bounceDirection = arrowPointer.up; // Direction arrow is pointing

        while (arrowPointer.gameObject.activeInHierarchy)
        {
            float bounce = Mathf.Sin(Time.time * bounceSpeed) * bounceDistance;
            arrowPointer.position = basePosition + bounceDirection * bounce;
            yield return null;
        }
    }

    public void CancelTutorial()
    {
        tutorialPanel.SetActive(false);
        arrowPointer.gameObject.SetActive(false);
        StopArrowAnimation();
        FinishTutorial();
    }
    private void FinishTutorial(bool forceRemember = false)
    {
        tutorialPanel.SetActive(false);
        arrowPointer.gameObject.SetActive(false);

        // if user ticked “Don’t show again”, or you want to always remember on completion
        if (forceRemember || (dontShowAgainToggle && dontShowAgainToggle.isOn))
        {
            PlayerPrefs.SetInt(prefsKey, 1);
            PlayerPrefs.Save();
        }
    }
    public void SkipAndNeverShow()
    {
        if (dontShowAgainToggle) dontShowAgainToggle.isOn = true;
        CancelTutorial(); // this will call FinishTutorial() and persist the flag
    }


}
