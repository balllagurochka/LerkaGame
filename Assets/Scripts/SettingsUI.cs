using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates and manages the settings overlay panel from code.
/// Attached to the Canvas by StartMenu.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    private GameObject settingsPanel;
    private GameObject settingsButton;

    private Slider volumeSlider;
    private Button easyBtn, normalBtn, hardBtn;
    private bool isOpen = false;

    private static TMP_FontAsset cachedFont;

    private void Start()
    {
        // Cache font once
        if (cachedFont == null)
        {
            cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (cachedFont == null)
            {
                TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                if (fonts.Length > 0) cachedFont = fonts[0];
            }
        }

        RectTransform canvasRt = GetComponent<RectTransform>();

        // Create button + overlay
        CreateSettingsButton(canvasRt);
        CreateSettingsOverlay(canvasRt);
        settingsPanel.SetActive(false);
    }

    // ---------------------------------------------------------------
    // CREATE UI
    // ---------------------------------------------------------------

    private void CreateSettingsButton(RectTransform parent)
    {
        settingsButton = MakeButton(parent, "SettingsBtn", "SETTINGS",
            new Vector2(0.5f, 0.25f), new Vector2(187, 45),
            Color.white, () => ToggleSettings());
    }

    private void CreateSettingsOverlay(RectTransform parent)
    {
        // Full-screen dim overlay
        GameObject overlay = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(parent, false);
        overlay.layer = 5;

        RectTransform rt = overlay.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = overlay.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);
        bg.raycastTarget = true;

        settingsPanel = overlay;

        // Title
        MakeLabel(rt, "SETTINGS", new Vector2(0.5f, 0.88f), 36);

        // Volume
        MakeLabel(rt, "Sound Volume", new Vector2(0.5f, 0.75f), 22);
        volumeSlider = MakeSlider(rt, "VolumeSlider", new Vector2(0.5f, 0.68f),
            SettingsManager.Instance.SfxVolume, OnVolumeChanged);

        // Difficulty
        MakeLabel(rt, "Difficulty", new Vector2(0.5f, 0.55f), 22);

        float startX = -150f;
        easyBtn   = MakeDiffButton(rt, "EasyBtn",   "Easy",   new Vector2(startX, 0.47f), 0);
        normalBtn = MakeDiffButton(rt, "NormalBtn", "Normal", new Vector2(startX + 150f, 0.47f), 1);
        hardBtn   = MakeDiffButton(rt, "HardBtn",   "Hard",   new Vector2(startX + 300f, 0.47f), 2);

        UpdateDifficultyVisuals((int)SettingsManager.Instance.CurrentDifficulty);

        // Close button
        MakeButton(rt, "BackBtn", "BACK", new Vector2(0.5f, 0.28f), new Vector2(200, 50),
            new Color(0.3f, 0.3f, 0.3f, 1), () => ToggleSettings());
    }

    // ---------------------------------------------------------------
    // UI ELEMENT BUILDERS
    // ---------------------------------------------------------------

    private void MakeLabel(Transform parent, string text, Vector2 anchorPos, int fontSize)
    {
        GameObject go = new GameObject("lbl_" + text, typeof(RectTransform), typeof(TMP_Text));
        go.transform.SetParent(parent, false);
        go.layer = 5;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorPos;
        rt.anchorMax = anchorPos;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 40);

        TMP_Text tmp = go.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.font = cachedFont;
    }

    private Slider MakeSlider(Transform parent, string name, Vector2 anchorPos,
        float value, UnityEngine.Events.UnityAction<float> onChange)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        go.layer = 5;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorPos;
        rt.anchorMax = anchorPos;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(300, 20);

        Slider slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.value = value;

        // Background
        var bg = CreateChild(go.transform, "Background", typeof(Image));
        bg.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1);
        Stretch(bg.GetComponent<RectTransform>(), new Vector2(0, 4), new Vector2(0, -4));

        // Fill
        var fillArea = CreateChild(go.transform, "FillArea");
        Stretch(fillArea.GetComponent<RectTransform>(), new Vector2(0, 4), new Vector2(0, -4));
        var fill = CreateChild(fillArea.transform, "Fill", typeof(Image));
        fill.GetComponent<Image>().color = new Color(0.2f, 0.6f, 1f, 1);
        Stretch(fill.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);

        // Handle
        var handleArea = CreateChild(go.transform, "HandleSlideArea");
        Stretch(handleArea.GetComponent<RectTransform>(), new Vector2(0, 4), new Vector2(0, -4));
        var handle = CreateChild(handleArea.transform, "Handle", typeof(Image));
        handle.GetComponent<Image>().color = Color.white;
        RectTransform hRt = handle.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0.5f, 0.5f);
        hRt.anchorMax = new Vector2(0.5f, 0.5f);
        hRt.sizeDelta = new Vector2(20, 20);

        slider.targetGraphic = handle.GetComponent<Image>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = hRt;
        slider.direction = Slider.Direction.LeftToRight;
        slider.onValueChanged.AddListener(onChange);

        return slider;
    }

    private Button MakeDiffButton(Transform parent, string name, string text,
        Vector2 anchorPos, int diffIndex)
    {
        Color color = diffIndex == 0 ? new Color(0, 0.8f, 0, 1) :
                      diffIndex == 2 ? new Color(0.8f, 0, 0, 1) :
                                       new Color(0.6f, 0.6f, 0.6f, 1);

        return MakeButton(parent, name, text, anchorPos, new Vector2(120, 45), color, () =>
        {
            SettingsManager.Instance.SetDifficulty(diffIndex);
            UpdateDifficultyVisuals(diffIndex);
        });
    }

    private GameObject MakeButton(Transform parent, string name, string text,
        Vector2 anchorPos, Vector2 size, Color color, System.Action onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.layer = 5;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorPos;
        rt.anchorMax = anchorPos;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        img.color = color;
        Sprite uiSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        if (uiSprite != null) { img.sprite = uiSprite; img.type = Image.Type.Sliced; }

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick());

        // Button text (white or black depending on button color)
        GameObject label = new GameObject("Text", typeof(RectTransform), typeof(TMP_Text));
        label.transform.SetParent(go.transform, false);
        RectTransform lRt = label.GetComponent<RectTransform>();
        lRt.anchorMin = Vector2.zero;
        lRt.anchorMax = Vector2.one;
        lRt.offsetMin = Vector2.zero;
        lRt.offsetMax = Vector2.zero;

        TMP_Text tmp = label.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color == Color.white ? Color.black : Color.white;
        tmp.font = cachedFont;

        return go;
    }

    // ---------------------------------------------------------------
    // INTERNAL HELPERS
    // ---------------------------------------------------------------

    private GameObject CreateChild(Transform parent, string name, params System.Type[] components)
    {
        GameObject go = new GameObject(name, components);
        go.transform.SetParent(parent, false);
        go.layer = 5;
        return go;
    }

    private void Stretch(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    private void UpdateDifficultyVisuals(int selected)
    {
        SetAlpha(easyBtn,   selected == 0 ? 1f : 0.3f);
        SetAlpha(normalBtn, selected == 1 ? 1f : 0.3f);
        SetAlpha(hardBtn,   selected == 2 ? 1f : 0.3f);
    }

    private void SetAlpha(Button btn, float alpha)
    {
        if (btn != null)
        {
            Color c = btn.targetGraphic.color;
            c.a = alpha;
            btn.targetGraphic.color = c;
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetVolume(value);
    }

    public void ToggleSettings()
    {
        isOpen = !isOpen;
        settingsPanel.SetActive(isOpen);
        if (settingsButton != null)
            settingsButton.SetActive(!isOpen);
    }
}