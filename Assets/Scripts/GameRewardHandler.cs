using Adinmo;
using UnityEngine;

public class GameRewardHandler : MonoBehaviour
{
    private void OnEnable()
    {
        AdinmoManager.OnGiveReward += HandleRewardedAdCompleted;
    }

    private void OnDisable()
    {
        AdinmoManager.OnGiveReward -= HandleRewardedAdCompleted;
    }

    private void HandleRewardedAdCompleted(int _rewardAmount)
    {
        Debug.Log($"Ad completed! Reward amount: {_rewardAmount}");
        // Implement reward logic here, e.g., add coins to the player's balance
    }
}
