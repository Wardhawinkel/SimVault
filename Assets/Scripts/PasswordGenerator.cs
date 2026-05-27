using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wachtwoordgenerator met instelbare criteria.
/// Kan zelfstandig als panel worden geopend of ingebouwd
/// worden in andere schermen (webshop, kluis).
/// </summary>
public class PasswordGenerator : MonoBehaviour
{
    [Header("Generator UI panel")]
    [SerializeField] private GameObject generatorPanel;
    [SerializeField] private Button     openButton;
    [SerializeField] private Button     closeButton;
    [SerializeField] private Button     generateButton;
    [SerializeField] private Button     copyButton;
    [SerializeField] private Button     usePasswordButton;      // Stuur naar aanroeper

    [Header("Instellingen")]
    [SerializeField] private Slider          lengthSlider;
    [SerializeField] private TextMeshProUGUI lengthLabel;
    [SerializeField] private Toggle          useUppercase;
    [SerializeField] private Toggle          useLowercase;
    [SerializeField] private Toggle          useDigits;
    [SerializeField] private Toggle          useSymbols;

    [Header("Output")]
    [SerializeField] private TextMeshProUGUI generatedPasswordLabel;
    [SerializeField] private TextMeshProUGUI copyConfirmLabel;
    [SerializeField] private Slider          strengthSlider;
    [SerializeField] private Image           strengthFill;
    [SerializeField] private TextMeshProUGUI strengthLabel;

    [Header("Referenties")]
    [SerializeField] private TutorialManager tutorialManager;

    // Event: aanroeper kan luisteren voor het gegenereerde wachtwoord
    public event Action<string> OnPasswordSelected;

    // Karaktersets
    private const string UPPERCASE = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LOWERCASE = "abcdefghijklmnopqrstuvwxyz";
    private const string DIGITS    = "0123456789";
    private const string SYMBOLS   = "!@#$%^&*()-_=+[]{}|;:,.<>?";

    private string lastGenerated = string.Empty;

    private readonly Color weakColor   = new Color(0.85f, 0.22f, 0.22f);
    private readonly Color mediumColor = new Color(0.95f, 0.65f, 0.10f);
    private readonly Color strongColor = new Color(0.18f, 0.72f, 0.42f);

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        openButton?.onClick.AddListener(OpenPanel);
        closeButton.onClick.AddListener(ClosePanel);
        generateButton.onClick.AddListener(GenerateAndShow);
        copyButton.onClick.AddListener(CopyToClipboard);
        usePasswordButton?.onClick.AddListener(UsePassword);

        lengthSlider.onValueChanged.AddListener(v => lengthLabel.text = ((int)v).ToString());
        lengthSlider.value = 16f;
        lengthLabel.text   = "16";

        copyConfirmLabel.gameObject.SetActive(false);
        generatorPanel.SetActive(false);
    }

    // ─── Publieke methoden ────────────────────────────────────────────────────

    /// <summary>Genereer een wachtwoord op basis van huidige instellingen.</summary>
    public string Generate()
    {
        int length = (int)lengthSlider.value;

        var charset = new StringBuilder();
        var required = new StringBuilder();   // Garandeert minstens 1 van elk type

        if (useUppercase.isOn) { charset.Append(UPPERCASE); required.Append(PickRandom(UPPERCASE)); }
        if (useLowercase.isOn) { charset.Append(LOWERCASE); required.Append(PickRandom(LOWERCASE)); }
        if (useDigits.isOn)    { charset.Append(DIGITS);    required.Append(PickRandom(DIGITS));    }
        if (useSymbols.isOn)   { charset.Append(SYMBOLS);   required.Append(PickRandom(SYMBOLS));   }

        if (charset.Length == 0)
        {
            // Fallback: alles aan
            charset.Append(LOWERCASE + UPPERCASE + DIGITS + SYMBOLS);
        }

        string pool = charset.ToString();
        var result  = new char[length];

        // Vul verplichte tekens in op willekeurige posities
        string req = Shuffle(required.ToString());
        int fillLen = Math.Min(req.Length, length);
        for (int i = 0; i < fillLen; i++) result[i] = req[i];

        // Vul de rest willekeurig
        for (int i = fillLen; i < length; i++)
            result[i] = pool[UnityEngine.Random.Range(0, pool.Length)];

        // Schud eindresultaat door elkaar
        lastGenerated = Shuffle(new string(result));
        return lastGenerated;
    }

    // ─── Privé helpers ────────────────────────────────────────────────────────

    private void GenerateAndShow()
    {
        string pwd = Generate();
        generatedPasswordLabel.text = pwd;

        var eval = PasswordStrengthEvaluator.Evaluate(pwd);
        strengthSlider.value = eval.Score / 4f;

        (Color kleur, string label) = eval.Score switch
        {
            0 => (weakColor,   "Zwak"),
            1 => (weakColor,   "Zwak"),
            2 => (mediumColor, "Matig"),
            3 => (strongColor, "Sterk"),
            _ => (strongColor, "Zeer sterk"),
        };
        strengthFill.color  = kleur;
        strengthLabel.text  = label;
        strengthLabel.color = kleur;

        tutorialManager?.TriggerStep("password_generated");
    }

    private void CopyToClipboard()
    {
        if (string.IsNullOrEmpty(lastGenerated)) return;
        GUIUtility.systemCopyBuffer = lastGenerated;

        copyConfirmLabel.gameObject.SetActive(true);
        CancelInvoke(nameof(HideCopyConfirm));
        Invoke(nameof(HideCopyConfirm), 1.8f);
    }

    private void HideCopyConfirm() => copyConfirmLabel.gameObject.SetActive(false);

    private void UsePassword()
    {
        OnPasswordSelected?.Invoke(lastGenerated);
        ClosePanel();
    }

    private void OpenPanel()
    {
        generatorPanel.SetActive(true);
        tutorialManager?.TriggerStep("generator_opened");
    }

    private void ClosePanel() => generatorPanel.SetActive(false);

    private static string PickRandom(string source)
        => source[UnityEngine.Random.Range(0, source.Length)].ToString();

    private static string Shuffle(string input)
    {
        char[] arr = input.ToCharArray();
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return new string(arr);
    }
}
