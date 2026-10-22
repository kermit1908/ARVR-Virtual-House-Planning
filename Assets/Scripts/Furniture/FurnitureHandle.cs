using System;
using UnityEngine;
using HousePlanner.Core;

namespace HousePlanner.Furniture
{
    /// <summary>
    /// Component attached to placed furniture GameObjects.
    /// Manages interaction state (selection, dragging, rotation) and keeps the
    /// GameObject's transform synchronized with its underlying <see cref="FurnitureItemData"/>.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class FurnitureHandle : MonoBehaviour
    {
        [SerializeField] private FurnitureItemData _data;
        private bool _isSelected;
        private Outline _outlineComponent;
        private Renderer[] _renderers;
        private Color[] _originalColors;

        public FurnitureItemData Data => _data;
        public bool IsSelected => _isSelected;

        public event Action<FurnitureHandle> OnSelected;
        public event Action<FurnitureHandle> OnTransformChanged;

        public void Initialize(FurnitureItemData data)
        {
            _data = data;
            _renderers = GetComponentsInChildren<Renderer>();
            ApplyDataTransform();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateSelectionVisual();
        }

        private void UpdateSelectionVisual()
        {
            if (_renderers == null)
            {
                _renderers = GetComponentsInChildren<Renderer>();
            }

            foreach (var r in _renderers)
            {
                if (r != null && r.material != null)
                {
                    if (_isSelected)
                    {
                        // Slight highlight tint
                        if (r.material.HasProperty("_EmissionColor"))
                        {
                            r.material.EnableKeyword("_EMISSION");
                            r.material.SetColor("_EmissionColor", new Color(0.15f, 0.35f, 0.6f));
                        }
                    }
                    else
                    {
                        if (r.material.HasProperty("_EmissionColor"))
                        {
                            r.material.SetColor("_EmissionColor", Color.black);
                        }
                    }
                }
            }
        }

        public void ApplyDataTransform()
        {
            if (_data == null) return;
            transform.position = _data.position;
            transform.rotation = Quaternion.Euler(_data.rotation);
            transform.localScale = _data.scale;
        }

        public void SyncToData()
        {
            if (_data == null) return;
            _data.position = transform.position;
            _data.rotation = transform.rotation.eulerAngles;
            _data.scale = transform.localScale;
            OnTransformChanged?.Invoke(this);
        }

        private void OnMouseDown()
        {
            OnSelected?.Invoke(this);
        }
    }
}
