
using System.Collections;
using UnityEngine;

public class SlotEffectsController : MonoBehaviour
{
    [Header("WIN Effect Anchors")]
    [SerializeField] private WinEffectAnchor[] spawnAnchors;

    [Header("Spawn Settings")]
    [SerializeField, Min(0.1f)]
    private float spawnDuration = 2f;

    [SerializeField, Min(0.01f)]
    private float spawnInterval = 0.2f;

    private Coroutine spawnCoroutine;

    // Indicates whether the effect is generating new texts.
    public bool IsPlaying => spawnCoroutine != null;

    // Starts the WIN text effect.
    public void PlayWinEffect()
    {
        // Prevent overlapping spawn sequences.
        if (IsPlaying)
            return;

        if (spawnAnchors == null || spawnAnchors.Length == 0)
        {
            Debug.LogWarning("No WIN effect anchors assigned.");
            return;
        }

        spawnCoroutine = StartCoroutine(SpawnSequence());
    }

    // Continuously spawns text for the configured duration.
    private IEnumerator SpawnSequence()
    {
        float elapsedTime = 0f;

        while (elapsedTime < spawnDuration)
        {
            // Spawn one WIN text from every configured anchor.
            foreach (WinEffectAnchor anchor in spawnAnchors)
            {
                if (anchor != null)
                    anchor.SpawnWinText();
            }

            yield return new WaitForSeconds(spawnInterval);

            elapsedTime += spawnInterval;
        }

        // Stop generating new texts.
        // Existing texts continue travelling and return to the pool.
        spawnCoroutine = null;
    }

    private void OnDisable()
    {
        // Stop spawning if this controller is disabled.
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
}
