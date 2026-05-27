using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

public class BiometricManager : MonoBehaviour
{
    [Header("Setup Popup")]
    [SerializeField] private GameObject      biometricSetupPopup;
    [SerializeField] private Button          registerBiometricButton;
    [SerializeField] private Button          skipBiometricButton;
    [SerializeField] private TextMeshProUGUI popupStatusLabel;

    [Header("Login Panel")]
    [SerializeField] private GameObject      loginBiometricPanel;
    [SerializeField] private Button          loginBiometricButton;

    [Header("Referenties")]
    [SerializeField] private MasterLoginController masterLogin;
    [SerializeField] private TutorialManager       tutorialManager;

    private readonly Color successColor = new Color(0.18f, 0.72f, 0.42f);
    private readonly Color errorColor   = new Color(0.85f, 0.22f, 0.22f);
    private readonly Color infoColor    = new Color(0.95f, 0.65f, 0.10f);

    private string currentEmail = string.Empty;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RegisterBiometric(string userId, string userName);

    [DllImport("__Internal")]
    private static extern void AuthenticateWithBiometric();

    [DllImport("__Internal")]
    private static extern bool IsBiometricAvailable();
#endif

    private void Awake()
    {
        registerBiometricButton.onClick.AddListener(RegisterBiometrics);
        skipBiometricButton.onClick.AddListener(() =>
            {
                ClosePopup();
                tutorialManager?.TriggerStep("biometric_skipped");
            });
        loginBiometricButton.onClick.AddListener(LoginWithBiometrics);

        biometricSetupPopup.SetActive(false);
    }

    private void Start()
    {
        UpdateLoginPanel();
    }

    // ── Publieke methoden ─────────────────────────────────────────────────────

    public void SetCurrentEmail(string email)
    {
        currentEmail = email;
        UpdateLoginPanel();
    }

    public void ShowSetupPopup()
    {
        biometricSetupPopup.SetActive(true);
        popupStatusLabel.text = string.Empty;
    
        // Tutorial triggeren
        tutorialManager?.TriggerStep("biometric_setup_shown");
    }

    public void OnBiometricLoginSuccess()
    {
        masterLogin?.OnBiometricLoginSuccess();
    }

    // ── Registratie ───────────────────────────────────────────────────────────

    private void RegisterBiometrics()
    {
        if (string.IsNullOrEmpty(currentEmail))
        {
            ShowPopupStatus("⚠️ Geen e-mailadres gevonden.", infoColor);
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        ShowPopupStatus("🔐 Volg de instructies van je browser...", infoColor);
        RegisterBiometric(currentEmail, currentEmail);
#else
        ShowPopupStatus("ℹ️ Biometrics werkt alleen in de browser.", infoColor);
#endif
    }

    private void LoginWithBiometrics()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        AuthenticateWithBiometric();
#else
        ShowPopupStatus("ℹ️ Biometrics werkt alleen in de browser.", infoColor);
#endif
    }

    private void ClosePopup()
    {
        biometricSetupPopup.SetActive(false);       
    }

    private void UpdateLoginPanel()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        bool available = IsBiometricAvailable();
        loginBiometricPanel.SetActive(available);
#else
        loginBiometricPanel.SetActive(false);
#endif
    }

    // ── Callbacks vanuit JavaScript ───────────────────────────────────────────

    public void OnBiometricRegistered(string result)
    {
        ShowPopupStatus("✅ Biometrics geregistreerd!", successColor);
        Invoke(nameof(ClosePopup), 2f);
        UpdateLoginPanel();
        tutorialManager?.TriggerStep("biometric_registered");
    }

    public void OnBiometricAuthenticated(string result)
    {
        masterLogin?.OnBiometricLoginSuccess();
        tutorialManager?.TriggerStep("biometric_authenticated");
    }

    public void OnBiometricError(string error)
    {
        ShowPopupStatus($"❌ {error}", errorColor);
    }

    public void OnBiometricNotSupported(string message)
    {
        ShowPopupStatus("ℹ️ Je apparaat ondersteunt geen biometrics.", infoColor);
        registerBiometricButton.interactable = false;
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private void ShowPopupStatus(string msg, Color color)
    {
        popupStatusLabel.text  = msg;
        popupStatusLabel.color = color;
    }

    private void OnEnable()
    {
        UpdateLoginPanel();
    }

    public void UpdateLoginPanelPublic()
    {
        UpdateLoginPanel();
    }

    
}