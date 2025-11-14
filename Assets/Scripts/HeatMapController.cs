using UnityEngine;
using UnityEngine.UI;

public class HeatmapController : MonoBehaviour
{
    public GameObject cellPrefab;
    public int rows = 16;
    public int columns = 16;

    private float[,] heatValues;
    private GameObject[] cellObjects;

    void Awake()
    {
        heatValues = new float[rows, columns];
        cellObjects = new GameObject[rows * columns];
        GenerateHeatmap();
    }

    void GenerateHeatmap()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int index = y * columns + x;
                GameObject cell = Instantiate(cellPrefab, transform);
                cellObjects[index] = cell;

                // Set initial color to blue (cold)
                Image bg = cell.GetComponent<Image>();
                if (bg != null)
                    bg.color = Color.blue;
            }
        }
    }

    public void ResetHeatmap()
    {
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
                heatValues[y, x] = 0f;

        UpdateHeatmap();
    }

    public void AddHeatToCell(int cellIndex, float heatAmount)
    {
        int y = cellIndex / columns;
        int x = cellIndex % columns;

        if (y >= 0 && y < rows && x >= 0 && x < columns)
        {
            heatValues[y, x] = Mathf.Clamp01(heatValues[y, x] + heatAmount);
            UpdateHeatmap();
        }
    }

    void UpdateHeatmap()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int index = y * columns + x;
                float heat = Mathf.Clamp01(heatValues[y, x]);

                GameObject cell = cellObjects[index];
                Image bg = cell.GetComponent<Image>();
                Image bar = cell.transform.Find("HeatBar")?.GetComponent<Image>();

                if (bg != null)
                {
                    Color color;

                    if (heat <= 0.1f)
                    {
                        // Blue → White for cold
                        color = Color.Lerp(Color.blue, Color.white, heat / 0.2f);
                    }
                    else if (heat <= 0.5f)
                    {
                        // White → Yellow for warm-up
                        color = Color.Lerp(Color.white, Color.yellow, (heat - 0.2f) / 0.3f);
                    }
                    else if (heat <= 0.9f)
                    {
                        // Yellow → Red for hot
                        color = Color.Lerp(Color.yellow, Color.red, (heat - 0.5f) / 0.4f);
                    }
                    else
                    {
                        // Pulse effect for very high heat
                        float pulse = Mathf.Sin(Time.time * 6f) * 0.15f + 0.85f;
                        color = new Color(1f, pulse, 0f); // pulsing orange
                    }

                    bg.color = color;
                }


                // Bar fill visualization
                if (bar != null)
                {
                    RectTransform cellRT = cell.GetComponent<RectTransform>();
                    RectTransform barRT = bar.GetComponent<RectTransform>();
                    float maxHeight = cellRT.rect.height;
                    float height = Mathf.Max(4f, maxHeight * heat); // Always visible
                    barRT.sizeDelta = new Vector2(barRT.sizeDelta.x, height);
                }
            }
        }
    }

    public int MapMemoryToHeatCell(int blockIndex, int totalBlocks)
    {
        int totalCells = rows * columns;
        float percent = (float)blockIndex / totalBlocks;
        int cellIndex = Mathf.FloorToInt(percent * totalCells);
        return Mathf.Clamp(cellIndex, 0, totalCells - 1);
    }
}
