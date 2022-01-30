using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Simple class to ensure a GameObject is not destroyed when a scene is 
    /// loaded.
    /// </summary>
    public class DontDestroyOnLoad : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
