using System;
using UnityEngine;
using UnityEngine.UI;

namespace HousePlanner.UI
{
    /// <summary>
    /// Modal dialog controller for confirmation prompts, text inputs, alerts, and error messages.
    /// Provides static helper methods to show popups anywhere in the application.
    /// </summary>
    public class DialogUI : MonoBehaviour
    {
        private static DialogUI _instance;
        public static DialogUI Instance => _instance;

        [Header("Containers")]
        [SerializeField] private GameObject _dialogRoot;
        [SerializeField] private GameObject _confirmPanel;
        [SerializeField] private GameObject _inputPanel;
        [SerializeField] private GameObject _messagePanel;

        [Header("Confirm Dialog")]
        [SerializeField] private Text _confirmTitleText;
        [SerializeField] private Text _confirmMessageText;
        [SerializeField] private Button _confirmOkButton;
        [SerializeField] private Button _confirmCancelButton;

        [Header("Input Dialog")]
        [SerializeField] private Text _inputTitleText;
        [SerializeField] private InputField _inputTextField;
        [SerializeField] private Button _inputSubmitButton;
        [SerializeField] private Button _inputCancelButton;

        [Header("Message Dialog")]
        [SerializeField] private Text _messageTitleText;
        [SerializeField] private Text _messageBodyText;
        [SerializeField] private Button _messageOkButton;

        private Action _onConfirmCallback;
        private Action _onCancelCallback;
        private Action<string> _onInputSubmitCallback;
        private Action _onMessageOkCallback;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // Wire button clicks
            if (_confirmOkButton != null) _confirmOkButton.onClick.AddListener(OnConfirmOkClicked);
            if (_confirmCancelButton != null) _confirmCancelButton.onClick.AddListener(OnConfirmCancelClicked);
            if (_inputSubmitButton != null) _inputSubmitButton.onClick.AddListener(OnInputSubmitClicked);
            if (_inputCancelButton != null) _inputCancelButton.onClick.AddListener(OnInputCancelClicked);
            if (_messageOkButton != null) _messageOkButton.onClick.AddListener(OnMessageOkClicked);

            HideAll();
        }

        public void HideAll()
        {
            if (_confirmPanel != null) _confirmPanel.SetActive(false);
            if (_inputPanel != null) _inputPanel.SetActive(false);
            if (_messagePanel != null) _messagePanel.SetActive(false);
            if (_dialogRoot != null) _dialogRoot.SetActive(false);
        }

        public static void ShowConfirmation(string title, string message, Action onConfirm, Action onCancel = null)
        {
            if (_instance == null)
            {
                Debug.Log($"[DialogUI] Confirm: {title} - {message}");
                onConfirm?.Invoke();
                return;
            }

            _instance.HideAll();
            if (_instance._dialogRoot != null) _instance._dialogRoot.SetActive(true);
            if (_instance._confirmPanel != null) _instance._confirmPanel.SetActive(true);

            if (_instance._confirmTitleText != null) _instance._confirmTitleText.text = title;
            if (_instance._confirmMessageText != null) _instance._confirmMessageText.text = message;

            _instance._onConfirmCallback = onConfirm;
            _instance._onCancelCallback = onCancel;
        }

        public static void ShowInputDialog(string title, string defaultText, Action<string> onSubmit, Action onCancel = null)
        {
            if (_instance == null)
            {
                Debug.Log($"[DialogUI] Input: {title}");
                onSubmit?.Invoke(defaultText);
                return;
            }

            _instance.HideAll();
            if (_instance._dialogRoot != null) _instance._dialogRoot.SetActive(true);
            if (_instance._inputPanel != null) _instance._inputPanel.SetActive(true);

            if (_instance._inputTitleText != null) _instance._inputTitleText.text = title;
            if (_instance._inputTextField != null) _instance._inputTextField.text = defaultText;

            _instance._onInputSubmitCallback = onSubmit;
            _instance._onCancelCallback = onCancel;
        }

        public static void ShowMessage(string title, string message, Action onOk = null)
        {
            if (_instance == null)
            {
                Debug.Log($"[DialogUI] Message: {title} - {message}");
                onOk?.Invoke();
                return;
            }

            _instance.HideAll();
            if (_instance._dialogRoot != null) _instance._dialogRoot.SetActive(true);
            if (_instance._messagePanel != null) _instance._messagePanel.SetActive(true);

            if (_instance._messageTitleText != null) _instance._messageTitleText.text = title;
            if (_instance._messageBodyText != null) _instance._messageBodyText.text = message;

            _instance._onMessageOkCallback = onOk;
        }

        public static void ShowError(string title, string error)
        {
            ShowMessage($"⚠️ {title}", error);
        }

        private void OnConfirmOkClicked()
        {
            HideAll();
            _onConfirmCallback?.Invoke();
        }

        private void OnConfirmCancelClicked()
        {
            HideAll();
            _onCancelCallback?.Invoke();
        }

        private void OnInputSubmitClicked()
        {
            string value = _inputTextField != null ? _inputTextField.text : "";
            HideAll();
            _onInputSubmitCallback?.Invoke(value);
        }

        private void OnInputCancelClicked()
        {
            HideAll();
            _onCancelCallback?.Invoke();
        }

        private void OnMessageOkClicked()
        {
            HideAll();
            _onMessageOkCallback?.Invoke();
        }
    }
}
