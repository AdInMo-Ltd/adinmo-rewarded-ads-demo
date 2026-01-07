using Adinmo;
using UnityEngine;

public class CoinGranter : MonoBehaviour
{
    [Header("Settings")]
    public string coinPlayerPrefKey = "PlayerCoins"; // Must match the other script!
    public int amountToGrant = 5;

    private void OnEnable()
    {
        AdinmoManager.OnGiveReward += AddCoins;
    }

    private void OnDisable()
    {
        AdinmoManager.OnGiveReward -= AddCoins;
    }


    /// <summary>
    /// Call this from a Button click to add the default amount
    /// </summary>
    public void GrantCoins()
    {
        AddCoins(amountToGrant);
    }

    /// <summary>
    /// Call this from code if you want to specify a specific amount (e.g. AddCoins(100))
    /// </summary>
    public void AddCoins(int amount)
    {
        // 1. Get current coins (default to 0 if key doesn't exist)
        int currentCoins = PlayerPrefs.GetInt(coinPlayerPrefKey, 0);

        // 2. Add new amount
        currentCoins += amount;

        // 3. Save back to storage
        PlayerPrefs.SetInt(coinPlayerPrefKey, currentCoins);
        PlayerPrefs.Save();

        Debug.Log($"Granted {amount} coins! New Balance: {currentCoins}");
    }

    // This allows you to right-click the script in the Inspector to test it without playing
    [ContextMenu("Test Grant Coins")]
    public void TestGrant()
    {
        GrantCoins();
    }
}