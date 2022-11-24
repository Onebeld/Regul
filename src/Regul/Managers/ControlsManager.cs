using Avalonia.Controls;

namespace Regul.Managers;

public static class ControlsManager
{
    public static T GetDataContext<T>(this Control? control) => (T)control?.DataContext!;
}