using Game.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Movement
{
    public class Vaulter : MonoBehaviour
    {
        public event VaultStarted VaultStarted;

        [Tooltip("NavMeshAgent of the humanoid this vault component acts upon.")]
        public NavMeshAgent NavMeshAgent;

        [Header("OffMeshLink Traversal")]
        /// <summary>
        /// Apex of the parabolic jump used to move player across
        /// OffMeshLinks.
        /// </summary>
        public float JumpHeight;

        /// <summary>
        /// Duration of the parabolic jump used to move player across
        /// OffMeshLinks.
        /// </summary>
        public float MoveDuration;

        private IEnumerator Start()
        {
            NavMeshAgent.autoTraverseOffMeshLink = false;
            while (true)
            {
                if (NavMeshAgent.isOnOffMeshLink)
                {
                    yield return StartCoroutine(ParabolicJump(NavMeshAgent, JumpHeight, MoveDuration));
                    NavMeshAgent.CompleteOffMeshLink();
                    yield return null;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Function to manually move a nav mesh agent in a parabolic shape to
        /// simulate jumping over an object. Uses Vector3.Lerp to perform movement.
        /// </summary>
        /// <param name="agent">Nav Mesh Agent to move.</param>
        /// <param name="height">Apex of the parabola (delta).</param>
        /// <param name="duration">Time of travel</param>
        private IEnumerator ParabolicJump(NavMeshAgent agent, float height, float duration)
        {
            VaultStarted?.Invoke();
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            Vector3 startPos = agent.transform.position;
            Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
            float normalizedTime = 0.0f;
            while (normalizedTime < duration)
            {
                Vector3 lookTarget = new Vector3(endPos.x, agent.transform.position.y, endPos.z);
                agent.transform.LookAt(lookTarget);
                float yOffset = height * (normalizedTime - normalizedTime * normalizedTime);
                agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
                normalizedTime += Time.deltaTime / duration;
                yield return null;
            }
        }
    }

}
