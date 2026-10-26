
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

    [Header("Audio")]
    [SerializeField] private SlotAudioController slotAudioController;

    [Header("UI References")]
    [SerializeField] private Button spinButton;

    [Header("Spin Timing")]
    [SerializeField, Min(0f)]
    private float initialSpinDuration = 2f;

    [SerializeField, Min(0f)]
    private float reelStopDelay = 0.5f;

    [Header("Visual Effects")]
    [SerializeField] private SlotEffectsController slotEffectsController;

    [Header("Shared Reel Animation Timing")]
    [SerializeField, Min(0f)]
    private float accelerationDuration = 0.35f;

    [SerializeField, Min(0.1f)]
    private float decelerationDuration = 1.5f;

    [Header("Shared Reel Landing Bounce")]
    [SerializeField] private bool enableLandingBounce = true;

    [SerializeField, Min(0f)]
    private float bounceDistance = 12f;

    [SerializeField, Min(0.01f)]
    private float bounceDuration = 0.2f;

    private bool isSpinInProgress;
    private int stoppedReelCount;

    private SymbolData[] currentOutcome;

    // Lock the bet amount for the current spin.
    private int currentSpinBet;

    public bool IsSpinInProgress => isSpinInProgress;

    public event Action OnSpinStarted;
    public event Action OnSpinCompleted;

    private void OnEnable()
    {
        // Listen for individual reel stop events.
        if (reels != null)
        {
            foreach (ReelController reel in reels)
            {
                if (reel != null)
                    reel.OnReelStopped += HandleReelStopped;
            }
        }

        // Update SPIN button when credits change.
        if (walletManager != null)
        {
            walletManager.OnBalanceChanged += HandleBalanceChanged;
        }
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
        {
            walletManager.OnBalanceChanged -= HandleBalanceChanged;
        }

        // Prevent a looping reel sound from continuing
        // if the controller is disabled.
        slotAudioController?.StopReelLoop();
    }

    // Called by the SPIN button.
    public void StartSpin()
    {
        if (isSpinInProgress || !ValidateSetup())
            return;

        int selectedBet = betSelectionUI.SelectedBet;

        // Reject the spin if credits are insufficient.
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

        RefreshSpinButton();
        OnSpinStarted?.Invoke();

        // Generate the outcome before spinning.
        currentOutcome = outcomeGenerator.GenerateOutcome(
            reels.Length
        );

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
                Debug.LogError(
                    "Generated outcome contains a null symbol."
                );

                FinishSpin();
                yield break;
            }
        }

        // Deduct credits only after obtaining a valid outcome.
        if (!walletManager.TryPlaceBet(currentSpinBet))
        {
            FinishSpin();
            yield break;
        }

        // Configure all reels once before they start.
        foreach (ReelController reel in reels)
        {
            reel.ConfigureMotion(
                accelerationDuration,
                decelerationDuration
            );

            reel.ConfigureBounce(
                enableLandingBounce,
                bounceDistance,
                bounceDuration
            );
        }

        // Assign outcomes and start all three reels.
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].SetTargetSymbol(currentOutcome[i]);
            reels[i].StartSpin();
        }

        // AUDIO: Play spin-start and begin looping reel sound.
        slotAudioController?.PlaySpinStart();
        slotAudioController?.StartReelLoop();

        string outcomeText = string.Join(
            ", ",
            Array.ConvertAll(
                currentOutcome,
                symbol => symbol.SymbolId
            )
        );

        Debug.Log(
            $"Bet: {currentSpinBet}G | Outcome: {outcomeText}"
        );

        // Let all three reels spin initially.
        yield return new WaitForSeconds(initialSpinDuration);

        // Request reel stops sequentially.
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].StopSpin();

            if (i < reels.Length - 1)
            {
                yield return new WaitForSeconds(reelStopDelay);
            }
        }

        // Wait until all reels have finished stopping
        // and completed their landing bounce.
        yield return new WaitUntil(
            () => stoppedReelCount == reels.Length
        );

        // AUDIO: All reels have finished.
        slotAudioController?.StopReelLoop();

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
            // Award credits and trigger existing UI animation.
            walletManager.AddReward(payout);

            // Play the existing flying WIN text effects.
            slotEffectsController?.PlayWinEffect();

            // AUDIO: Play winning sound.
            slotAudioController?.PlayWin();

            Debug.Log(
                $"WIN! {winningSymbol.SymbolId} x3 | " +
                $"Payout: {payout}G"
            );
        }
    }

    private void FinishSpin()
    {
        // Safety: stop reel audio even if the spin ends early.
        slotAudioController?.StopReelLoop();

        isSpinInProgress = false;

        RefreshSpinButton();

        OnSpinCompleted?.Invoke();

        Debug.Log("Spin completed.");
    }

    private void HandleReelStopped(ReelController reel)
    {
        stoppedReelCount++;

        // AUDIO: Each reel plays a sound after its landing bounce.
        slotAudioController?.PlayReelStop();
    }

    private void HandleBalanceChanged(int newBalance)
    {
        RefreshSpinButton();
    }

    // Called when the player selects another bet.
    public void HandleBetChanged(int newBet)
    {
        RefreshSpinButton();
    }

    private void RefreshSpinButton()
    {
        if (spinButton == null ||
            walletManager == null ||
            betSelectionUI == null)
        {
            return;
        }

        spinButton.interactable =
            !isSpinInProgress &&
            walletManager.CanAffordBet(
                betSelectionUI.SelectedBet
            );
    }

    private bool ValidateSetup()
    {
        if (reels == null ||
            reels.Length != 3 ||
            outcomeGenerator == null ||
            winEvaluator == null ||
            payoutCalculator == null ||
            walletManager == null ||
            betSelectionUI == null ||
            spinButton == null)
        {
            Debug.LogError(
                "Slot machine references are incomplete."
            );

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
