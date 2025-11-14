using UnityEngine;
using UnityEngine.UI;

public class PipelineHeatMap : MonoBehaviour
{
    private Image image;
    private float heatLevel = 0f;  // 0 = cold, 1 = hot
    public float heatDecaySpeed = 0f;

    void Start()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("PipelineHeatMap requires an Image component.");
        }
    }

    void Update()
    {
        if (heatLevel > 0f)
        {
            //heatLevel -= Time.deltaTime * heatDecaySpeed;
            //heatLevel = Mathf.Clamp01(heatLevel);
            UpdateColor();
        }
    }

    public void ApplyHeat(float intensity)
    {

        heatLevel += intensity;
        heatLevel = Mathf.Clamp01(heatLevel);
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
            if (image == null)
            {
                Debug.LogWarning("No Image component found on " + gameObject.name);
                return;
            }
        }

        Color coolColor = Color.white; // Blue
        Color hotColor = new Color(1f, 0.3f, 0f);    // Red-Orange
        image.color = Color.Lerp(coolColor, hotColor, heatLevel);
    }

    public void ResetHeat()
    {
        heatLevel = 0f;
        UpdateColor();
    }
}
