using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MasterLoginController : MonoBehaviour
{
    [Header("UI Velden")]
    [SerializeField] private TMP_InputField  masterEmailField;
    [SerializeField] private TMP_InputField  masterPasswordField;
    [SerializeField] private TMP_InputField  masterConfirmField;
    [SerializeField] private Slider          strengthSlider;
    [SerializeField] private Image           strengthFill;
    [SerializeField] private TextMeshProUGUI strengthLabel;
    [SerializeField] private TextMeshProUGUI matchLabel;
    [SerializeField] private TextMeshProUGUI feedbackLabel;
    [SerializeField] private Button          beginButton;

    [Header("Login UI")]
    [SerializeField] private GameObject      registerPanel;   // Registratie formulier
    [SerializeField] private GameObject      loginPanel;      // Login formulier
    [SerializeField] private TMP_InputField  loginEmailField;
    [SerializeField] private TMP_InputField  loginPasswordField;
    [SerializeField] private Button          loginButton;
    [SerializeField] private TextMeshProUGUI loginFeedbackLabel;
    [SerializeField] private Button          switchToRegisterButton;

    [Header("Visibility knoppen")]
    [SerializeField] private Button          toggleMasterPwd;
    [SerializeField] private Button          toggleConfirmPwd;
    [SerializeField] private Button          toggleLoginPwd;
    [SerializeField] private Image           toggleMasterIcon;
    [SerializeField] private Image           toggleConfirmIcon;
    [SerializeField] private Image           toggleLoginIcon;

    [Header("Uitloggen")]
    [SerializeField] private Button          logoutButton;

    [Header("Panelen")]
    [SerializeField] private GameObject      masterLoginPanel;
    [SerializeField] private GameObject      pageContainer;

    [Header("Referenties")]
    [SerializeField] private PasswordVault      vault;
    [SerializeField] private TutorialManager    tutorialManager;
    [SerializeField] private BiometricManager   biometricManager;

    private readonly Color weakColor   = new Color(0.85f, 0.22f, 0.22f);
    private readonly Color mediumColor = new Color(0.95f, 0.65f, 0.10f);
    private readonly Color strongColor = new Color(0.18f, 0.72f, 0.42f);

    private bool masterPwdVisible  = false;
    private bool confirmPwdVisible = false;
    private bool loginPwdVisible   = false;

    // Opgeslagen credentials voor login na registratie
    private string savedEmail    = string.Empty;
    private string savedPassword = string.Empty;

    private void Awake()
    {
        // Registratie
        masterPasswordField.onValueChanged.AddListener(OnPasswordChanged);
        masterConfirmField.onValueChanged.AddListener(_ => CheckPasswordMatch());
        beginButton.onClick.AddListener(TryCreateAccount);

        // Login
        loginButton.onClick.AddListener(TryLogin);
        switchToRegisterButton?.onClick.AddListener(ShowRegisterPanel);

        // Visibility
        toggleMasterPwd.onClick.AddListener(ToggleMasterVisibility);
        toggleConfirmPwd.onClick.AddListener(ToggleConfirmVisibility);
        toggleLoginPwd?.onClick.AddListener(ToggleLoginVisibility);

        // Uitloggen
        logoutButton.onClick.AddListener(Logout);

        // Start met registratie panel
        ShowRegisterPanel();

        pageContainer.SetActive(false);
        matchLabel.text    = string.Empty;
        feedbackLabel.text = string.Empty;

        // Verberg uitlogknop bij start
        logoutButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            var current = EventSystem.current.currentSelectedGameObject;
            if (current != null)
            {
                var selectable = current.GetComponent<Selectable>();
                Selectable next = selectable?.FindSelectableOnDown();
                if (next != null)
                {
                    next.Select();
                    var inputField = next.GetComponent<TMP_InputField>();
                    inputField?.ActivateInputField();
                }
            }
        }
    }

    // ── Panels ───────────────────────────────────────────────────────────────

    private void ShowRegisterPanel()
    {
        registerPanel.SetActive(true);
        loginPanel.SetActive(false);
    }

    private void ShowLoginPanel()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        loginFeedbackLabel.text = string.Empty;

        // Vul email automatisch in
        if (!string.IsNullOrEmpty(savedEmail))
            loginEmailField.text = savedEmail;
    }

    // ── Registratie ───────────────────────────────────────────────────────────

    private void OnPasswordChanged(string pwd)
    {
        var result = PasswordStrengthEvaluator.Evaluate(pwd);
        strengthSlider.value = result.Score / 4f;

        (Color kleur, string label) = result.Score switch
        {
            0 => (weakColor,   "Zeer zwak"),
            1 => (weakColor,   "Zwak"),
            2 => (mediumColor, "Matig"),
            3 => (strongColor, "Sterk"),
            _ => (strongColor, "Zeer sterk"),
        };

        strengthFill.color  = kleur;
        strengthLabel.text  = label;
        strengthLabel.color = kleur;
        CheckPasswordMatch();
    }

    private void CheckPasswordMatch()
    {
        string p1 = masterPasswordField.text;
        string p2 = masterConfirmField.text;

        if (string.IsNullOrEmpty(p2))
        {
            matchLabel.text = string.Empty;
            return;
        }

        bool ok = p1 == p2;
        matchLabel.text  = ok ? "✅ Wachtwoorden komen overeen"
                              : "❌ Wachtwoorden komen niet overeen";
        matchLabel.color = ok ? strongColor : weakColor;
    }

    private void TryCreateAccount()
    {
        string email = masterEmailField.text.Trim();
        string pwd   = masterPasswordField.text;
        string pwd2  = masterConfirmField.text;

        if (string.IsNullOrEmpty(email))
        {
            ShowFeedback("⚠️ Vul je e-mailadres in.", weakColor);
            return;
        }

        if (string.IsNullOrEmpty(pwd))
        {
            ShowFeedback("⚠️ Kies een masterwachtwoord.", weakColor);
            return;
        }

        if (pwd != pwd2)
        {
            ShowFeedback("❌ Wachtwoorden komen niet overeen.", weakColor);
            return;
        }

        var eval = PasswordStrengthEvaluator.Evaluate(pwd);
        if (eval.Score < 2)
        {
            ShowFeedback("⚠️ Je masterwachtwoord is te zwak.", weakColor);
            return;
        }

        // Sla credentials op voor later inloggen
        savedEmail    = email;
        savedPassword = pwd;

        // Open browser
        OpenBrowser();

        // Biometrics instellen
        biometricManager?.SetCurrentEmail(email);
        biometricManager?.ShowSetupPopup();

        tutorialManager?.TriggerStep("master_account_created");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    private void TryLogin()
    {
        string email = loginEmailField.text.Trim();
        string pwd   = loginPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
        {
            loginFeedbackLabel.text  = "⚠️ Vul alle velden in.";
            loginFeedbackLabel.color = weakColor;
            return;
        }

        if (email == savedEmail && pwd == savedPassword)
        {
            OpenBrowser();
        }
        else
        {
            loginFeedbackLabel.text  = "❌ Onbekend e-mailadres of onjuist wachtwoord.";
            loginFeedbackLabel.color = weakColor;
        }
    }

    // ── Uitloggen ─────────────────────────────────────────────────────────────

    public void Logout()
    {
        pageContainer.SetActive(false);
        masterLoginPanel.SetActive(true);
        vault?.CloseVault();
        logoutButton.gameObject.SetActive(false);
        ShowLoginPanel();

        // Biometrics knop updaten
        biometricManager?.UpdateLoginPanelPublic();
    }

    // ── Biometrics ────────────────────────────────────────────────────────────

    public void OnBiometricLoginSuccess()
    {
        OpenBrowser();
    }

    // ── Browser openen ────────────────────────────────────────────────────────

    private void OpenBrowser()
    {
        masterLoginPanel.SetActive(false);
        pageContainer.SetActive(true);
        logoutButton.gameObject.SetActive(true);
        vault?.OpenVault();
    }

    // ── Visibility ────────────────────────────────────────────────────────────

    private void ToggleMasterVisibility()
    {
        masterPwdVisible = !masterPwdVisible;
        masterPasswordField.contentType = masterPwdVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        masterPasswordField.ForceLabelUpdate();
        toggleMasterIcon.color = masterPwdVisible
            ? new Color(0.15f, 0.45f, 0.85f)
            : new Color(0.6f, 0.6f, 0.6f);
    }

    private void ToggleConfirmVisibility()
    {
        confirmPwdVisible = !confirmPwdVisible;
        masterConfirmField.contentType = confirmPwdVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        masterConfirmField.ForceLabelUpdate();
        toggleConfirmIcon.color = confirmPwdVisible
            ? new Color(0.15f, 0.45f, 0.85f)
            : new Color(0.6f, 0.6f, 0.6f);
    }

    private void ToggleLoginVisibility()
    {
        loginPwdVisible = !loginPwdVisible;
        loginPasswordField.contentType = loginPwdVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        loginPasswordField.ForceLabelUpdate();
        toggleLoginIcon.color = loginPwdVisible
            ? new Color(0.15f, 0.45f, 0.85f)
            : new Color(0.6f, 0.6f, 0.6f);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private void ShowFeedback(string msg, Color kleur)
    {
        feedbackLabel.text  = msg;
        feedbackLabel.color = kleur;
    }
}