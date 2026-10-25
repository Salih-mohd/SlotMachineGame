
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetSelectionUI : MonoBehaviour
{
    [Header("Bet UI References")]
    [SerializeField] private Button betButton;
    [SerializeField] private GameObject betSelectionPanel;
    [SerializeField] private TMP_Text betValueText;

    [Header("Bet Option Buttons")]
    [SerializeField] private Button bet10Button;
    [SerializeField] private Button bet50Button;
    [SerializeField] private Button bet100Button;

    [Header("System References")]
    [SerializeField] private WalletManager walletManager;
    [SerializeField] private SlotMachineController slotMachineController;

    // Available bet values.
    private const int DefaultBet = 10;
    private const int MediumBet = 50;
    private const int HighBet = 100;

    private int selectedBet = DefaultBet;

    // Allows other systems to read the selected bet.
    public int SelectedBet => selectedBet;

    // Notifies listeners when the player changes the bet.
    public event Action<int> OnBetChanged;

    private void Awake()
    {
        // Register button callbacks once.
        betButton.onClick.AddListener(ToggleBetPanel);

        bet10Button.onClick.AddListener(() => SelectBet(DefaultBet));
        bet50Button.onClick.AddListener(() => SelectBet(MediumBet));
        bet100Button.onClick.AddListener(() => SelectBet(HighBet));

        // Initially hide the dropdown.
        betSelectionPanel.SetActive(false);

        UpdateBetText();
    }

    private void OnEnable()
    {


        slotMachineController.OnSpinStarted += HandleSpinStarted;

        walletManager.OnBalanceChanged += HandleBalanceChanged;
        slotMachineController.OnSpinCompleted += HandleSpinCompleted;

        OnBetChanged += slotMachineController.HandleBetChanged;


        RefreshBetButtons();
    }

    private void OnDisable()
    {


        slotMachineController.OnSpinStarted -= HandleSpinStarted;

        walletManager.OnBalanceChanged -= HandleBalanceChanged;
        slotMachineController.OnSpinCompleted -= HandleSpinCompleted;

        OnBetChanged -= slotMachineController.HandleBetChanged;

    }

    private void OnDestroy()
    {
        betButton.onClick.RemoveListener(ToggleBetPanel);
        bet10Button.onClick.RemoveAllListeners();
        bet50Button.onClick.RemoveAllListeners();
        bet100Button.onClick.RemoveAllListeners();
    }

    // Opens or closes the selection panel.
    private void ToggleBetPanel()
    {
        if (slotMachineController.IsSpinInProgress)
            return;

        bool shouldOpen = !betSelectionPanel.activeSelf;

        if (shouldOpen)
            RefreshBetButtons();

        betSelectionPanel.SetActive(shouldOpen);
    }

    // Changes the selected bet when allowed.
    private void SelectBet(int amount)
    {
        if (slotMachineController.IsSpinInProgress)
            return;

        if (!walletManager.CanAffordBet(amount))
            return;

        selectedBet = amount;

        UpdateBetText();

        betSelectionPanel.SetActive(false);

        OnBetChanged?.Invoke(selectedBet);

        Debug.Log($"Selected bet: {selectedBet}G");
    }

    private void UpdateBetText()
    {
        betValueText.text = $"{selectedBet}G";
    }

    // Updates button availability based on credits.
    private void RefreshBetButtons()
    {
        bool isSpinning = slotMachineController.IsSpinInProgress;

        betButton.interactable = !isSpinning;

        bet10Button.interactable =
            !isSpinning && walletManager.CanAffordBet(DefaultBet);

        bet50Button.interactable =
            !isSpinning && walletManager.CanAffordBet(MediumBet);

        bet100Button.interactable =
            !isSpinning && walletManager.CanAffordBet(HighBet);
    }

    private void HandleBalanceChanged(int newBalance)
    {
        RefreshBetButtons();
    }

    private void HandleSpinCompleted()
    {
        RefreshBetButtons();
    }

    // Called by the game controller when spinning begins.
    public void HandleSpinStarted()
    {
        betSelectionPanel.SetActive(false);
        RefreshBetButtons();
    }
}
