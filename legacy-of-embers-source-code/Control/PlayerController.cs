using Game.Building;
using Game.Combat;
using Game.Core;
using Game.Entity;
using Game.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Control
{
    /// <summary>
    /// Uses the New Unity Input System to allow the player to interact with the 
    /// aligned actors.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("Camera through which click raycasts are executed.")]
        public Camera MainCamera;

        [Header("End Turn Button")]

        [Tooltip("End Turn Button")]
        public Button EndTurnButton;

        [Header("Switch Weapon Button")]

        [Tooltip("Switch Weapon Button")]
        public Button SwitchWeaponButton;

        [Header("Build Bot Buttons")]

        [Tooltip("Button to build a collector bot for the player actor. This " +
            "can be safely left blank for non-player builders.")]
        public Button BuildCollectorBotButton;

        [Tooltip("Image representing the build collector bot button.")]
        public Image BuildCollectorBotImage;

        [Tooltip("Button to build a defender bot for the player actor. This " +
            "can be safely left blank for non-player builders.")]
        public Button BuildDefenderBotButton;

        [Tooltip("Image representing the build defender bot button.")]
        public Image BuildDefenderBotImage;

        [Tooltip("Button to build a warrior bot for the player actor. This " +
            "can be safely left blank for non-player builders.")]
        public Button BuildWarriorBotButton;

        [Tooltip("Image representing the build warrior bot button.")]
        public Image BuildWarriorBotImage;

        /// <summary>
        /// Actor the controller is currently controlling.
        /// </summary>
        public Actor ActorToControl { get; set; }

        /// <summary>
        /// Current position of the player's mouse.
        /// </summary>        
        public Vector2 MousePosition { get; set; }

        /// <summary>
        /// Array to store build menu buttons for easy interactability toggling.
        /// </summary>
        private Button[] buildButtons;
        private Image[] buildImages;

        private void Awake()
        {
            buildButtons =
                new Button[3] {BuildCollectorBotButton, BuildDefenderBotButton,
                BuildWarriorBotButton};
            buildImages =
                new Image[3] {BuildCollectorBotImage, BuildDefenderBotImage,
                BuildWarriorBotImage};
        }

        private void OnEnable()
        {
            Actor.TurnEnded += ToggleBuildButtons;
            Actor.TurnStarted += ToggleBuildButtons;

            Builder.UpdateBuildAbility += UpdateBuildButtons;

            Mover.UpdateMoveState += ToggleEndTurnButton;
        }

        private void OnDisable()
        {
            Builder.UpdateBuildAbility -= UpdateBuildButtons;
            Mover.UpdateMoveState -= ToggleEndTurnButton;
        }

        private void Start()
        {
            BuildCollectorBotButton.onClick.AddListener(() =>
                    { OnBuildBot(ActorSubtype.Collector); });
            BuildDefenderBotButton.onClick.AddListener(() =>
                    { OnBuildBot(ActorSubtype.Defender); });
            BuildWarriorBotButton.onClick.AddListener(() =>
                    { OnBuildBot(ActorSubtype.Warrior); });

            SwitchWeaponButton.onClick.AddListener(OnSwitchWeapon);
        }

        private void Update()
        {
            if (ActorToControl != null)
            {
                if (Physics.Raycast(GetMouseRay(), out RaycastHit hitInfo))
                {
                    if (hitInfo.transform.CompareTag("Enemy"))
                    {
                        ICombatTarget enemyTarget =
                            hitInfo.transform.gameObject.GetComponent<ICombatTarget>();
                        Vector3 weaponShotOrigin =
                            ActorToControl.Fighter.HandgunSlot.position;
                        if (CoverManager.Instance.CheckCover(weaponShotOrigin,
                            enemyTarget.TargetPoint.position))
                        {
                            Actor targetActor = hitInfo.transform.GetComponent<Actor>();
                            if (targetActor != null)
                            {
                                targetActor.DisplayCoverIndicator();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the infinite ray that runs from the camera through the
        /// mouse's current position on screen.
        /// </summary>
        /// <returns>The infinite ray that runs from the camera through the
        /// mouse's current position on screen.</returns>
        public Ray GetMouseRay()
        {
            return MainCamera.ScreenPointToRay(MousePosition);
        }

        /// <summary>
        /// Builds a bot when the player presses an interactable building menu
        /// button.
        /// </summary>
        /// <param name="botType">Type of bot to be built.</param>
        public void OnBuildBot(ActorSubtype botType)
        {
            if (ActorToControl != null)
            {
                AttemptBuild(botType);
            }
            else
            {
                Debug.Log("No actor to control!");
            }
        }

        /// <summary>
        /// Responds to mouse movement from the New Input System.
        /// </summary>
        /// <param name="context">Input action from which to read input.</param>
        public void OnMouseMove(InputAction.CallbackContext context)
        {
            MousePosition = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Responds to pause input from the New Input System.
        /// </summary>
        /// <param name="context">Input action from which to read input.</param>
        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                GameManager.Instance.TogglePauseState();
            }
        }

        /// <summary>
        /// Responds to right click event from the New Input System.
        /// </summary>
        /// <param name="context">Input action from which to read input.</param>
        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (ActorToControl != null)
                {
                    if (Physics.Raycast(GetMouseRay(), out RaycastHit hitInfo))
                    {
                        if (hitInfo.transform.CompareTag("Ground"))
                        {
                            AttemptMove(hitInfo);
                        }
                        if (hitInfo.transform.CompareTag("Enemy") ||
                            hitInfo.transform.CompareTag("Crate") ||
                            hitInfo.transform.CompareTag("Actor"))
                        {
                            AttemptAttack(hitInfo);
                        }
                    }
                }
                else
                {
                    Debug.Log("No Actor To Control!");
                }
            }
        }

        /// <summary>
        /// Prompts the fighter component of the controlled acotr to switch 
        /// weapons, if possible.
        /// </summary>
        public void OnSwitchWeapon()
        {
            StartCoroutine(ActorToControl.Fighter.SwitchWeapon());
        }

        /// <summary>
        /// Starts the controlled actor's turn.
        /// </summary>
        public void StartTurn()
        {
            ActorToControl.StartTurn();
            EndTurnButton.interactable = true;
        }

        /// <summary>
        /// Attempts to attack whatever the raycast hits. Will fail if the actor
        /// does not have an Fighter component or if the actor is currently 
        /// moving.
        /// </summary>
        /// <param name="hitInfo">Hit information resulting from the mouse ray
        /// cast when clicked.</param>
        private void AttemptAttack(RaycastHit hitInfo)
        {
            if (!ActorToControl.Mover.IsMoving ||
                ActorToControl.Mover == null)
            {
                if (ActorToControl.Fighter != null)
                {
                    StartCoroutine(ActorToControl.Fighter.Attack(hitInfo));
                }
                else
                {
                    Debug.Log("This actor cannot attack " +
                        "(or take damage!!)");
                }
            }
            else
            {
                Debug.Log("Cannot attack while moving!");
            }
        }

        /// <summary>
        /// Attempts to build the specified botType. Will fail if the actor does
        /// not have a builder component or if the actor is currently moving.
        /// </summary>
        /// <param name="botType">Type of bot to be built.</param>
        private void AttemptBuild(ActorSubtype botType)
        {
            if (ActorToControl.Builder != null)
            {
                if (!ActorToControl.Mover.IsMoving ||
                            ActorToControl.Mover == null)
                {
                    ActorToControl.Builder.BuildBot(botType);
                }
                else
                {
                    Debug.Log("Cannot build while moving!");
                }
            }
            else
            {
                Debug.Log("This actor cannot build!");
            }
        }

        /// <summary>
        /// Attempts to move the actor to the passed ground intersection point.
        /// Will fail if the actor does not have a Mover component.
        /// </summary>
        /// <param name="hitInfo">Hit information resulting from the mouse ray
        /// cast when clicked.</param>
        private void AttemptMove(RaycastHit hitInfo)
        {
            if (ActorToControl.Mover != null)
            {
                ActorToControl.Mover.MoveTo(hitInfo.point);
            }
            else
            {
                Debug.Log("This actor is stationary!");
            }
        }

        /// <summary>
        /// Disallows player to click build buttons when it is not their turn.
        /// </summary>
        /// <param name="toActive">Determines if the buttons should be made
        /// interactable or not interactable.</param>
        private void ToggleBuildButtons(ActorIDObject actorId, bool toActive)
        {
            if (ActorToControl != null)
            {
                if (ActorToControl.Builder == null)
                {
                    for (int i = 0; i < buildButtons.Length; i++)
                    {
                        buildButtons[i].interactable = false;
                        buildImages[i].color = new Color(1, 1, 1, 0.5f);
                    }
                    return;
                }
                if (actorId == ActorToControl.ActorId)
                {
                    for (int i = 0; i < buildButtons.Length; i++)
                    {
                        buildButtons[i].interactable = toActive;

                        if (toActive)
                        {
                            buildImages[i].color = buildImages[i].color = new Color(1, 1, 1, 1);
                        }
                        else
                        {
                            buildImages[i].color = new Color(1, 1, 1, 0.5f);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < buildButtons.Length; i++)
                {
                    buildButtons[i].interactable = false;
                    buildImages[i].color = new Color(1, 1, 1, 0.5f);
                }
            }
        }

        /// <summary>
        /// Toggles interactability of the end turn button.
        /// </summary>
        /// <param name="toActive">Wheter the end turn button should be made
        /// interactable or not.</param>
        private void ToggleEndTurnButton(ActorIDObject actorId, bool toActive)
        {
            EndTurnButton.interactable = toActive;
        }

        /// <summary>
        /// Sets the build buttons as interactable if the ActorToControl's
        /// builder currently has the capacity to build the specified bot.
        /// </summary>
        /// <param name="actorId">ID of the builder who's inventory has
        /// updated.</param>
        /// <param name="buildAbility">Represents ability to build bots. True
        /// means the bot can be built and the button should be 
        /// interactable.</param>
        private void UpdateBuildButtons(ActorIDObject actorId,
            bool[] buildAbility)
        {
            if (ActorToControl != null)
            {
                if (actorId == ActorToControl.ActorId)
                {
                    for (int i = 0; i < buildAbility.Length; i++)
                    {
                        buildButtons[i].interactable = buildAbility[i];
                        if (buildAbility[i])
                        {
                            buildImages[i].color = new Color(1, 1, 1, 1);
                        }
                        else
                        {
                            buildImages[i].color = new Color(1, 1, 1, 0.5f);
                        }
                    }
                }
            }
        }
    }
}
