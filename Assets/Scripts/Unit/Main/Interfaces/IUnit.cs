public interface IUnit : ICommandExecutor, ISelectable
{
    uint Id { get; }
    int Team { get; }
    UnitData UnitData { get; }
}