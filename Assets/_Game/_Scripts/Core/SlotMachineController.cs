
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineController : MonoBehaviour
{
    [Header("Reel References")]
    [SerializeField] private ReelController[] reels;

    [Header("Game Systems")]
    [SerializeField] private SlotOutcomeGenerator outcomeGenerator;
    [SerializeField] private WinEvaluator winEvaluator;
    [SerializeField] private PayoutCalculator payoutCalculator;
    [SerializeField] private WalletManager walletManager;
    [SerializeField] private BetSelectionUI betSelectionUI;

    [Header("UI References")]
    [SerializeField] private Button spinButton;

    [Header("Spin Timing")]
    [SerializeField] private float initialSpinDuration = 2f;
    [SerializeField] private float reelStopDelay = 0.5f;

    private bool isSpinInProgress;
    private int stoppedReelCount;

    private SymbolData[] currentOutcome;

    // Store the bet used for the current spin.
    // Prevents changes from affecting payout calculations.
    private int currentSpinBet;

    public bool IsSpinInProgress => isSpinInProgress;

    public event Action OnSpinStarted;
    public event Action OnSpinCompleted;

    private void OnEnable()
    {
        if (reels != null)
        {
            foreach (ReelController reel in reels)
            {
                if (reel != null)
                    reel.OnReelStopped += HandleReelStopped;
            }
        }

        if (walletManager != null)
            walletManager.OnBalanceChanged += HandleBalanceChanged;
    }

    private void Start()
    {
        RefreshSpinButton();
    }

    private void OnDisable()
    {
        if (reels != null)
        {
            foreach (ReelController reel in reels)
            {
                if (reel != null)
                    reel.OnReelStopped -= HandleReelStopped;
            }
        }

        if (walletManager != null)
            walletManager.OnBalanceChanged -= HandleBalanceChanged;
    }

    // Called by the SPIN button.
    public void StartSpin()
    {
        if (isSpinInProgress || !ValidateSetup())
            return;

        int selectedBet = betSelectionUI.SelectedBet;

        // Prevent spinning without sufficient credits.
        if (!walletManager.CanAffordBet(selectedBet))
        {
            Debug.LogWarning("Not enough credits to spin.");
            RefreshSpinButton();
            return;
        }

        StartCoroutine(SpinSequence(selectedBet));
    }

    private IEnumerator SpinSequence(int betAmount)
    {
        isSpinInProgress = true;
        stoppedReelCount = 0;
        currentSpinBet = betAmount;

        // Disable spinning and notify UI systems.
        RefreshSpinButton();
        OnSpinStarted?.Invoke();

        // Generate the result before reel animation.
        currentOutcome = outcomeGenerator.GenerateOutcome(reels.Length);

        if (currentOutcome == null ||
            currentOutcome.Length != reels.Length)
        {
            Debug.LogError("Failed to generate outcome.");
            FinishSpin();
            yield break;
        }

        for (int i = 0; i < reels.Length; i++)
        {
            if (currentOutcome[i] == null)
            {
                Debug.LogError("Generated outcome contains a null symbol.");
                FinishSpin();
                yield break;
            }
        }

        // Deduct credits only after generating a valid outcome.
        if (!walletManager.TryPlaceBet(currentSpinBet))
        {
            FinishSpin();
            yield break;
        }

        // Assign the results and start all three reels.
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].SetTargetSymbol(currentOutcome[i]);
            reels[i].StartSpin();
        }

        // Convert the generated symbols into a readable string.
        string outcomeText = string.Join(", ",
            Array.ConvertAll(currentOutcome, symbol => symbol.SymbolId));

        // Log the selected bet and generated outcome.
        Debug.Log($"Bet: {currentSpinBet}G | Outcome: {outcomeText}");

        // Keep all reels spinning initially.
        yield return new WaitForSeconds(initialSpinDuration);

        // Stop the reels sequentially.
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].StopSpin();

            if (i < reels.Length - 1)
                yield return new WaitForSeconds(reelStopDelay);
        }

        // Wait for all reels to stop.
        yield return new WaitUntil(
            () => stoppedReelCount == reels.Length
        );

        // Evaluate the center-row combination.
        EvaluateSpinResult();

        FinishSpin();
    }

    private void EvaluateSpinResult()
    {
        bool hasWon = winEvaluator.EvaluateMatch(
            currentOutcome,
            out SymbolData winningSymbol
        );

        if (!hasWon)
        {
            Debug.Log("No winning combination.");
            return;
        }

        int payout = payoutCalculator.CalculatePayout(
            winningSymbol,
            currentSpinBet
        );

        if (payout > 0)
        {
            walletManager.AddReward(payout);

            Debug.Log(
                $"WIN! {winningSymbol.SymbolId} x3 | " +
                $"Payout: {payout}G"
            );
        }
    }

    private void FinishSpin()
    {
        isSpinInProgress = false;

        RefreshSpinButton();

        OnSpinCompleted?.Invoke();

        Debug.Log("Spin completed.");
    }

    private void HandleReelStopped(ReelController reel)
    {
        stoppedReelCount++;
    }

    private void HandleBalanceChanged(int newBalance)
    {
        RefreshSpinButton();
    }

    // Called when the player selects a different bet.
    public void HandleBetChanged(int newBet)
    {
        RefreshSpinButton();
    }

    private void RefreshSpinButton()
    {
        if (spinButton == null || walletManager == null ||
            betSelectionUI == null)
            return;

        spinButton.interactable =
            !isSpinInProgress &&
            walletManager.CanAffordBet(betSelectionUI.SelectedBet);
    }

    private bool ValidateSetup()
    {
        if (reels == null || reels.Length != 3 ||
            outcomeGenerator == null ||
            winEvaluator == null ||
            payoutCalculator == null ||
            walletManager == null ||
            betSelectionUI == null ||
            spinButton == null)
        {
            Debug.LogError("Slot machine references are incomplete.");
            return false;
        }

        foreach (ReelController reel in reels)
        {
            if (reel == null || reel.IsSpinning)
                return false;
        }

        return true;
    }
}
