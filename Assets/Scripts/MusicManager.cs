using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _startMusic;

    [SerializeField]
    private AudioSource _holdMusic;

    [SerializeField]
    private AudioSource _themeMusicIntro;

    [SerializeField]
    private AudioSource _themeMusicLoop;

	[SerializeField]
	private AnimationCurve _musicPitchIncrease;

    private void Awake()
    {

        _themeMusicLoop.pitch = _musicPitchIncrease.Evaluate(0);
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
	}

    private void OnActiveSceneChanged(Scene previous, Scene current)
    {
        if (current.name == "GamePlay")
        {
            Main.Instance.GameOver += OnGameOver;
            Main.Instance.BirdSpawned += OnBirdSpawned;
            StartCoroutine(CrossFade(_startMusic, _themeMusicIntro, 5f, nextMusic: _themeMusicLoop));
        }
    }

    private void OnBirdSpawned()
    {
		_themeMusicLoop.pitch = _musicPitchIncrease.Evaluate(Main.Instance.MinutesOfGamePlay);
    }

    private void OnGameOver()
    {
        StartCoroutine(CrossFade(_themeMusicLoop, _holdMusic));
        _themeMusicLoop.pitch = _musicPitchIncrease.Evaluate(0);
    }

    private static IEnumerator CrossFade(AudioSource oldMusic, AudioSource newMusic, float fadeTime=2f, AudioSource nextMusic = null)
    {
        float startVolume = oldMusic.volume;
        float endVolume = newMusic.volume;
        newMusic.volume = 0;
        newMusic.Play();

        if (nextMusic != null)
        {
            nextMusic.PlayDelayed(newMusic.clip.length);
        }

        while (oldMusic.volume > 0 || newMusic.volume < endVolume)
        {
            if (oldMusic.volume > 0)
            {
                oldMusic.volume -= startVolume * Time.deltaTime / fadeTime;
            }

            if (newMusic.volume < endVolume)
            {
                newMusic.volume += endVolume * Time.deltaTime / fadeTime;
            }

            yield return null;
        }

        oldMusic.Stop();
        oldMusic.volume = startVolume;
    }
}
