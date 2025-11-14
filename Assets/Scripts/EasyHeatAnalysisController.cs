using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class EasyHeatAnalysisController : MonoBehaviour
{
    [Header("Grid Setup")]
    public GameObject memoryBlockPrefab;
    public Transform memoryGridParent;

    [Header("Game Stats UI")]
    public TextMeshProUGUI gameStatsText;

    [Header("Output")]
    public TextMeshProUGUI FeedbackText;

    [Header("Message Prompt")]
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    public Button okButton;

    [Header("Hint UI")]
    public GameObject hintPanel;
    public TextMeshProUGUI hintText;

    [Header("Heatmap")]
    public HeatmapController heatmapController;

    private readonly List<GameObject> memoryBlocks = new();
    private int? selectedStartBlock = null;
    private int? selectedEndBlock = null;

    private int attemptsLeft = 5;
    private int actualKeyAddress;
    private int heatScore = 0;
    private float previousHeat = -1f;

    private const int MIN_MEMORY = 0;
    private const int MAX_MEMORY = 4096;
    private const int BLOCK_SIZE = 16;
    private const int BLOCK_COUNT = MAX_MEMORY / BLOCK_SIZE; // 256

    private int selectedBlockCount = 0;
    private const int MAX_BLOCK_SELECTION = 5;

    private int consecutiveColdAttempts = 0;
    private bool regionRevealed = false;
    private Color smallRegionColor = new(0.7f, 0.85f, 1f);
    private Color mediumRegionColor = new(1f, 1f, 0.75f);
    private Color largeRegionColor = new(1f, 0.75f, 0.75f);
    private string revealedKeyRegion = null;

    private HashSet<int> horizontalHintBlocks = new();
    private HashSet<int> verticalHintBlocks = new();

    private void Start()
    {
        GenerateMemoryGrid();
        actualKeyAddress = Random.Range(MIN_MEMORY, MAX_MEMORY + 1);
        Debug.Log("Actual Key Address: " + actualKeyAddress);
        Debug.Log("Actual Key Block: " + (actualKeyAddress / BLOCK_SIZE));
        okButton.onClick.AddListener(HideMessage);
        UpdateGameStatsUI();
    }

    private void ShowMessage(string Message)
    {
        messageText.text = Message;
        messagePanel.SetActive(true);
    }

    private void HideMessage()
    {
        messagePanel.SetActive(false);
    }

    private void UpdateGameStatsUI()
    {
        gameStatsText.text = $"Score: {heatScore}   Attempts Left: {attemptsLeft}";
    }

    private void GenerateMemoryGrid()
    {
        for (int i = 0; i < BLOCK_COUNT; i++)
        {
            GameObject block = Instantiate(memoryBlockPrefab, memoryGridParent);
            block.GetComponentInChildren<TextMeshProUGUI>().text = i.ToString();

            int blockIndex = i;
            block.GetComponent<Button>().onClick.AddListener(() => OnMemoryBlockClicked(blockIndex));
            memoryBlocks.Add(block);
        }
    }

    private void OnMemoryBlockClicked(int index)
    {
        if (selectedStartBlock == null)
        {
            selectedStartBlock = index;
            HighlightBlock(index, Color.yellow);
            selectedBlockCount = 1;
        }
        else if (selectedEndBlock == null)
        {
            selectedEndBlock = index;

            int start = Mathf.Min(selectedStartBlock.Value, selectedEndBlock.Value);
            int end = Mathf.Max(selectedStartBlock.Value, selectedEndBlock.Value);

            selectedBlockCount = (end - start) + 1;

            if (selectedBlockCount > MAX_BLOCK_SELECTION)
            {
                ResetBlockSelection();
                ShowHint($"You can only select up to {MAX_BLOCK_SELECTION} blocks. Try a smaller range.");
                return;
            }

            for (int i = start; i <= end; i++)
                HighlightBlock(i, Color.cyan);

            Debug.Log($"Selected Block Range: {start} - {end}");
        }
        else
        {
            ResetBlockSelection();
            OnMemoryBlockClicked(index);
        }
    }

    private void HighlightBlock(int index, Color color, bool forceOverride = false)
    {
        var blockImage = memoryBlocks[index].GetComponent<Image>();
        if (blockImage == null) return;

        if (forceOverride || !horizontalHintBlocks.Contains(index))
            blockImage.color = color;
    }

    private void ResetBlockSelection()
    {
        for (int i = 0; i < memoryBlocks.Count; i++)
        {
            Image img = memoryBlocks[i].GetComponent<Image>();
            if (horizontalHintBlocks.Contains(i) || verticalHintBlocks.Contains(i))
                continue;

            if (regionRevealed)
            {
                var (smallMax, mediumMax) = RegionBoundaries();
                if (revealedKeyRegion == "Small" && i <= smallMax)
                    img.color = smallRegionColor;
                else if (revealedKeyRegion == "Medium" && i > smallMax && i <= mediumMax)
                    img.color = mediumRegionColor;
                else if (revealedKeyRegion == "Large" && i > mediumMax)
                    img.color = largeRegionColor;
                else
                    img.color = new Color(0.9f, 0.9f, 0.9f);
            }
            else
            {
                img.color = Color.white;
            }
        }

        selectedStartBlock = null;
        selectedEndBlock = null;
        selectedBlockCount = 0;
    }

    private void RegionRevealColors()
    {
        revealedKeyRegion = GetKeyRegion();
        regionRevealed = true;

        var (smallMax, mediumMax) = RegionBoundaries();

        for (int i = 0; i < memoryBlocks.Count; i++)
        {
            Image img = memoryBlocks[i].GetComponent<Image>();

            if (revealedKeyRegion == "Small" && i <= smallMax)
                img.color = smallRegionColor;
            else if (revealedKeyRegion == "Medium" && i > smallMax && i <= mediumMax)
                img.color = mediumRegionColor;
            else if (revealedKeyRegion == "Large" && i > mediumMax)
                img.color = largeRegionColor;
            else
                img.color = new Color(0.9f, 0.9f, 0.9f);
        }
    }

    public void AnalyzeHeat()
    {
        if (attemptsLeft <= 0)
        {
            messageText.text = $"No attempts left! The key was near address {actualKeyAddress}";
            DisableButtons();
            return;
        }

        if (selectedStartBlock == null || selectedEndBlock == null)
        {
            messageText.text = "Please select a range by clicking on the grid blocks.";
            return;
        }

        int startBlock = Mathf.Min(selectedStartBlock.Value, selectedEndBlock.Value);
        int endBlock = Mathf.Max(selectedStartBlock.Value, selectedEndBlock.Value);

        int startAddress = startBlock * BLOCK_SIZE;
        int endAddress = (endBlock + 1) * BLOCK_SIZE - 1;

        float heat = CalculateRawHeat(startBlock, endBlock);

        int midBlock = (startBlock + endBlock) / 2;
        int heatmapCellIndex = heatmapController.MapMemoryToHeatCell(midBlock, BLOCK_COUNT);

        bool containsKeyBlock = (actualKeyAddress >= startAddress && actualKeyAddress <= endAddress);
        if (containsKeyBlock) heat = 1.0f;

        Debug.Log($"Adding heat: {heat} to cellIndex: {heatmapCellIndex}");
        heatmapController.AddHeatToCell(heatmapCellIndex, heat);

        if (!heatmapController.isActiveAndEnabled)
            heatmapController.gameObject.SetActive(true);

        if (containsKeyBlock)
        {
            messageText.color = Color.green;
            messageText.text += $"\nSuccess! You found the key!";
            ShowMessage(messageText.text);
            DisableButtons();
            HighlightBlock(actualKeyAddress / BLOCK_SIZE, Color.green, true);
            return;
        }

        int scoreThisAttempt = 0;
        string heatComment;

        if (heat >= 0.75f) { heatComment = "Very hot!"; scoreThisAttempt = 100; }
        else if (heat >= 0.5f) { heatComment = "Warm."; scoreThisAttempt = 75; ShowHint("You're getting closer, keep trying!"); }
        else if (heat >= 0.3f) { heatComment = "Lukewarm."; scoreThisAttempt = 50; ShowHint("You're far. Try moving much closer."); }
        else { heatComment = (heat >= 0.1f) ? "Cold." : "Nothing"; scoreThisAttempt = (heat >= 0.1f) ? 25 : 1; ShowHint("This is far from the key. Try a different region."); consecutiveColdAttempts++; }

        if (consecutiveColdAttempts == 2 && !regionRevealed) { ShowHint(GetExpectedRegionHint()); RegionRevealColors(); regionRevealed = true; }
        else if (attemptsLeft == 3 && regionRevealed) { RevealHorizontalLines(); }
        else if (consecutiveColdAttempts >= 3 && regionRevealed && attemptsLeft == 2) { RevealVerticalHintLines(); }

        heatScore += scoreThisAttempt;
        UpdateGameStatsUI();

        if (heat > previousHeat && previousHeat >= 0) messageText.color = Color.yellow;
        else if (heat < previousHeat && previousHeat >= 0) messageText.color = Color.cyan;
        else messageText.color = Color.white;

        string attemptMsg = $"Heat: {heat:F2} — {heatComment}";
        ShowMessage(attemptMsg);

        previousHeat = heat;
        attemptsLeft--;
        UpdateGameStatsUI();

        RestoreHorizontalHintColors();
        RestoreVerticalHintColors();
        ResetBlockSelection();

        if (attemptsLeft <= 0)
        {
            messageText.text = $"Heat: {heat:F2} — {heatComment}\nScore: {scoreThisAttempt}\nTotal: {heatScore}\n(Block: {startBlock + 1}-{endBlock + 1})";

            if (heatScore >= 200)
            {
                messageText.color = Color.green;
                messageText.text += $"\nSuccess! Heat score: {heatScore}. You narrowed it well.";

            }
            else
            {
                messageText.color = Color.red;
                messageText.text += $"\nFailed. Final score: {heatScore}. Key was at {actualKeyAddress}.";
                HighlightBlock(actualKeyAddress / BLOCK_SIZE, Color.red);
            }

            ShowMessage(messageText.text);
            DisableButtons();
        }
    }

    // === Heat model (factor removed; edge-based distance) ===
    private float CalculateRawHeat(int startBlock, int endBlock)
    {
        int startAddr = startBlock * BLOCK_SIZE;
        int endAddr = (endBlock + 1) * BLOCK_SIZE - 1;

        // Distance from key to nearest edge (0 if inside)
        float distanceBytes;
        if (actualKeyAddress < startAddr) distanceBytes = startAddr - actualKeyAddress;
        else if (actualKeyAddress > endAddr) distanceBytes = actualKeyAddress - endAddr;
        else distanceBytes = 0f;

        // Proximity: linear falloff (tune PROX_SCALE to taste)
        const float PROX_SCALE = 512f; // bytes window for strong heat
        float proximity = 1f - Mathf.Clamp01(distanceBytes / PROX_SCALE);

        // Penalize wide selections (1 for tiny ranges → 0 for full memory)
        float rangeSize = endAddr - startAddr + 1;
        float rangePenalty = Mathf.Clamp01(1f - (rangeSize / (float)(MAX_MEMORY - MIN_MEMORY)));

        // Gentle boost if the selection's center is near the key
        float center = (startAddr + endAddr) / 2f;
        float affinity = 1f - Mathf.Clamp01(Mathf.Abs(center - actualKeyAddress) / (MAX_MEMORY / 2f));
        float affinityBoost = Mathf.Lerp(0.9f, 1f, affinity);

        float heat = proximity * rangePenalty * affinityBoost;
        return Mathf.Clamp01(heat);
    }

    // === Region helpers with computed boundaries ===
    private (int smallMaxIndex, int mediumMaxIndex) RegionBoundaries()
    {
        int smallMax = (BLOCK_COUNT / 4) - 1;                 // e.g., 64 -> index 63
        int mediumMax = (BLOCK_COUNT * 3 / 4) - 1;             // e.g., 192 -> index 191
        return (smallMax, mediumMax);
    }

    private string GetKeyRegion()
    {
        int keyBlock = actualKeyAddress / BLOCK_SIZE;
        var (smallMax, mediumMax) = RegionBoundaries();

        if (keyBlock <= smallMax) return "Small";
        else if (keyBlock <= mediumMax) return "Medium";
        else return "Large";
    }

    private void RevealVerticalHintLines()
    {
        string region = GetKeyRegion();
        var (smallMax, mediumMax) = RegionBoundaries();

        int minBlock, maxBlock;
        if (region == "Small") { minBlock = 0; maxBlock = smallMax; }
        else if (region == "Medium") { minBlock = smallMax + 1; maxBlock = mediumMax; }
        else { minBlock = mediumMax + 1; maxBlock = BLOCK_COUNT - 1; }

        int actualBlock = actualKeyAddress / BLOCK_SIZE;

        List<int> nearbyBlocks = new();
        int minDistance = 1;
        int maxDistance = 6;

        for (int i = minBlock; i <= maxBlock; i++)
        {
            int distance = Mathf.Abs(i - actualBlock);
            if (distance >= minDistance && distance <= maxDistance)
                nearbyBlocks.Add(i);
        }

        if (nearbyBlocks.Count < 3)
        {
            nearbyBlocks.Clear();
            for (int i = minBlock; i <= maxBlock; i++)
            {
                int distance = Mathf.Abs(i - actualBlock);
                if (distance >= minDistance && distance <= 10)
                    nearbyBlocks.Add(i);
            }
        }

        int blocksToShow = Mathf.Min(Random.Range(3, 6), nearbyBlocks.Count);
        for (int i = 0; i < blocksToShow && nearbyBlocks.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, nearbyBlocks.Count);
            int selectedBlock = nearbyBlocks[randomIndex];
            nearbyBlocks.RemoveAt(randomIndex);

            HighlightBlock(selectedBlock, new Color(0.95f, 0.9f, 0.5f), true);
            verticalHintBlocks.Add(selectedBlock);
        }

        ShowHint("These blocks may help, although they may not be the exact key block you are looking for.");
    }

    private void RevealHorizontalLines()
    {
        string region = GetKeyRegion();
        var (smallMax, mediumMax) = RegionBoundaries();

        int minBlock, maxBlock;
        if (region == "Small") { minBlock = 0; maxBlock = smallMax; }
        else if (region == "Medium") { minBlock = smallMax + 1; maxBlock = mediumMax; }
        else { minBlock = mediumMax + 1; maxBlock = BLOCK_COUNT - 1; }

        int totalRange = maxBlock - minBlock + 1;
        int bandHeight = Mathf.CeilToInt(totalRange / 4f);
        int skipBand = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            if (i == skipBand) continue;

            int bandStart = minBlock + i * bandHeight;
            int bandEnd = Mathf.Min(bandStart + bandHeight - 1, maxBlock);

            for (int b = bandStart; b <= bandEnd; b++)
            {
                Color hintColor = new Color(0.9f, 0.5f, 0.5f);
                HighlightBlock(b, hintColor);
                horizontalHintBlocks.Add(b);
            }
        }

        ShowHint("Try one of these rows");
    }

    private void RestoreHorizontalHintColors()
    {
        foreach (int i in horizontalHintBlocks)
            HighlightBlock(i, new Color(0.9f, 0.5f, 0.5f), true);
    }

    private void RestoreVerticalHintColors()
    {
        foreach (int i in verticalHintBlocks)
            HighlightBlock(i, new Color(0.95f, 0.9f, 0.5f), true);
    }

    private string GetExpectedRegionHint() { int keyBlock = actualKeyAddress / BLOCK_SIZE; if (keyBlock <= 63) return "The key seems to reside in the lower memory blocks (0–63)."; else if (keyBlock <= 191) return "The key appears to be in the middle memory blocks (64–191)."; else return "The key is likely in the upper memory blocks (192–255)."; }

    public void ResetGame()
    {
        heatScore = 0;
        previousHeat = -1f;
        attemptsLeft = 5;
        selectedStartBlock = null;
        selectedEndBlock = null;
        selectedBlockCount = 0;
        consecutiveColdAttempts = 0;
        regionRevealed = false;
        revealedKeyRegion = null;

        actualKeyAddress = Random.Range(MIN_MEMORY, MAX_MEMORY + 1);
        Debug.Log("Reset: Key at address " + actualKeyAddress);

        for (int i = 0; i < memoryBlocks.Count; i++)
        {
            Image img = memoryBlocks[i].GetComponent<Image>();
            img.color = Color.white;
            memoryBlocks[i].GetComponent<Button>().interactable = true;
        }

        UpdateGameStatsUI();
        ResetBlockSelection();
    }

    private void DisableButtons()
    {
        foreach (var block in memoryBlocks)
            block.GetComponent<Button>().interactable = false;
    }

    private Coroutine hintCoroutine;
    public void ShowHint(string message)
    {
        hintText.text = message;
        hintPanel.SetActive(true);

        if (hintCoroutine != null)
            StopCoroutine(hintCoroutine);
        hintCoroutine = StartCoroutine(HideHintAfterDelay(3f));
    }

    private IEnumerator HideHintAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hintPanel.SetActive(false);
    }
}
