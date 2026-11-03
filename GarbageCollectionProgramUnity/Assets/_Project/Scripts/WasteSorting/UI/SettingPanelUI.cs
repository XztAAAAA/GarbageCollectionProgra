using UnityEngine.UI;
using UnityEngine;

public class SettingPanelUI : MonoBehaviour
{
    private Button chineseLanguageButton;
    private Button koreanLanguageButton;
    private Button closeButton;

    private void Awake()
    {
        chineseLanguageButton = FindButton(
            "Content/Desc/DescBg/CHlanguageButton"
        );
        koreanLanguageButton = FindButton(
            "Content/Desc/DescBg/KOlanguageButton"
        );
        closeButton = FindButton("Content/CloseBtn");
    }

    private void OnEnable()
    {
        if (chineseLanguageButton != null)
        {
            chineseLanguageButton.onClick.AddListener(
                OnChineseLanguageSelected
            );
        }

        if (koreanLanguageButton != null)
        {
            koreanLanguageButton.onClick.AddListener(
                OnKoreanLanguageSelected
            );
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    private void OnDisable()
    {
        if (chineseLanguageButton != null)
        {
            chineseLanguageButton.onClick.RemoveListener(
                OnChineseLanguageSelected
            );
        }

        if (koreanLanguageButton != null)
        {
            koreanLanguageButton.onClick.RemoveListener(
                OnKoreanLanguageSelected
            );
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
        }
    }

    private Button FindButton(string path)
    {
        Transform buttonTransform = transform.Find(path);
        if (buttonTransform == null)
        {
            CommonTool.LogError(
                $"[SettingPanelUI] 找不到按钮：{path}",
                this
            );
            return null;
        }

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null)
        {
            CommonTool.LogError(
                $"[SettingPanelUI] 对象上未找到 Button 组件：{path}",
                this
            );
        }

        return button;
    }

    private void OnChineseLanguageSelected()
    {
        TableSystem.SetLanguage(GameLanguage.Chinese);
    }

    private void OnKoreanLanguageSelected()
    {
        TableSystem.SetLanguage(GameLanguage.Korean);
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
