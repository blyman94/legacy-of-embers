using Game.Combat;
using System.Collections;
using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    public Fighter Fighter;

    [Tooltip("Duration of the camera shake.")]
    public float ShakeDuration;

    [Tooltip("Magnitude of screen position deviation during shake.")]
    public float ShakeIntensity;

    /// <summary>
    /// Tracks if camera is currently shaking.
    /// </summary>
    private bool isShaking;

    /// <summary>
    /// Position of the camera before any shaking.
    /// </summary>
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void OnEnable()
    {
        Fighter.DamageTaken += CameraShake;
        Fighter.Fire += CameraShake;
    }

    /// <summary>
    /// Responds to game start and shakes camera during the lighting of the
    /// match.
    /// </summary>
    /// <param name="gameState">Current state of the game. Only responds
    /// if the game state changes to "GameState.Running"</param>
    public void CameraShake()
    {
        if (!isShaking)
        {
            StartCoroutine(CameraShakeRoutine(false));
        }
    }

    /// <summary>
    /// Shakes the camera using Random.InsideUnitSphere.
    /// </summary>
    /// <param name="isBonfire">Determines which shake duration and 
    /// intensity to use based on whether the source of the shake is 
    /// a bonfire.</param>
    public IEnumerator CameraShakeRoutine(bool isBonfire)
    {
        yield return new WaitForSeconds(0.2f);
        isShaking = true;
        float elapsedTime = 0;
        while (elapsedTime < ShakeDuration)
        {
            transform.localPosition = startPos + (Random.insideUnitSphere *
                ShakeIntensity);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = startPos;
        isShaking = false;
    }
}
