using UnityEngine;

/// <summary>
/// Global cache for expensive Find/GetComponent calls.
/// Initialized once and reused across all systems for maximum performance.
/// </summary>
public static class CachedReferences
{
    // Player references
    private static GameObject _player;
    private static Camera _playerCamera;
    private static Transform _playerEyeTransform;

    // Camera rig references
    private static GameObject _cameraRig;
    private static Collider[] _rigColliders;

    // Scene references
    private static GameObject _skybox;
    private static Collider[] _skyboxColliders;

    // Cached collider arrays
    private static Collider[] _playerColliders;
    private static Collider[] _gunColliders;

    /// <summary>
    /// Player GameObject (finds once and caches)
    /// </summary>
    public static GameObject Player
    {
        get
        {
            if (_player == null)
            {
                _player = GameObject.FindGameObjectWithTag("Player");
                if (_player == null)
                {
                    Debug.LogError("[CachedReferences] No GameObject with 'Player' tag found!");
                }
                else
                {
                    Debug.Log($"[CachedReferences] Cached Player: {_player.name}");
                }
            }
            return _player;
        }
    }

    /// <summary>
    /// Player Camera (finds once and caches)
    /// </summary>
    public static Camera PlayerCamera
    {
        get
        {
            if (_playerCamera == null && Player != null)
            {
                _playerCamera = Player.GetComponentInChildren<Camera>();
                if (_playerCamera == null)
                {
                    _playerCamera = Camera.main;
                }

                if (_playerCamera != null)
                {
                    Debug.Log($"[CachedReferences] Cached PlayerCamera: {_playerCamera.name}");
                }
            }
            return _playerCamera;
        }
    }

    /// <summary>
    /// Player eye/head transform (CenterEyeAnchor for VR)
    /// </summary>
    public static Transform PlayerEyeTransform
    {
        get
        {
            if (_playerEyeTransform == null && Player != null)
            {
                // Try to find CenterEyeAnchor (VR)
                _playerEyeTransform = FindTransformByName(Player.transform, "CenterEyeAnchor");

                // Fallback to camera transform
                if (_playerEyeTransform == null && PlayerCamera != null)
                {
                    _playerEyeTransform = PlayerCamera.transform;
                }

                if (_playerEyeTransform != null)
                {
                    Debug.Log($"[CachedReferences] Cached PlayerEyeTransform: {_playerEyeTransform.name}");
                }
            }
            return _playerEyeTransform;
        }
    }

    /// <summary>
    /// Camera Rig GameObject (finds once and caches)
    /// </summary>
    public static GameObject CameraRig
    {
        get
        {
            if (_cameraRig == null)
            {
                _cameraRig = GameObject.Find("OVRCameraRig");
                if (_cameraRig == null) _cameraRig = GameObject.Find("XR Origin");
                if (_cameraRig == null) _cameraRig = GameObject.Find("CameraRig");

                if (_cameraRig != null)
                {
                    Debug.Log($"[CachedReferences] Cached CameraRig: {_cameraRig.name}");
                }
            }
            return _cameraRig;
        }
    }

    /// <summary>
    /// Skybox GameObject (finds once and caches)
    /// </summary>
    public static GameObject Skybox
    {
        get
        {
            if (_skybox == null)
            {
                _skybox = GameObject.Find("NightSkybox");
                if (_skybox == null) _skybox = GameObject.Find("Skybox");

                if (_skybox != null)
                {
                    Debug.Log($"[CachedReferences] Cached Skybox: {_skybox.name}");
                }
            }
            return _skybox;
        }
    }

    /// <summary>
    /// Player colliders (cached array)
    /// </summary>
    public static Collider[] PlayerColliders
    {
        get
        {
            if (_playerColliders == null && Player != null)
            {
                _playerColliders = Player.GetComponentsInChildren<Collider>();
                Debug.Log($"[CachedReferences] Cached {_playerColliders.Length} player colliders");
            }
            return _playerColliders;
        }
    }

    /// <summary>
    /// Camera rig colliders (cached array)
    /// </summary>
    public static Collider[] RigColliders
    {
        get
        {
            if (_rigColliders == null && CameraRig != null)
            {
                _rigColliders = CameraRig.GetComponentsInChildren<Collider>();
                Debug.Log($"[CachedReferences] Cached {_rigColliders.Length} rig colliders");
            }
            return _rigColliders;
        }
    }

    /// <summary>
    /// Skybox colliders (cached array)
    /// </summary>
    public static Collider[] SkyboxColliders
    {
        get
        {
            if (_skyboxColliders == null && Skybox != null)
            {
                _skyboxColliders = Skybox.GetComponentsInChildren<Collider>();
                Debug.Log($"[CachedReferences] Cached {_skyboxColliders.Length} skybox colliders");
            }
            return _skyboxColliders;
        }
    }

    /// <summary>
    /// Gun colliders (cached array) - set by GunScript
    /// </summary>
    public static Collider[] GunColliders
    {
        get => _gunColliders;
        set
        {
            _gunColliders = value;
            if (_gunColliders != null)
            {
                Debug.Log($"[CachedReferences] Cached {_gunColliders.Length} gun colliders");
            }
        }
    }

    /// <summary>
    /// Clear all cached references (call on scene load)
    /// </summary>
    public static void ClearCache()
    {
        Debug.Log("[CachedReferences] Clearing all cached references");
        _player = null;
        _playerCamera = null;
        _playerEyeTransform = null;
        _cameraRig = null;
        _rigColliders = null;
        _skybox = null;
        _skyboxColliders = null;
        _playerColliders = null;
        _gunColliders = null;
    }

    /// <summary>
    /// Recursive transform search (helper method)
    /// </summary>
    private static Transform FindTransformByName(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindTransformByName(child, name);
            if (result != null)
                return result;
        }

        return null;
    }
}
