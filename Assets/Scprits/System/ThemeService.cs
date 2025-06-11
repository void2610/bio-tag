using System;
using UnityEngine;

public class ThemeService : IThemeService
{
    private const string THEME_PREF_KEY = "BioTag_SelectedTheme";
    
    private readonly string[] _availableThemes;
    private int _currentThemeIndex;
    
    public string CurrentTheme => _availableThemes[_currentThemeIndex];
    public string[] AvailableThemes => _availableThemes;
    
    public event Action<string> ThemeChanged;
    
    public ThemeService()
    {
        _availableThemes = new[] {
            "", // Default theme (no class)
            "theme-forest-gold",
            "theme-ocean-blue"
        };
        
        _currentThemeIndex = 0;
        LoadSavedTheme();
    }
    
    public void SetTheme(string themeName)
    {
        int themeIndex = FindThemeIndex(themeName);
        if (themeIndex >= 0)
        {
            _currentThemeIndex = themeIndex;
            SaveTheme(themeName);
            ThemeChanged?.Invoke(themeName);
            
            Debug.Log($"[ThemeService] Theme set to: {themeName}");
        }
        else
        {
            Debug.LogWarning($"[ThemeService] Theme '{themeName}' not found in available themes");
        }
    }
    
    public void CycleTheme()
    {
        _currentThemeIndex = (_currentThemeIndex + 1) % _availableThemes.Length;
        string newTheme = CurrentTheme;
        
        SaveTheme(newTheme);
        ThemeChanged?.Invoke(newTheme);
        
        if (string.IsNullOrEmpty(newTheme))
        {
            Debug.Log("[ThemeService] Cycled to default theme");
        }
        else
        {
            Debug.Log($"[ThemeService] Cycled to theme: {newTheme}");
        }
    }
    
    public string GetSavedTheme()
    {
        return PlayerPrefs.GetString(THEME_PREF_KEY, "");
    }
    
    public void SaveTheme(string themeName)
    {
        PlayerPrefs.SetString(THEME_PREF_KEY, themeName);
        PlayerPrefs.Save();
        Debug.Log($"[ThemeService] Saved theme preference: {themeName}");
    }
    
    public void ClearSavedTheme()
    {
        PlayerPrefs.DeleteKey(THEME_PREF_KEY);
        PlayerPrefs.Save();
        _currentThemeIndex = 0;
        ThemeChanged?.Invoke(CurrentTheme);
        Debug.Log("[ThemeService] Cleared saved theme preference");
    }
    
    private void LoadSavedTheme()
    {
        string savedTheme = GetSavedTheme();
        
        if (!string.IsNullOrEmpty(savedTheme))
        {
            int themeIndex = FindThemeIndex(savedTheme);
            if (themeIndex >= 0)
            {
                _currentThemeIndex = themeIndex;
                Debug.Log($"[ThemeService] Loaded saved theme: {savedTheme}");
            }
            else
            {
                Debug.LogWarning($"[ThemeService] Invalid saved theme '{savedTheme}', using default");
                _currentThemeIndex = 0;
            }
        }
        else
        {
            Debug.Log("[ThemeService] No saved theme found, using default");
            _currentThemeIndex = 0;
        }
    }
    
    private int FindThemeIndex(string themeName)
    {
        for (int i = 0; i < _availableThemes.Length; i++)
        {
            if (_availableThemes[i] == themeName)
            {
                return i;
            }
        }
        return -1;
    }
}