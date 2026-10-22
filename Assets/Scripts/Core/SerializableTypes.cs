using System;
using System.Globalization;
using UnityEngine;

namespace HousePlanner.Core
{
    /// <summary>
    /// JSON-friendly serializable version of <see cref="UnityEngine.Vector2"/>.
    /// </summary>
    [Serializable]
    public struct Vector2Serializable
    {
        /// <summary>X component of the vector.</summary>
        public float x;

        /// <summary>Y component of the vector.</summary>
        public float y;

        /// <summary>
        /// Creates a new <see cref="Vector2Serializable"/> with the given components.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        public Vector2Serializable(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>Returns a <see cref="Vector2Serializable"/> with all components set to zero.</summary>
        public static Vector2Serializable Zero => new Vector2Serializable(0f, 0f);

        /// <summary>Returns a <see cref="Vector2Serializable"/> with all components set to one.</summary>
        public static Vector2Serializable One => new Vector2Serializable(1f, 1f);

        /// <summary>Returns the normalized vector as a Unity Vector2.</summary>
        public Vector2 normalized => ((Vector2)this).normalized;

        /// <summary>Returns the length of this vector.</summary>
        public float magnitude => ((Vector2)this).magnitude;

        /// <summary>Returns the squared length of this vector.</summary>
        public float sqrMagnitude => ((Vector2)this).sqrMagnitude;

        /// <summary>
        /// Implicitly converts a <see cref="Vector2Serializable"/> to a Unity <see cref="Vector2"/>.
        /// </summary>
        public static implicit operator Vector2(Vector2Serializable v)
        {
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        /// Implicitly converts a Unity <see cref="Vector2"/> to a <see cref="Vector2Serializable"/>.
        /// </summary>
        public static implicit operator Vector2Serializable(Vector2 v)
        {
            return new Vector2Serializable(v.x, v.y);
        }

        // --- Operator Overloads ---
        public static Vector2Serializable operator +(Vector2Serializable a, Vector2Serializable b)
            => new Vector2Serializable(a.x + b.x, a.y + b.y);

        public static Vector2Serializable operator -(Vector2Serializable a, Vector2Serializable b)
            => new Vector2Serializable(a.x - b.x, a.y - b.y);

        public static Vector2Serializable operator *(Vector2Serializable a, float d)
            => new Vector2Serializable(a.x * d, a.y * d);

        public static Vector2Serializable operator *(float d, Vector2Serializable a)
            => new Vector2Serializable(a.x * d, a.y * d);

        public static Vector2Serializable operator /(Vector2Serializable a, float d)
            => new Vector2Serializable(a.x / d, a.y / d);

        public static Vector2 operator +(Vector2Serializable a, Vector2 b)
            => new Vector2(a.x + b.x, a.y + b.y);

        public static Vector2 operator +(Vector2 a, Vector2Serializable b)
            => new Vector2(a.x + b.x, a.y + b.y);

        public static Vector2 operator -(Vector2Serializable a, Vector2 b)
            => new Vector2(a.x - b.x, a.y - b.y);

        public static Vector2 operator -(Vector2 a, Vector2Serializable b)
            => new Vector2(a.x - b.x, a.y - b.y);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({x:F2}, {y:F2})";
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is Vector2Serializable other)
            {
                return Mathf.Approximately(x, other.x) && Mathf.Approximately(y, other.y);
            }
            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }

    /// <summary>
    /// JSON-friendly serializable version of <see cref="UnityEngine.Vector3"/>.
    /// </summary>
    [Serializable]
    public struct Vector3Serializable
    {
        /// <summary>X component of the vector.</summary>
        public float x;

        /// <summary>Y component of the vector.</summary>
        public float y;

        /// <summary>Z component of the vector.</summary>
        public float z;

        /// <summary>
        /// Creates a new <see cref="Vector3Serializable"/> with the given components.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        public Vector3Serializable(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>Returns a <see cref="Vector3Serializable"/> with all components set to zero.</summary>
        public static Vector3Serializable Zero => new Vector3Serializable(0f, 0f, 0f);

        /// <summary>Returns a <see cref="Vector3Serializable"/> with all components set to one.</summary>
        public static Vector3Serializable One => new Vector3Serializable(1f, 1f, 1f);

        /// <summary>Returns the normalized vector as a Unity Vector3.</summary>
        public Vector3 normalized => ((Vector3)this).normalized;

        /// <summary>Returns the length of this vector.</summary>
        public float magnitude => ((Vector3)this).magnitude;

        /// <summary>Returns the squared length of this vector.</summary>
        public float sqrMagnitude => ((Vector3)this).sqrMagnitude;

        /// <summary>
        /// Implicitly converts a <see cref="Vector3Serializable"/> to a Unity <see cref="Vector3"/>.
        /// </summary>
        public static implicit operator Vector3(Vector3Serializable v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        /// <summary>
        /// Implicitly converts a Unity <see cref="Vector3"/> to a <see cref="Vector3Serializable"/>.
        /// </summary>
        public static implicit operator Vector3Serializable(Vector3 v)
        {
            return new Vector3Serializable(v.x, v.y, v.z);
        }

        // --- Operator Overloads ---
        public static Vector3Serializable operator +(Vector3Serializable a, Vector3Serializable b)
            => new Vector3Serializable(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vector3Serializable operator -(Vector3Serializable a, Vector3Serializable b)
            => new Vector3Serializable(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Vector3Serializable operator *(Vector3Serializable a, float d)
            => new Vector3Serializable(a.x * d, a.y * d, a.z * d);

        public static Vector3Serializable operator *(float d, Vector3Serializable a)
            => new Vector3Serializable(a.x * d, a.y * d, a.z * d);

        public static Vector3Serializable operator /(Vector3Serializable a, float d)
            => new Vector3Serializable(a.x / d, a.y / d, a.z / d);

        public static Vector3 operator +(Vector3Serializable a, Vector3 b)
            => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vector3 operator +(Vector3 a, Vector3Serializable b)
            => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vector3 operator +(Vector3Serializable a, Vector2 b)
            => new Vector3(a.x + b.x, a.y, a.z + b.y);

        public static Vector3 operator +(Vector2 a, Vector3Serializable b)
            => new Vector3(a.x + b.x, b.y, a.y + b.z);

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({x:F2}, {y:F2}, {z:F2})";
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is Vector3Serializable other)
            {
                return Mathf.Approximately(x, other.x)
                    && Mathf.Approximately(y, other.y)
                    && Mathf.Approximately(z, other.z);
            }
            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
    }

    /// <summary>
    /// JSON-friendly serializable version of <see cref="UnityEngine.Color"/>.
    /// </summary>
    [Serializable]
    public struct ColorSerializable
    {
        /// <summary>Red channel (0–1).</summary>
        public float r;

        /// <summary>Green channel (0–1).</summary>
        public float g;

        /// <summary>Blue channel (0–1).</summary>
        public float b;

        /// <summary>Alpha channel (0–1).</summary>
        public float a;

        /// <summary>
        /// Creates a new <see cref="ColorSerializable"/> with the given channels.
        /// </summary>
        /// <param name="r">Red channel (0–1).</param>
        /// <param name="g">Green channel (0–1).</param>
        /// <param name="b">Blue channel (0–1).</param>
        /// <param name="a">Alpha channel (0–1).</param>
        public ColorSerializable(float r, float g, float b, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        /// <summary>
        /// Implicitly converts a <see cref="ColorSerializable"/> to a Unity <see cref="Color"/>.
        /// </summary>
        public static implicit operator Color(ColorSerializable c)
        {
            return new Color(c.r, c.g, c.b, c.a);
        }

        /// <summary>
        /// Implicitly converts a Unity <see cref="Color"/> to a <see cref="ColorSerializable"/>.
        /// </summary>
        public static implicit operator ColorSerializable(Color c)
        {
            return new ColorSerializable(c.r, c.g, c.b, c.a);
        }

        /// <summary>
        /// Creates a <see cref="ColorSerializable"/> from a hexadecimal color string.
        /// </summary>
        public static ColorSerializable FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                return new ColorSerializable(1f, 1f, 1f, 1f);
            }

            string cleaned = hex.TrimStart('#');

            if (cleaned.Length != 6 && cleaned.Length != 8)
            {
                return new ColorSerializable(1f, 1f, 1f, 1f);
            }

            float r = int.Parse(cleaned.Substring(0, 2), NumberStyles.HexNumber) / 255f;
            float g = int.Parse(cleaned.Substring(2, 2), NumberStyles.HexNumber) / 255f;
            float b = int.Parse(cleaned.Substring(4, 2), NumberStyles.HexNumber) / 255f;
            float a = cleaned.Length == 8
                ? int.Parse(cleaned.Substring(6, 2), NumberStyles.HexNumber) / 255f
                : 1f;

            return new ColorSerializable(r, g, b, a);
        }

        /// <summary>
        /// Converts a Unity <see cref="Color"/> to a hexadecimal string in "#RRGGBB" format.
        /// </summary>
        public static string ToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255f);
            int g = Mathf.RoundToInt(color.g * 255f);
            int b = Mathf.RoundToInt(color.b * 255f);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            Color c = this;
            return ToHex(c);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is ColorSerializable other)
            {
                return Mathf.Approximately(r, other.r)
                    && Mathf.Approximately(g, other.g)
                    && Mathf.Approximately(b, other.b)
                    && Mathf.Approximately(a, other.a);
            }
            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(r, g, b, a);
        }
    }
}
