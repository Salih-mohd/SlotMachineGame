
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
    [SerializeField] private float spinSpeed = 600f;

    [Header("Stop Settings")]
    [SerializeField] private float stopDuration = 2f;
    [SerializeField] private int extraCycles = 1;

    [Header("Testing Only")]
    [SerializeField] private bool autoTest = false;

    private readonly List<RectTransform> symbolRects = new();

    private SymbolData targetSymbol;
    private bool isSpinning;
    private bool isStopping;

    public event Action<ReelController> OnReelStopped;

    public bool IsSpinning => isSpinning;
    public SymbolData TargetSymbol => targetSymbol;

    private float SymbolStep => symbolHeight + symbolSpacing;
    private float LoopHeight => symbolRects.Count * SymbolStep;

    private void Start()
    {
        GenerateSymbols();

        // Temporary test. Remove when SlotMachineController is ready.
        if (autoTest && availableSymbols.Length > 1)
        {
            SetTargetSymbol(availableSymbols[1]); // Cherry
            StartSpin();
            Invoke(nameof(StopSpin), 2f);
        }
    }

    private void Update()
    {
        // Normal constant-speed spinning.
        // LeanTween takes control during stopping.
        if (isSpinning && !isStopping)
        {
            MoveSymbols(spinSpeed * Time.deltaTime);
        }
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

            rect.sizeDelta = new Vector2(symbolHeight, symbolHeight);

            float y = -(i * SymbolStep + symbolHeight * 0.5f);

            rect.anchoredPosition = new Vector2(0f, y);

            symbolRects.Add(rect);
        }

        float totalHeight =
            symbolRects.Count * SymbolStep - symbolSpacing;

        symbolStrip.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, totalHeight);
    }

    // Move symbols downward and recycle them.
    private void MoveSymbols(float distance)
    {
        if (symbolRects.Count == 0)
            return;

        float loopHeight = LoopHeight;
        float bottomLimit = -loopHeight - symbolHeight * 0.5f;

        foreach (RectTransform rect in symbolRects)
        {
            Vector2 position = rect.anchoredPosition;

            position.y -= distance;

            // Handle even large movement distances safely.
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

        isSpinning = true;
        isStopping = false;
    }

    // Starts smooth deceleration toward the target symbol.
    public void StopSpin()
    {
        if (!isSpinning || isStopping || targetSymbol == null)
            return;

        int targetIndex = Array.IndexOf(availableSymbols, targetSymbol);

        if (targetIndex < 0)
            return;

        isStopping = true;

        RectTransform targetRect = symbolRects[targetIndex];

        // Find the symbol's actual visual center in reel space.
        RectTransform reelRect = (RectTransform)transform;

        Vector3 symbolWorldCenter =
            targetRect.TransformPoint(targetRect.rect.center);

        float currentY =
            reelRect.InverseTransformPoint(symbolWorldCenter).y;

        // Target is the center of the visible reel window.
        float desiredY = reelRect.rect.center.y;

        // Find forward downward travel needed for alignment.
        float alignmentDistance =
            Mathf.Repeat(currentY - desiredY, LoopHeight);

        // Travel extra full cycles before landing.
        float totalDistance =
            extraCycles * LoopHeight + alignmentDistance;

        float previousDistance = 0f;

        LeanTween.value(gameObject, 0f, totalDistance, stopDuration)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnUpdate((float traveledDistance) =>
            {
                float delta = traveledDistance - previousDistance;

                MoveSymbols(delta);

                previousDistance = traveledDistance;
            })
            .setOnComplete(() =>
            {
                isSpinning = false;
                isStopping = false;

                Debug.Log($"{name} stopped at {targetSymbol.SymbolId}");

                OnReelStopped?.Invoke(this);
            });
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
