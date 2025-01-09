using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Globalization;

public class MainLoopSimulation : MonoBehaviour
{
    private float[,] currentValues; 
    private float[,] newValues; 
    private HexTileInfo[,] hexGrid;

    public GameObject simSpeedInputPanel;
    public GameObject simParamPanel;
    public TMP_InputField simSpeedInputField;
    public TextMeshProUGUI simParamMessageText;

    public GameObject simJumpInputPanel;
    public TMP_InputField simJumpInputField;

    public Button SaveButton;

    private static int simLength = 0;
    private static int simJump = 1;
    private static int simSpeed = 5;
    private static int sizeX;
    private static int sizeY;
    private bool isTransitioning = false; 
    private int pendingSimSpeed = -1;
    private int pendingSimJump = -1; 
    public TextMeshProUGUI yearText;
    private static int year = 0;
 
    void Start()
    {
        simulationActiveFlag = true;
        stastisticsActiveFlag = false;
        simLength = MainMenu.instance.SimLength;
        simJump = MainMenu.instance.SimJump;
        simSpeed = MainMenu.instance.SimSpeed;
        simSpeed = (int)Mathf.Lerp(5f, 1f, (simSpeed - 1) / 99f);
        sizeX = MainMenu.instance.SizeX;
        sizeY = MainMenu.instance.SizeY;
        Debug.Log($"MAINLOOPSIMULATION x: {sizeX}, y: {sizeY}");

        if (simSpeedInputField != null)
        {
            simSpeedInputField.onEndEdit.AddListener(OnSimSpeedSubmit);
            simSpeedInputField.text = MainMenu.instance.SimSpeed.ToString();
        }

        if (simJumpInputField != null)
        {
            simJumpInputField.onEndEdit.AddListener(OnSimJumpSubmit);
            simJumpInputField.text = simJump.ToString();
        }

        if (StatisticsButton != null)
        {
            StatisticsButton.onClick.AddListener(ToggleStatistics);
        }

        Time.timeScale = 1;
        year = 0;

        if (simParamPanel != null)
        {
            simParamPanel.gameObject.SetActive(false);
        }

        if (GraphPanel != null)
        {
            GraphPanel.gameObject.SetActive(false);
        }

        InitializeGrid();
        StartCoroutine(SimulationLoop());
    }

    void InitializeGrid()
    {
        hexGrid = new HexTileInfo[sizeX, sizeY];
        currentValues = new float[sizeX, sizeY];
        newValues = new float[sizeX, sizeY];

        float[,] dataGrid = CsvLoader.GetDataGrid();

        foreach (HexTileInfo hex in UnityEngine.Object.FindObjectsByType<HexTileInfo>(FindObjectsSortMode.InstanceID))
        {
            string[] parts = hex.name.Split('_');
            if (parts.Length == 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
            {
                hexGrid[x, y] = hex;
                if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
                {
                    currentValues[x, y] = dataGrid[x, y];
                    hex.originalDSI = dataGrid[x, y]; 

                    Color initialColor = GetColorBasedOnDSI(dataGrid[x, y]);
                    hex.GetComponent<SpriteRenderer>().color = initialColor;
                }
            }
        }
    }

    IEnumerator SimulationLoop()
    {
        yield return new WaitForSecondsRealtime(3);
        while ((year < simLength) || simLength == 0)
        {
            for (int i = 0; i < simJump; i++)
            {
                CalculateNewValues();
                ApplyNewValues();
            }

            Debug.Log($"Year: {year}, Step: {simJump}, Speed: {simSpeed}");

            yield return StartCoroutine(AnimateTransitions());
            year += simJump;
            UpdateYearText();
            UpdateGraph();            

            yield return new WaitForSeconds(simSpeed);
        }

        if (year >= simLength)
        {
            UpdateYearText();
            StartCoroutine(ShowMessage($"Simulation finished!", Color.white));

            Debug.Log("Simulation finished.");
        }
    }

    void ApplyNewValues()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (hexGrid == null || hexGrid[x, y] == null) continue;

                currentValues[x, y] = newValues[x, y];
            }
        }
    }


    void CalculateNewValues()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (hexGrid == null || hexGrid[x, y] == null) continue;

                float sNc = CalculateSNc(x, y);

                newValues[x, y] = CalculateNewValue(currentValues[x, y], sNc);
            }
        }
    }

    float CalculateSNc(int x, int y)
    {
        List<float> neighborValues = GetNeighborValues(x, y);
        float product = 1.0f;

        foreach (float value in neighborValues)
        {
            product *= value;
        }

        return Mathf.Pow(product, 1f / neighborValues.Count);
    }

    List<float> GetNeighborValues(int x, int y)
    {
        List<float> neighbors = new List<float>();

        int[,] offsets = new int[,]
        {
            { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 1 }, { 1, -1 }, { 1, 0 }
        };

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int nx = x + offsets[i, 0];
            int ny = y + offsets[i, 1];

            if (nx >= 0 && nx < sizeX && ny >= 0 && ny < sizeY && hexGrid[nx, ny] != null)
            {
                neighbors.Add(currentValues[nx, ny]);
            }
            else
            {               
                neighbors.Add(currentValues[x, y]);
            }
        }

        return neighbors;
    }
    float CalculateNewValue(float sc, float sNc)
    {
        if (sc >= 1.78f && sc <= 2.0f && ((sNc >= 1.38f && sNc < 1.53f) || (sNc >= 1.53f && sNc < 1.78f)))
        {
            return sc;
        }
        else if (sc >= 1.53f && sc < 1.78f && sNc > 1.78f && sNc <= 2.0f)
        {
            return sNc;
        }
        else if (sc >= 1.655f && sc < 1.78f && sNc >= 1.655f && sNc < 1.78f)
        {
            return 1.78f;
        }
        else if (sc >= 1.455f && sc < 1.53f && sNc >= 1.89f)
        {
            return 1.78f;
        }
        else
        {
            return Mathf.Pow(sc * sNc, 0.5f);
        }
    }

    IEnumerator AnimateTransitions()
    {
        isTransitioning = true; 
        float elapsedTime = 0f;
        float currentSimSpeed = simSpeed;

        Color[,] startColors = new Color[sizeX, sizeY];
        Color[,] targetColors = new Color[sizeX, sizeY];

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (hexGrid == null || hexGrid[x, y] == null) continue;

                startColors[x, y] = hexGrid[x, y].GetComponent<SpriteRenderer>().color;
                targetColors[x, y] = GetColorBasedOnDSI(newValues[x, y]);
            }
        }

        while (elapsedTime < currentSimSpeed)
        {
            float t = elapsedTime / currentSimSpeed;

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if (hexGrid == null || hexGrid[x, y] == null) continue;

                    Color interpolatedColor = Color.Lerp(startColors[x, y], targetColors[x, y], t);
                    hexGrid[x, y].GetComponent<SpriteRenderer>().color = interpolatedColor;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (hexGrid == null || hexGrid[x, y] == null) continue;

                hexGrid[x, y].UpdateDSI(newValues[x, y]);

                hexGrid[x, y].GetComponent<SpriteRenderer>().color = targetColors[x, y];

                currentValues[x, y] = newValues[x, y];
            }
        }

        isTransitioning = false; 

        if (pendingSimSpeed > 0)
        {
            simSpeed = pendingSimSpeed;
            pendingSimSpeed = -1;
            Debug.Log($"Applied pending SimSpeed: {simSpeed}");
        }
        if (pendingSimJump > 0)
        {
            simJump = pendingSimJump;
            pendingSimJump = -1; 
            Debug.Log($"Applied pending SimJump: {simJump}");
        }
    }

    Color GetColorBasedOnDSI(float dsiValue)
    {
        if (dsiValue >= 1.78f && dsiValue <= 2.0f)
        {
            return new Color(1.0f, 0.8f, 0.0f);
        }
        else if (dsiValue >= 1.53f && dsiValue < 1.78f)
        {
            return new Color(1.0f, 0.9f, 0.3f);
        }
        else if (dsiValue >= 1.38f && dsiValue < 1.53f)
        {
            return new Color(1.0f, 1.0f, 0.0f);
        }
        else if (dsiValue >= 1.22f && dsiValue < 1.38f)
        {
            return new Color(0.6f, 1.0f, 0.3f);
        }
        else if (dsiValue >= 1.0f && dsiValue < 1.22f)
        {
            return Color.green;
        }
        else
        {
            return Color.gray;
        }
    }
    public TextMeshProUGUI toggleButtonText;

    private bool simulationActiveFlag = true;

    public void ToggleSimulation()
    {
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0; 
            simulationActiveFlag = false;
            Debug.Log("Symulacja wstrzymana");

            if (toggleButtonText != null)
            {
                toggleButtonText.text = "Start Simulation";
            }
        }
        else
        {
            Time.timeScale = 1;
            simulationActiveFlag = true;
            Debug.Log("Symulacja wznowiona");

            if (toggleButtonText != null)
            {
                toggleButtonText.text = "Pause Simulation";
            }
        }
    }

    private void OnSimSpeedSubmit(string simSpeedText)
    {
        if (string.IsNullOrEmpty(simSpeedText))
        {
            Debug.Log("Cannot be null");
            StartCoroutine(ShowMessage($"Field cannot be blank!", Color.white));
            return;
        }

        if (!int.TryParse(simSpeedText, out int newSimSpeed))
        {
            StartCoroutine(ShowMessage($"Please provide a valid input!", Color.red));
            Debug.LogError("Invalid input. Please enter a valid number.");
        }
        else if (newSimSpeed <= 0 || newSimSpeed > 100)
        {
            StartCoroutine(ShowMessage($"Simulation speed must be in the range from 1 to 100!", Color.red));
            Debug.LogError("Invalid input. Simulation speed must be a positive number.");
        }
        else
        {
            StartCoroutine(ShowMessage($"Successfully changed simulation speed to {newSimSpeed}!", Color.white));
            if (isTransitioning)
            {
                pendingSimSpeed = (int)Mathf.Lerp(5f, 1f, (newSimSpeed - 1) / 99f);
               
                Debug.Log($"New SimSpeed ({newSimSpeed}) will be applied after the current transition.");
            }
            else
            {
                simSpeed = (int)Mathf.Lerp(5f, 1f, (newSimSpeed - 1) / 99f);         
                Debug.Log($"Changed SimSpeed to: {newSimSpeed}");
            }
        }
    }

    private void OnSimJumpSubmit(string simJumpText)
    {
        if (string.IsNullOrEmpty(simJumpText))
        {
            Debug.Log("Cannot be null");
            StartCoroutine(ShowMessage($"Field cannot be blank!", Color.white));
            return;
        }

        if (!int.TryParse(simJumpText, out int newSimJump))
        {
            StartCoroutine(ShowMessage($"Please provide a valid input!", Color.red));
            Debug.LogError("Invalid input. Please enter a valid number.");
        }
        else if (newSimJump <= 0)
        {
            StartCoroutine(ShowMessage($"Simulation step must be greater than 0!", Color.red));
            Debug.LogError("Invalid input. Simulation step must be a positive number.");
        }
        else
        {
            StartCoroutine(ShowMessage($"Successfully changed simulation step to {newSimJump}!", Color.white));
            if (isTransitioning)
            {
                pendingSimJump = newSimJump; 
                Debug.Log($"New SimJump ({newSimJump}) will be applied after the current transition.");
            }
            else
            {
                simJump = newSimJump;      
                Debug.Log($"Changed SimJump to: {newSimJump}");
            }
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu"); 
    }

    private IEnumerator ShowMessage(string message, Color color)
    {
        simParamPanel.gameObject.SetActive(true);
        if (simParamMessageText != null)
        {
            simParamMessageText.text = message;
            simParamMessageText.color = color;

            yield return new WaitForSecondsRealtime(3);

            simParamPanel.gameObject.SetActive(false);
        }
    }

    void UpdateYearText()
    {
        if (yearText != null)
        {
            yearText.text = $"Year: {year}"; 
        }
    }

    public void SaveCurrentValuesToFile()
    {
        Time.timeScale = 0;

        string timestamp = DateTime.Now.ToString("dd.MM.yyyy_HH-mm-ss");
        string fileName = $"{timestamp}.csv";

        List<string> lines = new List<string>();

        lines.Add("x,y,vc,dr,ep,r,a,t,se,t_x,d,s_d,cr,pa");

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (hexGrid[x, y] != null) 
                {
                    string value = currentValues[x, y].ToString(CultureInfo.InvariantCulture);
                    string row = $"{x},{y},{value},{value},{value},{value},{value},{value},{value},{value},{value},{value},{value},{value}";
                    lines.Add(row);
                }
            }
        }

        string filePath = Path.Combine(Application.dataPath, "Resources", fileName);
        File.WriteAllLines(filePath, lines.ToArray());
        Debug.Log($"Saved grid data to {filePath}");

        StartCoroutine(SaveMessageSequence(fileName));
    }

    private IEnumerator SaveMessageSequence(string fileName)
    {
        StartCoroutine(ShowMessage($"Saved grid data to {fileName}", Color.white));

        yield return new WaitForSecondsRealtime(3);


        if (simulationActiveFlag == true)
        {
            Time.timeScale = 1;

            if (year < simLength)
            {
                StartCoroutine(ShowMessage("Simulation continues", Color.white));
            }
        }       
    }

    public GameObject GraphPanel;
    public Button StatisticsButton;
    public TextMeshProUGUI StatisticsButtonText;
    public GameObject LowObject;
    public GameObject ModerateObject;
    public GameObject HighObject;
    public GameObject DegradedObject;
    public GameObject VeryDegradedObject;
    public TextMeshProUGUI LowText;
    public TextMeshProUGUI ModerateText;
    public TextMeshProUGUI HighText;
    public TextMeshProUGUI DegradedText;
    public TextMeshProUGUI VeryDegradedText;

    private bool stastisticsActiveFlag = false;
    private const int maxHeight = 300; 

    public void ToggleStatistics()
    {
        if (stastisticsActiveFlag == true)
        {
            stastisticsActiveFlag = false;

            if (GraphPanel != null)
            {
                GraphPanel.gameObject.SetActive(false);
            }

            if (StatisticsButtonText != null)
            {
                StatisticsButtonText.text = "Statistics: OFF";
            }
        }
        else
        {
            stastisticsActiveFlag = true;

            if (GraphPanel != null)
            {
                GraphPanel.gameObject.SetActive(true);
            }

            if (StatisticsButtonText != null)
            {
                StatisticsButtonText.text = "Statistics: ON";
            }

            UpdateGraph();
        }
    }

    public void UpdateGraph()
    {
        float[] graphPercentages = new float[5];

        for (int i = 0; i < 40; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                float value = currentValues[i, j];
                if (value >= 1.78f && value <= 2.0f)
                    graphPercentages[0]++;
                else if (value > 1.53f && value < 1.78f)
                    graphPercentages[1]++;
                else if (value > 1.38f && value <= 1.53f)
                    graphPercentages[2]++;
                else if (value > 1.22f && value <= 1.38f)
                    graphPercentages[3]++;
                else if (value >= 1.0f && value <= 1.22f)
                    graphPercentages[4]++;
            }
        }

        for (int i = 0; i < graphPercentages.Length; i++)
        {
            graphPercentages[i] = (graphPercentages[i] / 1600f) * 100f;
        }

        LowText.text = $"{graphPercentages[4]:F1}%";
        ModerateText.text = $"{graphPercentages[3]:F1}%";
        HighText.text = $"{graphPercentages[2]:F1}%";
        DegradedText.text = $"{graphPercentages[1]:F1}%";
        VeryDegradedText.text = $"{graphPercentages[0]:F1}%";

        SetBarHeight(LowObject, graphPercentages[4]);
        SetBarHeight(ModerateObject, graphPercentages[3]);
        SetBarHeight(HighObject, graphPercentages[2]);
        SetBarHeight(DegradedObject, graphPercentages[1]);
        SetBarHeight(VeryDegradedObject, graphPercentages[0]);
    }

    private void SetBarHeight(GameObject barObject, float percentage)
    {
        RectTransform rectTransform = barObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            float newHeight = (percentage / 100f) * maxHeight; 
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
        }
    }
}
