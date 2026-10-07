using System.Collections.Generic;
using UnityEngine;

public class UnitSFX : UnitModule
{
    [Header("Module Settings")]
    [SerializeField] private AudioClip[] basicAttackClips;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private UnitSFXCustomSound[] sounds;
    [System.Serializable]
    public class UnitSFXCustomSound
    {
        public string soundName;
        public AudioClip[] clip;
    }
    private Dictionary<string, AudioClip[]> soundsDictionary;

    public override void OnAwake(Unit unit)
    {
        base.OnAwake(unit);

        soundsDictionary = new Dictionary<string, AudioClip[]>();

        foreach (var sfx in sounds)
        {
            if (string.IsNullOrEmpty(sfx.soundName)) continue;

            if (!soundsDictionary.ContainsKey(sfx.soundName))
            {
                soundsDictionary.Add(sfx.soundName, sfx.clip);
            }
            else
            {
                Debug.LogError($"Эффект под названием {sfx.soundName} уже присутствует");
            }
        }
    }

    public void BasicAttack()
    {
        audioSource.PlayOneShot(basicAttackClips[Random.Range(0, basicAttackClips.Length)]);
    }

    public void PlaySound(string name)
    {
        if (soundsDictionary.TryGetValue(name, out AudioClip[] clip))
        {
            audioSource.PlayOneShot(clip[Random.Range(0, clip.Length)]);
        }
    }
}