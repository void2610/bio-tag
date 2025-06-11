using System;

public interface IThemeService
{
    string CurrentTheme { get; }
    string[] AvailableThemes { get; }
    
    void SetTheme(string themeName);
    void CycleTheme();
    string GetSavedTheme();
    void SaveTheme(string themeName);
    void ClearSavedTheme();
    
    event Action<string> ThemeChanged;
}