public interface ICrosswordBoard
{
    Dimensions Dimensions { get; }
    void HideContent(string content);
    void ShowHint();
    event Action<bool> ContentHighlighted;
}