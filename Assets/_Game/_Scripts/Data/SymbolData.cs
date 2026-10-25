using UnityEngine;


[CreateAssetMenu(fileName = "NewSymbol", menuName = "SLotGame/ symbol Data")]

public class SymbolData : ScriptableObject
{
    [Header("Symbol Identity")]

    [SerializeField] private string symbolId;
    [SerializeField] private Sprite symbolSprite;

    [Header("Payout Settings")]

    [SerializeField] private bool canMatchForPayout = true;
    [SerializeField] private int payoutMultipier = 1;

    public string SymbolId => symbolId;
    public Sprite SymbolSprite => symbolSprite;
    public bool CanMatchForPayout => canMatchForPayout;
    public int PayoutMultiplier => payoutMultipier;


}
