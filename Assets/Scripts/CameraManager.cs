using LuaFlow.Core;
using LuaFlow.Interface;
using UnityEngine;

/// <summary>
/// Camera Manager.
/// This source code is designed based on Unity's Scene Adaptive approach (e.g., Chap1 scene, Player scene, Manager scene, ...).
/// This class sets the camera's target.
/// It was written for testing purposes and does not use Cinemachine.
/// </summary>
public class CameraManager : MonoBehaviour, ICameraManager
{
    public static CameraManager Instance { get; private set; }
    public Camera MainCamera { get; private set; }

    [field: SerializeField] public Vector3 PositionOffset { get; set; }

    [SerializeField] [Range(0, 1)] private float smoothTime;
    private Transform _cameraTarget;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LuaFlowServiceLocator.Register<ICameraManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        MainCamera = Camera.main;
    }

    /// <summary>
    /// Camera movement.
    /// </summary>
    private void FixedUpdate()
    {
        if (!_cameraTarget) return;
        var targetPosition = _cameraTarget.position + PositionOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothTime);
    }

    private void OnDestroy()
    {
        Instance = null;
        LuaFlowServiceLocator.Unregister<ICameraManager>();
    }

    /// <summary>
    /// Set camera targeting.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="followSpeed"></param>
    public void ChangeCameraTarget(Transform target, float followSpeed = 0.1f)
    {
        _cameraTarget = target;
        smoothTime = followSpeed;
    }
}