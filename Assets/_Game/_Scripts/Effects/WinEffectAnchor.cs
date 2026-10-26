
using UnityEngine;

public class WinEffectAnchor : MonoBehaviour
{
    [Header("Movement Direction")]
    [SerializeField]
    private WinTextEffect.MoveDirection direction =
        WinTextEffect.MoveDirection.LeftToRight;

    [Header("References")]
    [SerializeField] private WinEffectPool effectPool;
    [SerializeField] private RectTransform effectContainer;

    private RectTransform spawnPoint;

    private void Awake()
    {
        spawnPoint = GetComponent<RectTransform>();
    }

    // Requests one pooled WIN text and launches it.
    public void SpawnWinText()
    {
        if (effectPool == null || effectContainer == null ||
            spawnPoint == null)
        {
            Debug.LogWarning(
                $"WIN effect anchor is not configured: {name}");
            return;
        }

        // Get an inactive WIN text from the pool.
        RectTransform winText = effectPool.Get();

        if (winText == null)
            return;

        // Get the movement controller from the pooled object.
        WinTextEffect effect =
            winText.GetComponent<WinTextEffect>();

        if (effect == null)
        {
            Debug.LogError(
                "WinTextEffect component is missing from the prefab.");

            effectPool.Return(winText);
            return;
        }

        // Start movement from this anchor in the selected direction.
        effect.Play(
            spawnPoint,
            effectContainer,
            direction,
            effectPool
        );
    }
}
