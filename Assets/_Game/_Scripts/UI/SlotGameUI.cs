
using TMPro;
using UnityEngine;

public class SlotGameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WalletManager walletManager;
    [SerializeField] private RectTransform creditPanel;
    [SerializeField] private TMP_Text creditValueText;

    [Header("Credit Animation")]
    [SerializeField, Min(0.1f)] private float countDuration = 1.5f;

    [Header("Reward Shake")]
    [SerializeField] private float shakeAmount = 5f;
    [SerializeField, Min(0.05f)] private float shakeCycleDuration = 0.15f;

    private int displayedBalance;
    private int lastActualBalance;

    private int countTweenId = -1;
    private int shakeTweenId = -1;

    private Quaternion originalPanelRotation;
    private float originalZRotation;

    private bool isRewardAnimationActive;

    private void Awake()
    {
        // Cache the panel's original rotation.
        originalPanelRotation = creditPanel.localRotation;
        originalZRotation = creditPanel.localEulerAngles.z;
    }

    private void Start()
    {
        // WalletManager has initialized its balance in Awake.
        displayedBalance = walletManager.CurrentBalance;
        lastActualBalance = displayedBalance;

        UpdateCreditText(displayedBalance);
    }

    private void OnEnable()
    {
        walletManager.OnBalanceChanged += HandleBalanceChanged;
    }

    private void OnDisable()
    {
        walletManager.OnBalanceChanged -= HandleBalanceChanged;
        CancelAnimations();
    }

    private void HandleBalanceChanged(int newBalance)
    {
        // Compare against the previous actual wallet balance.
        bool isReward = newBalance > lastActualBalance;

        lastActualBalance = newBalance;

        // Stop any previous animation before starting a new one.
        CancelAnimations();

        if (isReward)
        {
            AnimateCredit(newBalance, true);
        }
        else
        {
            AnimateCredit(newBalance, false);
        }
    }

    private void AnimateCredit(int targetBalance, bool shake)
    {
        int startingBalance = displayedBalance;

        // Start the counting animation for both wins and bets.
        LTDescr countTween = LeanTween.value(
            gameObject,
            (float)startingBalance,
            (float)targetBalance,
            countDuration
        );

        countTween.setEase(LeanTweenType.easeOutCubic);

        countTween.setOnUpdate((float value) =>
        {
            displayedBalance = Mathf.RoundToInt(value);
            UpdateCreditText(displayedBalance);
        });

        countTween.setOnComplete(() =>
        {
            displayedBalance = targetBalance;
            UpdateCreditText(displayedBalance);

            countTweenId = -1;

            // Stop shaking only when the credit reaches its target.
            StopShake();
        });

        countTweenId = countTween.id;

        // Shake only when a reward is received.
        if (shake)
        {
            isRewardAnimationActive = true;
            StartShake();
        }
    }

    // Repeatedly rotate the panel left and right.
    private void StartShake()
    {
        if (!isRewardAnimationActive)
            return;

        LTDescr shakeTween = LeanTween.value(
            gameObject,
            -shakeAmount,
            shakeAmount,
            shakeCycleDuration
        );

        shakeTween.setEase(LeanTweenType.easeInOutSine);

        shakeTween.setOnUpdate((float angle) =>
        {
            creditPanel.localRotation = Quaternion.Euler(
                0f,
                0f,
                originalZRotation + angle
            );
        });

        shakeTween.setOnComplete(() =>
        {
            shakeTweenId = -1;

            // Repeat until the credit counting animation finishes.
            if (isRewardAnimationActive)
                StartShake();
        });

        shakeTweenId = shakeTween.id;
    }

    private void StopShake()
    {
        isRewardAnimationActive = false;

        if (shakeTweenId >= 0)
        {
            LeanTween.cancel(shakeTweenId);
            shakeTweenId = -1;
        }

        creditPanel.localRotation = originalPanelRotation;
    }

    private void UpdateCreditText(int amount)
    {
        creditValueText.text = $"{amount}G";
    }

    private void CancelAnimations()
    {
        // Stop counting.
        if (countTweenId >= 0)
        {
            LeanTween.cancel(countTweenId);
            countTweenId = -1;
        }

        // Stop shaking and restore rotation.
        StopShake();
    }
}
