using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HowToOnePager : MonoBehaviour
{
    [Header("Hook up in Inspector")]
    public GameObject root;                // assign HowToOnePager (the root panel)
    public TextMeshProUGUI titleText;      // Title TMP
    public TextMeshProUGUI bodyText;       // Body TMP (inside ScrollView/Content)
    public Button closeBtn;                // Close button
    public CanvasGroup cg;                 // CanvasGroup on root (optional fade)

    [Header("Behavior")]
    public float fadeDuration = 0.15f;
    public bool pauseGameWhileOpen = false;

    void Awake()
    {
        if (closeBtn) closeBtn.onClick.AddListener(Close);
        if (root) root.SetActive(false);
        if (cg) cg.alpha = 0f;
    }

    public void Open(string title, string body)
    {
        if (pauseGameWhileOpen) Time.timeScale = 0f;

        titleText.text = title;
        bodyText.text = body;

        root.SetActive(true);
        if (cg) StartCoroutine(Fade(0f, 1f));
        else cg = null; // optional
    }

    public void OpenBullets(string title, IEnumerable<string> bullets)
    {
        var sb = new StringBuilder();
        foreach (var b in bullets)
        {
            if (string.IsNullOrWhiteSpace(b)) continue;
            sb.AppendLine("• " + b.Trim());
        }
        Open(title, sb.ToString());
    }

    public void Close()
    {
        if (cg) StartCoroutine(Fade(1f, 0f));
        else
        {
            root.SetActive(false);
            if (pauseGameWhileOpen) Time.timeScale = 1f;
        }
    }

    void Update()
    {
        if (!root || !root.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    IEnumerator Fade(float from, float to)
    {
        cg.alpha = from;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        cg.alpha = to;
        if (to == 0f)
        {
            root.SetActive(false);
            if (pauseGameWhileOpen) Time.timeScale = 1f;
        }
    }
}
