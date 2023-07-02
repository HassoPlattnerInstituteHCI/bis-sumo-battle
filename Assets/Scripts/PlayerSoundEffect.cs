using UnityEngine;
using SpeechIO;
public class PlayerSoundEffect : MonoBehaviour
{
    public AudioClip dropInClip;
    public AudioClip gameOverClip;
    public AudioClip collisionClip;
    public float maxPitch = 1.2f;
    public float minPitch = 0.8f;
    private GameObject previousEnemy;
    private AudioSource audioSource;
    public SpeechOut speechOut;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // speechOut = new SpeechOut();
    }
    public float PlayerFellDown()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(gameOverClip);
        return gameOverClip.length;
    }
    public void PlayHit()
    {
        PlayClipPitched(collisionClip, minPitch, maxPitch);
    }
    public void PlayDropIn()
    {
        audioSource.PlayOneShot(dropInClip);
    }
    
    public void PlayEnemyHitClip(AudioClip clip, GameObject go = null)
    {
        if (go)
        {
            if (previousEnemy)
            {
                if (go.Equals(previousEnemy))
                    return;
            }
            previousEnemy = go;
        }
        audioSource.PlayOneShot(clip);
    }


    public void PlayEnemyHitClip(GameObject go = null)
    {
        if (go)
        {
            if (previousEnemy)
            {
                if (go.Equals(previousEnemy))
                    return;
            }
            previousEnemy = go;
        }
        SayName(go.GetComponent<Enemy>());
    }
    
    private async void SayName(Enemy e)
    {
        speechOut.Stop();
        if (e.displayName == "Shohei")
        {
            await speechOut.Speak("Enemy hit me"); 
        }
        else
        {
            await speechOut.Speak(e.displayName + " hit me");
        }
    }

    public void StopPlayback()
    {
        audioSource.Stop();
    }

    public void PlayClipPitched(AudioClip clip, float minPitch, float maxPitch)
    {
        // little trick to make clip sound less redundant
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        // plays same clip only once, this way no overlapping
        audioSource.PlayOneShot(clip);
        audioSource.pitch = 1f;
    }

}
