using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game Object Caching Manager.
/// This source code is designed based on Unity's Scene Adaptive approach (e.g., Chap1 scene, Player scene, Manager scene, ...).
/// This class is used for the purpose of maintaining object references across scenes and optimizing performance.
/// </summary>
public class GameEntityManager : MonoBehaviour
{
    public static GameEntityManager Instance { get; private set; }

    private static readonly Dictionary<string, GameObject> _cachedGameObjects = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Register Game Object.
    /// </summary>
    /// <param name="key">Unique key to identify the object.</param>
    /// <param name="value">Game object to register.</param>
    public static void RegisterGameObject(string key, GameObject value)
    {
        _cachedGameObjects[key] = value;
        Debug.Log($"Game object registered: {key}");
    }

    /// <summary>
    /// Get Game Object.
    /// </summary>
    /// <param name="key">Key of the object to find.</param>
    public GameObject GetGameObject(string key)
    {
        if (_cachedGameObjects.TryGetValue(key, out var cached))
            return cached;

        Debug.LogWarning($"Could not find game object corresponding to key '{key}'");
        return null;
    }

    /// <summary>
    /// Unregister a specific game object.
    /// </summary>
    /// <param name="key">Key of the object to remove.</param>
    public static void UnregisterGameObject(string key)
    {
        if (_cachedGameObjects.Remove(key)) Debug.Log($"Game object removed: {key}");
    }

    /// <summary>
    /// Remove all stored game objects.
    /// </summary>
    public void RemoveAllEntities()
    {
        _cachedGameObjects.Clear();
        Debug.Log("All game object references have been removed.");
    }
}