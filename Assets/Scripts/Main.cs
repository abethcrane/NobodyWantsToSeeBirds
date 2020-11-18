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
	public event System.Action<bool> PauseToggled;

	public static Main Instance;

    public float SecondsBetweenSpawns = 3f; // Used only as a visual for debugging
    public float Speed = 3f;
    public bool IsRainbowBirdsEnabled;
    public bool AreBirdsUntappable;

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
	private AnimationCurve _birdSpeedIncrease;

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
    private Animator _cameraAnim;
    private float _screenWidth;
    private float _screenHeight;
    private float _minBirdYPos = 0;
    private float _maxBirdYPos = 1;
    private float _maxBalloonYPos =  1;
    // starts out at 1.7 seconds between birds, so we need to have a 1/120 chance of spawning. Given we're having 3 locations, needs to be 1/360.
    private float _spawnProbability = 1f/302f;
    // Maybe I need an animation curve for spawnProbability too, like I have for seconds between? For now, the aim is to get to 0.9 after 30 seconds, so that's 1/162
    // SO yeah, let's take the value in the secondsBetween, multiple by 180
    private float _birdHeight = 1.5f; // This should be calculated from bird extents but *shrug*
    private int _numSpawnSlots = 1;

    public bool IsGameActive => !_isGameOver && !_isGamePaused;
    public float SecondsOfGamePlay { get; private set; } = 0f;
    public float MinutesOfGamePlay => SecondsOfGamePlay / 60;

    private void Awake()
    {
        Instance = this;
        _camera = Camera.main;
        _cameraAnim = GetComponent<Animator>();
        UpdateScreenDimensions();

        _numLives = _lives.Length - 1;
        _timeSinceLastSpawn = 1000; // arbitrarily large so we generate one on first update
		// Stop screen dimming
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

    // So currently we're putting out a new bird at decreasing speed intervals, with increasing velocity
    // I want to change it so that we have a number of height slots and maybe we increase the probability that we fire from them as time goes on
    // And maybe we increase the average velocity over time but still have a range
    private void Update()
    {
        if (IsGameActive)
        {
            SecondsOfGamePlay += Time.deltaTime;

            if (_screenHeight != Screen.height || _screenWidth != Screen.width)
            {
                UpdateScreenDimensions();
            }

            float secondsBetweenSpawns = _spawnSpeedIncrease.Evaluate(MinutesOfGamePlay);
            // This is just magic haha
            _spawnProbability = 200 / (SecondsBetweenSpawns * (1 / Time.deltaTime));
            Debug.Log($"Seconds between {SecondsBetweenSpawns}, deltaTime {Time.deltaTime} = spawn prob {_spawnProbability}");

            float requiredSpawnProb = _spawnProbability / _numSpawnSlots;
            float birdHeightRange =_maxBirdYPos - _minBirdYPos;
            float balloonHeightRange =_maxBalloonYPos - _minBirdYPos;
            for (int i = 0; i < _numSpawnSlots; i++)
            {
                // Do a probability - should we use this spot?
                // If yes, choose whether to spawn a bird or balloon
                // Once chosen, choose where to put it:
                // startHeight + (heightRange / i) + Random.Range(-0.25f, 0.25f); 

                // Should we use this spot?
                if (Random.Range(0f, 100f) < requiredSpawnProb)
                {
                    // Choose whether to spawn a bird or balloon
                    if (Random.Range(0f, 100f) < _percentageBalloons)
                    {
                        // If we want to actually calculate the variance we could look at how much space there'll be
                        // In between slots - e.g. if birds are 1f high on a 3.5f space, there'll be 3 starting slots with 0.5f space extra
                        // So 0.5f/3 is ~0.16f of variance, so it'd be -0.08f->0.08f either side if we didn't want any to overlap. But...they sort themselves out, so a bit of overlap is fine
                        SpawnBalloon(_minBirdYPos + (balloonHeightRange * i / _numSpawnSlots) + _birdHeight/2 + Random.Range(-0.25f, 0.25f));
                    }
                    else
                    {
                        SpawnBird(_minBirdYPos + (birdHeightRange * i / _numSpawnSlots) + _birdHeight/2 + Random.Range(-0.25f, 0.25f));
                    }
                }
            }
        }
    }

    private void UpdateScreenDimensions()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        
        _minBirdYPos = _grandpaTransform.position.y + 2f; // For padding

        var topOfScreen = _camera.ViewportToWorldPoint(Vector2.one).y;
        _maxBirdYPos = topOfScreen - 0.25f;
        _maxBalloonYPos = topOfScreen + 0.2f;

        _numSpawnSlots = Mathf.FloorToInt((_maxBirdYPos - _minBirdYPos) / _birdHeight);

        Debug.Log(string.Format("min and max bird are {0} and {1} and there are {2} slots", _minBirdYPos, _maxBirdYPos, _numSpawnSlots));
    }

    public void StartGame()
    {
        // Reload the scene, but that doesn't reload this script
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        _timeSinceLastSpawn = 0f;

        _numLives = _lives.Length - 1;
        _score = 0;

        float birdHeightRange =_maxBirdYPos - _minBirdYPos;
        SpawnBird(_minBirdYPos + (birdHeightRange * Random.Range(0, _numSpawnSlots)/_numSpawnSlots) + _birdHeight/2 + Random.Range(-0.25f, 0.25f));

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

    public void Explosion()
    {
        _cameraAnim.SetTrigger("Shake");
        LoseLife();
    }

    public void TogglePause()
    {
        _isGamePaused = !_isGamePaused;
        PauseToggled?.Invoke(_isGamePaused);

        // This blogpost indicated this would be the best thing to do: https://gamedevbeginner.com/the-right-way-to-pause-the-game-in-unity/
        // But it doesn't seem like time.timeScale is affected by this.
        // So I'm using it for pausing the bird's velocity but managing my own SecondsOfGamePlay 
        Time.timeScale = _isGamePaused ? 0 : 1f;
    }

	public void ToggleAudio()
	{
		AudioListener.pause = !AudioListener.pause;
	}

    private void SpawnBird(float startingHeight)
    {
        GameObject birdObj = GetOrCreatePooledBird();
        birdObj.SetActive(true);
        float leftSideOfScreen = _camera.ViewportToWorldPoint(new Vector3(0, 0, 10)).x;
        birdObj.transform.position = new Vector3(leftSideOfScreen - Random.Range(1f, 2.5f), startingHeight, birdObj.transform.position.z);
        Bird bird = birdObj.GetComponent<Bird>();
        bird.Reset();
        Fly fly = birdObj.GetComponent<Fly>();
		Speed = _birdSpeedIncrease.Evaluate(MinutesOfGamePlay);
		fly.Speed = Speed;
        fly.Rotation = 0f;//Random.Range(-5f, 5f);

		BirdSpawned?.Invoke();
	}

	private void SpawnBalloon(float startingHeight)
	{
        GameObject balloonObj = GetOrCreatePooledBalloon();
        balloonObj.SetActive(true);
        float leftSideOfScreen = _camera.ViewportToWorldPoint(new Vector3(0, 0, 10)).x;
		balloonObj.transform.position = new Vector3(leftSideOfScreen - 1, startingHeight, balloonObj.transform.position.z); // higher up and further back than the birds
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
            newBird.transform.parent = _birdPool.transform;
            Bird bird = newBird.GetComponent<Bird>();
            bird.SortOrder = _birdPool.transform.childCount;
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
