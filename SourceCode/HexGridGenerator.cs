using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    public GameObject hexPrefab;
    public Vector3 originPosition = Vector3.zero;

    private int sizeX = 40;
    private int sizeY = 40;

    private float hexWidth = 0.63f*0.75f*2f;  
    private float hexHeight = 0.726f/1.75f*2f;

    void Start()
    {
        Debug.Log($"HEXGRIDGENERATOR x: {sizeX}, y: {sizeY}");
        Debug.Log($"Hex Width: {hexWidth}, Hex Height: {hexHeight}");

        AdjustOriginPosition();

        GenerateGrid();
    }

    void AdjustOriginPosition()
    {
        float totalWidth = (sizeX - 1) * hexWidth;
        float totalHeight = (sizeY - 1) * hexHeight; 

        originPosition = new Vector3(-totalWidth / 2, -(totalHeight / 2 + 0.5f), 0);
    }

    void GenerateGrid()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++) 
            {
                Vector3 position = new Vector3(x*hexWidth, y*hexHeight, 0) +
                ((y % 2) == 1 ? new Vector3(hexWidth/2f, 0, 0) : Vector3.zero) + originPosition;

                GameObject hex = Instantiate(hexPrefab, position, Quaternion.identity);
                hex.transform.parent = this.transform;
                hex.name = $"Hex_{x}_{y}";
            }
        }
    }
}
