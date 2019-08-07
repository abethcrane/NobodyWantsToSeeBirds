using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _holdMusic;

    [SerializeField]
    private AudioSource _themeMusicIntro;

    [SerializeField]
    private AudioSource _themeMusicLoop;

    private void Awake()
    {
        _themeMusicIntro.Play();
        _themeMusicLoop.PlayDelayed(_themeMusicIntro.clip.length);
    }

    private void Start()
    {
        Main.Instance.GameOver += OnGameOver;
    }

    private void OnGameOver()
    {
        StartCoroutine(CrossFade(_themeMusicLoop, _holdMusic));
    }

    private static IEnumerator CrossFade(AudioSource oldMusic, AudioSource newMusic, float FadeTime=2f)
    {
        float startVolume = oldMusic.volume;
        float endVolume = newMusic.volume;
        newMusic.volume = 0;
        newMusic.Play();

        while (oldMusic.volume > 0 || newMusic.volume < endVolume)
        {
            if (oldMusic.volume > 0)
            {
                oldMusic.volume -= startVolume * Time.deltaTime / FadeTime;
            }

            if (newMusic.volume < endVolume)
            {
                newMusic.volume += endVolume * Time.deltaTime / FadeTime;
            }

            yield return null;
        }

        oldMusic.Stop();
        oldMusic.volume = startVolume;
    }
}
