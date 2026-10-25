
using System;
using UnityEngine;

public class WalletManager : MonoBehaviour
{
    [Header("Wallet Settings")]
    [SerializeField, Min(0)] private int startingBalance = 500;

    private int currentBalance;

    // Notifies subscribers whenever the credit balance changes.
    // Sends the updated balance as an integer.
    public event Action<int> OnBalanceChanged;

    // Other systems can read the balance but cannot change it directly.
    public int CurrentBalance => currentBalance;

    private void Awake()
    {
        // Initialize the wallet when the game starts.
        currentBalance = startingBalance;
    }

    // Checks whether the player has enough credits.
    public bool CanAffordBet(int betAmount)
    {
        return betAmount > 0 && currentBalance >= betAmount;
    }

    // Attempts to deduct the bet.
    // Returns false if the player cannot afford it.
    public bool TryPlaceBet(int betAmount)
    {
        if (!CanAffordBet(betAmount))
        {
            Debug.LogWarning("Insufficient credits or invalid bet.");
            return false;
        }

        currentBalance -= betAmount;

        // Notify UI and other subscribers.
        OnBalanceChanged?.Invoke(currentBalance);

        Debug.Log($"Bet placed: {betAmount}G | Balance: {currentBalance}G");

        return true;
    }

    // Adds the winning reward to the player's balance.
    public void AddReward(int rewardAmount)
    {
        if (rewardAmount <= 0)
            return;

        // Prevent integer overflow for unusually large payouts.
        if (rewardAmount > int.MaxValue - currentBalance)
        {
            Debug.LogError("Reward exceeds the wallet's maximum balance.");
            return;
        }

        currentBalance += rewardAmount;

        // Notify UI and other subscribers.
        OnBalanceChanged?.Invoke(currentBalance);

        Debug.Log($"Reward: +{rewardAmount}G | Balance: {currentBalance}G");
    }
}
