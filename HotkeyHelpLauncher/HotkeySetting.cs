using System.Windows.Input;

public class HotkeySetting
{
    public string Name { get; set; }
    public List<string> PathsOrUrls { get; set; } = new();
    public Key Key { get; set; }
    public ModifierKeys Modifiers { get; set; }
    public string Browser { get; set; }
}

