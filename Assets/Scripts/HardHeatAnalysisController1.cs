using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class HardHeatAnalysisController : MonoBehaviour
{
    [Header("Grid Setup")]
    public GameObject memoryBlockPrefab;
    public Transform memoryGridParent;

    [Header("Buttons")]
    public Button smallKeyButton;
    public Button mediumKeyButton;
    public Button largeKeyButton;


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

    private readonly List<GameObject> memoryBlocks = new();
    private int? selectedStartBlock = null;
    private int? selectedEndBlock = null;

    private string selectedKeySize;
    private float selectedKeyFactor;
    private int attemptsLeft = 5;
    private int actualKeyAddress;

    private int heatScore = 0;

    private float previousHeat = -1f;
    private const int HARD_MIN_MEMORY = 512;
    private const int HARD_MAX_MEMORY = 1536;
    private const int BLOCK_SIZE = 8; 


    private int selectedBlockCount = 0;
    private const int MAX_BLOCK_SELECTION = 5;

    int blockStart = HARD_MIN_MEMORY / BLOCK_SIZE;
    int blockEnd = HARD_MAX_MEMORY / BLOCK_SIZE;

    //private int consecutiveColdAttempts = 0;
    //private bool regionRevealed = false;
    //private Color smallRegionColor = new (0.7f, 0.85f, 1f); //Blue for the small key region
    //private Color mediumRegionColor = new (1f, 1f, 0.75f); // Yellow for the mid key region
    //private Color largeRegionColor = new(1f, 0.75f, 0.75f); //Red for the large key region
    //private string revealedKeyRegion = null;

    public void SelectSmallKey() 
    { 
        selectedKeySize = "Small"; 
        selectedKeyFactor = 0.2f; 
    }
    public void SelectMediumKey() 
    { 
        selectedKeySize = "Medium"; 
        selectedKeyFactor = 0.5f; 
    }
    public void SelectLargeKey() 
    { 
        selectedKeySize = "Large"; 
        selectedKeyFactor = 0.9f; 
    }

    private void Start()
    {
        GenerateMemoryGrid();
        actualKeyAddress = Random.Range(HARD_MIN_MEMORY, HARD_MAX_MEMORY + 1);
        Debug.Log("Actual Key Address: " + actualKeyAddress);
        int actualKeyBlock = actualKeyAddress / BLOCK_SIZE;
        Debug.Log("Actual Key Block: " + actualKeyBlock);
        int blockStart = HARD_MIN_MEMORY / BLOCK_SIZE;
        Debug.Log("Visual Block Index: " + (actualKeyBlock - blockStart + 1) );

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
       

        for (int i = blockStart; i < blockEnd; i++)
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

    private void HighlightBlock(int index, Color color)
    {
        int visualIndex = index - blockStart;

        var blockImage = memoryBlocks[visualIndex].GetComponent<Image>();
        if (blockImage != null)
            blockImage.color = color;
    }

    private void ResetBlockSelection()
    {
        for (int i = 0; i < memoryBlocks.Count; i++)
        {
            Image img = memoryBlocks[i].GetComponent<Image>();

            //if (regionRevealed)
            //{
            //    if (revealedKeyRegion == "Small" && i <= 63)
            //        img.color = smallRegionColor;
            //    else if (revealedKeyRegion == "Medium" && i >= 64 && i <= 191)
            //        img.color = mediumRegionColor;
            //    else if (revealedKeyRegion == "Large" && i >= 192)
            //        img.color = largeRegionColor;
            //    else
            //        img.color = new Color(0.9f, 0.9f, 0.9f);

            //}
            //else
            //{
                img.color = Color.white;
            //}
        }


        selectedStartBlock = null;
        selectedEndBlock = null;
        selectedBlockCount = 0;
    }

    //private void RegionRevealColors()
    //{
    //    //revealedKeyRegion = GetKeyRegion();
    //    //regionRevealed = true;
    //    for (int i = 0; i < memoryBlocks.Count; i++)
    //    {
    //        Image img = memoryBlocks[i].GetComponent<Image>();

    //        if (revealedKeyRegion == "Small" && i<=63)
    //        img.color = smallRegionColor;
    //        else if (revealedKeyRegion == "Medium" && i >=64 && i<=191)
    //            img.color = mediumRegionColor;
    //        else if (revealedKeyRegion == "Large" && i >= 192)
    //            img.color = largeRegionColor;
    //        else
    //            img.color = new Color(0.9f, 0.9f, 0.9f); //Grayed out areas
    //    }

    //}
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

        int baseBlockIndex = HARD_MIN_MEMORY / BLOCK_SIZE;


        int startAddress = (startBlock + baseBlockIndex) * BLOCK_SIZE;
        int endAddress = (endBlock + baseBlockIndex + 1) * BLOCK_SIZE - 1;
        float heat = CalculateRawHeat(startBlock, endBlock);

        if (actualKeyAddress >= startAddress && actualKeyAddress <= endAddress)
        {
            messageText.color = Color.green;
            messageText.text += $"\nSuccess! You found the key!";
            ShowMessage(messageText.text);
            heat = Mathf.Max(heat, 1.0f);
            DisableButtons();
            return;
        }

        int scoreThisAttempt = 0;
        string heatComment;

      
        if (heat >= 0.75f) 
        { 
            heatComment = "Very hot!"; 
            scoreThisAttempt = 100;
           // consecutiveColdAttempts = 0;

        }
        else if (heat >= 0.5f) 
        { 
            heatComment = "Warm."; scoreThisAttempt = 75;
            //ShowHint("You're getting closer, keep trying!.");
            //consecutiveColdAttempts = 0;

        }
        else if (heat >= 0.3f) 
        { 
            heatComment = "Lukewarm."; scoreThisAttempt = 50;
            //ShowHint("You're far. Try moving much closer.");
            //consecutiveColdAttempts = 0;
        }
        else if (heat >= 0.1f) 
        { 
            heatComment = "Cold."; scoreThisAttempt = 25;
            //consecutiveColdAttempts++;

            //if (consecutiveColdAttempts == 1)
            //{
            //    ShowHint("You're searching way off — try a completely different region.");
            //}
            //else if(consecutiveColdAttempts >= 2 && !regionRevealed)
            //{
            //    string regionHint = GetExpectedRegionHint();
            //    ShowHint(regionHint);
            //    RegionRevealColors();
            //}

        }
        else 
        { 
            heatComment = "Nothing";
            //ShowHint(" This is the farthest from where the key is. Try a different memory region");
            scoreThisAttempt ++;
            //consecutiveColdAttempts ++;

            //if(consecutiveColdAttempts == 1)
            //{
            //    ShowHint(" This is the farthest from where the key is. Try a different memory region");
            //}
            //else if(consecutiveColdAttempts >= 2)
            //{
            //    string regionHint = GetExpectedRegionHint();
            //    ShowHint(regionHint);
            //    if (!regionRevealed)
            //        RegionRevealColors();
            //}


        }

        heatScore += scoreThisAttempt;
        UpdateGameStatsUI();

        if (heat > previousHeat && previousHeat >= 0)
            messageText.color = Color.yellow;
        else if (heat < previousHeat && previousHeat >= 0)
            messageText.color = Color.cyan;
        else
            messageText.color = Color.white;

        string attemptMsg = $"Heat: {heat:F2} — {heatComment}\nScore: {scoreThisAttempt}\nTotal: {heatScore})";
        ShowMessage(attemptMsg);

        previousHeat = heat;
        attemptsLeft--;
        UpdateGameStatsUI();

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

    private float CalculateRawHeat(int startBlock, int endBlock)
    {
        int startAddr = startBlock * BLOCK_SIZE;
        int endAddr = (endBlock + 1) * BLOCK_SIZE - 1;
        float center = (startAddr + endAddr) / 2f;

        float distance = Mathf.Abs(center - actualKeyAddress);
        float maxDistance = (HARD_MAX_MEMORY - HARD_MIN_MEMORY);

        float proximity = 1f - Mathf.Clamp01(distance / maxDistance);
        float rangeSize = endAddr - startAddr + 1;

        int preferredMin = GetPreferredMin(selectedKeySize);
        int preferredMax = GetPreferredMax(selectedKeySize);
        float locationAffinity = (center >= preferredMin && center <= preferredMax) ? 1f : 0.4f;

        float heat = selectedKeyFactor * proximity * Mathf.Clamp01(1024f / rangeSize) * locationAffinity;



        return Mathf.Clamp01(heat);
    }

    private int GetPreferredMin(string keySize)
    {
        switch (keySize)
        {
            case "Small": return 512;
            case "Medium": return 854;
            case "Large": return 1196;
            default: return 0;
        }
    }

    private int GetPreferredMax(string keySize)
    {
        switch (keySize)
        {
            case "Small": return 853;
            case "Medium": return 1195;
            case "Large": return 1536;
            default: return 1536;
        }
    }

    void DisableButtons()
    {
        foreach (var block in memoryBlocks)
            block.GetComponent<Button>().interactable = false;

        smallKeyButton.interactable = false;
        mediumKeyButton.interactable = false;
        largeKeyButton.interactable = false;
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

    //private string GetKeyRegion()
    //{
    //    int keyBlock = actualKeyAddress / BLOCK_SIZE;
    //    if (keyBlock <= 63)
    //        return "Small";
    //    else if (keyBlock <= 191)
    //        return "Medium";
    //    else 
    //        return "Large";
    //}

    //private string GetExpectedRegionHint()
    //{
    //    int keyBlock = actualKeyAddress / BLOCK_SIZE;

    //    if (keyBlock <= 63)
    //        return "The key seems to reside in the lower memory blocks (0–63).";
    //    else if (keyBlock <= 191)
    //        return "The key appears to be in the middle memory blocks (64–191).";
    //    else
    //        return "The key is likely in the upper memory blocks (192–255).";
    //}



    public void ResetGame()
    {
        // Reset values
        heatScore = 0;
        previousHeat = -1f;
        attemptsLeft = 5;
        selectedStartBlock = null;
        selectedEndBlock = null;
        selectedBlockCount = 0;
        //consecutiveColdAttempts = 0;
        //regionRevealed = false;
        //revealedKeyRegion = null;

        // Generate new key
        actualKeyAddress = Random.Range(HARD_MIN_MEMORY, HARD_MAX_MEMORY + 1);

        Debug.Log("Reset: Key at address " + actualKeyAddress);

        // Reset blocks
        for (int i = 0; i < memoryBlocks.Count; i++)
        {
            Image img = memoryBlocks[i].GetComponent<Image>();
            img.color = Color.white;
            memoryBlocks[i].GetComponent<Button>().interactable = true;
        }

        smallKeyButton.interactable = true;
        mediumKeyButton.interactable = true;
        largeKeyButton.interactable = true;

        UpdateGameStatsUI();

        ResetBlockSelection();
    }





}
