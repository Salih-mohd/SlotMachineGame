
using UnityEngine;

public class WinEffectTester : MonoBehaviour
{
    [Header("Effect Controller")]
    [SerializeField] private SlotEffectsController effectsController;

    [Header("Testing")]
    [SerializeField] private KeyCode testKey = KeyCode.T;

    private void Update()
    {
        // Temporary testing only.
        if (Input.GetKeyDown(testKey))
        {
            effectsController.PlayWinEffect();
        }
    }
}
