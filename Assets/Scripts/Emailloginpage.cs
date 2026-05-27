using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class EmailLoginPage : MonoBehaviour
{
    [Header("Tabs")]
    [SerializeField] private Button     tabLogin;
    [SerializeField] private Button     tabRegister;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;

    [Header("Login formulier")]
    [SerializeField] private TMP_InputField  emailField;
    [SerializeField] private TMP_InputField  passwordField;
    [SerializeField] private Button          togglePasswordVisibility;
    [SerializeField] private TextMeshProUGUI visibilityIcon;
    [SerializeField] private Slider          strengthSlider;
    [SerializeField] private Image           strengthFill;
    [SerializeField] private TextMeshProUGUI strengthLabel;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button          loginButton;
    [SerializeField] private Button          autofillButton;
    [SerializeField] private GameObject      successPanel;
    [SerializeField] private GameObject      errorPanel;
    [SerializeField] private TextMeshProUGUI errorMessage;

    [Header("Registratie formulier")]
    [SerializeField] private TMP_InputField  mailRegEmailField;
    [SerializeField] private TMP_InputField  mailRegPasswordField;
    [SerializeField] private TMP_InputField  mailRegPasswordConfirmField;
    [SerializeField] private Slider          mailStrengthSlider;
    [SerializeField] private Image           mailStrengthFill;
    [SerializeField] private TextMeshProUGUI mailStrengthLabel;
    [SerializeField] private TextMeshProUGUI mailMatchLabel;
    [SerializeField] private Button          mailGenerateButton;
    [SerializeField] private Button          mailSaveToVaultButton;
    [SerializeField] private Button          mailRegisterButton;
    [SerializeField] private TextMeshProUGUI mailFeedbackLabel;
    [SerializeField] private GameObject      mailSuccessPanel;

    [Header("Kleuren")]
    [SerializeField] private Color weakColor   = new Color(0.85f, 0.22f, 0.22f);
    [SerializeField] private Color mediumColor = new Color(0.95f, 0.65f, 0.10f);
    [SerializeField] private Color strongColor = new Color(0.18f, 0.72f, 0.42f);

    [Header("Referenties")]
    [SerializeField] private PasswordVault     vault;
    [SerializeField] private TutorialManager   tutorialManager;
    [SerializeField] private PasswordGenerator generator;

    [Header("Visibility registratie")]
    [SerializeField] private Button toggleMailRegPwd;
    [SerializeField] private Button toggleMailConfirmPwd;

    private bool mailRegPwdVisible     = false;
    private bool mailConfirmPwdVisible = false;    
    private bool passwordVisible = false;

    private void Awake()
    {
        tabLogin.onClick.AddListener(()    => SwitchTab(showLogin: true));
        tabRegister.onClick.AddListener(() => SwitchTab(showLogin: false));
        toggleMailRegPwd.onClick.AddListener(ToggleMailRegVisibility);
        toggleMailConfirmPwd.onClick.AddListener(ToggleMailConfirmVisibility);

        passwordField.onValueChanged.AddListener(OnPasswordChanged);
        loginButton.onClick.AddListener(TryLogin);
        togglePasswordVisibility.onClick.AddListener(ToggleVisibility);
        autofillButton.onClick.AddListener(AutofillFromVault);

        mailRegPasswordField.onValueChanged.AddListener(OnMailPasswordChanged);
        mailRegPasswordConfirmField.onValueChanged.AddListener(_ => CheckMailPasswordMatch());
        mailGenerateButton.onClick.AddListener(GenerateMailPassword);
        mailSaveToVaultButton.onClick.AddListener(SaveMailToVault);
        mailRegisterButton.onClick.AddListener(TryMailRegister);

        if (generator != null)
            generator.OnPasswordSelected += OnPasswordSelectedFromGenerator;
    }

    private void OnDestroy()
    {
        if (generator != null)
            generator.OnPasswordSelected -= OnPasswordSelectedFromGenerator;
    }

    private void OnEnable()
    {
        SwitchTab(showLogin: true);
        tutorialManager?.TriggerStep("email_login_opened");
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

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var current = EventSystem.current.currentSelectedGameObject;
            if (current != null)
            {
                var inputField = current.GetComponent<TMP_InputField>();
                if (inputField != null && !inputField.isFocused)
                    inputField.ActivateInputField();
            }
        }
    }

    // ── Tabs ─────────────────────────────────────────────────────────────────

    private void SwitchTab(bool showLogin)
    {
        loginPanel.SetActive(showLogin);
        registerPanel.SetActive(!showLogin);
        SetTabActive(tabLogin,    showLogin);
        SetTabActive(tabRegister, !showLogin);
    }

    private void SetTabActive(Button tab, bool active)
    {
        var colors = tab.colors;
        colors.normalColor = active
            ? new Color(0.15f, 0.45f, 0.85f)
            : new Color(0.22f, 0.22f, 0.28f);
        tab.colors = colors;
    }

    // ── Login ─────────────────────────────────────────────────────────────────

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
        feedbackText.text   = result.Feedback;
        feedbackText.color  = result.Score < 2 ? weakColor : Color.white;
    }

    private void TryLogin()
    {
        string email = emailField.text.Trim();
        string pwd   = passwordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
        {
            ShowError("Vul alle velden in om in te loggen.");
            return;
        }

        var entry = vault?.GetEntryForSiteAndUser("sim://mail", email);
        if (entry != null && pwd == entry.Password)
        {
            successPanel.SetActive(true);
            errorPanel.SetActive(false);
            tutorialManager?.TriggerStep("login_success");
            Invoke(nameof(HideSuccessPanel), 3f);
            return;
        }

        var eval = PasswordStrengthEvaluator.Evaluate(pwd);
        if (eval.Score < 2)
        {
            ShowError("⚠️ Je wachtwoord is te zwak. Gebruik de generator.");
            tutorialManager?.TriggerStep("weak_password_detected");
            return;
        }

        ShowError("❌ Onbekend e-mailadres of onjuist wachtwoord.");
    }

    private void AutofillFromVault()
    {
        if (vault == null) return;

        string email = emailField.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            feedbackText.text  = "ℹ️ Vul eerst je e-mailadres in, " +
                                 "dan vullen we het wachtwoord automatisch in.";
            feedbackText.color = mediumColor;
            return;
        }

        var entry = vault.GetEntryForSiteAndUser("sim://mail", email);
        if (entry != null)
        {
            passwordField.text = entry.Password;
            feedbackText.text  = "✅ Wachtwoord ingevuld vanuit je kluis!";
            feedbackText.color = strongColor;
            tutorialManager?.TriggerStep("autofill_used");
        }
        else
        {
            feedbackText.text  = $"ℹ️ Geen wachtwoord gevonden voor {email}. " +
                                  "Maak eerst een account aan via Registreren.";
            feedbackText.color = mediumColor;
        }
    }

    private void HideSuccessPanel()
    {
        successPanel.SetActive(false);
    }

    // ── Registratie ───────────────────────────────────────────────────────────

    private void OnMailPasswordChanged(string pwd)
    {
        var result = PasswordStrengthEvaluator.Evaluate(pwd);
        mailStrengthSlider.value = result.Score / 4f;

        (Color kleur, string label) = result.Score switch
        {
            0 => (weakColor,   "Zeer zwak"),
            1 => (weakColor,   "Zwak"),
            2 => (mediumColor, "Matig"),
            3 => (strongColor, "Sterk"),
            _ => (strongColor, "Zeer sterk"),
        };

        mailStrengthFill.color  = kleur;
        mailStrengthLabel.text  = label;
        mailStrengthLabel.color = kleur;
        CheckMailPasswordMatch();
    }

    private void CheckMailPasswordMatch()
    {
        string p1 = mailRegPasswordField.text;
        string p2 = mailRegPasswordConfirmField.text;
        if (string.IsNullOrEmpty(p2)) { mailMatchLabel.text = ""; return; }
        bool ok = p1 == p2;
        mailMatchLabel.text  = ok ? "✅ Wachtwoorden komen overeen"
                                  : "❌ Wachtwoorden komen niet overeen";
        mailMatchLabel.color = ok ? strongColor : weakColor;
    }

    private void GenerateMailPassword()
    {
        if (generator == null) return;
        string pwd = generator.Generate();
        mailRegPasswordField.text        = pwd;
        mailRegPasswordConfirmField.text = pwd;
        mailFeedbackLabel.text  = "✅ Sterk wachtwoord gegenereerd!";
        mailFeedbackLabel.color = strongColor;
    }

    private void SaveMailToVault()
    {
        if (vault == null) return;
        string email = mailRegEmailField.text.Trim();
        string pwd   = mailRegPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
        {
            mailFeedbackLabel.text  = "⚠️ Vul je e-mailadres en wachtwoord in.";
            mailFeedbackLabel.color = mediumColor;
            return;
        }

        vault.SaveEntry(new VaultEntry
        {
            SiteUrl  = "sim://mail",
            SiteName = "MailSim",
            Username = email,
            Password = pwd
        });

        mailFeedbackLabel.text  = "✅ Opgeslagen in kluis!";
        mailFeedbackLabel.color = strongColor;
        vault.OpenVault();
        Debug.Log($"[Mail] TutorialManager: {(tutorialManager == null ? "NULL" : "OK")}");
        tutorialManager?.TriggerStep("mail_password_saved");
        Debug.Log("[Mail] TriggerStep aangeroepen");
    }

    private void TryMailRegister()
    {
        string email = mailRegEmailField.text.Trim();
        string pwd   = mailRegPasswordField.text;
        string pwd2  = mailRegPasswordConfirmField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
        {
            mailFeedbackLabel.text  = "⚠️ Vul alle velden in.";
            mailFeedbackLabel.color = weakColor;
            return;
        }

        if (pwd != pwd2)
        {
            mailFeedbackLabel.text  = "❌ Wachtwoorden komen niet overeen.";
            mailFeedbackLabel.color = weakColor;
            return;
        }

        var eval = PasswordStrengthEvaluator.Evaluate(pwd);
        if (eval.Score < 2)
        {
            mailFeedbackLabel.text  = "⚠️ Kies een sterker wachtwoord!";
            mailFeedbackLabel.color = weakColor;
            return;
        }

        mailSuccessPanel.SetActive(true);
        tutorialManager?.TriggerStep("mail_registration_success");
        Invoke(nameof(HideMailSuccessPanel), 3f);
    }

    private void ToggleMailRegVisibility()
{
    mailRegPwdVisible = !mailRegPwdVisible;
    mailRegPasswordField.contentType = mailRegPwdVisible
        ? TMP_InputField.ContentType.Standard
        : TMP_InputField.ContentType.Password;
    mailRegPasswordField.ForceLabelUpdate();
}

private void ToggleMailConfirmVisibility()
{
    mailConfirmPwdVisible = !mailConfirmPwdVisible;
    mailRegPasswordConfirmField.contentType = mailConfirmPwdVisible
        ? TMP_InputField.ContentType.Standard
        : TMP_InputField.ContentType.Password;
    mailRegPasswordConfirmField.ForceLabelUpdate();
}

    private void HideMailSuccessPanel()
    {
        mailSuccessPanel.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void ToggleVisibility()
    {
        passwordVisible = !passwordVisible;
        passwordField.contentType = passwordVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        passwordField.ForceLabelUpdate();
        visibilityIcon.text = passwordVisible ? "🙈" : "👁";
    }

    private void OnPasswordSelectedFromGenerator(string password)
    {
        if (loginPanel.activeSelf)
        {
            passwordField.text = password;
            feedbackText.text  = "✅ Wachtwoord ingevuld vanuit generator!";
            feedbackText.color = strongColor;
        }
        else
        {
            mailRegPasswordField.text        = password;
            mailRegPasswordConfirmField.text = password;
            mailFeedbackLabel.text  = "✅ Wachtwoord ingevuld vanuit generator!";
            mailFeedbackLabel.color = strongColor;
        }
    }

    private void ShowError(string msg)
    {
        errorPanel.SetActive(true);
        successPanel.SetActive(false);
        errorMessage.text = msg;
    }
}