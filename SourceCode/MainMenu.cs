using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    private int sizeX = 40;
    private int sizeY = 40;
    private int simJump = 1;
    private int simSpeed = 100;
    private int simLength = 0;

    public Button GenerateRandomDataButton; 
    public Button loadCsvButton; 
    public TMP_Dropdown loadCsvDropdown;
    private string csvFolderPath;

    public Button startSimulationButton; 
    private bool csvValueFlag = false;

    public GameObject inputPanel;
    public GameObject MessagePanel;
    public TextMeshProUGUI MessageText;

    public Color successColor;
    public Color defaultColor = Color.white;

    public Button showParametersButton; 
    public GameObject parametersPanel;   
    public TMP_InputField parameter1Field;  
    public TMP_InputField parameter2Field;  
    public TMP_InputField parameter3Field;   
    public Button submitParametersButton; 

    private bool parametersValueFlag = false;

    public int SizeX
    {
        get => sizeX;
        set
        {
            if (value > 0) sizeX = value;
            else Debug.LogWarning("SizeX musi być większy od 0!");
        }
    }

    public int SizeY
    {
        get => sizeY;
        set
        {
            if (value > 0) sizeY = value;
            else Debug.LogWarning("SizeY musi być większy od 0!");
        }
    }

    public int SimJump
    {
        get => simJump;
        set
        {
            if (value > 0) simJump = value;
            else Debug.LogWarning("SimJump musi być większy od 0!");
        }
    }

    public int SimSpeed
    {
        get => simSpeed;
        set
        {
            if (value > 0) simSpeed = value;
            else Debug.LogWarning("SimSpeed musi być większy od 0!");
        }
    }

    public int SimLength
    {
        get => simLength;
        set
        {
            if (value >= 0) simLength = value;
            else Debug.LogWarning("SimLength nie może być ujemny!");
        }
    }

    public static MainMenu instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("MainMenu instance created and marked as DontDestroyOnLoad.");
        }
        else if (instance != this)
        {
            Debug.Log("Duplicate MainMenu instance found and destroyed.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log($"MAINMENU x: {sizeX}, y: {sizeY}");
        csvFolderPath = Path.Combine(Application.dataPath, "Resources");
        parametersValueFlag = false;
        csvValueFlag = false;

        if (inputPanel != null)
            inputPanel.SetActive(false);

        if (MessagePanel != null)
            MessagePanel.gameObject.SetActive(false);

        if (RandomDataInputPanel != null)
            RandomDataInputPanel.SetActive(false);

        if (loadCsvButton != null)
        {
            loadCsvButton.onClick.AddListener(ShowInputPanel);
            loadCsvButton.GetComponent<Image>().color = defaultColor; 
            LoadFilesToDropdown();
        }

        if (GenerateRandomDataButton != null)
        {
            GenerateRandomDataButton.onClick.AddListener(ShowRandomDataInputPanel);
            GenerateRandomDataButton.GetComponent<Image>().color = defaultColor; 
        }

        if (parametersPanel != null)
            parametersPanel.SetActive(false);

        if (showParametersButton != null)
            showParametersButton.onClick.AddListener(ShowParametersPanel);

        if (submitParametersButton != null)
            submitParametersButton.onClick.AddListener(OnSubmitParameters);

        if (ConfirmRandomData != null)
            ConfirmRandomData.onClick.AddListener(ValidateRandomData);

        if (startSimulationButton != null)
        {
            startSimulationButton.onClick.AddListener(StartSimulation);
        }
    }

    public void StartSimulation()
    {
        if (parametersValueFlag == true && csvValueFlag == true)
        {
            SceneManager.LoadScene("HexGridScene");
        }
        else
        {
            StartCoroutine(ShowMessage("Please select grid and parameters first!", Color.red));
        }
    }

    private void ShowInputPanel()
    {
        if (inputPanel != null)
        {
            inputPanel.SetActive(true);  
        }
    }
    void LoadFilesToDropdown()
    {
        if (!Directory.Exists(csvFolderPath))
        {
            Debug.LogError($"Folder {csvFolderPath} nie istnieje!");
            return;
        }

        string[] files = Directory.GetFiles(csvFolderPath);

        loadCsvDropdown.ClearOptions();

        List<string> options = new List<string>();
        foreach (string file in files)
        {
            if (Path.GetExtension(file) == ".csv") 
            {
                options.Add(Path.GetFileName(file));
            }
        }

        loadCsvDropdown.AddOptions(options);
    }

    public void OnFileSelected(int index)
    {
        string selectedFile = loadCsvDropdown.options[index].text;
        Debug.Log($"index: {index}");
        if (inputPanel != null)
            inputPanel.SetActive(false);

        StartCoroutine(ShowMessage("Successfully loaded the file!", Color.white));
        csvValueFlag = true;

        loadCsvButton.GetComponent<Image>().color = successColor;
        GenerateRandomDataButton.GetComponent<Image>().color = defaultColor;

        string filePath = Path.Combine(csvFolderPath, selectedFile);
        Debug.Log($"Ładowanie pliku CSV: {filePath}");
        CsvLoader.LoadCsv(filePath);
        Debug.Log($"Wybrano plik: {selectedFile}");
    }

    private IEnumerator ShowMessage(string message, Color color)
    {
        if (MessagePanel != null)
        {
            MessagePanel.SetActive(true);
            if (MessageText != null)
            {
                MessageText.text = message;
                MessageText.color = color;
            }

            yield return new WaitForSeconds(3); 

            MessagePanel.gameObject.SetActive(false); 
        }
    }

    private void ShowParametersPanel()
    {
        if (parametersPanel != null)
        {
            parametersPanel.SetActive(true);
        }
    }

    private void OnSubmitParameters()
    {
        string parameter1 = parameter1Field != null ? parameter1Field.text.Trim() : "";
        string parameter2 = parameter2Field != null ? parameter2Field.text.Trim() : "";
        string parameter3 = parameter3Field != null ? parameter3Field.text.Trim() : "";

        if (string.IsNullOrEmpty(parameter1) || string.IsNullOrEmpty(parameter2) || string.IsNullOrEmpty(parameter3))
        {
            StartCoroutine(ShowMessage("Fields cannot be blank!", Color.red));
            return;
        }

        if (!int.TryParse(parameter1, out int param1Value) || !int.TryParse(parameter2, out int param2Value) || !int.TryParse(parameter3, out int param3Value))
        {
            StartCoroutine(ShowMessage("All fields must contain valid values!", Color.red));
            return;
        }

        if (param1Value <= 0 || param3Value <= 0)
        {
            StartCoroutine(ShowMessage("Step and Speed must be positive!", Color.red));
            return;
        }

        if (param3Value > 100)
        {
            StartCoroutine(ShowMessage("Speed must be in the range from 1 to 100!", Color.red));
            return;
        }

        if (param2Value < 0)
        {
            StartCoroutine(ShowMessage("Length cannot be negative!", Color.red));
            return;
        }

        simJump = Int32.Parse(parameter1);
        simSpeed = Int32.Parse(parameter3);
        simLength = Int32.Parse(parameter2);

        Debug.Log($"Parametry: {parameter1}, {parameter2}, {parameter3}");

        StartCoroutine(ShowMessage("Parameters saved successfully!", Color.white));
        parametersValueFlag = true;
        showParametersButton.GetComponent<Image>().color = successColor;

        if (parametersPanel != null)
            parametersPanel.SetActive(false);
    }

    public TMP_InputField numberOfClustersField; 
    public TMP_InputField clusterSizeField;
    public TMP_InputField degradedValueField;
    public TMP_InputField otherValuesField; 

    public void ValidateRandomData()
    {
        string degradedValueText = degradedValueField != null ? degradedValueField.text.Trim() : "";
        string otherValuesText = otherValuesField != null ? otherValuesField.text.Trim() : "";
        string numberOfClustersText = numberOfClustersField != null ? numberOfClustersField.text.Trim() : "";
        string clusterSizeText = clusterSizeField != null ? clusterSizeField.text.Trim() : "";

        if (string.IsNullOrEmpty(degradedValueText) || string.IsNullOrEmpty(otherValuesText) ||
            string.IsNullOrEmpty(numberOfClustersText) || string.IsNullOrEmpty(clusterSizeText))
        {
            StartCoroutine(ShowMessage("Fields cannot be blank!", Color.red));
            return;
        }

        string normalizedDegradedValueText = degradedValueText.Replace(",", ".");
        string normalizedOtherValuesText = otherValuesText.Replace(",", ".");

        if (!float.TryParse(normalizedDegradedValueText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float degradedValue) ||
            !float.TryParse(normalizedOtherValuesText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float otherValues))
        {
            StartCoroutine(ShowMessage("All fields must contain valid float values!", Color.red));
            return;
        }

        if (!int.TryParse(numberOfClustersText, out int numberOfClusters) ||
            !int.TryParse(clusterSizeText, out int clusterSize) ||
            numberOfClusters <= 0 || clusterSize <= 0)
        {
            StartCoroutine(ShowMessage("Number of clusters and cluster size must be positive integers!", Color.red));
            return;
        }

        if (degradedValue < 1.0f || degradedValue > 2.0f || otherValues < 1.0f || otherValues > 2.0f)
        {
            StartCoroutine(ShowMessage("Values must be in the range 1.0 to 2.0!", Color.red));
            return;
        }
        StartCoroutine(ShowMessage("Successfully generated random grid", Color.white));

        GenerateRandomDataButton.GetComponent<Image>().color = successColor;
        loadCsvButton.GetComponent<Image>().color = defaultColor;

        csvValueFlag = true;
        CsvLoader.GenerateRandomData(numberOfClusters, clusterSize, degradedValue, otherValues);

        if (RandomDataInputPanel != null)
            RandomDataInputPanel.SetActive(false);
    }

    public Button ConfirmRandomData;
    public GameObject RandomDataInputPanel;

    public void ShowRandomDataInputPanel()
    {
        if (RandomDataInputPanel != null)
        {
            RandomDataInputPanel.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
