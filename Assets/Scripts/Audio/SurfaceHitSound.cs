using UnityEngine;

[CreateAssetMenu(fileName = "New Surface", menuName = "RTS/Create Surface Hit Sounds")]
public class SurfaceHitSound : ScriptableObject
{
    [SerializeField] private AudioClip[] flesh;
    [SerializeField] private AudioClip[] metal;
    [SerializeField] private AudioClip[] stone;
    [SerializeField] private AudioClip[] wood;

    public AudioClip GetClip(UnitData.UnitBodyType bodyType)
    {
        AudioClip[] clips = bodyType switch
        {
            UnitData.UnitBodyType.Flesh => flesh,
            UnitData.UnitBodyType.Metal => metal,
            UnitData.UnitBodyType.Stone => stone,
            UnitData.UnitBodyType.Wood => wood,
            _ => null
        };
        Debug.Log(bodyType);
        return (clips != null && clips.Length > 0) ? clips[Random.Range(0, clips.Length)] : null;
    }
}