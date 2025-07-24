using System;
using System.Collections.Generic;

public interface IThemeService
{
    string CurrentTheme { get; }
    List<string> AvailableThemes { get; }
    
    void SetTheme(string themeName);
    void CycleTheme();
    string GetSavedTheme();
    void SaveTheme(string themeName);
    void ClearSavedTheme();
    
    event Action<string> ThemeChanged;
}