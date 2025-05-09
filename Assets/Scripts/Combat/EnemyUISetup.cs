using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUISetup : MonoBehaviour
{
    // This method would run in the editor to create the UI structure
    [ContextMenu("Setup Enemy UI Structure")]
    public void SetupEnemyUIStructure()
    {
        // Clear existing children
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Create health bar
        GameObject healthBarObj = CreateUIElement("HealthBar", this.transform);
        Slider healthBar = healthBarObj.AddComponent<Slider>();
        healthBar.minValue = 0;
        healthBar.maxValue = 100;
        healthBar.value = 100;

        // Create fill area and fill
        GameObject fillArea = CreateUIElement("Fill Area", healthBarObj.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-5, 0);

        GameObject fill = CreateUIElement("Fill", fillArea.transform);
        fill.AddComponent<Image>().color = Color.red;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        healthBar.fillRect = fill.GetComponent<RectTransform>();

        // Create health text
        GameObject healthTextObj = CreateUIElement("HealthText", this.transform);
        TMP_Text healthText = healthTextObj.AddComponent<TextMeshProUGUI>();
        healthText.text = "100/100";
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.fontSize = 14;
        SetRectTransform(healthTextObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-50, -20), new Vector2(50, 0));

        // Create block display
        GameObject blockDisplay = CreateUIElement("BlockDisplay", this.transform);
        Image blockBg = blockDisplay.AddComponent<Image>();
        blockBg.color = new Color(0, 0.7f, 1, 0.7f);
        SetRectTransform(blockDisplay.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -40), new Vector2(0, 0));

        GameObject blockTextObj = CreateUIElement("BlockText", blockDisplay.transform);
        TMP_Text blockText = blockTextObj.AddComponent<TextMeshProUGUI>();
        blockText.text = "0";
        blockText.alignment = TextAlignmentOptions.Center;
        blockText.fontSize = 16;
        blockText.color = Color.white;
        SetRectTransform(blockTextObj.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));

        // Create intent display
        GameObject intentDisplay = CreateUIElement("IntentDisplay", this.transform);
        SetRectTransform(intentDisplay.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-50, 0), new Vector2(50, 40));

        GameObject intentIcon = CreateUIElement("IntentIcon", intentDisplay.transform);
        intentIcon.AddComponent<Image>().color = Color.yellow;
        SetRectTransform(intentIcon.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 1), new Vector2(5, 5), new Vector2(-5, -20));

        GameObject intentTextObj = CreateUIElement("IntentText", intentDisplay.transform);
        TMP_Text intentText = intentTextObj.AddComponent<TextMeshProUGUI>();
        intentText.text = "Attack: 10";
        intentText.alignment = TextAlignmentOptions.Center;
        intentText.fontSize = 12;
        SetRectTransform(intentTextObj.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -20), new Vector2(0, 0));

        // Create status effects container
        GameObject statusContainer = CreateUIElement("StatusEffectsContainer", this.transform);
        HorizontalLayoutGroup layout = statusContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.MiddleCenter;
        SetRectTransform(statusContainer.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 30));

        Debug.Log("Enemy UI hierarchy created successfully!");
    }

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private void SetRectTransform(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}