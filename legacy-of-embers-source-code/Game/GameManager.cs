using Game.Core;
using Game.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    /// <summary>
    /// The GameManager is responsible for scene management, application 
    /// management, and game state management.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public event GameStateChanged GameStateChanged;

        [Header("Screens")]

        [Tooltip("Scene Fader to use between scenes.")]
        public SceneFader SceneFader;

        [Tooltip("Screen to show while game is loading.")]
        public LoadingScreen LoadingScreen;

        [Header("Transition Timing")]

        [Tooltip("Time to load scene")]
        public float LoadTime = 3.0f;

        [Tooltip("Time to fade out of a scene.")]
        public float FadeInTime = 1.5f;

        [Tooltip("Time to fade out of a scene.")]
        public float FadeOutTime = 1.5f;

        [Header("Audio")]

        [Tooltip("2D Audio source to play win and loss music.")]
        public AudioSource WinLossAudio;
        public AudioClip WinClip;
        public AudioClip LossClip;

        /// <summary>
        /// Game State Property. Behaviours of all objects in the game change
        /// based on which state the game is in.
        /// </summary>
        public GameState GameState
        {
            get
            {
                return gameState;
            }
            set
            {
                GameStateChanged?.Invoke(gameState, value);
                gameState = value;
            }
        }

        /// <summary>
        /// Tracks the game's currnet state. Behaviours of all objects in the 
        /// game change based on which state the game is in.
        /// </summary>
        private GameState gameState;

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            GameStateChanged = null;
        }

        private void Start()
        {
            GameState = GameState.Default;
            LoadingScreen.HideLoadingScreenGroup();
            LoadScene("Title");
        }

        /// <summary>
        /// Starts the End Game routine.
        /// </summary>
        public void EndGame(bool isWin)
        {
            GameState = GameState.Postgame;
            StartCoroutine(EndGameRoutine(isWin));
        }

        /// <summary>
        /// Loads a new scene with transitions.
        /// </summary>
        /// <param name="sceneName">Scene to be loaded.</param>
        public void LoadScene(string sceneName)
        {
            if (sceneName == "Title")
            {
                StartCoroutine(TransitionToScene(sceneName, false, true, false));
            }
            else
            {   
                StartCoroutine(TransitionToScene(sceneName, true, true, true));
            }

        }

        /// <summary>
        /// Shuts down the application. Called from a UI button.
        /// </summary>
        public void QuitGame()
        {
            Application.Quit();
        }

        /// <summary>
        /// Reloads the Main scene of the game, if that is the current scene.
        /// </summary>
        public void ReloadScene()
        {
            if (SceneManager.GetActiveScene().name == "Main" && 
                gameState == GameState.Running || gameState == GameState.Paused)
            {
                LoadScene("Main");
            }
        }

        /// <summary>
        /// Pauses/Unpauses the game based on current game state.
        /// </summary>
        public void TogglePauseState()
        {
            if (GameState == GameState.Paused)
            {
                UIManager.Instance.HidePauseGroup();
                Time.timeScale = 1.0f;
                GameState = GameState.Running;
            }
            else
            {
                if (GameState != GameState.Running)
                {
                    // Can only pause if game is running.
                    return;
                }
                else
                {
                    UIManager.Instance.ShowPauseGroup();
                    Time.timeScale = 0.0f;
                    GameState = GameState.Paused;
                }
            }
        }

        /// <summary>
        /// Ends the game with a countdown to restart. Shows "You win" or "You 
        /// lose" based on the boolean paramater passed.
        /// </summary>
        /// <param name="isWin">Whether or not the end game routine has been
        /// triggered by the win condition.</param>
        private IEnumerator EndGameRoutine(bool isWin)
        {
            if (isWin)
            {
                WinLossAudio.PlayOneShot(WinClip);
            }
            else
            {
                WinLossAudio.PlayOneShot(LossClip);
            }
            yield return UIManager.Instance.EndGameCountdownRoutine(isWin);
            GameStateChanged = null;
            LoadScene("Main");
        }

        /// <summary>
        /// Method invoked when a scene has finished loading.
        /// </summary>
        /// <param name="scene">Scene that has been loaded.</param>
        /// <param name="loadSceneMode">UNUSED/param>
        private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == "Main")
            {
                GameState = GameState.Pregame;
            }
        }

        /// <summary>
        /// Clears all listeners to the GameStateChanged event when the main
        /// scene is exited.
        /// </summary>
        /// <param name="scene">Scene being unloaded.</param>
        private void OnSceneUnload(Scene scene)
        {
            if (scene.name == "Main")
            {
                GameStateChanged = null;
            }
        }

        /// <summary>
        /// Shows LoadingScreen for the given LoadTime.
        /// </summary>
        private IEnumerator ShowLoadingScreen()
        {
            LoadingScreen.ShowLoadingScreenGroup();
            yield return SceneFader.FadeIn(FadeInTime);
            yield return new WaitForSeconds(LoadTime);
            yield return SceneFader.FadeOut(FadeOutTime);
            LoadingScreen.HideLoadingScreenGroup();
        }

        /// <summary>
        /// Fades out the current scene, loads the new scene, then fades the 
        /// new scene in.
        /// </summary>
        /// <param name="sceneName">Name of scene to transition to.</param>
        private IEnumerator TransitionToScene(string sceneName, bool fadeOut, bool fadeIn, bool showLoadingScreen)
        {
            if (fadeOut)
            {
                yield return SceneFader.FadeOut(FadeOutTime);
            }
            SceneManager.LoadScene(sceneName);
            if (showLoadingScreen)
            {
                yield return ShowLoadingScreen();
            }
            if (fadeIn)
            {
                yield return SceneFader.FadeIn(FadeInTime);
            }
        }
    }
}

