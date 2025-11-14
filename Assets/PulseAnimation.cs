using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PulseAnimation : MonoBehaviour
{
    private Vector3 originalScale;
    private float pulseSpeed = 2f;
    private float pulseAmount = 0.05f;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * scale;
    }
}
