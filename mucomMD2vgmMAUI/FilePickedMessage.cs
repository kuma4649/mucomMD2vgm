using CommunityToolkit.Mvvm.Messaging.Messages;

namespace mucomMD2vgmMAUI;

public class FilePickedMessage : ValueChangedMessage<string?>
{
    public FilePickedMessage(string? value) : base(value) { }
}
