using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArrowPulse : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private float pulseDuration = 0.18f;
    [SerializeField] private float returnDuration = 0.20f;
    [SerializeField] private float scaleUp = 1.15f;

    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color activeColor = new Color(1f, 0.6f, 0.1f); // warm glow

    Coroutine routine;
    Vector3 baseScale;

    void Awake()
    {
        if (!img) img = GetComponent<Image>();
        baseScale = transform.localScale;
        SetIdle();
    }

    public void Pulse(float intensity = 1f)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PulseRoutine(Mathf.Clamp01(intensity)));
    }

    IEnumerator PulseRoutine(float intensity)
    {
        // up
        float t = 0f;
        while (t < pulseDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / pulseDuration);

            if (img) img.color = Color.Lerp(idleColor, activeColor, p * intensity);
            transform.localScale = Vector3.Lerp(baseScale, baseScale * Mathf.Lerp(1f, scaleUp, intensity), EaseOut(p));
            yield return null;
        }

        // back
        t = 0f;
        while (t < returnDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / returnDuration);

            if (img) img.color = Color.Lerp(activeColor, idleColor, p);
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, p);
            yield return null;
        }

        SetIdle();
    }

    void SetIdle()
    {
        if (img) img.color = idleColor;
        transform.localScale = baseScale;
    }

    float EaseOut(float x) => 1f - Mathf.Pow(1f - x, 3f);
}
