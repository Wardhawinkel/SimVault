using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Collections;

public class LanguageSwitcher : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonLabel;
    private bool isNL = true;

    private void Start()
    {
        Debug.Log("[Language] LanguageSwitcher gestart");
        Debug.Log($"[Language] LocalizationSettings: " +
            $"{(LocalizationSettings.Instance == null ? "NULL" : "OK")}");
    }

    public void ToggleLanguage()
    {
        Debug.Log("[Language] ToggleLanguage aangeroepen!");
        StartCoroutine(SwitchLanguage());
    }

    private IEnumerator SwitchLanguage()
    {
        isNL = !isNL;
        string targetLocale = isNL ? "nl" : "en";

        Debug.Log($"[Language] Wisselen naar: {targetLocale}");

        var locale = LocalizationSettings.AvailableLocales.GetLocale(targetLocale);
        Debug.Log($"[Language] Locale gevonden: {(locale == null ? "NULL" : locale.LocaleName)}");

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            Debug.Log($"[Language] Locale ingesteld: {LocalizationSettings.SelectedLocale.LocaleName}");
        }

        if (buttonLabel != null)
            buttonLabel.text = isNL ? "EN" : "NL";

        yield return null;
    }
}