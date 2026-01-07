using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    [Header("Economy Settings")]
    public string coinPlayerPrefKey = "PlayerCoins";
    public int startingCoins = 10;
    public int costPerCrop = 1;

    [Header("Spawning Settings")]
    public GameObject cropPrefab;
    public Transform planeTarget;
    [Tooltip("Space between crops")]
    public float padding = 0.1f;

    // Internal State
    private int currentCoins;
    private int cropsSpawnedCount = 0;
    
    // Cached dimensions
    private Vector3 startPosition;
    private float cropWidth;
    private float cropLength;
    private float planeWidth;
    private float planeLength;
    private int maxCropsPerRow;
    private Vector3 targetScale; // To store the plane's scale

    void Start()
    {
        // 1. Load Coins
        if (!PlayerPrefs.HasKey(coinPlayerPrefKey))
        {
            PlayerPrefs.SetInt(coinPlayerPrefKey, startingCoins);
        }
        currentCoins = PlayerPrefs.GetInt(coinPlayerPrefKey);

        // 2. Analyze Sizes
        CalculateGridDimensions();
    }

    void CalculateGridDimensions()
    {
        Renderer planeRenderer = planeTarget.GetComponent<Renderer>();
        Renderer cropRenderer = cropPrefab.GetComponent<Renderer>();

        if (planeRenderer == null || cropRenderer == null)
        {
            Debug.LogError("Plane or Crop Prefab is missing a Renderer!");
            return;
        }

        // --- NEW: Capture Plane Scale ---
        targetScale = planeTarget.localScale;

        // Plane limits (Renderer.bounds handles world size automatically)
        Bounds planeBounds = planeRenderer.bounds;
        planeWidth = planeBounds.size.x;
        planeLength = planeBounds.size.z;

        // --- UPDATED: Calculate Crop Size accounting for Scale ---
        // We get the original size of the mesh (bounds / localScale) 
        // then multiply by the NEW target scale to find out how big it WILL be.
        // (This assumes the prefab is uniformly scaled. If the prefab in the project is (1,1,1), this is perfect.)
        Vector3 originalMeshSize = cropRenderer.bounds.size;
        
        // If the prefab has a scale other than 1, we normalize it first
        if (cropPrefab.transform.localScale.x != 0) 
             originalMeshSize.x /= cropPrefab.transform.localScale.x;
        if (cropPrefab.transform.localScale.z != 0) 
             originalMeshSize.z /= cropPrefab.transform.localScale.z;

        // Now calculate the spacing required for the new scaled crop
        cropWidth = (originalMeshSize.x * targetScale.x) + padding;
        cropLength = (originalMeshSize.z * targetScale.z) + padding;

        // Calculate Start Position
        startPosition = planeTarget.position 
                        - new Vector3(planeWidth / 2, 0, planeLength / 2) 
                        + new Vector3(cropWidth / 2, 0, cropLength / 2);

        maxCropsPerRow = Mathf.FloorToInt(planeWidth / cropWidth);
    }

    public void TryBuyAndSpawnCrop()
    {
        currentCoins = PlayerPrefs.GetInt(coinPlayerPrefKey);

        if (currentCoins >= costPerCrop)
        {
            currentCoins -= costPerCrop;
            PlayerPrefs.SetInt(coinPlayerPrefKey, currentCoins);
            PlayerPrefs.Save();

            SpawnCrop();
            Debug.Log($"Crop Planted! Coins remaining: {currentCoins}");
        }
        else
        {
            Debug.Log($"Not enough coins! Need {costPerCrop}");
        }
    }

    void SpawnCrop()
    {
        int row = cropsSpawnedCount / maxCropsPerRow;
        int col = cropsSpawnedCount % maxCropsPerRow;

        Vector3 spawnPos = startPosition + new Vector3(col * cropWidth, 0, row * cropLength);
        spawnPos.y = planeTarget.position.y;

        // Instantiate
        GameObject newCrop = Instantiate(cropPrefab, spawnPos, Quaternion.identity);

        // --- NEW: Apply Scale ---
        // Force the new crop to match the plane's scale
        newCrop.transform.localScale = targetScale;

        cropsSpawnedCount++;
    }

    [ContextMenu("Reset Coins")]
    public void ResetCoins()
    {
        PlayerPrefs.SetInt(coinPlayerPrefKey, startingCoins);
        currentCoins = startingCoins;
        Debug.Log("Coins Reset.");
    }
}