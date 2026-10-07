public enum BuffType
{
    Temporary,
    Permanent,
    OneTime,
}
public interface IBuff
{
    public BuffType buffType { get; set; }
    public bool isDebuff { get; set; }
}