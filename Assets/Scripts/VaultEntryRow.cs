using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VaultEntryRow : MonoBehaviour
{
    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI siteLabel;
    [SerializeField] private TextMeshProUGUI usernameLabel;
    [SerializeField] private TextMeshProUGUI passwordLabel;

    [Header("Knoppen")]
    [SerializeField] private Button          togglePasswordButton;
    [SerializeField] private Button          copyPasswordButton;
    [SerializeField] private Button          deleteButton;
    [SerializeField] private TextMeshProUGUI copyConfirmLabel;

    [Header("Hash educatie")]
    [SerializeField] private Button               showHashButton;
    

    private VaultEntry   data;
    private PasswordVault vault;
    private bool          passwordShown = false;

    public void Setup(VaultEntry entry, PasswordVault parentVault)
    {
        data  = entry;
        vault = parentVault;

        siteLabel.text     = entry.SiteName;
        usernameLabel.text = entry.Username;
        passwordLabel.text = MaskPassword(entry.Password);

        togglePasswordButton.onClick.AddListener(TogglePassword);
        copyPasswordButton.onClick.AddListener(CopyPassword);
        deleteButton.onClick.AddListener(DeleteEntry);
        showHashButton.onClick.AddListener(ShowHash);

        copyConfirmLabel.gameObject.SetActive(false);
    }

    private void TogglePassword()
    {
        passwordShown      = !passwordShown;
        passwordLabel.text = passwordShown
            ? data.Password
            : MaskPassword(data.Password);

        var btnLabel = togglePasswordButton.GetComponentInChildren<TextMeshProUGUI>();
        if (btnLabel != null)
            btnLabel.text = passwordShown ? "🙈" : "👁";
    }

    private void CopyPassword()
    {
        GUIUtility.systemCopyBuffer = data.Password;
        copyConfirmLabel.gameObject.SetActive(true);
        CancelInvoke(nameof(HideCopyConfirm));
        Invoke(nameof(HideCopyConfirm), 1.8f);
    }

    private void HideCopyConfirm() => copyConfirmLabel.gameObject.SetActive(false);

    private void DeleteEntry()
    {
        vault.DeleteEntry(data.SiteUrl, data.Username);
        // Vault.DeleteEntry roept RefreshUI aan
    }

    private void ShowHash()
{
    Debug.Log("[Hash] ShowHash aangeroepen!");
    // true = zoek ook in inactieve objecten
    var popup = FindObjectOfType<HashPopupController>(true);
    Debug.Log($"[Hash] Popup gevonden: {(popup == null ? "NULL" : popup.name)}");
    popup?.Show(data.Password);
}

    private string MaskPassword(string pwd)
    {
        return new string('•', Mathf.Min(pwd.Length, 20));
    }
}