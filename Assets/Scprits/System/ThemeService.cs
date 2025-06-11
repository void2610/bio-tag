using System;
using System.Collections.Generic;
using UnityEngine;

public class ThemeService : IThemeService
{
    private const string THEME_PREF_KEY = "BioTag_SelectedTheme";

    private int _currentThemeIndex;
    
    public string CurrentTheme => AvailableThemes[_currentThemeIndex];
    public List<string> AvailableThemes { get; }

    public event Action<string> ThemeChanged;
    
    public ThemeService()
    {
        AvailableThemes = new List<string> {
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
        _currentThemeIndex = (_currentThemeIndex + 1) % AvailableThemes.Count;
        var newTheme = CurrentTheme;
        
        SaveTheme(newTheme);
        ThemeChanged?.Invoke(newTheme);
    }
    
    public string GetSavedTheme() => PlayerPrefs.GetString(THEME_PREF_KEY, "");
    
    public void SaveTheme(string themeName)
    {
        PlayerPrefs.SetString(THEME_PREF_KEY, themeName);
        PlayerPrefs.Save();
    }
    
    public void ClearSavedTheme()
    {
        PlayerPrefs.DeleteKey(THEME_PREF_KEY);
        PlayerPrefs.Save();
        _currentThemeIndex = 0;
        ThemeChanged?.Invoke(CurrentTheme);
    }
    
    private void LoadSavedTheme()
    {
        var savedTheme = GetSavedTheme();
        
        if (!string.IsNullOrEmpty(savedTheme))
        {
            var themeIndex = FindThemeIndex(savedTheme);
            if (themeIndex >= 0)
                Debug.Log($"[ThemeService] Loaded saved theme: {savedTheme}");
            else
                _currentThemeIndex = 0;
        }
        else
        {
            _currentThemeIndex = 0;
        }
    }
    
    private int FindThemeIndex(string themeName)
    {
        for (var i = 0; i < AvailableThemes.Count; i++)
        {
            if (AvailableThemes[i] == themeName) return i;
        }
        return -1;
    }
}