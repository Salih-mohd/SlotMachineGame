
using UnityEngine;

public class WinEvaluator : MonoBehaviour
{
    // Checks whether all center-row symbols match.
    public bool EvaluateMatch(SymbolData[] symbols, out SymbolData winningSymbol)
    {
        winningSymbol = null;

        // Validate the result.
        if (symbols == null || symbols.Length != 3)
        {
            Debug.LogWarning("Win evaluation requires exactly 3 symbols.");
            return false;
        }

        SymbolData firstSymbol = symbols[0];

        if (firstSymbol == null || !firstSymbol.CanMatchForPayout)
            return false;

        // Compare every symbol against the first symbol.
        for (int i = 1; i < symbols.Length; i++)
        {
            if (symbols[i] != firstSymbol)
                return false;
        }

        // All three match.
        winningSymbol = firstSymbol;
        return true;
    }
}
