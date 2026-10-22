using UnityEngine;
using UnityEngine.UI;
using HousePlanner.Core;
using HousePlanner.Furniture;

namespace HousePlanner.UI
{
    /// <summary>
    /// UI Controller for the Selected Object Inspector panel.
    /// Allows inspecting and modifying an active furniture item's name, coordinates,
    /// rotation, uniform scale, material color, duplicating, or deleting it.
    /// </summary>
    public class InspectorPanelUI : MonoBehaviour
    {
        [Header("Container")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Fields")]
        [SerializeField] private InputField _txtName;
        [SerializeField] private Text _lblDimensions;
        [SerializeField] private Slider _sliderScale;
        [SerializeField] private Text _lblScaleValue;
        [SerializeField] private Button _btnRotateLeft;
        [SerializeField] private Button _btnRotateRight;

        [Header("Quick Color Swatches")]
        [SerializeField] private Button _btnColorWhite;
        [SerializeField] private Button _btnColorWood;
        [SerializeField] private Button _btnColorBlue;
        [SerializeField] private Button _btnColorGreen;
        [SerializeField] private Button _btnColorDark;

        [Header("Actions")]
        [SerializeField] private Button _btnDuplicate;
        [SerializeField] private Button _btnDelete;
        [SerializeField] private Button _btnClose;

        private FurnitureHandle _activeHandle;

        private void Start()
        {
            SetupListeners();

            if (FurniturePlacementController.Instance != null)
            {
                FurniturePlacementController.Instance.OnFurnitureSelected += HandleSelectionChanged;
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void SetupListeners()
        {
            if (_txtName != null) _txtName.onEndEdit.AddListener(OnNameEdited);
            if (_sliderScale != null) _sliderScale.onValueChanged.AddListener(OnScaleSliderChanged);

            if (_btnRotateLeft != null) _btnRotateLeft.onClick.AddListener(() => RotateSelected(-45f));
            if (_btnRotateRight != null) _btnRotateRight.onClick.AddListener(() => RotateSelected(45f));

            if (_btnColorWhite != null) _btnColorWhite.onClick.AddListener(() => SetColor(Color.white));
            if (_btnColorWood != null) _btnColorWood.onClick.AddListener(() => SetColor(new Color(0.6f, 0.4f, 0.22f)));
            if (_btnColorBlue != null) _btnColorBlue.onClick.AddListener(() => SetColor(new Color(0.25f, 0.45f, 0.75f)));
            if (_btnColorGreen != null) _btnColorGreen.onClick.AddListener(() => SetColor(new Color(0.35f, 0.65f, 0.4f)));
            if (_btnColorDark != null) _btnColorDark.onClick.AddListener(() => SetColor(new Color(0.2f, 0.2f, 0.22f)));

            if (_btnDuplicate != null) _btnDuplicate.onClick.AddListener(OnDuplicateClicked);
            if (_btnDelete != null) _btnDelete.onClick.AddListener(OnDeleteClicked);
            if (_btnClose != null) _btnClose.onClick.AddListener(Deselect);
        }

        private void HandleSelectionChanged(FurnitureHandle handle)
        {
            _activeHandle = handle;

            if (_panelRoot == null) return;

            if (handle == null || handle.Data == null)
            {
                _panelRoot.SetActive(false);
                return;
            }

            _panelRoot.SetActive(true);

            if (_txtName != null)
            {
                _txtName.text = !string.IsNullOrEmpty(handle.Data.customName) 
                    ? handle.Data.customName 
                    : handle.Data.catalogueId;
            }

            if (_sliderScale != null)
            {
                _sliderScale.value = handle.Data.scale.x;
            }

            if (_lblScaleValue != null)
            {
                _lblScaleValue.text = $"{handle.Data.scale.x:F1}x";
            }
        }

        private void OnNameEdited(string newName)
        {
            if (_activeHandle != null && _activeHandle.Data != null)
            {
                _activeHandle.Data.customName = newName;
            }
        }

        private void OnScaleSliderChanged(float val)
        {
            if (_activeHandle != null)
            {
                _activeHandle.transform.localScale = Vector3.one * val;
                _activeHandle.SyncToData();
                if (_lblScaleValue != null)
                {
                    _lblScaleValue.text = $"{val:F1}x";
                }
            }
        }

        private void RotateSelected(float delta)
        {
            if (FurniturePlacementController.Instance != null)
            {
                FurniturePlacementController.Instance.RotateSelected(delta);
            }
        }

        private void SetColor(Color col)
        {
            if (FurniturePlacementController.Instance != null)
            {
                FurniturePlacementController.Instance.SetSelectedColor(col);
            }
        }

        private void OnDuplicateClicked()
        {
            if (FurniturePlacementController.Instance != null)
            {
                FurniturePlacementController.Instance.DuplicateSelected();
            }
        }

        private void OnDeleteClicked()
        {
            if (FurniturePlacementController.Instance != null)
            {
                FurniturePlacementController.Instance.DeleteSelected();
            }
        }

        private void Deselect()
        {
            if (FurniturePlacementController.Instance != null)
            {
                FurniturePlacementController.Instance.DeselectCurrent();
            }
        }
    }
}
