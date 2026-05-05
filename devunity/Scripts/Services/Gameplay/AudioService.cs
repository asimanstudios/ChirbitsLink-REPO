using UnityEngine;

public class AudioService : MonoBehaviour
{
    [Header("Step Audio")]
    public AudioClip[] stepSounds;
    public float normalStepInterval = 0.4f;
    public float runStepInterval = 0.3f;

    private AudioSource audioSource;
    private float stepTimer;
    private bool isRunning;

    public void Initialize(AudioSource source)
    {
        audioSource = source;
        SetupAudioSource();
    }

    private void SetupAudioSource()
    {
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.priority = 0;
            audioSource.volume = 1f;
        }
    }

    public void UpdateStepAudio(bool grounded, float velocityMagnitude, bool running)
    {
        bool canPlaySteps = grounded && velocityMagnitude >= 0.1f;
        if (!canPlaySteps)
        {
            stepTimer = 0;
        }
        else
        {
            isRunning = running;
            float interval = running ? runStepInterval : normalStepInterval;
            stepTimer += Time.deltaTime;

            if (stepTimer >= interval)
            {
                stepTimer = 0;
                PlayRandomStep();
            }
        }
    }

    private void PlayRandomStep()
    {
        bool hasStepAudio = stepSounds != null && stepSounds.Length > 0 && audioSource != null;
        if (hasStepAudio)
        {
            int index = Random.Range(0, stepSounds.Length);
            audioSource.PlayOneShot(stepSounds[index]);
        }
    }

    public void PlayJumpSound(AudioClip jumpSound)
    {
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }
}

