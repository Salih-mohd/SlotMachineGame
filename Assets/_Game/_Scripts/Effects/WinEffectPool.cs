
using System.Collections.Generic;
using UnityEngine;

public class WinEffectPool : MonoBehaviour
{
    [Header("Pool References")]
    [SerializeField] private RectTransform winTextPrefab;
    [SerializeField] private RectTransform poolParent;

    [Header("Pool Settings")]
    [SerializeField, Min(1)] private int initialPoolSize = 30;
    [SerializeField] private bool allowExpansion = true;

    // Stores available objects ready for reuse.
    private readonly Queue<RectTransform> availableObjects = new();

    // Tracks every object created by this pool.
    private readonly HashSet<RectTransform> pooledObjects = new();

    // Prevents returning the same object multiple times.
    private readonly HashSet<RectTransform> activeObjects = new();

    private void Awake()
    {
        InitializePool();
    }

    // Creates the initial collection of reusable objects.
    private void InitializePool()
    {
        if (winTextPrefab == null || poolParent == null)
        {
            Debug.LogError("WIN Effect Pool references are missing.");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledObject();
        }
    }

    // Creates one reusable WIN text object.
    private RectTransform CreatePooledObject()
    {
        RectTransform newObject = Instantiate(
            winTextPrefab,
            poolParent
        );

        newObject.gameObject.SetActive(false);

        newObject.name = "Pooled_WIN_Text";

        pooledObjects.Add(newObject);
        availableObjects.Enqueue(newObject);

        return newObject;
    }

    // Requests an available object from the pool.
    public RectTransform Get()
    {
        if (availableObjects.Count == 0)
        {
            if (!allowExpansion)
            {
                Debug.LogWarning("WIN Effect Pool is exhausted.");
                return null;
            }

            // Expand only when absolutely necessary.
            CreatePooledObject();
        }

        RectTransform obj = availableObjects.Dequeue();

        activeObjects.Add(obj);

        // Reset transformations before reusing.
        obj.localScale = Vector3.one;
        obj.localRotation = Quaternion.identity;

        obj.gameObject.SetActive(true);

        return obj;
    }

    // Returns an object to the pool for future reuse.
    public void Return(RectTransform obj)
    {
        if (obj == null || !pooledObjects.Contains(obj))
            return;

        // Prevent duplicate returns.
        if (!activeObjects.Remove(obj))
            return;

        // Stop animations before making the object available again.
        LeanTween.cancel(obj.gameObject);

        obj.gameObject.SetActive(false);

        availableObjects.Enqueue(obj);
    }
}
