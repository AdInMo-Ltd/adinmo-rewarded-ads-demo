using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Standard Unity Text namespace

public class CoinUI : MonoBehaviour
{
    [Header("Settings")]
    public TextMeshProUGUI coinTextLabel;
    public string coinPlayerPrefKey = "PlayerCoins"; // Must match your other scripts
    public string prefix = "Coins: ";

    // Internal tracker to prevent redrawing the text if nothing changed
    private int displayedCoinCount = -1;

    void Update()
    {
        // 1. Get the current actual value
        int currentCoins = PlayerPrefs.GetInt(coinPlayerPrefKey, 0);

        // 2. Only update the text if the number has actually changed
        // (This saves performance vs setting the string every single frame)
        if (currentCoins != displayedCoinCount)
        {
            displayedCoinCount = currentCoins;
            UpdateText();
        }
    }

    void UpdateText()
    {
        if (coinTextLabel != null)
        {
            coinTextLabel.text = prefix + displayedCoinCount.ToString();
        }
    }
}