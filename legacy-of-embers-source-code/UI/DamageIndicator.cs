using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// A worldspace UI object that appears when a fighter takes damage.
    /// </summary>
    public class DamageIndicator : MonoBehaviour
    {
        [Tooltip("TMPro text component of the damage indicator object.")]
        public TextMeshProUGUI damageText;

        [Header("Fonts and Font Colors")]

        [Tooltip("Font asset to use for regular hits.")]
        public TMP_FontAsset RegularFont;

        [Tooltip("Font asset to use for critical hits.")]
        public TMP_FontAsset CriticalFont;

        [Tooltip("Color for damage indicator text when it is not a critical.")]
        public Color RegularFontColor;

        [Tooltip("Color for damage indicator text when it is a critical.")]
        public Color CriticalFontColor;

        [Header("Damage Popup Behaviour")]

        [Tooltip("How long the popup lasts for.")]
        public float Lifetime = 1.2f;

        [Tooltip("How far into the life time does the popup start to fade.")]
        public float FadeStartTime = 0.6f;

        [Tooltip("Minimum distance the popup can travel away from source " +
            "during its lifetime.")]
        public float MinDistance = 1.0f;

        [Tooltip("Maximum distance the popup can travel away from source " +
            "during its lifetime.")]
        public float MaxDistance = 2.0f;

        [HideInInspector]
        [Tooltip("Gameobject towards which the damage indicator should " + 
            "be facing.")]
        public GameObject flCam;

        /// <summary>
        /// Starting position of the damage popup.
        /// </summary>
        public Vector3 StartPos { get; set; }

        /// <summary>
        /// Target position of the damage popup.
        /// </summary>
        private Vector3 targetPos;

        /// <summary>
        /// Timer to track how long the damage popup has been active for.
        /// </summary>
        private float timer;

        private void OnEnable()
        {
            if (TurnManager.Instance != null)
            {
                if (TurnManager.Instance.ActiveCam != null)
                {
                    flCam = TurnManager.Instance.ActiveCam;
                }
                if (flCam != null)
                {
                    transform.LookAt(2 * transform.position -
                    flCam.transform.position);
                }
            }
            
            transform.position = StartPos;
            timer = 0.0f;

            float direction = Random.rotation.eulerAngles.z;
            float distance = Random.Range(MinDistance, MaxDistance);
            targetPos = StartPos +
                (Quaternion.Euler(0, 0, direction) *
                new Vector3(distance, distance, 0));

            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer > Lifetime)
            {
                gameObject.SetActive(false);
            }
            else if (timer > FadeStartTime)
            {
                damageText.color =
                    Color.Lerp(damageText.color, Color.clear,
                    (timer - FadeStartTime) / (Lifetime - FadeStartTime));
            }

            transform.localPosition =
                Vector3.Lerp(StartPos, targetPos, Mathf.Sin(timer / Lifetime));
            transform.localScale =
                Vector3.Lerp(Vector3.zero, Vector3.one,
                Mathf.Sin(timer / Lifetime));
        }

        /// <summary>
        /// Set the text displayed to the damage taken.
        /// </summary>
        /// <param name="damage">Damage number to be displayed.</param>
        public void SetDamageText(int damage, bool critical)
        {
            // Check if the incoming damage is a miss.
            if (damage == 0)
            {
                damageText.color = RegularFontColor;
                damageText.font = RegularFont;
                damageText.text = "Miss!";
                return;
            }

            // Othewise, color the damage text appropriately and display damage
            // taken.
            if (critical)
            {
                damageText.color = CriticalFontColor;
                damageText.font = CriticalFont;
            }
            else
            {
                damageText.color = RegularFontColor;
                damageText.font = RegularFont;
            }
            damageText.text = damage.ToString();
        }
    }
}
