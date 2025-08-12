using System.Collections.Generic;
using UnityEngine;

namespace NekoLegends
{
    public class RainArea : MonoBehaviour
    {
        // **Basic Configuration Fields**
        [Tooltip("The particle system emitting rain particles")]
        public ParticleSystem RainParticles;

        [Tooltip("Particle system for horizontal splash effects (ripples on the ground)")]
        public ParticleSystem HorizontalSplashParticles;

        [Tooltip("Particle system for vertical splash effects (upward splashes)")]
        public ParticleSystem VerticalSplashParticles;

        [Range(0, 90)]
        [Tooltip("Maximum slope angle in degrees where splashes can occur")]
        public float MaxSlopeForSplash = 35;

        [Min(0.01f)]
        [Tooltip("Minimum distance between splash effects to prevent overlap")]
        public float SplashSpacing = 0.1f;

        [Range(0.1f, 5f)]
        [Tooltip("Adjusts the radius and radius thickness of the rain particle shape")]
        public float RainSize = 1f;

        // **Advanced Settings**
        [System.Serializable]
        public class SplashSettings
        {
            [Tooltip("If true, particles will face the camera; if false, they align with the surface normal")]
            public bool FaceCamera;

            [Range(-180f, 180f)]
            [Tooltip("Rotation offset around X-axis in degrees")]
            public float RotationXOffset = 0f;

            [Range(-180f, 180f)]
            [Tooltip("Rotation offset around Y-axis in degrees")]
            public float RotationYOffset = 0f;

            [Range(-180f, 180f)]
            [Tooltip("Rotation offset around Z-axis in degrees")]
            public float RotationZOffset = 0f;
        }

        [Header("Advanced Settings")]
        [Tooltip("Settings for horizontal splash orientation")]
        public SplashSettings HorizontalSplashSettings = new SplashSettings
        {
            FaceCamera = false, // Default: flat on surface
            RotationXOffset = 0f,
            RotationYOffset = 0f,
            RotationZOffset = 0f
        };

        [Tooltip("Settings for vertical splash orientation")]
        public SplashSettings VerticalSplashSettings = new SplashSettings
        {
            FaceCamera = true,  // Default: faces camera
            RotationXOffset = 0f,
            RotationYOffset = 90f, // Default: upright
            RotationZOffset = 0f
        };

        // **Internal State**
        private SpatialGrid _decalGrid;
        private float _lastRainSize;

        // **Initialization**
        void Awake()
        {
            _decalGrid = new SpatialGrid(SplashSpacing * 2);
            _lastRainSize = RainSize;
            UpdateRainParticleShape();
        }

        void Update()
        {
            // Validate transform
            if (float.IsNaN(transform.position.x) || float.IsInfinity(transform.position.x))
            {
                Debug.LogError($"Invalid transform position on {gameObject.name}: {transform.position}");
                transform.position = Vector3.zero; // Reset to safe value
            }

            // Update rain shape if size changed
            if (!Mathf.Approximately(_lastRainSize, RainSize))
            {
                UpdateRainParticleShape();
                _lastRainSize = RainSize;
            }
        }

        // **Update Rain Particle Shape**
        private void UpdateRainParticleShape()
        {
            if (RainParticles == null)
            {
                Debug.LogWarning("RainParticles is not assigned.");
                return;
            }

            var shapeModule = RainParticles.shape;
            shapeModule.radius = RainSize;
            shapeModule.radiusThickness = RainSize;
        }

        // **Public Methods**
        public void HandleCollisionFromChild(GameObject other)
        {
            HandleParticleCollision(other);
        }

        // **Collision Handling**
        void HandleParticleCollision(GameObject other)
        {
            if (RainParticles == null)
            {
                Debug.LogWarning("RainParticles is not assigned, cannot detect collisions.");
                return;
            }

            List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
            int numCollisions = RainParticles.GetCollisionEvents(other, collisionEvents);
            foreach (var collision in collisionEvents)
            {
                if (IsValidCollision(collision) && !IsPositionOccupied(collision.intersection))
                {
                    EmitDecal(collision.intersection, collision.normal);
                    _decalGrid.RegisterPosition(collision.intersection);
                }
            }
        }

        // **Validation Methods**
        bool IsValidCollision(ParticleCollisionEvent collision)
        {
            return Vector3.Angle(collision.normal, Vector3.up) <= MaxSlopeForSplash;
        }

        bool IsPositionOccupied(Vector3 position)
        {
            return _decalGrid.IsPositionOccupied(position, SplashSpacing);
        }

        // **Decal Emission**
        void EmitDecal(Vector3 position, Vector3 surfaceNormal)
        {
            // Validate position
            if (float.IsNaN(position.x) || float.IsInfinity(position.x) ||
                float.IsNaN(position.y) || float.IsInfinity(position.y) ||
                float.IsNaN(position.z) || float.IsInfinity(position.z))
            {
                Debug.LogWarning($"Skipping emission due to invalid position: {position}");
                return;
            }

            // Emit horizontal splash
            if (HorizontalSplashParticles != null)
            {
                Vector3 rotation = CalculateDecalRotation(position, surfaceNormal, HorizontalSplashSettings);
                if (IsValidRotation(rotation))
                {
                    var emitParams = new ParticleSystem.EmitParams
                    {
                        position = position,
                        rotation3D = rotation
                    };
                    HorizontalSplashParticles.Emit(emitParams, 1);
                }
                else
                {
                    Debug.LogWarning($"Skipping horizontal splash due to invalid rotation: {rotation}");
                }
            }

            // Emit vertical splash
            if (VerticalSplashParticles != null)
            {
                Vector3 rotation = CalculateDecalRotation(position, surfaceNormal, VerticalSplashSettings);
                if (IsValidRotation(rotation))
                {
                    var emitParams = new ParticleSystem.EmitParams
                    {
                        position = position,
                        rotation3D = rotation
                    };
                    VerticalSplashParticles.Emit(emitParams, 1);
                }
                else
                {
                    Debug.LogWarning($"Skipping vertical splash due to invalid rotation: {rotation}");
                }
            }
        }

        // **Validation Method for Rotation**
        bool IsValidRotation(Vector3 rotation)
        {
            return !(float.IsNaN(rotation.x) || float.IsInfinity(rotation.x) ||
                     float.IsNaN(rotation.y) || float.IsInfinity(rotation.y) ||
                     float.IsNaN(rotation.z) || float.IsInfinity(rotation.z));
        }


        Vector3 CalculateDecalRotation(Vector3 position, Vector3 surfaceNormal, SplashSettings settings)
        {
            // Step 1: Validate and normalize surfaceNormal
            if (surfaceNormal.sqrMagnitude < 0.001f || 
                float.IsNaN(surfaceNormal.x) || float.IsInfinity(surfaceNormal.x))
            {
                surfaceNormal = Vector3.up; // Default to up if invalid
            }
            else
            {
                surfaceNormal = surfaceNormal.normalized; // Ensure unit length
            }

            Quaternion baseRotation;

            // Step 2: Handle camera-facing rotation if enabled
            if (settings.FaceCamera)
            {
                // Calculate direction to camera
                Vector3 toCamera = Camera.main != null ? Camera.main.transform.position - position : Vector3.forward;

                // Validate and normalize toCamera
                if (toCamera.sqrMagnitude < 0.001f || 
                    float.IsNaN(toCamera.x) || float.IsInfinity(toCamera.x))
                {
                    toCamera = Vector3.forward; // Default direction if invalid
                }
                else
                {
                    toCamera = toCamera.normalized; // Ensure unit length
                }

                // Step 3: Check for parallel vectors
                float angleBetween = Vector3.Angle(toCamera, surfaceNormal);
                if (Mathf.Abs(angleBetween) < 0.1f || Mathf.Abs(angleBetween - 180f) < 0.1f)
                {
                    // Vectors are nearly parallel; use a fallback up vector
                    Vector3 fallbackUp = Vector3.Cross(toCamera, Vector3.right);
                    if (fallbackUp.sqrMagnitude < 0.001f)
                    {
                        fallbackUp = Vector3.Cross(toCamera, Vector3.up);
                    }
                    baseRotation = Quaternion.LookRotation(toCamera, fallbackUp.normalized);
                }
                else
                {
                    // Normal case: vectors are not parallel
                    baseRotation = Quaternion.LookRotation(toCamera, surfaceNormal);
                }
            }
            else
            {
                // Non-camera-facing: align with surface normal
                baseRotation = Quaternion.LookRotation(-surfaceNormal, Vector3.up);
            }

            // Step 4: Apply custom rotation offsets
            Quaternion offsetRotation = Quaternion.Euler(
                settings.RotationXOffset,
                settings.RotationYOffset,
                settings.RotationZOffset
            );

            Quaternion finalRotation = baseRotation * offsetRotation;

            // Step 5: Validate the final Euler angles
            Vector3 euler = finalRotation.eulerAngles;
            if (float.IsNaN(euler.x) || float.IsInfinity(euler.x) ||
                float.IsNaN(euler.y) || float.IsInfinity(euler.y) ||
                float.IsNaN(euler.z) || float.IsInfinity(euler.z))
            {
                Debug.LogWarning("Invalid rotation detected in CalculateDecalRotation. Returning default rotation.");
                return Vector3.zero; // Safe default rotation
            }

            return euler;
        }
    }

   
    public class SpatialGrid
    {
        private readonly float _cellSize;
        private readonly Dictionary<Vector3Int, List<Vector3>> _grid = new Dictionary<Vector3Int, List<Vector3>>();

        public SpatialGrid(float cellSize)
        {
            _cellSize = cellSize;
        }

        public bool IsPositionOccupied(Vector3 position, float minDistance)
        {
            Vector3Int cellKey = GetCellKey(position);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Vector3Int neighborKey = cellKey + new Vector3Int(x, y, z);
                        if (_grid.TryGetValue(neighborKey, out var positions))
                        {
                            foreach (var storedPos in positions)
                            {
                                if (Vector3.Distance(position, storedPos) < minDistance)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void RegisterPosition(Vector3 position)
        {
            Vector3Int cellKey = GetCellKey(position);
            if (!_grid.ContainsKey(cellKey))
            {
                _grid[cellKey] = new List<Vector3>();
            }
            _grid[cellKey].Add(position);
        }

        private Vector3Int GetCellKey(Vector3 position)
        {
            return new Vector3Int(
                Mathf.FloorToInt(position.x / _cellSize),
                Mathf.FloorToInt(position.y / _cellSize),
                Mathf.FloorToInt(position.z / _cellSize)
            );
        }
    }
}