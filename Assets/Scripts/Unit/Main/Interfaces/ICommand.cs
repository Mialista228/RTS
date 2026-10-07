public interface ICommand
{
    void Start(Unit unit);
    void OnTick(Unit unit, float deltaInterval);
    bool IsFinished { get; }
    void Cancel(Unit unit);
}