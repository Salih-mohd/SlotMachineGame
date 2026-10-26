
using UnityEngine;

public class WinTextEffect : MonoBehaviour
{
    public enum MoveDirection
    {
        LeftToRight,
        RightToLeft
    }

    [Header("Movement Settings")]
    [SerializeField, Min(1f)] private float moveSpeed = 600f;
    [SerializeField, Min(0f)] private float boundaryPadding = 20f;

    private RectTransform rectTransform;
    private int movementTweenId = -1;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Starts the flying animation from a specified anchor.
    public void Play(
        RectTransform spawnAnchor,
        RectTransform effectContainer,
        MoveDirection direction,
        WinEffectPool pool)
    {
        if (spawnAnchor == null || effectContainer == null || pool == null)
        {
            Debug.LogError("WIN effect references are missing.");
            return;
        }

        // Cancel any previous animation.
        StopMovement();

        // Convert the anchor position into the container's local space.
        Vector3 worldPosition = spawnAnchor.position;

        Vector3 localPosition =
            effectContainer.InverseTransformPoint(worldPosition);

        rectTransform.anchoredPosition = new Vector2(
            localPosition.x,
            localPosition.y
        );

        // Get the container's horizontal boundaries.
        Rect bounds = effectContainer.rect;

        // Move completely outside the boundary before recycling.
        float halfTextWidth = rectTransform.rect.width * 0.5f;

        float destinationX = direction == MoveDirection.LeftToRight
            ? bounds.xMax + halfTextWidth + boundaryPadding
            : bounds.xMin - halfTextWidth - boundaryPadding;

        float distance = Mathf.Abs(
            destinationX - rectTransform.anchoredPosition.x
        );

        // Speed stays consistent regardless of travel distance.
        float duration = distance / moveSpeed;

        Vector2 destination = new Vector2(
            destinationX,
            rectTransform.anchoredPosition.y
        );

        // Move horizontally with constant speed.
        LTDescr tween = LeanTween.move(
            rectTransform,
            destination,
            duration
        );

        tween.setEase(LeanTweenType.linear);

        tween.setOnComplete(() =>
        {
            movementTweenId = -1;

            // Recycle the object instead of destroying it.
            pool.Return(rectTransform);
        });

        movementTweenId = tween.id;
    }

    // Stops an active movement animation.
    private void StopMovement()
    {
        if (movementTweenId >= 0)
        {
            LeanTween.cancel(movementTweenId);
            movementTweenId = -1;
        }
    }

    private void OnDisable()
    {
        StopMovement();
    }
}
