using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EntityRegister that registers game objects to GameEntityManager.
/// This source code is designed based on Unity's Scene Adaptive approach (e.g., Chap1 scene, Player scene, Manager scene, ...).
/// This class is used for the purpose of registering objects within the scene to GameEntityManager.
/// </summary>
public class EntityRegister : MonoBehaviour
{
    [SerializeField] private List<GameObject> sceneObjects = new List<GameObject>();

    private void Awake()
    {
        foreach (var objectData in sceneObjects.Where(objectData => objectData.gameObject))
        {
            GameEntityManager.RegisterGameObject(objectData.name, objectData);
        }
    }
}