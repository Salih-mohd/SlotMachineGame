
using UnityEngine;

public class PayoutCalculator : MonoBehaviour
{
    // Calculates the payout for a winning combination.
    // Returns 0 if the player did not win.
    public int CalculatePayout(SymbolData winningSymbol, int betAmount)
    {
        // No winning symbol means no payout.
        if (winningSymbol == null || !winningSymbol.CanMatchForPayout)
            return 0;

        // Prevent invalid bet amounts.
        if (betAmount <= 0)
        {
            Debug.LogWarning("Invalid bet amount.");
            return 0;
        }

        // Calculate reward using the symbol's multiplier.
        int payout = betAmount * winningSymbol.PayoutMultiplier;

        return payout;
    }
}
