public interface ICommandExecutor
{
    void Enqueue(ICommand command);
    void ExecuteNext();
    void Clear();
    bool RemoveAt(int index);
}