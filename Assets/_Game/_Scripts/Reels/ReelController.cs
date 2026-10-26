
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReelController : MonoBehaviour
{
    [Header("Reel References")]
    [SerializeField] private RectTransform symbolStrip;
    [SerializeField] private Image symbolPrefab;

    [Header("Available Symbols")]
    [SerializeField] private SymbolData[] availableSymbols;

    [Header("Symbol Layout")]
    [SerializeField] private float symbolHeight = 150f;
    [SerializeField] private float symbolSpacing = 0f;

    [Header("Spin Settings")]
    [SerializeField, Min(1f)]
    private float spinSpeed = 600f;

    [SerializeField, Min(0f)]
    private float accelerationDuration = 0.35f;

    [Header("Stop Settings")]
    [SerializeField, Min(0.1f)]
    private float stopDuration = 1.5f;

    [SerializeField, Min(0)]
    private int extraCycles = 1;

    [Header("Landing Bounce")]
    [SerializeField]
    private bool enableLandingBounce = true;

    [SerializeField, Min(0f)]
    private float bounceDistance = 12f;

    [SerializeField, Min(0.01f)]
    private float bounceDuration = 0.2f;

    [Header("Testing Only")]
    [SerializeField] private bool autoTest = false;

    private readonly List<RectTransform> symbolRects = new();

    private SymbolData targetSymbol;

    private bool isSpinning;
    private bool isStopping;

    private float spinStartTime;

    // Store the strip's resting position.
    private Vector2 originalStripPosition;

    public event Action<ReelController> OnReelStopped;

    public bool IsSpinning => isSpinning;
    public SymbolData TargetSymbol => targetSymbol;

    private float SymbolStep => symbolHeight + symbolSpacing;
    private float LoopHeight => symbolRects.Count * SymbolStep;

    private void Start()
    {
        if (symbolStrip != null)
        {
            originalStripPosition = symbolStrip.anchoredPosition;
        }

        GenerateSymbols();

        if (autoTest && availableSymbols.Length > 1)
        {
            SetTargetSymbol(availableSymbols[1]);
            StartSpin();
            Invoke(nameof(StopSpin), 2f);
        }
    }

    private void Update()
    {
        // Normal spinning with smooth acceleration.
        // LeanTween takes control when stopping starts.
        if (isSpinning && !isStopping)
        {
            float currentSpeed = GetCurrentSpinSpeed();

            MoveSymbols(currentSpeed * Time.deltaTime);
        }
    }

    // Configure shared acceleration and deceleration timing.
    public void ConfigureMotion(
        float acceleration,
        float deceleration)
    {
        accelerationDuration = Mathf.Max(0f, acceleration);
        stopDuration = Mathf.Max(0.1f, deceleration);
    }

    // Configure shared landing bounce settings.
    public void ConfigureBounce(
        bool enabled,
        float distance,
        float duration)
    {
        enableLandingBounce = enabled;
        bounceDistance = Mathf.Max(0f, distance);
        bounceDuration = Mathf.Max(0.01f, duration);
    }

    private float GetCurrentSpinSpeed()
    {
        if (accelerationDuration <= 0f)
            return spinSpeed;

        float progress = Mathf.Clamp01(
            (Time.time - spinStartTime) / accelerationDuration
        );

        float easedProgress = progress * progress * progress;

        return spinSpeed * easedProgress;
    }

    private void GenerateSymbols()
    {
        if (symbolStrip == null || symbolPrefab == null ||
            availableSymbols == null || availableSymbols.Length == 0)
        {
            Debug.LogError($"Reel setup incomplete on {name}");
            return;
        }

        symbolRects.Clear();

        for (int i = 0; i < availableSymbols.Length; i++)
        {
            SymbolData symbol = availableSymbols[i];

            if (symbol == null || symbol.SymbolSprite == null)
            {
                Debug.LogError($"Invalid symbol at index {i} on {name}");
                return;
            }

            Image image = Instantiate(symbolPrefab, symbolStrip);

            image.name = $"Symbol_{symbol.SymbolId}";
            image.sprite = symbol.SymbolSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rect = image.rectTransform;

            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.sizeDelta = new Vector2(
                symbolHeight,
                symbolHeight
            );

            float y = -(
                i * SymbolStep + symbolHeight * 0.5f
            );

            rect.anchoredPosition = new Vector2(0f, y);

            symbolRects.Add(rect);
        }

        float totalHeight =
            symbolRects.Count * SymbolStep - symbolSpacing;

        symbolStrip.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            totalHeight
        );
    }

    // Move symbols downward and recycle them.
    private void MoveSymbols(float distance)
    {
        if (symbolRects.Count == 0)
            return;

        float loopHeight = LoopHeight;

        float bottomLimit =
       -(symbolRects.Count - 1) * SymbolStep
       - symbolHeight * 0.5f;

        foreach (RectTransform rect in symbolRects)
        {
            Vector2 position = rect.anchoredPosition;

            position.y -= distance;

            while (position.y < bottomLimit)
            {
                position.y += loopHeight;
            }

            rect.anchoredPosition = position;
        }
    }

    public void StartSpin()
    {
        if (isSpinning || symbolRects.Count == 0)
            return;

        // Make sure the strip starts at its resting position.
        symbolStrip.anchoredPosition = originalStripPosition;

        isSpinning = true;
        isStopping = false;

        spinStartTime = Time.time;
    }

    public void StopSpin()
    {
        if (!isSpinning || isStopping || targetSymbol == null)
            return;

        int targetIndex = Array.IndexOf(
            availableSymbols,
            targetSymbol
        );

        if (targetIndex < 0)
            return;

        // Capture current speed to avoid a sudden jump.
        float startingSpeed = Mathf.Max(
            GetCurrentSpinSpeed(),
            1f
        );

        isStopping = true;

        RectTransform targetRect = symbolRects[targetIndex];
        RectTransform reelRect = (RectTransform)transform;

        Vector3 symbolWorldCenter =
            targetRect.TransformPoint(targetRect.rect.center);

        float currentY =
            reelRect.InverseTransformPoint(symbolWorldCenter).y;

        float desiredY = reelRect.rect.center.y;

        float alignmentDistance = Mathf.Repeat(
            currentY - desiredY,
            LoopHeight
        );

        float brakingDistance =
            startingSpeed * stopDuration * 0.5f;

        float totalDistance =
            extraCycles * LoopHeight + alignmentDistance;

        // Add full cycles until we have enough stopping distance.
        while (totalDistance < brakingDistance)
        {
            totalDistance += LoopHeight;
        }

        float cruiseDistance =
            totalDistance - brakingDistance;

        float cruiseDuration =
            cruiseDistance / startingSpeed;

        float totalDuration =
            cruiseDuration + stopDuration;

        float previousDistance = 0f;

        LeanTween.value(gameObject, 0f, 1f, totalDuration)
            .setEase(LeanTweenType.linear)
            .setOnUpdate((float progress) =>
            {
                float elapsed = progress * totalDuration;

                float traveledDistance;

                if (elapsed <= cruiseDuration)
                {
                    // Continue at the current speed.
                    traveledDistance = startingSpeed * elapsed;
                }
                else
                {
                    // Gradually reduce speed to zero.
                    float brakeTime = Mathf.Clamp(
                        elapsed - cruiseDuration,
                        0f,
                        stopDuration
                    );

                    float brakingTravel =
                        startingSpeed * brakeTime
                        - 0.5f
                        * (startingSpeed / stopDuration)
                        * brakeTime * brakeTime;

                    traveledDistance =
                        cruiseDistance + brakingTravel;
                }

                float delta =
                    traveledDistance - previousDistance;

                MoveSymbols(delta);

                previousDistance = traveledDistance;
            })
            .setOnComplete(() =>
            {
                MoveSymbols(totalDistance - previousDistance);

                // Play landing bounce before reporting the reel stopped.
                PlayLandingBounce();
            });
    }

    private void PlayLandingBounce()
    {
        if (!enableLandingBounce || bounceDistance <= 0f)
        {
            CompleteStop();
            return;
        }

        Vector2 restingPosition = originalStripPosition;

        // Briefly move the strip downward.
        Vector2 dipPosition = restingPosition +
            Vector2.down * bounceDistance;

        float dipDuration = bounceDuration * 0.4f;
        float returnDuration = bounceDuration * 0.6f;

        LeanTween.move(symbolStrip, dipPosition, dipDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                // Return to the exact original position.
                LeanTween.move(
                    symbolStrip,
                    restingPosition,
                    returnDuration
                )
                .setEase(LeanTweenType.easeOutCubic)
                .setOnComplete(() =>
                {
                    symbolStrip.anchoredPosition = restingPosition;

                    CompleteStop();
                });
            });
    }

    private void CompleteStop()
    {
        isSpinning = false;
        isStopping = false;

        Debug.Log($"{name} stopped at {targetSymbol.SymbolId}");

        // Result evaluation happens only after the bounce.
        OnReelStopped?.Invoke(this);
    }

    public void SetTargetSymbol(SymbolData symbol)
    {
        if (symbol == null || availableSymbols == null ||
            Array.IndexOf(availableSymbols, symbol) < 0)
        {
            Debug.LogWarning($"Invalid target symbol on {name}");
            return;
        }

        targetSymbol = symbol;
    }
}
