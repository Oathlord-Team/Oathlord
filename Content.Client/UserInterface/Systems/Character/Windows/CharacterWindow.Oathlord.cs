namespace Content.Client.UserInterface.Systems.Character.Windows;

public partial class CharacterWindow
{
    public static event Action<CharacterWindow>? OnOpened;

    protected override void Opened()
    {
        base.Opened();
        OnOpened?.Invoke(this);
    }
}
