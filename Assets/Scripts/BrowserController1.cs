using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Hoofdcontroller voor de fictieve browser.
/// Beheert navigatie, adresbalk en paginawisseling.
/// </summary>
public class BrowserController : MonoBehaviour
{
    [Header("Browser Chrome UI")]
    [SerializeField] private TMP_InputField addressBar;
    [SerializeField] private Button backButton;
    [SerializeField] private Button forwardButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button goButton;
    [SerializeField] private Image secureIcon;       // Slotje groen/rood
    [SerializeField] private TextMeshProUGUI pageTitle;

    [Header("Pagina Containers")]
    [SerializeField] private GameObject homePagePanel;
    [SerializeField] private GameObject emailLoginPanel;
    [SerializeField] private GameObject webshopPanel;
    [SerializeField] private GameObject errorPagePanel;

    [Header("Visuele feedback")]
    [SerializeField] private Color secureColor = new Color(0.18f, 0.72f, 0.42f);
    [SerializeField] private Color insecureColor = new Color(0.85f, 0.25f, 0.25f);
    [SerializeField] private Sprite lockClosedSprite;
    [SerializeField] private Sprite lockOpenSprite;

    [SerializeField] private TutorialManager tutorialManager;
    private int homeClickCount = 0;

    private bool isTutorialNavigating = false;

    // Interne navigatiegeschiedenis
    private List<string> history = new List<string>();
    private int historyIndex = -1;

    // Bekende fictieve URLs
    private readonly Dictionary<string, PageInfo> knownPages = new Dictionary<string, PageInfo>()
    {
        { "home",           new PageInfo("Homepage",              "sim://home",               true)  },
        { "sim://home",     new PageInfo("Homepage",              "sim://home",               true)  },
        { "sim://mail",     new PageInfo("MailSim – Inloggen",      "sim://mail",               true)  },
        { "sim://shop",     new PageInfo("ShopSim – Webshop",       "sim://shop",               true)  },
    };

    private GameObject currentPanel;

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        backButton.onClick.AddListener(GoBack);
        forwardButton.onClick.AddListener(GoForward);
        refreshButton.onClick.AddListener(Refresh);
        goButton.onClick.AddListener(() => NavigateTo(addressBar.text));
        addressBar.onSubmit.AddListener(url => NavigateTo(url));
    }

    private void Start()
    {
        NavigateTo("home", recordHistory: false);
    }

    // ─── Navigatie ────────────────────────────────────────────────────────────

    /// <summary>Navigeer naar een URL of alias.</summary>
    public void NavigateTo(string url, bool recordHistory = true)
    {
        string normalised = url.Trim().ToLower();

        // Alias "home" opvangen
        if (string.IsNullOrEmpty(normalised)) normalised = "sim://home";

        // Geschiedenis bijhouden
        if (recordHistory)
        {
            // Verwijder forward-history bij nieuwe navigatie
            if (historyIndex < history.Count - 1)
                history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);

            history.Add(normalised);
            historyIndex = history.Count - 1;
        }

        UpdateNavButtons();
        LoadPage(normalised);
        if (!isTutorialNavigating)
        {
            if (normalised == "sim://home")
            {
                homeClickCount++;
                if (homeClickCount == 1)
                    tutorialManager?.TriggerStep("home_clicked");
                else
                    tutorialManager?.ForceNextStep();
            }
            if (normalised == "sim://mail")
                tutorialManager?.TriggerStep("mailsim_clicked");
            if (normalised == "sim://shop")
                tutorialManager?.TriggerStep("shopsim_clicked");
        }
            
    }

    public void NavigateFromTutorial(string url)
    {
        isTutorialNavigating = true;
        NavigateTo(url, false);
        isTutorialNavigating = false;
    }

    private void GoBack()
    {
        if (historyIndex <= 0) return;
        historyIndex--;
        LoadPage(history[historyIndex]);
        UpdateNavButtons();
    }

    private void GoForward()
    {
        if (historyIndex >= history.Count - 1) return;
        historyIndex++;
        LoadPage(history[historyIndex]);
        UpdateNavButtons();
    }

    private void Refresh()
    {
        if (historyIndex >= 0) LoadPage(history[historyIndex]);
    }

    // ─── Pagina laden ─────────────────────────────────────────────────────────

    private void LoadPage(string url)
    {
        // Adresbalk bijwerken
        addressBar.text = url.Replace("sim://", "https://");

        // Alle panels verbergen
        SetAllPanelsInactive();

        if (knownPages.TryGetValue(url, out PageInfo info))
        {
            pageTitle.text = info.Title;
            UpdateSecureIndicator(info.IsSecure);
            ActivatePanel(url);
        }
        else
        {
            // Onbekende URL → foutpagina
            pageTitle.text = "Pagina niet gevonden";
            UpdateSecureIndicator(false);
            errorPagePanel.SetActive(true);
            currentPanel = errorPagePanel;
        }
    }

    private void ActivatePanel(string url)
    {
        GameObject target = url switch
        {
            "sim://mail" => emailLoginPanel,
            "sim://shop" => webshopPanel,
            _            => homePagePanel,
        };

        target.SetActive(true);
        currentPanel = target;
    }

    private void SetAllPanelsInactive()
    {
        homePagePanel.SetActive(false);
        emailLoginPanel.SetActive(false);
        webshopPanel.SetActive(false);
        errorPagePanel.SetActive(false);
    }

    // ─── UI helpers ───────────────────────────────────────────────────────────

    private void UpdateNavButtons()
    {
        backButton.interactable    = historyIndex > 0;
        forwardButton.interactable = historyIndex < history.Count - 1;
    }

    private void UpdateSecureIndicator(bool secure)
    {
        secureIcon.color  = secure ? secureColor : insecureColor;
        secureIcon.sprite = secure ? lockClosedSprite : lockOpenSprite;
    }

    // ─── Publieke helpers voor andere scripts ─────────────────────────────────

    /// <summary>Navigeer vanuit een knop op een pagina (bv. "Ga naar webshop").</summary>
    public void NavigateButton(string url) => NavigateTo(url);
}

// ─── Data-klasse ──────────────────────────────────────────────────────────────

[System.Serializable]
public class PageInfo
{
    public string Title;
    public string Url;
    public bool   IsSecure;

    public PageInfo(string title, string url, bool secure)
    {
        Title    = title;
        Url      = url;
        IsSecure = secure;
    }
}
