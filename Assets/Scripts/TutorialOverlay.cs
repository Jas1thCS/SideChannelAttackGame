using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialOverlay : MonoBehaviour
{
    // Generic step definition (reusable for any scene)
    [Serializable]
    public class Step
    {
        public string title;
        [TextArea(2, 6)] public string text;
        public RectTransform target;          // What to highlight (can be null)
        public Vector2 tooltipOffset = new Vector2(120, 80);
        public bool allowClickThrough = false;
        public float extraPadding = 10f;
        public GateType gateType = GateType.InfoOnly;
        public float delay = -1f;                    // overrides defaultDelay when >= 0
        public bool followTarget = false;            // keep highlight/tooltip glued to target each frame
        public bool advanceOnTargetClick = false;    // clicking the target continues

        public Sprite image;                         // optional visual in tooltip
        public string signalKey;                     // used when gateType == WaitForSignal
    }

    public static class TutorialSignals
    {
        public static event Action<string> OnSignal;
        public static void Fire(string key) => OnSignal?.Invoke(key);
    }

    public enum GateType
    {
        InfoOnly,          // Just show info; advance on Next
        WaitForSignal,     // Wait until Continue() is called
        WaitForDelay       // Wait for a number of seconds
    }

    [Header("UI References")]
    [SerializeField] private Image dim;
    [SerializeField] private RectTransform holeMask;
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Image tooltipImage; // optional image slot inside tooltip


    [Header("Settings")]
    public List<Step> steps = new List<Step>();
    public float defaultDelay = 1f;

    private int currentStep = 0;
    private bool waitingForGate = false;

    void Awake()
    {
        //if (nextButton) nextButton.onClick.AddListener(NextStep);
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.RightArrow)) NextStep();
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape)) EndTutorial();

        //if (skipButton) skipButton.onClick.AddListener(EndTutorial);
    }

    public void StartTutorial()
    {
        gameObject.SetActive(true);
        currentStep = 0;
        ShowStep();
    }

    public void NextStep()
    {
        if (waitingForGate) return;
        currentStep++;
        ShowStep();
    }

    public void EndTutorial()
    {
        StopAllCoroutines();

        gameObject.SetActive(false);
    }

    // Called by external scripts when a gate step is satisfied
    public void Continue()
    {
        if (!waitingForGate) return;
        waitingForGate = false;
        currentStep++;
        ShowStep();
    }

    private void ShowStep()
    {
        if (currentStep >= steps.Count)
        {
            EndTutorial();
            return;
        }

        var step = steps[currentStep];
        waitingForGate = false;

        // Dim visibility & raycast
        if (dim) dim.raycastTarget = !step.allowClickThrough;
        tooltipText.text = step.text;

        MoveHighlight(step);
        MoveTooltip(step);

        switch (step.gateType)
        {
            case GateType.InfoOnly:
                waitingForGate = false;
                nextButton.gameObject.SetActive(true);
                break;

            case GateType.WaitForSignal:
                waitingForGate = true;
                nextButton.gameObject.SetActive(false);
                break;

            case GateType.WaitForDelay:
                waitingForGate = true;
                nextButton.gameObject.SetActive(false);
                StartCoroutine(AutoAdvance(step));
                break;
        }
    }

    private IEnumerator AutoAdvance(Step s)
    {
        yield return new WaitForSeconds(defaultDelay);
        waitingForGate = false;
        currentStep++;
        ShowStep();
    }

    private void MoveHighlight(Step s)
    {
        if (!holeMask) return;
        if (s.target == null)
        {
            holeMask.gameObject.SetActive(false);
            return;
        }

        holeMask.gameObject.SetActive(true);
        var cam = GetCanvasCamera(holeMask);
        Vector3[] corners = new Vector3[4];
        s.target.GetWorldCorners(corners);

        var overlayRT = holeMask.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            overlayRT, RectTransformUtility.WorldToScreenPoint(cam, s.target.position), cam, out var center);

        Vector2 size = s.target.rect.size + Vector2.one * s.extraPadding;
        holeMask.anchoredPosition = center;
        holeMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        holeMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    private void MoveTooltip(Step s)
    {
        var overlayRT = tooltipPanel.parent as RectTransform;
        var cam = GetCanvasCamera(tooltipPanel);
        Vector2 pos = s.target
            ? RectTransformUtility.WorldToScreenPoint(cam, s.target.position)
            : new Vector2(Screen.width / 2, Screen.height / 2);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRT, pos, cam, out var local);
        tooltipPanel.anchoredPosition = local + s.tooltipOffset;
    }

    private Camera GetCanvasCamera(Transform t)
    {
        var canvas = t.GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            return canvas.worldCamera;
        return null;
    }
}
