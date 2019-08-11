using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Main : MonoBehaviour
{
    public const int NumBirdsPerQuoteLevelQuote = 50;
    private const float DefaultDecayRate = 0.75f;

    public event System.Action LostLife;
    public event System.Action GameOver;
	public event System.Action BirdSpawned;

	public static Main Instance;
    public float SecondsBetweenSpawns = 3f; // Used only as a visual for debugging
    public float Velocity = 1f;

    [SerializeField]
    private TextMeshProUGUI _healthText;

    [SerializeField]
    private GameObject _tapToPlay;

    [SerializeField]
    private GameObject _pauseButton;

    [SerializeField]
    private TextMeshProUGUI _scoreText;

	[SerializeField]
    private GameObject _birdPrefab;

	[SerializeField]
	private GameObject _airBalloonPrefab;

	[SerializeField]
    private string[] _lives;

    [SerializeField]
    private GameObject _birdPool;

	[SerializeField]
	private GameObject _airBalloonPool;

	[SerializeField]
    private Transform _grandpaTransform;

	[SerializeField]
	private AnimationCurve _spawnSpeedIncrease;

	[SerializeField]
	private AnimationCurve _birdVelocityIncrease;

    [SerializeField]
    [RangeAttribute(0, 100)]
    private int _percentageBalloons = 8;

    private bool _isGameOver = false;
    private bool _isGamePaused = false;
    private float _timeSinceLastSpawn;
    private List<GameObject> _birds = new List<GameObject>();
    private int _numLives;
    private int _score = 0;
    private Camera _camera;
	private float _topOfScreen;
    float _screenWidth;
    float _screenHeight;

    public float TimeFactor => _isGamePaused ? 0f : 1f;

    public bool IsGameActive => !_isGameOver && !_isGamePaused;

    private void Awake()
    {
        Instance = this;
        _camera = Camera.main;

        _topOfScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 10)).y;
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        _numLives = _lives.Length - 1;

		SpawnBird();

		// Stop screen dimming
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

    private void Update()
    {
        if (IsGameActive)
        {
            if (_screenHeight != Screen.height || _screenWidth != Screen.width)
            {
                _screenWidth = Screen.width;
                _screenHeight = Screen.height;
                _topOfScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 10)).y;
            }

            _timeSinceLastSpawn += Time.deltaTime;

            if (_timeSinceLastSpawn > SecondsBetweenSpawns)
            {
                // Roughly 5% of the time, spawn a balloon instead
                if (Random.Range(0f, 100f) < _percentageBalloons)
                {
                    SpawnBalloon();
                }
                else
                {
                    SpawnBird();
                }
                _timeSinceLastSpawn = 0f;
                SecondsBetweenSpawns = _spawnSpeedIncrease.Evaluate(Time.timeSinceLevelLoad / 60);
            }
        }
    }

    public void StartGame()
    {
        // Reload the scene, but that doesn't reload this script
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        foreach (GameObject bird in _birds)
        {
            GameObject.Destroy(bird);
        }
        _birds.Clear();

        _timeSinceLastSpawn = 0f;

        _numLives = _lives.Length - 1;
        _score = 0;
        SpawnBird();

        _isGameOver = false;
        _isGamePaused = false;
    }

    public void OffScreen()
    {
        if (IsGameActive)
        {
            _score += 1;
            _scoreText.text = "Score: " + _score.ToString();
        }
    }

    public void LoseLife()
    {
        _numLives -= 1;

        if (_numLives < 0)
        {
            return;
        }

        _healthText.text = string.Format("Lives: {0} {1}", _numLives, _lives[_numLives]);
        LostLife?.Invoke();

        if (_numLives == 0)
        {
            EndGame();
        }
    }

    public void TogglePause()
    {
        _isGamePaused = !_isGamePaused;
    }

	public void ToggleAudio()
	{
		AudioListener.pause = !AudioListener.pause;
	}

    private void SpawnBird()
    {
        GameObject birdObj = GetOrCreatePooledBird();
        birdObj.SetActive(true);
        float leftSideOfScreen = _camera.ViewportToWorldPoint(new Vector3(0, 0, 10)).x;
        birdObj.transform.position = new Vector3(leftSideOfScreen - 1, Random.Range(_grandpaTransform.position.y + 1.2f, _topOfScreen - 1f), -0.3f);
        Bird bird = birdObj.GetComponent<Bird>();
        bird.Reset();
        Fly fly = birdObj.GetComponent<Fly>();
		Velocity = _birdVelocityIncrease.Evaluate(Time.timeSinceLevelLoad / 60);
		fly.Velocity = new Vector3(Velocity, 0, 0);

		BirdSpawned?.Invoke();
	}

	private void SpawnBalloon()
	{
        GameObject balloonObj = GetOrCreatePooledBalloon();
        balloonObj.SetActive(true);
        float leftSideOfScreen = _camera.ViewportToWorldPoint(new Vector3(0, 0, 10)).x;
		balloonObj.transform.position = new Vector3(leftSideOfScreen - 1, Random.Range(_grandpaTransform.position.y +2f, _topOfScreen + 0.2f), 0.5f); // higher up and further back than the birds
		balloonObj.transform.parent = _airBalloonPool.transform;
    }

    private GameObject GetOrCreatePooledBalloon()
    {
        GameObject newBalloon = GetPooledObject(typeof(Balloon), _airBalloonPool);

        if (newBalloon == null)
        {
            newBalloon = GameObject.Instantiate(_airBalloonPrefab);
            newBalloon.transform.parent = _airBalloonPool.transform;
        }
        else
        {
            Balloon balloon = newBalloon.GetComponent<Balloon>();
            balloon.Reset();
        }

        return newBalloon;
    }

    private GameObject GetOrCreatePooledBird()
    {
        GameObject newBird = GetPooledObject(typeof(Bird), _birdPool);

        if (newBird == null)
        {
            newBird = GameObject.Instantiate(_birdPrefab);
            newBird.transform.parent = _birdPrefab.transform;
        }
        else
        {
            Bird bird = newBird.GetComponent<Bird>();
            bird.Reset();
        }

        return newBird;
    }

    private GameObject GetPooledObject(System.Type t, GameObject parent)
    {
        foreach (var obj in parent.GetComponentsInChildren(t, true))
        {
            if (!obj.gameObject.activeSelf)
            {
                return obj.gameObject;
            }
        }

        return null;
    }

    private void EndGame()
    {
        _isGameOver = true;
        _tapToPlay.SetActive(true);
        _pauseButton.SetActive(false);
        GameOver?.Invoke();
    }
}
