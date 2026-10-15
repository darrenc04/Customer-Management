namespace MauiApp1_Assignment1_.Services;

public class ActionMessageService
{
    private string? _pendingMessage;

    public void Set(string message)
    {
        _pendingMessage = message;
    }

    //Return the waiting message once, then forget it
    public string? Take()
    {
        string? message = _pendingMessage;
        _pendingMessage = null;
        return message;
    }
}
