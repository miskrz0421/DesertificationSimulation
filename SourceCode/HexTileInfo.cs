using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class HexTileInfo : MonoBehaviour
{
    private int sizeX = 40;
    private int sizeY = 40;
    private float dsiValue = 1.0f; 
    public float originalDSI = 0.0f; 
    public GameObject tooltipPrefab; 
    private GameObject currentTooltip;

    private GameObject hoverLayer; 
    public Sprite hoverSprite;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public Button HoverSwitchButton;
    public TextMeshProUGUI HoverSwitchMessageText;
    private static bool hoverSwitchFlag = false;

    private int gridX;
    private int gridY; 

    void Start()
    {
        hoverSwitchFlag = false;

        if (tooltipPrefab != null)
        {
            tooltipPrefab.SetActive(false);
        }

        if (HoverSwitchButton != null)
        {
            HoverSwitchButton.onClick.AddListener(HoverSwitch);
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();

        ParseGridCoordinatesFromName();

        float[,] dataGrid = CsvLoader.GetDataGrid();
        if (gridX >= 0 && gridX < sizeX && gridY >= 0 && gridY < sizeY)
        {
            originalDSI = dataGrid[gridX, gridY];
            dsiValue = originalDSI;
        }
        else
        {
            Debug.LogWarning($"Nieprawidłowe indeksy gridX={gridX}, gridY={gridY}");
        }

        if (spriteRenderer != null)
        {
            Color dsiColor = GetColorBasedOnDSI(originalDSI);
            spriteRenderer.color = dsiColor;
            originalColor = spriteRenderer.color;
        }

        hoverLayer = new GameObject("HoverLayer");
        hoverLayer.transform.parent = transform;
        hoverLayer.transform.localPosition = Vector3.zero; 
        hoverLayer.transform.localScale = Vector3.one; 

        SpriteRenderer hoverSpriteRenderer = hoverLayer.AddComponent<SpriteRenderer>();
        hoverSpriteRenderer.sprite = spriteRenderer.sprite;
        hoverSpriteRenderer.color = new Color(0, 0, 0, 0.3f); 
        hoverSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; 

        hoverLayer.SetActive(false);
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

    void ParseGridCoordinatesFromName()
    {
        string[] parts = gameObject.name.Split('_');
        if (parts.Length == 3 && parts[0] == "Hex")
        {
            if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
            {
                gridX = x;
                gridY = y;
            }
            else
            {
                Debug.LogError($"HexTileInfo: Nie udało się sparsować współrzędnych z nazwy {gameObject.name}");
            }
        }
        else
        {
            Debug.LogError($"HexTileInfo: Nieprawidłowy format nazwy obiektu: {gameObject.name}");
        }
    }

    void OnMouseEnter()
    {
        if(hoverSwitchFlag == true)
        {
            ShowTooltip();
            ChangeSprite();
        }
        
    }

    void OnMouseExit()
    {
        HideTooltip();
        RestoreSprite();        
    }

    void ShowTooltip()
    {
        if (tooltipPrefab == null)
        {
            Debug.LogWarning("Tooltip prefab is not assigned!");
            return;
        }
        tooltipPrefab.SetActive(true);

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return;
        }

        currentTooltip = Instantiate(tooltipPrefab, canvas.transform);
        currentTooltip.transform.SetAsLastSibling(); 
        float zoomFactor = Camera.main.orthographicSize;
        float scaleFactor = Mathf.Clamp(5.0f / zoomFactor, 0.5f, 1.5f);

        currentTooltip.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

        TextMeshProUGUI[] textComponents = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>();
        if (textComponents != null && textComponents.Length >= 2)
        {
            textComponents[0].text = $"Current DSI: {dsiValue:F2}"; 
            textComponents[1].text = $"Original DSI: {originalDSI:F2}";
        }
        else
        {
            Debug.LogError("Tooltip prefab is missing TextMeshProUGUI components!");
        }

        Vector3 worldPosition = this.transform.position + new Vector3(0, 1.25f, 0); 
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransform tooltipRect = currentTooltip.GetComponent<RectTransform>();
        if (tooltipRect != null)
        {
            tooltipRect.position = screenPosition;
        }
    }

    void HideTooltip()
    {
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
        }
    }

    void ChangeSprite()
    {
        if (hoverLayer != null)
        {
            hoverLayer.SetActive(true); 
        }
    }

    void RestoreSprite()
    {
        if (hoverLayer != null)
        {
            hoverLayer.SetActive(false); 
        }
    }

    public void UpdateDSI(float newDSI)
    {
        dsiValue = newDSI;

        if (spriteRenderer != null)
        {
            originalColor = GetColorBasedOnDSI(dsiValue);
            spriteRenderer.color = originalColor;
        }
    }

    public void HoverSwitch()
    {
        hoverSwitchFlag = !hoverSwitchFlag;
        if (hoverSwitchFlag == true)
        {
            Debug.Log("Hover turned on");

            if (HoverSwitchMessageText != null)
            {
                HoverSwitchMessageText.text = "Hover: ON";
            }
        }
        else
        {
            Debug.Log("Hover turned off");

            if (HoverSwitchMessageText != null)
            {
                HoverSwitchMessageText.text = "Hover: OFF"; 
            }
        }
    }
}