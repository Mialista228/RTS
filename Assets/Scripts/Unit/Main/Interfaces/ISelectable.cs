public interface ISelectable
{
    void OnHoverEnter();
    void OnHoverExit();

    void Select();
    void Deselect();
    bool IsSelected { get; }
}