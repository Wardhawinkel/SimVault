using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gesimuleerde wachtwoordkluis.
/// Beheert opgeslagen entries en toont ze in een UI-lijst.
/// Alle data is fictief en bestaat alleen in memory (geen PlayerPrefs).
/// </summary>
public class PasswordVault : MonoBehaviour
{
    [Header("Kluis UI")]
    [SerializeField] private GameObject      vaultPanel;
    [SerializeField] private Button          openVaultButton;
    [SerializeField] private Button          closeVaultButton;
    [SerializeField] private Transform       entryContainer;    // Scroll content
    [SerializeField] private GameObject      entryPrefab;       // VaultEntryRow prefab
    [SerializeField] private TextMeshProUGUI emptyLabel;
    [SerializeField] private Button          logoutButton;

    [Header("Referenties")]
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private MasterLoginController masterLogin;

    // In-memory opslag (geen persistentie — sandbox)
    private readonly List<VaultEntry> entries = new List<VaultEntry>();

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        openVaultButton.onClick.AddListener(OpenVault);
        closeVaultButton.onClick.AddListener(CloseVault);
        vaultPanel.SetActive(false);
        logoutButton?.onClick.AddListener(OnLogoutClicked);
    }

    // ─── Publieke API ─────────────────────────────────────────────────────────

    /// <summary>Sla een nieuw item op of update een bestaand item voor dezelfde site.</summary>
    public void SaveEntry(VaultEntry newEntry)
{
    int idx = entries.FindIndex(e => 
        e.SiteUrl == newEntry.SiteUrl && 
        e.Username == newEntry.Username);
    
    if (idx >= 0)
        entries[idx] = newEntry;
    else
        entries.Add(newEntry);

    RefreshUI();
    tutorialManager?.TriggerStep("vault_entry_saved");
}

    /// <summary>Haal de opgeslagen entry op voor een bepaalde site-URL.</summary>
    public VaultEntry GetEntryForSite(string siteUrl)
        => entries.Find(e => e.SiteUrl == siteUrl);

    /// <summary>Verwijder een entry op basis van site-URL.</summary>
   public void DeleteEntry(string siteUrl, string username)
    {
        entries.RemoveAll(e => e.SiteUrl == siteUrl && e.Username == username);
        RefreshUI();
    }

    // ─── Kluis UI ─────────────────────────────────────────────────────────────

    public void OpenVault()
    {
        vaultPanel.SetActive(true);
        RefreshUI();
        //tutorialManager?.TriggerStep("vault_opened");
    }

    public void CloseVault()
    {
        vaultPanel.SetActive(false);
    }

    /// <summary>Haal entry op basis van site EN gebruikersnaam.</summary>
    public VaultEntry GetEntryForSiteAndUser(string siteUrl, string username)
        => entries.Find(e => e.SiteUrl == siteUrl && 
                         e.Username.ToLower() == username.ToLower());

    private void RefreshUI()
{
    Debug.Log($"[Vault] RefreshUI aangeroepen. Aantal entries: {entries.Count}");
    Debug.Log($"[Vault] EntryContainer: {(entryContainer == null ? "NULL" : entryContainer.name)}");
    Debug.Log($"[Vault] EntryPrefab: {(entryPrefab == null ? "NULL" : entryPrefab.name)}");

    // Verwijder bestaande rijen
    foreach (Transform child in entryContainer)
        Destroy(child.gameObject);

    emptyLabel.gameObject.SetActive(entries.Count == 0);

    foreach (var entry in entries)
    {
        Debug.Log($"[Vault] Aanmaken rij voor: {entry.SiteName}");
        var row = Instantiate(entryPrefab, entryContainer);
        Debug.Log($"[Vault] Rij aangemaakt: {row.name}");
        var rowScript = row.GetComponent<VaultEntryRow>();
        Debug.Log($"[Vault] RowScript: {(rowScript == null ? "NULL" : "OK")}");
        rowScript?.Setup(entry, this);
    }
}

    public void OpenVaultByUser()
    {
        OpenVault();
        tutorialManager?.TriggerStep("vault_opened");
    }

    private void OnLogoutClicked()
    {
        CloseVault();
        masterLogin?.Logout();
        tutorialManager?.TriggerStep("user_logged_out");
    }

}

// ─── Data-model ───────────────────────────────────────────────────────────────

[System.Serializable]
public class VaultEntry
{
    public string SiteUrl;
    public string SiteName;
    public string Username;
    public string Password;
}
