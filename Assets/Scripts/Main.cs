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
	private AnimationCurve _spawnProbabilityPerFrame;

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
    private float _screenWidth;
    private float _screenHeight;
    private float _minBirdYPos = 0;
    private float _maxBirdYPos = 1;
    private float _maxBalloonYPos =  1;
    // starts out at 1.7 seconds between birds, so we need to have a 1/120 chance of spawning. Given we're having 3 locations, needs to be 1/360.
    private float _spawnProbability = 1f/302f;
    // Maybe I need an animation curve for spawnProbability too, like I have for seconds between? For now, the aim is to get to 0.9 after 30 seconds, so that's 1/162
    // SO yeah, let's take the value in the secondsBetween, multiple by 180

    public float TimeFactor => _isGamePaused ? 0f : 1f;

    public bool IsGameActive => !_isGameOver && !_isGamePaused;

    private void Awake()
    {
        Instance = this;
        _camera = Camera.main;
        UpdateScreenDimensions();

        _numLives = _lives.Length - 1;
        _timeSinceLastSpawn = 1000; // arbitrarily large so we generate one on first update
		// Stop screen dimming
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}


    // So currently we're putting out a new bird at decreasing speed intervals, with increasing velocity
    // I want to change it so that we have a number of height slots and maybe we increase the probability that we fire from them as time goes on
    // And maybe we increase the average velocity over time but still have a range
    // Maybe if a height starting slot is used we don't send another one out until it's off screen? 
    // Or maybe we vary their heights a lot so that they don't fly straight and they go all over? In which case starting slot is probably just a delay not a stop
    // Dan says we can't vary their heights so much that they go off screen, so maybe we just add a lil more randomness to their flight patterns? 
    // let's start with the height slots.
    // How do I still keep the nice randomness of not having discrete 'slots' but not overlap my starting points.
    // maybe when i pick a starting point, I give a bird-sized buffer around it, and either:
    // - randomly decide if generate a 2nd bird and gen its height within constraints
    // - if next bird's random height is in range, it gets to exist else no go.
    // Let's start with former.
    // So every turn we need to decide how many birds to spawn. Maybe every turn we decide whether to spawn one in the middle, the upper, and the lower
    // Each gets a chance.
    // So instead of having a secondsPerSpawn, we just gradually increase the probability that we're spawning a bird.

    private void Update()
    {
        if (IsGameActive)
        {
            if (_screenHeight != Screen.height || _screenWidth != Screen.width)
            {
                UpdateScreenDimensions();
            }


            float secondsBetweenSpawns = _spawnSpeedIncrease.Evaluate(Time.timeSinceLevelLoad / 60);
            _spawnProbability = 100 / (SecondsBetweenSpawns * (1 / Time.deltaTime));
            Debug.Log("spawn prob: " + _spawnProbability + "deltaTime: " + Time.deltaTime);

            // Spawn something this frame!
            if (Random.Range(0f, 100f) < _spawnProbability)
            {
                // Need a way to choose which of 3 spots to spawn
                // And then of the 2 remaining, whether to spawn one and which
                // And then, whether to also spawn the 3rd
                // Eek. 
                // rand int 0/1/2 and pass that in. for no. 1. ez
                // 2 more tries. maybe then it's spawn 0->6 and if answer is unused good?
                // TODO: should have a minBalloonYPos yes I get it
                float birdHeightRange =_maxBirdYPos - _minBirdYPos;
                float balloonHeightRange =_maxBalloonYPos - _minBirdYPos;
                bool[] isSlotFree = new bool[] {true, true, true};
                for (int i = 0; i < 3; i++)
                {
                    int x = Random.Range(0, 3*(i+1)); // First Slot
                    if (x < 3 && isSlotFree[x])
                    {
                        isSlotFree[x] = false;
                        if (Random.Range(0f, 100f) < _percentageBalloons)
                        {
                            float startHeight = _minBirdYPos + balloonHeightRange * x/3;
                            float endHeight = startHeight + (balloonHeightRange * 1/3);
                            SpawnBalloon(startHeight, endHeight);
                        }
                        else
                        {
                            float startHeight = _minBirdYPos + birdHeightRange * x/3;
                            float endHeight = startHeight + (birdHeightRange * 1/3);
                            SpawnBird(startHeight, endHeight);
                        }
                    }
                }
            }
        }
    }

    private void UpdateScreenDimensions()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        _topOfScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 10)).y;
        _minBirdYPos = _grandpaTransform.position.y +2f;
        _maxBirdYPos = _topOfScreen - 1f;
        _maxBalloonYPos = _topOfScreen + 0.2f;
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
        SpawnBird(_minBirdYPos, _maxBirdYPos);

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

    private void SpawnBird(float minYPos, float maxYPos)
    {
        GameObject birdObj = GetOrCreatePooledBird();
        birdObj.SetActive(true);
        float leftSideOfScreen = _camera.ViewportToWorldPoint(new Vector3(0, 0, 10)).x;
        birdObj.transform.position = new Vector3(leftSideOfScreen - 1, Random.Range(minYPos, maxYPos), birdObj.transform.position.z);
        Bird bird = birdObj.GetComponent<Bird>();
        bird.Reset();
        Fly fly = birdObj.GetComponent<Fly>();
		Velocity = _birdVelocityIncrease.Evaluate(Time.timeSinceLevelLoad / 60);
		fly.Velocity = new Vector3(Velocity, 0, 0);

		BirdSpawned?.Invoke();
	}

	private void SpawnBalloon(float minYPos, float maxYPos)
	{
        GameObject balloonObj = GetOrCreatePooledBalloon();
        balloonObj.SetActive(true);
        float leftSideOfScreen = _camera.ViewportToWorldPoint(new Vector3(0, 0, 10)).x;
		balloonObj.transform.position = new Vector3(leftSideOfScreen - 1, Random.Range(minYPos, maxYPos), balloonObj.transform.position.z); // higher up and further back than the birds
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
