using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameOfLife : MonoBehaviour
{
    [Range(1, 100)] public int gridSize = 10;
    [Range(0.01f, 2f)] public float generationDelay = 1f;
    private bool[,] grid;
    private bool[,] nextGrid;
    private Image[,] gridImages;
    public GameObject cellPrefab; // Should be a UI Image prefab
    
    private float lastGenerationTime = 0f;

    public bool gameOfLife = false;
    public TextMeshProUGUI generationText;
    private int generationCount = 0;
    public TextMeshProUGUI populationText;
    private int populationCount = 0;

    private const float MIN_GENERATION_DELAY = 0.01f;
    private Coroutine gameOfLifeCoroutine;
    public bool showGridLines = true;

    public Canvas gameCanvas;
    public RectTransform gridParent;
    public float cellSize = 50f;
    public Color gridLineColor = Color.gray;
    public float gridLineWidth = 2f;

    // UI Components
    public Toggle gridLinesToggle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeUI();
        InitializeGrid();
        UpdateGridLinesVisibility();
        UpdateGenerationText();
        UpdatePopulationText();
    }

    void InitializeUI()
    {
        // Setup grid lines toggle
        if (gridLinesToggle != null)
        {
            gridLinesToggle.isOn = showGridLines;
            gridLinesToggle.onValueChanged.AddListener(OnGridLinesToggleChanged);
        }
    }

    void InitializeGrid()
    {
        grid = new bool[gridSize, gridSize];
        nextGrid = new bool[gridSize, gridSize];
        gridImages = new Image[gridSize, gridSize];

        // Calculate starting position for centering the grid
        float totalGridSize = gridSize * cellSize;
        float startX = -totalGridSize / 2f + cellSize / 2f;
        float startY = -totalGridSize / 2f + cellSize / 2f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject cell = Instantiate(cellPrefab, gridParent);
                RectTransform cellRect = cell.GetComponent<RectTransform>();
                cellRect.anchoredPosition = new Vector2(startX + x * cellSize, startY + y * cellSize);
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                
                Image cellImage = cell.GetComponent<Image>();
                cellImage.color = Color.black;
                
                // Add outline component for grid lines
                Outline outline = cell.GetComponent<Outline>();
                if (outline == null)
                    outline = cell.AddComponent<Outline>();
                
                outline.effectColor = gridLineColor;
                outline.effectDistance = new Vector2(gridLineWidth, gridLineWidth);
                outline.enabled = showGridLines;
                
                // Add button component for click detection
                Button cellButton = cell.GetComponent<Button>();
                if (cellButton == null)
                    cellButton = cell.AddComponent<Button>();
                
                int capturedX = x;
                int capturedY = y;
                cellButton.onClick.AddListener(() => ToggleCellState(capturedX, capturedY));
                
                gridImages[x, y] = cellImage;
                grid[x, y] = false;
                nextGrid[x, y] = false;
            }
        }
    }

    void GenerateGrid()
    {
        // Clear nextGrid before calculating
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                nextGrid[x, y] = false;
            }
        }

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                CheckRule(x, y);
            }
        }

        // Swap arrays
        bool[,] temp = grid;
        grid = nextGrid;
        nextGrid = temp;

        UpdateGrid();
        generationCount++;
        UpdateGenerationText();
        UpdatePopulationText();
    }

    void UpdateGrid()
    {
        populationCount = 0;
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                gridImages[x, y].color = grid[x, y] ? Color.white : Color.black;
                if(grid[x, y])
                {
                    populationCount++;
                }
            }
        }
    }

    void CheckRule(int currX, int currY)
    {
        int liveNeighbors = CountLiveNeighbors(currX, currY);

        if(grid[currX, currY] == true)
        {
            if(liveNeighbors < 2)
            {
                nextGrid[currX, currY] = false;
            }
            else if(liveNeighbors > 3)
            {
                nextGrid[currX, currY] = false;
            }
            else if(liveNeighbors == 2 || liveNeighbors == 3)
            {
                nextGrid[currX, currY] = true;
            }
        }
        else if(grid[currX, currY] == false)    
        {
            if(liveNeighbors == 3)
            {
                nextGrid[currX, currY] = true;
            }
        }
    }

    int CountLiveNeighbors(int x, int y)
    {
        int liveNeighbors = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                // Skip the center cell (the cell itself)
                if (dx == 0 && dy == 0)
                    continue;

                int neighborX = x + dx;
                int neighborY = y + dy;

                // Check if neighbor is within bounds
                if (neighborX >= 0 && neighborX < gridSize && 
                    neighborY >= 0 && neighborY < gridSize)
                {
                    if (grid[neighborX, neighborY])
                    {
                        liveNeighbors++;
                    }
                }
            }
        }

        return liveNeighbors;
    }

    void UpdateGridLinesVisibility()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Outline outline = gridImages[x, y].GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = showGridLines;
                }
            }
        }
    }

    public void ToggleGridLines()
    {
        showGridLines = !showGridLines;
        UpdateGridLinesVisibility();
    }

    public void ToggleCellState(int x, int y)
    {
        // Update population count based on current state
        if (grid[x, y])
        {
            populationCount--;
        }
        else
        {
            populationCount++;
        }

        // Toggle the cell state
        grid[x, y] = !grid[x, y];
        gridImages[x, y].color = grid[x, y] ? Color.white : Color.black;
        
        // Update the population text
        UpdatePopulationText();
    }

    void UpdateGenerationText()
    {
        generationText.text = "Generation: " + generationCount;
    }

    void UpdatePopulationText()
    {
        populationText.text = "Population: " + populationCount;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {   
            if(gameOfLife)
            {
                gameOfLife = false;
                if (gameOfLifeCoroutine != null)
                {
                    StopCoroutine(gameOfLifeCoroutine);
                    gameOfLifeCoroutine = null;
                }
            }
            else
            {
                gameOfLife = true;
                if (gameOfLifeCoroutine == null)
                {
                    gameOfLifeCoroutine = StartCoroutine(GameOfLifeCoroutine());
                }
            }
        }
        else if(Input.GetKeyDown(KeyCode.R))
        {
            GenerateGrid();
        }

        // Ensure generationDelay is not set below the minimum threshold
        generationDelay = Mathf.Max(generationDelay, MIN_GENERATION_DELAY);
    }

    IEnumerator GameOfLifeCoroutine()
    {
        while(gameOfLife)
        {
            GenerateGrid();
            yield return new WaitForSeconds(generationDelay);
        }
    }

    // Button methods
    public void ResetGame()
    {
        // Stop the game if it's running
        if (gameOfLife)
        {
            gameOfLife = false;
            if (gameOfLifeCoroutine != null)
            {
                StopCoroutine(gameOfLifeCoroutine);
                gameOfLifeCoroutine = null;
            }
        }

        // Reset all cells to dead state
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x, y] = false;
                nextGrid[x, y] = false;
            }
        }

        // Reset counters
        generationCount = 0;
        populationCount = 0;

        // Update display
        UpdateGrid();
        UpdateGenerationText();
        UpdatePopulationText();
    }

    public void TogglePlayPause()
    {
        if (gameOfLife)
        {
            gameOfLife = false;
            if (gameOfLifeCoroutine != null)
            {
                StopCoroutine(gameOfLifeCoroutine);
                gameOfLifeCoroutine = null;
            }
        }
        else
        {
            gameOfLife = true;
            if (gameOfLifeCoroutine == null)
            {
                gameOfLifeCoroutine = StartCoroutine(GameOfLifeCoroutine());
            }
        }
    }

    public void NextStep()
    {
        // Stop the game if it's running
        if (gameOfLife)
        {
            gameOfLife = false;
            if (gameOfLifeCoroutine != null)
            {
                StopCoroutine(gameOfLifeCoroutine);
                gameOfLifeCoroutine = null;
            }
        }

        // Generate next generation
        GenerateGrid();
    }

    public void OnGridLinesToggleChanged(bool isOn)
    {
        showGridLines = isOn;
        UpdateGridLinesVisibility();
    }
}
