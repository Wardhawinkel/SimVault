using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class WebshopPage : MonoBehaviour
{
    [Header("Navigatietabs")]
    [SerializeField] private Button      tabShop;
    [SerializeField] private Button      tabAccount;
    [SerializeField] private Button      tabLogin;
    [SerializeField] private GameObject  shopPanel;
    [SerializeField] private GameObject  accountPanel;
    [SerializeField] private GameObject  shopLoginPanel;

    [Header("Winkelmandje (decoratief)")]
    [SerializeField] private TextMeshProUGUI cartCountLabel;
    private int cartCount = 0;
    [SerializeField] private Button[]    addToCartButtons;

    [Header("Registratieformulier")]
    [SerializeField] private TMP_InputField  regNameField;
    [SerializeField] private TMP_InputField  regEmailField;
    [SerializeField] private TMP_InputField  regPasswordField;
    [SerializeField] private TMP_InputField  regPasswordConfirmField;
    [SerializeField] private Button          generatePasswordButton;
    [SerializeField] private Button          saveToVaultButton;
    [SerializeField] private Button          registerButton;
    [SerializeField] private Button          clearButton;

    [Header("Wachtwoordsterkte registratie")]
    [SerializeField] private Slider          strengthSlider;
    [SerializeField] private Image           strengthFill;
    [SerializeField] private TextMeshProUGUI strengthLabel;
    [SerializeField] private TextMeshProUGUI matchLabel;
    [SerializeField] private TextMeshProUGUI feedbackLabel;
    [SerializeField] private GameObject      successPanel;
    [SerializeField] private GameObject      vaultSavedPopup;

    [Header("Login formulier")]
    [SerializeField] private TMP_InputField  shopEmailField;
    [SerializeField] private TMP_InputField  shopPasswordField;
    [SerializeField] private Button          shopAutofillButton;
    [SerializeField] private Button          shopLoginButton;
    [SerializeField] private TextMeshProUGUI shopFeedbackLabel;
    [SerializeField] private GameObject      shopSuccessLoginPanel;

    [Header("Referenties")]
    [SerializeField] private PasswordVault        vault;
    [SerializeField] private PasswordGenerator    generator;
    [SerializeField] private TutorialManager      tutorialManager;

    [Header("Visibility login")]
    [SerializeField] private Button          toggleShopLoginPwd;
    [SerializeField] private TextMeshProUGUI toggleShopLoginIcon;

    [Header("Visibility")]
    [SerializeField] private Button toggleRegPwd;
    [SerializeField] private Button toggleRegConfirmPwd;

    private readonly Color weakColor   = new Color(0.85f, 0.22f, 0.22f);
    private readonly Color mediumColor = new Color(0.95f, 0.65f, 0.10f);
    private readonly Color strongColor = new Color(0.18f, 0.72f, 0.42f);

    private bool shopLoginPwdVisible = false;
    private bool regPwdVisible        = false;
    private bool regConfirmPwdVisible = false;


    private void Awake()
    {
        // Tabs
        tabShop.onClick.AddListener(()    => SwitchTab("shop"));
        tabAccount.onClick.AddListener(() => SwitchTab("account"));
        tabLogin.onClick.AddListener(()   => SwitchTab("login"));
        toggleShopLoginPwd.onClick.AddListener(ToggleShopLoginVisibility);
        toggleRegPwd.onClick.AddListener(ToggleRegPwdVisibility);
        toggleRegConfirmPwd.onClick.AddListener(ToggleRegConfirmVisibility);

        // Winkelmandje
        foreach (var btn in addToCartButtons)
            btn.onClick.AddListener(AddToCart);

        // Registratie
        regPasswordField.onValueChanged.AddListener(OnPasswordChanged);
        regPasswordConfirmField.onValueChanged.AddListener(_ => CheckPasswordMatch());
        generatePasswordButton.onClick.AddListener(GeneratePassword);
        saveToVaultButton.onClick.AddListener(SaveToVault);
        registerButton.onClick.AddListener(TryRegister);
        clearButton.onClick.AddListener(ClearForm);

        // Login
        shopAutofillButton.onClick.AddListener(AutofillShop);
        shopLoginButton.onClick.AddListener(TryShopLogin);

        // Generator
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
        SwitchTab("shop");
        tutorialManager?.TriggerStep("webshop_opened");
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

    private void ToggleShopLoginVisibility()
{
    shopLoginPwdVisible = !shopLoginPwdVisible;
    shopPasswordField.contentType = shopLoginPwdVisible
        ? TMP_InputField.ContentType.Standard
        : TMP_InputField.ContentType.Password;
    shopPasswordField.ForceLabelUpdate();
}

private void ToggleRegPwdVisibility()
{
    regPwdVisible = !regPwdVisible;
    regPasswordField.contentType = regPwdVisible
        ? TMP_InputField.ContentType.Standard
        : TMP_InputField.ContentType.Password;
    regPasswordField.ForceLabelUpdate();
}

private void ToggleRegConfirmVisibility()
{
    regConfirmPwdVisible = !regConfirmPwdVisible;
    regPasswordConfirmField.contentType = regConfirmPwdVisible
        ? TMP_InputField.ContentType.Standard
        : TMP_InputField.ContentType.Password;
    regPasswordConfirmField.ForceLabelUpdate();
}

    // ── Tabs ─────────────────────────────────────────────────────────────────

    private void SwitchTab(string tab)
    {
        shopPanel.SetActive(tab == "shop");
        accountPanel.SetActive(tab == "account");
        shopLoginPanel.SetActive(tab == "login");

        SetTabActive(tabShop,    tab == "shop");
        SetTabActive(tabAccount, tab == "account");
        SetTabActive(tabLogin,   tab == "login");

        if (tab == "account")
            tutorialManager?.TriggerStep("registration_tab_opened");
    }

    private void SetTabActive(Button btn, bool active)
    {
        var colors = btn.colors;
        colors.normalColor = active
            ? new Color(0.15f, 0.45f, 0.85f)
            : new Color(0.22f, 0.22f, 0.28f);
        btn.colors = colors;
    }

    // ── Winkelmandje ──────────────────────────────────────────────────────────

    private void AddToCart()
    {
        cartCount++;
        cartCountLabel.text = cartCount.ToString();
    }

    // ── Registratie ───────────────────────────────────────────────────────────

    private void GeneratePassword()
    {
        if (generator == null) return;
        string pwd = generator.Generate();
        regPasswordField.text        = pwd;
        regPasswordConfirmField.text = pwd;
        feedbackLabel.text  = "✅ Sterk wachtwoord gegenereerd! Sla het op in je kluis.";
        feedbackLabel.color = strongColor;
        tutorialManager?.TriggerStep("password_generated_on_shop");
    }

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
        string p1 = regPasswordField.text;
        string p2 = regPasswordConfirmField.text;
        if (string.IsNullOrEmpty(p2)) { matchLabel.text = ""; return; }
        bool ok = p1 == p2;
        matchLabel.text  = ok ? "✅ Wachtwoorden komen overeen" : "❌ Wachtwoorden komen niet overeen";
        matchLabel.color = ok ? strongColor : weakColor;
    }

    private void SaveToVault()
    {
        if (vault == null) return;
        string email = regEmailField.text.Trim();
        string pwd   = regPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
        {
            feedbackLabel.text  = "⚠️ Vul je e-mailadres en wachtwoord in voor je opslaat.";
            feedbackLabel.color = mediumColor;
            return;
        }

        vault.SaveEntry(new VaultEntry
        {
            SiteUrl  = "sim://shop",
            SiteName = "ShopSim",
            Username = email,
            Password = pwd
        });

        vaultSavedPopup.SetActive(true);
        Invoke(nameof(HideVaultPopup), 2.5f);
        tutorialManager?.TriggerStep("password_saved_to_vault");
    }

    private void HideVaultPopup() => vaultSavedPopup.SetActive(false);

    private void TryRegister()
    {
        string name  = regNameField.text.Trim();
        string email = regEmailField.text.Trim();
        string pwd   = regPasswordField.text;
        string pwd2  = regPasswordConfirmField.text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
        {
            feedbackLabel.text  = "⚠️ Vul alle velden in.";
            feedbackLabel.color = weakColor;
            return;
        }

        if (pwd != pwd2)
        {
            feedbackLabel.text  = "❌ Wachtwoorden komen niet overeen.";
            feedbackLabel.color = weakColor;
            return;
        }

        var eval = PasswordStrengthEvaluator.Evaluate(pwd);
        if (eval.Score < 2)
        {
            feedbackLabel.text  = "⚠️ Kies een sterker wachtwoord. Gebruik de generator!";
            feedbackLabel.color = weakColor;
            tutorialManager?.TriggerStep("weak_password_on_register");
            return;
        }

        successPanel.SetActive(true);
        tutorialManager?.TriggerStep("registration_success");
        Invoke(nameof(HideRegSuccessPanel), 3f);
    }

    private void HideRegSuccessPanel()
    {
        successPanel.SetActive(false);
    }

    private void ClearForm()
    {
        regNameField.text            = string.Empty;
        regEmailField.text           = string.Empty;
        regPasswordField.text        = string.Empty;
        regPasswordConfirmField.text = string.Empty;
        feedbackLabel.text           = string.Empty;
        strengthLabel.text           = string.Empty;
        matchLabel.text              = string.Empty;
        strengthSlider.value         = 0f;
        successPanel.SetActive(false);
        tutorialManager?.TriggerStep("wis");
    }

    // ── Shop Login ────────────────────────────────────────────────────────────

    private void AutofillShop()
    {
        if (vault == null) return;

        string email = shopEmailField.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            shopFeedbackLabel.text  = "ℹ️ Vul eerst je e-mailadres in, " +
                                    "dan vullen we het wachtwoord automatisch in.";
            shopFeedbackLabel.color = mediumColor;
            return;
        }

        var entry = vault.GetEntryForSiteAndUser("sim://shop", email);
        if (entry != null)
        {
            shopPasswordField.text  = entry.Password;
            shopFeedbackLabel.text  = "✅ Wachtwoord ingevuld vanuit je kluis!";
            shopFeedbackLabel.color = strongColor;
            tutorialManager?.TriggerStep("autofill_used");
        }
        else
        {
            shopFeedbackLabel.text  = $"ℹ️ Geen wachtwoord gevonden voor {email}. " +
                                    "Maak eerst een account aan.";
            shopFeedbackLabel.color = mediumColor;
        }
    }

    private void TryShopLogin()
    {
        string email = shopEmailField.text.Trim();
        string pwd   = shopPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd))
        {
            shopFeedbackLabel.text  = "⚠️ Vul alle velden in.";
            shopFeedbackLabel.color = weakColor;
            return;
        }

        var entry = vault?.GetEntryForSite("sim://shop");
        if (entry != null && email == entry.Username && pwd == entry.Password)
        {
            shopSuccessLoginPanel.SetActive(true);
            shopFeedbackLabel.text  = string.Empty;
            tutorialManager?.TriggerStep("shop_login_success");
            Invoke(nameof(HideShopSuccessPanel), 3f);
        }
        else
        {
            shopFeedbackLabel.text  = "❌ Onbekend e-mailadres of onjuist wachtwoord.";
            shopFeedbackLabel.color = weakColor;
        }
    }

    private void OnPasswordSelectedFromGenerator(string password)
    {
        if (accountPanel.activeSelf)
        {
            regPasswordField.text        = password;
            regPasswordConfirmField.text = password;
            feedbackLabel.text  = "✅ Wachtwoord ingevuld vanuit generator!";
            feedbackLabel.color = strongColor;
        }
    }

    private void HideShopSuccessPanel()
    {
        shopSuccessLoginPanel.SetActive(false);
    }
}