using System.Text;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace HousePlanner.AR
{
    /// <summary>
    /// Describes the current AR support status of the device.
    /// </summary>
    public enum ARSupportStatus
    {
        /// <summary>AR is fully supported.</summary>
        Supported,
        /// <summary>The support check is still in progress.</summary>
        CheckInProgress,
        /// <summary>The device hardware or OS does not support AR.</summary>
        Unsupported,
        /// <summary>AR runtime (e.g. ARCore / ARKit) is not installed.</summary>
        NotInstalled
    }

    /// <summary>
    /// Static utility that inspects the current device's AR capabilities
    /// and produces human-readable reports.
    /// </summary>
    public static class ARCapabilityChecker
    {
        // ──────────────────────────── Public API ─────────────────────────────────

        /// <summary>
        /// Synchronously map the current <see cref="ARSession.state"/> to an
        /// <see cref="ARSupportStatus"/> value.
        /// </summary>
        /// <returns>The derived support status.</returns>
        public static ARSupportStatus CheckARSupport()
        {
#if UNITY_ANDROID || UNITY_IOS
            switch (ARSession.state)
            {
                case ARSessionState.SessionTracking:
                case ARSessionState.SessionInitializing:
                case ARSessionState.Ready:
                    return ARSupportStatus.Supported;

                case ARSessionState.CheckingAvailability:
                case ARSessionState.Installing:
                    return ARSupportStatus.CheckInProgress;

                case ARSessionState.NeedsInstall:
                    return ARSupportStatus.NotInstalled;

                case ARSessionState.Unsupported:
                    return ARSupportStatus.Unsupported;

                case ARSessionState.None:
                default:
                    // The session hasn't been started yet – treat as in-progress.
                    return ARSupportStatus.CheckInProgress;
            }
#else
            return ARSupportStatus.Unsupported;
#endif
        }

        /// <summary>
        /// Check whether the device supports depth sensing (e.g. ToF camera,
        /// ARCore Depth API, or LiDAR on iOS).
        /// </summary>
        /// <returns>True if depth is available.</returns>
        public static bool IsDepthSupported()
        {
#if UNITY_ANDROID || UNITY_IOS
            // AR Foundation exposes depth through AROcclusionManager.
            // If one exists and its descriptor reports depth image support we
            // consider depth available.
            var occlusionManager = Object.FindFirstObjectByType<AROcclusionManager>();
            if (occlusionManager != null)
            {
                var descriptor = occlusionManager.descriptor;
                if (descriptor != null)
                {
                    return descriptor.supportsEnvironmentDepthImage ||
                           descriptor.supportsHumanDepthImage;
                }
            }

            return false;
#else
            return false;
#endif
        }

        /// <summary>
        /// Check whether plane detection is available (requires
        /// <see cref="ARPlaneManager"/> in the scene).
        /// </summary>
        /// <returns>True if the plane subsystem is present.</returns>
        public static bool IsPlaneDetectionSupported()
        {
#if UNITY_ANDROID || UNITY_IOS
            var planeManager = Object.FindFirstObjectByType<ARPlaneManager>();
            return planeManager != null && planeManager.subsystem != null;
#else
            return false;
#endif
        }

        /// <summary>
        /// Check whether light estimation is available (requires
        /// <see cref="ARCameraManager"/> in the scene).
        /// </summary>
        /// <returns>True if light estimation can be enabled.</returns>
        public static bool IsLightEstimationSupported()
        {
#if UNITY_ANDROID || UNITY_IOS
            var cameraManager = Object.FindFirstObjectByType<ARCameraManager>();
            return cameraManager != null;
#else
            return false;
#endif
        }

        /// <summary>
        /// Check whether anchor tracking is available (requires
        /// <see cref="ARAnchorManager"/> in the scene).
        /// </summary>
        /// <returns>True if anchors are supported.</returns>
        public static bool IsAnchorSupported()
        {
#if UNITY_ANDROID || UNITY_IOS
            var anchorManager = Object.FindFirstObjectByType<ARAnchorManager>();
            return anchorManager != null && anchorManager.subsystem != null;
#else
            return false;
#endif
        }

        /// <summary>
        /// Build a multi-line human-readable summary of every capability
        /// detected on the current device.
        /// </summary>
        /// <returns>A formatted capability report string.</returns>
        public static string GetSupportReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════ AR Capability Report ═══════");

            // Overall support
            var status = CheckARSupport();
            sb.AppendLine($"AR Support:          {FormatStatus(status)}");
            sb.AppendLine($"Platform:            {Application.platform}");
            sb.AppendLine($"AR Session State:    {ARSession.state}");

            // Individual features
            sb.AppendLine($"Plane Detection:     {BoolLabel(IsPlaneDetectionSupported())}");
            sb.AppendLine($"Depth Sensing:       {BoolLabel(IsDepthSupported())}");
            sb.AppendLine($"Light Estimation:    {BoolLabel(IsLightEstimationSupported())}");
            sb.AppendLine($"Anchor Tracking:     {BoolLabel(IsAnchorSupported())}");

            sb.AppendLine("════════════════════════════════════");
            return sb.ToString();
        }

        // ──────────────────────────── Helpers ────────────────────────────────────

        private static string FormatStatus(ARSupportStatus status)
        {
            return status switch
            {
                ARSupportStatus.Supported      => "✓ Supported",
                ARSupportStatus.CheckInProgress => "⏳ Checking…",
                ARSupportStatus.NotInstalled    => "⬇ AR runtime not installed",
                ARSupportStatus.Unsupported     => "✗ Not supported",
                _                               => "? Unknown"
            };
        }

        private static string BoolLabel(bool supported)
        {
            return supported ? "✓ Available" : "✗ Not available";
        }
    }
}
