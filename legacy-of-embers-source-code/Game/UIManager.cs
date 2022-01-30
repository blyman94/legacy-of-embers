using Game.Building;
using Game.Combat;
using Game.Core;
using Game.Movement;
using Game.UI;
using System.Collections;
using UnityEngine;
using TMPro;

namespace Game
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Player Component References")]

        [Tooltip("Player's fighter component.")]
        public Fighter PlayerFighter;

        [Tooltip("Player's Mover component.")]
        public Mover PlayerMover;

        [Tooltip("Player's Builder component.")]
        public Builder PlayerBuilder;

        [Header("HUD Group")]
        public CanvasGroup HUDGroup;

        [Header("Choose Weapon Group")]

        [Tooltip("Group to represent the weapon choice menu.")]
        public CanvasGroup ChooseWeaponGroup;

        [Tooltip("Group to represent the Rifle button in its inactive state.")]
        public CanvasGroup RifleInactiveGroup;

        [Tooltip("Group to represent the Assault Rifle button in its " +
            "inactive state.")]
        public CanvasGroup ARInactiveGroup;

        [Tooltip("Group to represent the shotgun button in its " +
            "inactive state.")]
        public CanvasGroup ShotgunInactiveGroup;

        [Tooltip("Group to represent the Rifle button in its active state.")]
        public CanvasGroup RifleActiveGroup;

        [Tooltip("Group to represent the Assualt Rifle button in its " +
            "active state.")]
        public CanvasGroup ARActiveGroup;

        [Tooltip("Group to represent the shotgun button in its active state.")]
        public CanvasGroup ShotgunActiveGroup;

        [Header("End Game Group")]

        [Tooltip("Group to represent the end game screen.")]
        public CanvasGroup EndGameGroup;

        [Tooltip("Text that will count down at game end before it restarts.")]
        public TextMeshProUGUI CountdownText;

        [Tooltip("Text that will tell the player if they have won or lost.")]
        public TextMeshProUGUI WinLossText;

        [Header("Pause Group")]

        [Tooltip("Group to represent the pause menu.")]
        public CanvasGroup PauseGroup;

        [Header("Action Failed Group")]

        [Tooltip("Group to represent what is shown to the player when they " +
            "attempt to complete an action that is not possible.")]
        public CanvasGroup ActionFailedGroup;

        [Tooltip("Text to display why an action has failed to the player.")]
        public TextMeshProUGUI ReasonText;

        [Tooltip("How long the action failed indicator takes to fade in.")]
        public float IndicatorFadeInTime;

        [Tooltip("How long the action failed indicator is shown for.")]
        public float IndicatorShowTime;

        [Tooltip("How long the action failed indicator takes to fade out.")]
        public float IndicatorFadeOutTime;

        [Header("Weapon Objects")]

        [Tooltip("Assault rifle object to be given to player if chosen.")]
        public WeaponObject AssaultRifleObject;

        [Tooltip("Rifle object to be given to player if chosen.")]
        public WeaponObject RifleObject;

        [Tooltip("Shotgun object to be given to player.")]
        public WeaponObject ShotgunObject;

        private void Start()
        {
            UpdateCountdownText(5);
        }

        private void OnEnable()
        {
            PlayerFighter.ShotFailed += ShowActionFailedGroup;
            PlayerMover.MovementFailed += ShowActionFailedGroup;
            PlayerBuilder.BuildFailed += ShowActionFailedGroup;
        }

        #region Action Failed Group Methods
        /// <summary>
        /// Hides the action failed group.
        /// </summary>
        private IEnumerator HideActionFailedGroupRoutine()
        {
            yield return new WaitForSeconds(IndicatorShowTime);
            float startAlpha = ActionFailedGroup.alpha;
            float elapsedTime = 0.0f;

            while (elapsedTime < IndicatorFadeInTime)
            {
                ActionFailedGroup.alpha =
                    Mathf.Lerp(startAlpha, 0.0f,
                    elapsedTime / IndicatorFadeInTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            ActionFailedGroup.alpha = 0f;
        }

        /// <summary>
        /// Calls coroutine to show the action failed group.
        /// </summary>
        private void ShowActionFailedGroup(string reason)
        {
            StopAllCoroutines();
            StartCoroutine(ShowActionFailedGroupRoutine(reason));
        }

        /// <summary>
        /// Shows the action failed group.
        /// </summary>
        private IEnumerator ShowActionFailedGroupRoutine(string reason)
        {
            float startAlpha = ActionFailedGroup.alpha;
            float elapsedTime = 0.0f;

            ReasonText.text = reason;

            while (elapsedTime < IndicatorFadeInTime)
            {
                ActionFailedGroup.alpha =
                    Mathf.Lerp(startAlpha, 1.0f,
                    elapsedTime / IndicatorFadeInTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            ActionFailedGroup.alpha = 1f;

            StartCoroutine(HideActionFailedGroupRoutine());
        }
        #endregion

        #region Choose Weapon Group Methods

        /// <summary>
        /// Assigns a weapon to the player based on which button they click in 
        /// the Choose Weapon screen. Called from UI buttons in the Choose 
        /// Weapon Group.
        /// </summary>
        /// <param name="weaponType">Type of weapon chosen.</param>
        public void AssignWeapon(string weaponType)
        {
            switch (weaponType)
            {
                case "AssaultRifle":
                    EntityManager.Instance.PlayerActor.Fighter.CurrentWeapon =
                        AssaultRifleObject;
                    break;
                case "Rifle":
                    EntityManager.Instance.PlayerActor.Fighter.CurrentWeapon =
                            RifleObject;
                    break;
                case "Shotgun":
                    EntityManager.Instance.PlayerActor.Fighter.CurrentWeapon =
                            ShotgunObject;
                    break;
                default:
                    EntityManager.Instance.PlayerActor.Fighter.CurrentWeapon =
                            AssaultRifleObject;
                    Debug.LogError("{{UIManager.cs}} Unrecognized string " +
                        "(weaponType) passed to AssignWeapon.");
                    break;
            }
            HideChooseWeaponGroup();
            HideTooltip();
            ShowCanvasGroup(HUDGroup);
            GameManager.Instance.GameState = GameState.Running;
        }

        /// <summary>
        /// Shows the hover state for the weapon type when the player hovers 
        /// over it in the Choose Weapon screen.
        /// </summary>
        /// <param name="weaponType">Type of weapon for which to show a 
        /// tooltip.</param>
        public void ShowActiveState(string weaponType)
        {
            switch (weaponType)
            {
                case "AssaultRifle":
                    ShowCanvasGroup(ARActiveGroup);
                    HideCanvasGroup(ARInactiveGroup);
                    break;
                case "Rifle":
                    ShowCanvasGroup(RifleActiveGroup);
                    HideCanvasGroup(RifleInactiveGroup);
                    break;
                case "Shotgun":
                    ShowCanvasGroup(ShotgunActiveGroup);
                    HideCanvasGroup(ShotgunInactiveGroup);
                    break;
                default:
                    Debug.LogError("{{UIManager.cs}} Unrecognized string " +
                            "(weaponType) passed to ShowActiveState.");
                    break;
            }
        }

        /// <summary>
        /// Shows the non-hover state for the weapon type when the player's
        /// cursor exits the button in the Choose Weapon screen.
        /// </summary>
        /// <param name="weaponType">Type of weapon for which to show a 
        /// tooltip.</param>
        public void ShowInactiveState(string weaponType)
        {
            switch (weaponType)
            {
                case "AssaultRifle":
                    HideCanvasGroup(ARActiveGroup);
                    ShowCanvasGroup(ARInactiveGroup);
                    break;
                case "Rifle":
                    HideCanvasGroup(RifleActiveGroup);
                    ShowCanvasGroup(RifleInactiveGroup);
                    break;
                case "Shotgun":
                    HideCanvasGroup(ShotgunActiveGroup);
                    ShowCanvasGroup(ShotgunInactiveGroup);
                    break;
                default:
                    Debug.LogError("{{UIManager.cs}} Unrecognized string " +
                            "(weaponType) passed to ShowInactiveState.");
                    break;
            }
        }

        /// <summary>
        /// Hides the tooltip when the player stops hovering over a potential 
        /// weapon choice.
        /// </summary>
        public void HideTooltip()
        {
            TooltipScreenSpaceUI.Instance.HideTooltip();
        }

        /// <summary>
        /// Shows the Choose Weapon screen.
        /// </summary>
        public void ShowChooseWeaponGroup()
        {
            ShowCanvasGroup(ChooseWeaponGroup);
        }

        /// <summary>
        /// Hides the Choose Weapon screen.
        /// </summary>
        public void HideChooseWeaponGroup()
        {
            HideCanvasGroup(ChooseWeaponGroup);
        }
        #endregion

        #region Pause Group Methods

        /// <summary>
        /// Shows the Pause screen.
        /// </summary>
        public void ShowPauseGroup()
        {
            ShowCanvasGroup(PauseGroup);
        }

        /// <summary>
        /// Hides the Pause screen.
        /// </summary>
        public void HidePauseGroup()
        {
            HideCanvasGroup(PauseGroup);
        }

        #endregion

        #region End Game Group Methods

        /// <summary>
        /// Displays whether the player has won or lost. Counts down from 5. At
        /// the end of the countdown, the Main scene will be reloaded.
        /// </summary>
        /// <param name="isWin">Whether or not this end game sequence was
        /// triggered by a win.</param>
        public IEnumerator EndGameCountdownRoutine(bool isWin)
        {
            if (isWin)
            {
                WinLossText.text = "You Win!";
            }
            else
            {
                WinLossText.text = "You Lose!";
            }
            ShowEndGameGroup();
            for (int i = 5; i > 0; i--)
            {
                UpdateCountdownText(i);
                yield return new WaitForSeconds(1.0f);
            }
            HideEndGameGroup();
        }

        /// <summary>
        /// Shows the End Game screen.
        /// </summary>
        public void ShowEndGameGroup()
        {
            ShowCanvasGroup(EndGameGroup);
        }

        /// <summary>
        /// Hides the End Game screen.
        /// </summary>
        public void HideEndGameGroup()
        {
            HideCanvasGroup(EndGameGroup);
        }

        /// <summary>
        /// Updates the countdown text with the given integer.
        /// </summary>
        /// <param name="newCountdownTime">New time to display.</param>
        public void UpdateCountdownText(int newCountdownTime)
        {
            CountdownText.text = newCountdownTime.ToString();
        }
        #endregion
        
        /// <summary>
        /// Shows the group and also makes the group a raycast 
        /// target/interactable.
        /// </summary>
        /// <param name="group">Canvas group to show.</param>
        private void ShowCanvasGroup(CanvasGroup group)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        /// <summary>
        /// Hides the group and also makes the group not a raycast 
        /// target/non-interactable.
        /// </summary>
        /// <param name="group">Canvas group to show.</param>
        private void HideCanvasGroup(CanvasGroup group)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
}
