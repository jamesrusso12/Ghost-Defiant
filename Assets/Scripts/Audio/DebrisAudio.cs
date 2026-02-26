using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class DebrisAudio : MonoBehaviour
{
    [SerializeField] private AudioClip collisionSound;
    [Tooltip("Volume of debris collision (lower to avoid clashing with gun and wall destruction).")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.55f;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;
    [SerializeField] private UnityEvent onCollisionTriggered;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter()
    {
        if (collisionSound != null)
        {
            _audioSource.pitch = Random.Range(minPitch, maxPitch);
            _audioSource.PlayOneShot(collisionSound, volume);
        }
        
        onCollisionTriggered?.Invoke();
    }
}
