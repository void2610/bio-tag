using UnityEngine;
using UnityEngine.UIElements;

namespace BioTag.UI
{
    /// <summary>
    /// CSS Custom Propertiesを使用したテーマ切り替えマネージャー
    /// Unity 6のCSS変数システムでテーマを動的に切り替える
    /// PlayerPrefsでテーマ設定を永続化
    /// シーン内の全てのUIDocumentに自動適用
    /// </summary>
    public class ThemeManager : MonoBehaviour
    {
        [Header("Theme Settings")]
        [SerializeField] private bool enableKeyboardToggle = true;
        [SerializeField] private bool saveThemePreference = true;
        [SerializeField] private bool autoFindUIDocuments = true;
        
        [Header("Available Themes")]
        [SerializeField] private string[] availableThemes = {
            "", // Default theme (no class)
            "theme-forest-gold",
            "theme-ocean-blue"
        };
        
        private int currentThemeIndex = 0;
        private UIDocument[] uiDocuments;
        private VisualElement[] rootElements;
        
        // PlayerPrefs key for theme storage
        private const string THEME_PREF_KEY = "BioTag_SelectedTheme";
        
        private void Start()
        {
            if (autoFindUIDocuments)
            {
                // シーン内の全てのUIDocumentを取得
                uiDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
                rootElements = new VisualElement[uiDocuments.Length];
                
                for (int i = 0; i < uiDocuments.Length; i++)
                {
                    rootElements[i] = uiDocuments[i].rootVisualElement;
                }
                
                Debug.Log($"[ThemeManager] Found {uiDocuments.Length} UIDocuments in scene");
            }
            else
            {
                // 従来の方法：このGameObjectのUIDocumentのみ
                UIDocument singleDocument = GetComponent<UIDocument>();
                if (singleDocument != null)
                {
                    uiDocuments = new[] { singleDocument };
                    rootElements = new[] { singleDocument.rootVisualElement };
                }
                else
                {
                    Debug.LogError("[ThemeManager] UIDocument not found on this GameObject!");
                    return;
                }
            }
            
            if (uiDocuments != null && uiDocuments.Length > 0)
            {
                // 保存されたテーマを読み込み
                if (saveThemePreference)
                {
                    LoadSavedTheme();
                }
                
                Debug.Log($"[ThemeManager] Initialized with {availableThemes.Length} themes on {uiDocuments.Length} UI documents. Current: {GetCurrentTheme()}");
            }
            else
            {
                Debug.LogError("[ThemeManager] No UIDocuments found!");
            }
        }
        
        private void Update()
        {
            if (enableKeyboardToggle && Input.GetKeyDown(KeyCode.T))
            {
                CycleTheme();
            }
        }
        
        /// <summary>
        /// 次のテーマに切り替え
        /// </summary>
        public void CycleTheme()
        {
            if (rootElements == null || rootElements.Length == 0) return;
            
            // 現在のテーマクラスを削除
            if (!string.IsNullOrEmpty(availableThemes[currentThemeIndex]))
            {
                foreach (var rootElement in rootElements)
                {
                    if (rootElement != null)
                        rootElement.RemoveFromClassList(availableThemes[currentThemeIndex]);
                }
            }
            
            // 次のテーマに移動
            currentThemeIndex = (currentThemeIndex + 1) % availableThemes.Length;
            
            // 新しいテーマクラスを追加
            if (!string.IsNullOrEmpty(availableThemes[currentThemeIndex]))
            {
                foreach (var rootElement in rootElements)
                {
                    if (rootElement != null)
                        rootElement.AddToClassList(availableThemes[currentThemeIndex]);
                }
                Debug.Log($"[ThemeManager] Switched to theme: {availableThemes[currentThemeIndex]} on {rootElements.Length} UI documents");
            }
            else
            {
                Debug.Log($"[ThemeManager] Switched to default theme on {rootElements.Length} UI documents");
            }
            
            // テーマ選択を保存
            if (saveThemePreference)
            {
                SaveCurrentTheme();
            }
        }
        
        /// <summary>
        /// 特定のテーマに設定
        /// </summary>
        /// <param name="themeName">テーマクラス名</param>
        public void SetTheme(string themeName)
        {
            if (rootElements == null || rootElements.Length == 0) return;
            
            // 全てのテーマクラスを削除
            foreach (string theme in availableThemes)
            {
                if (!string.IsNullOrEmpty(theme))
                {
                    foreach (var rootElement in rootElements)
                    {
                        if (rootElement != null)
                            rootElement.RemoveFromClassList(theme);
                    }
                }
            }
            
            // 新しいテーマを適用
            if (!string.IsNullOrEmpty(themeName))
            {
                foreach (var rootElement in rootElements)
                {
                    if (rootElement != null)
                        rootElement.AddToClassList(themeName);
                }
                
                // インデックスを更新
                for (int i = 0; i < availableThemes.Length; i++)
                {
                    if (availableThemes[i] == themeName)
                    {
                        currentThemeIndex = i;
                        break;
                    }
                }
                
                Debug.Log($"[ThemeManager] Set theme to: {themeName} on {rootElements.Length} UI documents");
            }
            else
            {
                currentThemeIndex = 0;
                Debug.Log($"[ThemeManager] Set to default theme on {rootElements.Length} UI documents");
            }
            
            // テーマ選択を保存
            if (saveThemePreference)
            {
                SaveCurrentTheme();
            }
        }
        
        /// <summary>
        /// 現在のテーマ名を取得
        /// </summary>
        /// <returns>現在のテーマクラス名</returns>
        public string GetCurrentTheme()
        {
            return availableThemes[currentThemeIndex];
        }
        
        /// <summary>
        /// PlayerPrefsに現在のテーマを保存
        /// </summary>
        private void SaveCurrentTheme()
        {
            string currentTheme = GetCurrentTheme();
            PlayerPrefs.SetString(THEME_PREF_KEY, currentTheme);
            PlayerPrefs.Save();
            Debug.Log($"[ThemeManager] Saved theme preference: {currentTheme}");
        }
        
        /// <summary>
        /// PlayerPrefsから保存されたテーマを読み込み、適用
        /// </summary>
        private void LoadSavedTheme()
        {
            string savedTheme = PlayerPrefs.GetString(THEME_PREF_KEY, "");
            
            if (!string.IsNullOrEmpty(savedTheme))
            {
                // 保存されたテーマが有効な選択肢かチェック
                bool isValidTheme = false;
                for (int i = 0; i < availableThemes.Length; i++)
                {
                    if (availableThemes[i] == savedTheme)
                    {
                        currentThemeIndex = i;
                        isValidTheme = true;
                        break;
                    }
                }
                
                if (isValidTheme)
                {
                    // テーマを適用（UIが初期化された後）
                    ApplyTheme(savedTheme);
                    Debug.Log($"[ThemeManager] Loaded saved theme: {savedTheme}");
                }
                else
                {
                    Debug.LogWarning($"[ThemeManager] Invalid saved theme '{savedTheme}', using default");
                    currentThemeIndex = 0;
                }
            }
            else
            {
                Debug.Log("[ThemeManager] No saved theme found, using default");
                currentThemeIndex = 0;
            }
        }
        
        /// <summary>
        /// テーマを実際にUIに適用（内部用）
        /// </summary>
        /// <param name="themeName">適用するテーマクラス名</param>
        private void ApplyTheme(string themeName)
        {
            if (rootElements == null || rootElements.Length == 0) return;
            
            // 全てのテーマクラスを削除
            foreach (string theme in availableThemes)
            {
                if (!string.IsNullOrEmpty(theme))
                {
                    foreach (var rootElement in rootElements)
                    {
                        if (rootElement != null)
                            rootElement.RemoveFromClassList(theme);
                    }
                }
            }
            
            // 新しいテーマを適用
            if (!string.IsNullOrEmpty(themeName))
            {
                foreach (var rootElement in rootElements)
                {
                    if (rootElement != null)
                        rootElement.AddToClassList(themeName);
                }
            }
        }
        
        /// <summary>
        /// 保存されたテーマ設定をクリア
        /// </summary>
        public void ClearSavedTheme()
        {
            PlayerPrefs.DeleteKey(THEME_PREF_KEY);
            PlayerPrefs.Save();
            Debug.Log("[ThemeManager] Cleared saved theme preference");
        }
    }
}