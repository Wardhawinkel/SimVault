using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial UI")]
    [SerializeField] private GameObject      overlayRoot;
    [SerializeField] private TextMeshProUGUI stepTitle;
    [SerializeField] private TextMeshProUGUI stepBody;
    [SerializeField] private TextMeshProUGUI stepCounter;
    [SerializeField] private Button          nextButton;
    [SerializeField] private Button          skipButton;
    [SerializeField] private Image           arrowImage;
    [SerializeField] private RectTransform   highlightRect;
    [SerializeField] private Image           dimOverlay;

    [Header("Stappen")]
    [SerializeField] private List<TutorialStep> steps;

    private int currentIndex = -1;
    private Dictionary<string, int> triggerMap;
    private float pulseTimer = 0f;

    private void Awake()
    {
        BuildTriggerMap();
        nextButton.onClick.AddListener(NextStep);
        skipButton.onClick.AddListener(EndTutorial);
        overlayRoot.SetActive(false);

        if (dimOverlay != null)
            dimOverlay.gameObject.SetActive(false);
    }

    private void Start()
    {
        Invoke(nameof(BeginTutorial), 0.5f);
    }

    private void Update()
    {
        // Pijl pulsen
        if (arrowImage.gameObject.activeSelf)
        {
            pulseTimer += Time.deltaTime * 3f;
            float scale = 1f + Mathf.Sin(pulseTimer) * 0.15f;
            arrowImage.rectTransform.localScale = Vector3.one * scale;
        }

        // Highlight knipperen
        if (highlightRect.gameObject.activeSelf)
        {
            var outline = highlightRect.GetComponent<Outline>();
            if (outline != null)
            {
                float alpha = (Mathf.Sin(pulseTimer * 2f) + 1f) / 2f;
                var col = outline.effectColor;
                col.a = alpha;
                outline.effectColor = col;
            }
        }
    }

    public void TriggerStep(string triggerKey)
    {
        Debug.Log($"[Tutorial] TriggerStep aangeroepen met key: '{triggerKey}'");
        Debug.Log($"[Tutorial] Huidige index: {currentIndex}");

        if (triggerMap.TryGetValue(triggerKey, out int idx))
        {
            Debug.Log($"[Tutorial] Key gevonden op index: {idx}");
            if (idx > currentIndex)
                ShowStep(idx);
            else
                Debug.Log($"[Tutorial] Index {idx} niet groter dan huidig {currentIndex} — stap overgeslagen");
        }
        else
        {
            Debug.Log($"[Tutorial] Key '{triggerKey}' NIET gevonden in triggerMap!");
        }
    }

    public void ForceNextStep()
    {
        NextStep();
    }

    private void BeginTutorial()
    {
        if (steps == null || steps.Count == 0) return;
        ShowStep(0);
    }

    private void NextStep()
    {
        int next = currentIndex + 1;
        if (next < steps.Count)
            ShowStep(next);
        else
            EndTutorial();
    }

    private void ShowStep(int index)
{
    currentIndex = index;
    var step = steps[index];

    overlayRoot.SetActive(true);
    stepTitle.text   = step.Title;
    stepBody.text    = step.Body;
    stepCounter.text = $"Stap {index + 1} / {steps.Count}";

    // Canvas referentie
    Canvas canvas = GetComponentInParent<Canvas>();
    if (canvas == null) canvas = FindObjectOfType<Canvas>();
    RectTransform canvasRect = canvas.GetComponent<RectTransform>();
    Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay 
        ? null 
        : canvas.worldCamera;

    // Highlight
    if (step.HighlightTarget != null)
    {
        highlightRect.gameObject.SetActive(true);

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            cam, step.HighlightTarget.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            cam,
            out Vector2 localPos
        );

        highlightRect.anchoredPosition = localPos;
        highlightRect.sizeDelta = step.HighlightTarget.rect.size + Vector2.one * 16f;
        highlightRect.GetComponent<Image>().raycastTarget = false;
    }
    else
    {
        highlightRect.gameObject.SetActive(false);
    }

    // Pijl
    if (step.ArrowTarget != null)
    {
        arrowImage.gameObject.SetActive(true);

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            cam, step.ArrowTarget.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            cam,
            out Vector2 localPos
        );

        arrowImage.rectTransform.anchoredPosition = localPos + step.ArrowOffset;
        arrowImage.rectTransform.rotation =
            Quaternion.Euler(0, 0, step.ArrowAngle);
    }
    else
    {
        arrowImage.gameObject.SetActive(false);
    }

    if (dimOverlay != null)
        dimOverlay.gameObject.SetActive(true);

    nextButton.GetComponentInChildren<TextMeshProUGUI>().text =
        index == steps.Count - 1 ? "Afronden ✓" : "Volgende →";
}

    public void EndTutorial()
    {
        overlayRoot.SetActive(false);
        highlightRect.gameObject.SetActive(false);
        arrowImage.gameObject.SetActive(false);
        if (dimOverlay != null)
            dimOverlay.gameObject.SetActive(false);
    }

    private void BuildTriggerMap()
    {
        triggerMap = new Dictionary<string, int>();
        for (int i = 0; i < steps.Count; i++)
        {
            Debug.Log($"[Tutorial] Stap {i}: TriggerKey='{steps[i].TriggerKey}' Title='{steps[i].Title}'");
            if (!string.IsNullOrEmpty(steps[i].TriggerKey))
                if (!triggerMap.ContainsKey(steps[i].TriggerKey))
                    triggerMap[steps[i].TriggerKey] = i;
        }
        Debug.Log($"[Tutorial] TriggerMap gebouwd met {triggerMap.Count} keys");
    }
}

[System.Serializable]
public class TutorialStep
{
    [Tooltip("Unieke sleutel die andere scripts gebruiken om deze stap te activeren.")]
    public string TriggerKey;

    [Tooltip("Korte titel van de stap.")]
    public string Title;

    [TextArea(2, 5)]
    [Tooltip("Uitleg in begrijpelijk Nederlands.")]
    public string Body;

    [Tooltip("UI-element om te highlighten (optioneel).")]
    public RectTransform HighlightTarget;

    [Tooltip("Element waar de pijl naar wijst (optioneel).")]
    public RectTransform ArrowTarget;

    [Tooltip("Offset van de pijl t.o.v. het doelwit.")]
    public Vector2 ArrowOffset = new Vector2(-60f, 0f);

    [Tooltip("Rotatiehoek van de pijl in graden.")]
    public float ArrowAngle = 0f;
}