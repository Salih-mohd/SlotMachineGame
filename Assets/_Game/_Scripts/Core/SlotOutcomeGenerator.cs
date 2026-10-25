
using UnityEngine;

public class SlotOutcomeGenerator : MonoBehaviour
{
    [Header("Available Symbols")]
    [SerializeField] private SymbolData[] availableSymbols;

    [Header("Debug Settings")]
    [SerializeField] private bool enableTestMode = false;
    [SerializeField] private SymbolData testSymbol;

    // Generates one result for each reel.
    public SymbolData[] GenerateOutcome(int reelCount)
    {
        if (availableSymbols == null || availableSymbols.Length == 0 ||
            reelCount <= 0)
        {
            Debug.LogError("Invalid RNG configuration.");
            return null;
        }

        SymbolData[] results = new SymbolData[reelCount];

        // TEST MODE: Force all reels to show the selected symbol.
        if (enableTestMode)
        {
            if (testSymbol == null ||
                System.Array.IndexOf(availableSymbols, testSymbol) < 0)
            {
                Debug.LogError("Invalid test symbol.");
                return null;
            }

            for (int i = 0; i < reelCount; i++)
            {
                results[i] = testSymbol;
            }

            Debug.Log($"TEST MODE: Forced {testSymbol.SymbolId} combination.");

            return results;
        }

        // NORMAL MODE: Generate independent random outcomes.
        for (int i = 0; i < reelCount; i++)
        {
            int randomIndex = Random.Range(0, availableSymbols.Length);
            results[i] = availableSymbols[randomIndex];
        }

        return results;
    }
}
