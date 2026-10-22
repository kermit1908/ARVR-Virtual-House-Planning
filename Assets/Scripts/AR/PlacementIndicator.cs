using UnityEngine;

namespace HousePlanner.AR
{
    /// <summary>
    /// Visual reticle that follows the AR raycast hit point, rotates to
    /// match the detected surface normal, and provides animated feedback.
    /// Colour-codes green (valid) / red (invalid) based on placement state.
    /// </summary>
    public class PlacementIndicator : MonoBehaviour
    {
        // ──────────────────────────── Inspector Fields ────────────────────────────

        [Header("Visual")]
        [SerializeField] private MeshRenderer indicatorRenderer;
        [SerializeField] private Color validColor   = new Color(0f, 1f, 0.4f, 0.6f);
        [SerializeField] private Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.6f);

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseMinScale = 0.85f;
        [SerializeField] private float pulseMaxScale = 1.15f;
        [SerializeField] private float rotationSpeed = 30f;

        [Header("Smoothing")]
        [SerializeField] private float positionLerpSpeed = 15f;
        [SerializeField] private float rotationLerpSpeed = 10f;

        // ──────────────────────────── Private State ──────────────────────────────

        private bool _isVisible;
        private bool _isValid = true;
        private Pose _targetPose;
        private Material _materialInstance;
        private Vector3 _baseScale;
        private static readonly int ColorProp = Shader.PropertyToID("_Color");
        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");

        // ──────────────────────────── Unity Lifecycle ────────────────────────────

        private void Awake()
        {
            EnsureRenderer();
            _baseScale = transform.localScale;
            CreateMaterialInstance();
            SetVisible(false);
        }

        private void Update()
        {
            if (!_isVisible) return;

            SmoothFollowTarget();
            AnimatePulse();
            AnimateRotation();
        }

        private void OnDestroy()
        {
            if (_materialInstance != null)
            {
                Destroy(_materialInstance);
            }
        }

        // ──────────────────────────── Public API ─────────────────────────────────

        /// <summary>
        /// Update the target pose the indicator should follow.
        /// </summary>
        /// <param name="pose">World-space pose from the AR raycast hit.</param>
        public void UpdatePose(Pose pose)
        {
            _targetPose = pose;
        }

        /// <summary>
        /// Show or hide the indicator.
        /// </summary>
        /// <param name="visible">True to show.</param>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// Mark the current placement location as valid or invalid,
        /// which changes the indicator colour.
        /// </summary>
        /// <param name="valid">True for green / valid.</param>
        public void SetValid(bool valid)
        {
            _isValid = valid;
            ApplyColor(valid ? validColor : invalidColor);
        }

        // ──────────────────────────── Private Helpers ────────────────────────────

        private void SmoothFollowTarget()
        {
            float dt = Time.deltaTime;

            transform.position = Vector3.Lerp(
                transform.position,
                _targetPose.position,
                dt * positionLerpSpeed);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                _targetPose.rotation,
                dt * rotationLerpSpeed);
        }

        private void AnimatePulse()
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float scaleMul = Mathf.Lerp(pulseMinScale, pulseMaxScale, t);
            transform.localScale = _baseScale * scaleMul;
        }

        private void AnimateRotation()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }

        private void EnsureRenderer()
        {
            if (indicatorRenderer != null) return;

            indicatorRenderer = GetComponentInChildren<MeshRenderer>();

            if (indicatorRenderer != null) return;

            // Auto-create a simple quad indicator.
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(transform, false);
            quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            quad.transform.localScale = Vector3.one * 0.15f;

            // Remove the collider – we don't need physics.
            var collider = quad.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            indicatorRenderer = quad.GetComponent<MeshRenderer>();
        }

        private void CreateMaterialInstance()
        {
            if (indicatorRenderer == null) return;

            // Create a unique instance so we don't mutate shared materials.
            _materialInstance = new Material(indicatorRenderer.sharedMaterial != null
                ? indicatorRenderer.sharedMaterial
                : GetFallbackMaterial());

            indicatorRenderer.material = _materialInstance;
            ApplyColor(validColor);
        }

        private void ApplyColor(Color color)
        {
            if (_materialInstance == null) return;

            // Support both legacy _Color and URP _BaseColor properties.
            if (_materialInstance.HasProperty(BaseColorProp))
            {
                _materialInstance.SetColor(BaseColorProp, color);
            }
            else if (_materialInstance.HasProperty(ColorProp))
            {
                _materialInstance.SetColor(ColorProp, color);
            }
        }

        private static Material GetFallbackMaterial()
        {
            // Attempt URP lit first, fall back to legacy.
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")
                                  ?? Shader.Find("Standard"));
            return mat;
        }
    }
}
