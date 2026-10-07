using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UnitAbilityEffect : MonoBehaviour
{
    [SerializeField] private UnitAbilityEffectSequence[] effectSequences;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private bool playOnAwake;
    [SerializeField] private bool dieAfterTime;
    [SerializeField] private float dieTime = 5f;

    private AudioSource audioSource;

    [System.Serializable]
    public class UnitAbilityEffectSequence
    {
        public float frame;
        public UnityEvent effectEvent;
    }

    void Awake()
    {
        if (GetComponent<AudioSource>())
            audioSource = GetComponent<AudioSource>();

        if (dieAfterTime)
            Destroy(gameObject, dieTime);

        if (playOnAwake)
            PlayEffect();
    }

    public void PlayEffect()
    {
        if (audioSource)
        {
            if (clips != null && clips.Length > 0)
            {
                audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            }
        }
        if (effectSequences == null)
            return;

        if (effectSequences.Length <= 0)
            return;

        for (int i = 0; i < effectSequences.Length; i++)
        {
            StartCoroutine(SequencePlay(effectSequences[i]));
        }
    }

    IEnumerator SequencePlay(UnitAbilityEffectSequence sequence)
    {
        yield return new WaitForSeconds(sequence.frame);
        sequence.effectEvent?.Invoke();
    }
}
