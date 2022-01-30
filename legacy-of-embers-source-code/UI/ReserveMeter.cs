using Game.Combat.Reserves;
using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// UI Object that tracks a subject reserve.
    /// </summary>
    public class ReserveMeter : MonoBehaviour
    {
        [RequireInterface(typeof(IReserve))]
        [Tooltip("Reserve to be tracked by this meter.")]
        public Object SubjectReserve;

        [Header("Display Components")]

        [Tooltip("TMPro text object displaying the fraction of the " +
            "resource's current amount to its maximum amount.")]
        public TextMeshProUGUI FractionText;

        [Tooltip("Image representing the reserve bar's fill amount.")]
        public Image ReserveFillImage;

        [Tooltip("Is the reserve meter in world space?")]
        public bool WorldSpace;

        /// <summary>
        /// The subject reserve cast to IReserve to utilize interface.
        /// </summary>
        private IReserve subjectReserve;

        /// <summary>
        /// The reserve's current amount.
        /// </summary>
        private float current;

        /// <summary>
        /// The reserve's maximum amount.
        /// </summary>
        private float max;

        /// <summary>
        /// Camera through which the meter will be viewed.
        /// </summary>
        private Camera MainCamera;

        private void OnEnable()
        {
            subjectReserve = (IReserve)SubjectReserve;

            subjectReserve.CurrentChanged += UpdateCurrent;
            subjectReserve.MaxChanged += UpdateMax;
        }

        private void OnDisable()
        {
            subjectReserve = (IReserve)SubjectReserve;

            subjectReserve.CurrentChanged += UpdateCurrent;
            subjectReserve.MaxChanged += UpdateMax;
        }

        private void Start()
        {
            UpdateReserveUI();
            MainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (WorldSpace && GameManager.Instance.GameState == GameState.Running)
            {
                transform.LookAt(TurnManager.Instance.ActiveCam.transform);
                transform.Rotate(0, 180, 0);
            }
        }

        /// <summary>
        /// Updates the current reserve amount.
        /// </summary>
        /// <param name="newCurrent">The new current reserve amount.</param>
        private void UpdateCurrent(float newCurrent, bool damage)
        {
            current = newCurrent;
            UpdateReserveUI();
        }

        /// <summary>
        /// Updates the maximum reserve amount.
        /// </summary>
        /// <param name="newCurrent">The new maximum reserve amount.</param>
        private void UpdateMax(float newMax)
        {
            max = newMax;
            UpdateReserveUI();
        }

        /// <summary>
        /// Updates the reserve UI elements by changing the displayed fraction
        /// and corresponding fill percentage of the fill bar image.
        /// </summary>
        private void UpdateReserveUI()
        {
            if (FractionText != null)
            {
                FractionText.text = current + "/" + max;
            }
            float fillPercentage = ((float)current / (float)max);
            ReserveFillImage.fillAmount = fillPercentage;
        }
    }
}
