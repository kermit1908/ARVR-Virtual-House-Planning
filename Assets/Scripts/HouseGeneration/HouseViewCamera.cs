using UnityEngine;

namespace HousePlanner.HouseGeneration
{
    public enum CameraViewMode
    {
        Orbit,
        Fly,
        Walk
    }

    /// <summary>
    /// Multi-mode camera controller for 3D house inspection.
    /// Supports Orbit (exterior/interior turntable), Fly (free-cam), and Walk (first-person interior).
    /// Fully touch and keyboard/mouse responsive.
    /// </summary>
    public class HouseViewCamera : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private CameraViewMode _mode = CameraViewMode.Orbit;

        [Header("Orbit Mode Settings")]
        [SerializeField] private Vector3 _orbitPivot = new Vector3(5f, 1f, 5f);
        [SerializeField] private float _orbitDistance = 12f;
        [SerializeField] private float _minOrbitDistance = 1.5f;
        [SerializeField] private float _maxOrbitDistance = 35f;
        [SerializeField] private float _orbitSensitivity = 3f;
        [SerializeField] private float _zoomSensitivity = 4f;

        [Header("Fly / Walk Settings")]
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _lookSensitivity = 2f;
        [SerializeField] private float _walkHeight = 1.7f;

        private float _yaw = 45f;
        private float _pitch = 30f;
        private Vector3 _currentVelocity;

        public CameraViewMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                if (_mode == CameraViewMode.Walk)
                {
                    Vector3 pos = transform.position;
                    pos.y = _walkHeight;
                    transform.position = pos;
                }
            }
        }

        public Vector3 OrbitPivot
        {
            get => _orbitPivot;
            set => _orbitPivot = value;
        }

        private void Start()
        {
            UpdateCameraTransform();
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // Handle touch inputs for mobile
            if (Input.touchCount > 0)
            {
                HandleTouchInput();
                return;
            }

            // Mouse / Keyboard inputs for desktop
            switch (_mode)
            {
                case CameraViewMode.Orbit:
                    HandleOrbitDesktop();
                    break;
                case CameraViewMode.Fly:
                    HandleFlyDesktop();
                    break;
                case CameraViewMode.Walk:
                    HandleWalkDesktop();
                    break;
            }
        }

        private void HandleOrbitDesktop()
        {
            // Rotate with right or middle mouse button, or left mouse when Alt is pressed
            if (Input.GetMouseButton(1) || Input.GetMouseButton(2) || (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt)))
            {
                _yaw += Input.GetAxis("Mouse X") * _orbitSensitivity;
                _pitch -= Input.GetAxis("Mouse Y") * _orbitSensitivity;
                _pitch = Mathf.Clamp(_pitch, 5f, 85f);
            }

            // Zoom with scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                _orbitDistance -= scroll * _zoomSensitivity;
                _orbitDistance = Mathf.Clamp(_orbitDistance, _minOrbitDistance, _maxOrbitDistance);
            }

            // Pan orbit pivot with Shift + right mouse
            if (Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftShift))
            {
                float panX = -Input.GetAxis("Mouse X") * 0.1f * (_orbitDistance * 0.1f);
                float panZ = -Input.GetAxis("Mouse Y") * 0.1f * (_orbitDistance * 0.1f);
                _orbitPivot += transform.right * panX + transform.up * panZ;
            }

            UpdateCameraTransform();
        }

        private void HandleFlyDesktop()
        {
            if (Input.GetMouseButton(1))
            {
                _yaw += Input.GetAxis("Mouse X") * _lookSensitivity;
                _pitch -= Input.GetAxis("Mouse Y") * _lookSensitivity;
                _pitch = Mathf.Clamp(_pitch, -85f, 85f);
                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }

            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            float moveY = 0f;

            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)) moveY += 1f;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl)) moveY -= 1f;

            Vector3 moveDir = (transform.forward * moveZ + transform.right * moveX + Vector3.up * moveY).normalized;
            transform.position += moveDir * (_moveSpeed * Time.deltaTime);

            // Prevent going under ground
            Vector3 pos = transform.position;
            if (pos.y < 0.2f) pos.y = 0.2f;
            transform.position = pos;
        }

        private void HandleWalkDesktop()
        {
            if (Input.GetMouseButton(1))
            {
                _yaw += Input.GetAxis("Mouse X") * _lookSensitivity;
                _pitch -= Input.GetAxis("Mouse Y") * _lookSensitivity;
                _pitch = Mathf.Clamp(_pitch, -75f, 75f);
                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }

            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            Vector3 moveDir = (forward * moveZ + right * moveX).normalized;

            transform.position += moveDir * (_moveSpeed * 0.6f * Time.deltaTime);

            // Lock Y to walking eye-level
            Vector3 pos = transform.position;
            pos.y = _walkHeight;
            transform.position = pos;
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved)
                {
                    _yaw += t.deltaPosition.x * 0.2f;
                    _pitch -= t.deltaPosition.y * 0.2f;
                    _pitch = Mathf.Clamp(_pitch, 5f, 85f);
                    UpdateCameraTransform();
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                Vector2 prevPos0 = t0.position - t0.deltaPosition;
                Vector2 prevPos1 = t1.position - t1.deltaPosition;

                float prevMag = (prevPos0 - prevPos1).magnitude;
                float currentMag = (t0.position - t1.position).magnitude;
                float diff = currentMag - prevMag;

                _orbitDistance -= diff * 0.02f;
                _orbitDistance = Mathf.Clamp(_orbitDistance, _minOrbitDistance, _maxOrbitDistance);
                UpdateCameraTransform();
            }
        }

        private void UpdateCameraTransform()
        {
            if (_mode != CameraViewMode.Orbit) return;

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -_orbitDistance);
            transform.position = _orbitPivot + offset;
            transform.LookAt(_orbitPivot);
        }

        /// <summary>
        /// Centers the orbit camera on a specific bounding area (e.g. house bounds).
        /// </summary>
        public void FocusOn(Vector3 center, float radius)
        {
            _orbitPivot = center;
            _orbitDistance = Mathf.Clamp(radius * 2.2f, _minOrbitDistance, _maxOrbitDistance);
            UpdateCameraTransform();
        }
    }
}
