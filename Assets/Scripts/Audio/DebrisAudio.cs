using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class DebrisAudio : MonoBehaviour
{
    [SerializeField] private AudioClip collisionSound;
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
            _audioSource.PlayOneShot(collisionSound);
        }
        
        onCollisionTriggered?.Invoke();
    }
}
