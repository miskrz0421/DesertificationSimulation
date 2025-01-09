using System;
using System.IO;
using UnityEngine;

public static class CsvLoader
{
    private static int sizeX = 40;
    private static int sizeY = 40;
    private static float[,] dataGrid = new float[sizeX, sizeY]; 

    public static void LoadCsv(string fileName)
    {
        Debug.Log($"LOADDATAFROMCSV x: {sizeX}, y: {sizeY}");
        for (int i = 0; i < dataGrid.GetLength(0); i++)
        {
            for (int j = 0; j < dataGrid.GetLength(1); j++)
            {
                dataGrid[i, j] = 1.4f;
            }
        }
        try
        {
            TextAsset csvFile = Resources.Load<TextAsset>(fileName);
            string[] lines = File.ReadAllLines(fileName);

            for (int i = 1; i < lines.Length; i++) 
            {
                string[] fields = lines[i].Split(',');

                int x = int.Parse(fields[0]);
                int y = int.Parse(fields[1]);
                float vc = ParseFloat(fields[2]);
                float dr = ParseFloat(fields[3]);
                float ep = ParseFloat(fields[4]);
                float r = ParseFloat(fields[5]);
                float a = ParseFloat(fields[6]);
                float t = ParseFloat(fields[7]);
                float se = ParseFloat(fields[8]);
                float t_x = ParseFloat(fields[9]);
                float d = ParseFloat(fields[10]);
                float s_d = ParseFloat(fields[11]);
                float cr = ParseFloat(fields[12]);
                float pa = ParseFloat(fields[13]);

                float v_xy = Mathf.Pow(vc * dr * ep, 1f / 3f);
                float c_xy = Mathf.Pow(r * a * t * se, 1f / 4f);
                float s_xy = Mathf.Pow(t_x * d * s_d, 1f / 3f);
                float m_xy = Mathf.Pow(cr * pa, 1f / 2f);
                float value = Mathf.Pow(v_xy * c_xy * s_xy * m_xy, 1f / 4f);

                if (x >= 0 && x < sizeX && y >= 0 && y < sizeY)
                {
                    dataGrid[x, y] = value;
                }
            }

            Debug.Log($"Dane wczytane poprawnie. {dataGrid[0, 0]:F2}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Błąd podczas wczytywania pliku CSV: {e.Message}");
        }
    }

    public static void GenerateRandomData(int numberOfClusters, int clusterSize, float degradedValue, float otherValues)
    {
        for (int i = 0; i < 40; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                dataGrid[i, j] = otherValues;
            }
        }

        for (int cluster = 0; cluster < numberOfClusters; cluster++) 
        {
            bool placed = false;
            int attempts = 0; 

            while (!placed && attempts < 100)
            {
                attempts++;
                
                int centerX = UnityEngine.Random.Range(0, 40); 
                int centerY = UnityEngine.Random.Range(0, 40); 

                bool overlaps = false;

                for (int x = 0; x < 40; x++) 
                {
                    for (int y = 0; y < 40; y++)
                    {
                        float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                        if (distance <= clusterSize) 
                        {
                            if (dataGrid[x, y] == degradedValue) 
                            {
                                overlaps = true;
                                break;
                            }
                        }
                    }
                    if (overlaps) break;
                }

                if (!overlaps)
                {
                    for (int x = 0; x < 40; x++)
                    {
                        for (int y = 0; y < 40; y++)
                        {
                            float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                            if (distance <= clusterSize) 
                            {
                                dataGrid[x, y] = degradedValue; 
                            }
                        }
                    }
                    placed = true; 
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"Unable to place cluster {cluster + 1} after 100 attempts.");
            }

        }
    }
    public static float[,] GetDataGrid()
    {
        return dataGrid;
    }

    public static int getSizeX()
    {
        return sizeX;
    }

    public static int getSizeY()
    {
        return sizeY;
    }

    private static float ParseFloat(string value)
    {
        value = value.Replace(',', '.');
        return float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
    }
}
