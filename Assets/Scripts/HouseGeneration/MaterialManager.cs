using System.Collections.Generic;
using UnityEngine;

namespace HousePlanner.HouseGeneration
{
    /// <summary>
    /// Manages materials used in 3D house generation and customization.
    /// Supports runtime creation and caching of materials for walls, floors, ceilings,
    /// and furniture with built-in color palettes and texture configurations.
    /// </summary>
    public class MaterialManager : MonoBehaviour
    {
        private static MaterialManager _instance;
        public static MaterialManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<MaterialManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MaterialManager");
                        _instance = go.AddComponent<MaterialManager>();
                    }
                }
                return _instance;
            }
        }

        private readonly Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();
        private Shader _defaultShader;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            InitializeShader();
        }

        private void InitializeShader()
        {
            // Try URP Lit shader first, fallback to Standard or mobile diffuse
            _defaultShader = Shader.Find("Universal Render Pipeline/Lit");
            if (_defaultShader == null)
            {
                _defaultShader = Shader.Find("Standard");
            }
            if (_defaultShader == null)
            {
                _defaultShader = Shader.Find("Mobile/Diffuse");
            }
            if (_defaultShader == null)
            {
                _defaultShader = Shader.Find("Diffuse");
            }
        }

        /// <summary>
        /// Gets an existing material or creates a new cached one with the specified color.
        /// </summary>
        public Material GetOrCreateMaterial(string key, Color color, float smoothness = 0.2f)
        {
            if (_materialCache.TryGetValue(key, out Material existing) && existing != null)
            {
                return existing;
            }

            if (_defaultShader == null)
            {
                InitializeShader();
            }

            Material mat = new Material(_defaultShader != null ? _defaultShader : Shader.Find("Sprites/Default"));
            mat.name = $"Mat_{key}";
            mat.color = color;

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", smoothness);
            }
            else if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", smoothness);
            }

            _materialCache[key] = mat;
            return mat;
        }

        /// <summary>
        /// Creates a wall material from a color hex or name.
        /// </summary>
        public Material CreateWallMaterial(Color color)
        {
            string key = $"wall_{ColorUtility.ToHtmlStringRGBA(color)}";
            return GetOrCreateMaterial(key, color, 0.1f);
        }

        /// <summary>
        /// Creates a floor material based on common floor surface types.
        /// </summary>
        public Material CreateFloorMaterial(string materialType)
        {
            string lower = string.IsNullOrEmpty(materialType) ? "wood" : materialType.ToLowerInvariant();
            switch (lower)
            {
                case "wood":
                case "hardwood":
                case "parquet":
                    return GetOrCreateMaterial("floor_wood", new Color(0.65f, 0.45f, 0.28f), 0.4f);

                case "tile":
                case "ceramic":
                    return GetOrCreateMaterial("floor_tile", new Color(0.85f, 0.85f, 0.88f), 0.6f);

                case "marble":
                    return GetOrCreateMaterial("floor_marble", new Color(0.92f, 0.92f, 0.94f), 0.8f);

                case "carpet":
                    return GetOrCreateMaterial("floor_carpet", new Color(0.4f, 0.45f, 0.5f), 0.05f);

                case "concrete":
                    return GetOrCreateMaterial("floor_concrete", new Color(0.55f, 0.55f, 0.55f), 0.15f);

                default:
                    // If it's a hex color code
                    if (lower.StartsWith("#") && ColorUtility.TryParseHtmlString(lower, out Color parsedColor))
                    {
                        return GetOrCreateMaterial($"floor_{lower}", parsedColor, 0.3f);
                    }
                    return GetOrCreateMaterial("floor_default", new Color(0.75f, 0.72f, 0.68f), 0.3f);
            }
        }

        /// <summary>
        /// Creates a clean white or off-white ceiling material.
        /// </summary>
        public Material CreateCeilingMaterial()
        {
            return GetOrCreateMaterial("ceiling_default", new Color(0.95f, 0.95f, 0.95f), 0.05f);
        }

        /// <summary>
        /// Creates a material for furniture instances.
        /// </summary>
        public Material CreateFurnitureMaterial(string id, Color color)
        {
            string key = $"furniture_{id}_{ColorUtility.ToHtmlStringRGBA(color)}";
            return GetOrCreateMaterial(key, color, 0.35f);
        }

        /// <summary>
        /// Clears all cached materials.
        /// </summary>
        public void ClearCache()
        {
            _materialCache.Clear();
        }
    }
}
