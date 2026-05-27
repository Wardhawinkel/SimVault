using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HashPopupController : MonoBehaviour
{
    [SerializeField] private GameObject      popupPanel;
    [SerializeField] private TextMeshProUGUI wachtwoordWaarde;
    [SerializeField] private TextMeshProUGUI hashWaardeLabel;
    [SerializeField] private Button          closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
        popupPanel.SetActive(false);
    }

    public void Show(string password)
{
    Debug.Log("[Hash] Show() aangeroepen!");
    Debug.Log($"[Hash] popupPanel null: {popupPanel == null}");
    Debug.Log($"[Hash] password: {password}");
    
    wachtwoordWaarde.text = password;
    hashWaardeLabel.text = HashHelper.ComputeSHA256(password);
    popupPanel.SetActive(true);
    
    Debug.Log($"[Hash] popupPanel actief na SetActive: {popupPanel.activeSelf}");
}

    public void Hide()
    {
        popupPanel.SetActive(false);
    }
}