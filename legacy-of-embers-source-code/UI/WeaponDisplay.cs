using Game.Combat;
using Game.Core;
using Game.Entity;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// UI Object that tracks the current weapon setup of the currently 
    /// controlled actor.
    /// </summary>
    public class WeaponDisplay : MonoBehaviour
    {
        [Tooltip("Sprite that appears when an actor is missing a weapon.")]
        public Sprite EmptySprite;

        [Header("Images")]

        [Tooltip("Image where the actor's current weapon will be displayed.")]
        public Image CurrentWeaponImage;

        [Tooltip("Image where the actor's other weapon will be displayed.")]
        public Image OtherWeaponImage;

        private void OnEnable()
        {
            TurnManager.Instance.TurnSequenceUpdated += UpdateWeaponDisplay;
            Fighter.WeaponSwitched += UpdateWeaponDisplay;
        }

        private void OnDisable()
        {
            Fighter.WeaponSwitched -= UpdateWeaponDisplay;
        }

        private void Start()
        {
            UpdateWeaponDisplay();
        }

        /// <summary>
        /// Calls the static instance of the TooltipScreenSpaceUI object to 
        /// display the summary of an weapon when the player hovers over an 
        /// image in the Weapon Display.
        /// </summary>
        /// <param name="current">represents whether the player is hovering
        /// over its current weapon or its other weapon (true if current).
        /// </param>
        public void ShowWeaponTooltip(bool current)
        {
            Actor currentActor = TurnManager.Instance.TurnQueue[0];
            WeaponObject weaponToDisplay = current ?
                currentActor.Fighter.CurrentWeapon :
                currentActor.Fighter.OtherWeapon;
            if (weaponToDisplay == null)
            {
                if (currentActor.Fighter.MeleeWeapon != null)
                {
                    weaponToDisplay = currentActor.Fighter.MeleeWeapon;
                    TooltipScreenSpaceUI.Instance.ShowTooltip(weaponToDisplay.GetWeaponSummary());
                }
                else
                {
                    TooltipScreenSpaceUI.Instance.ShowTooltip("This " +
                        "character is unarmed.");
                    return;
                }
            }
            else
            {
                TooltipScreenSpaceUI.Instance.ShowTooltip(weaponToDisplay.GetWeaponSummary());
            }
        }

        /// <summary>
        /// Calls the static instance of the TooltipScreenSpaceUI object to 
        /// display a simple label when hovering over the "Switch Weapon" 
        /// button.
        /// </summary>
        public void ShowSwitchWeaponTooltip()
        {
            TooltipScreenSpaceUI.Instance.ShowTooltip("<b>Switch Weapon</b>");
        }

        /// <summary>
        /// Calls the static instance of the TooltipScreenUI object to hide the 
        /// tooltip.
        /// </summary>
        public void HideTooltip()
        {
            TooltipScreenSpaceUI.Instance.HideTooltip();
        }

        /// <summary>
        /// Changes the player HUD to show which weapon is currently equipped as
        /// a Current weapon and which weapon is not equipped (other weapon).
        /// </summary>
        private void UpdateWeaponDisplay()
        {
            Actor currentActor = TurnManager.Instance.TurnQueue[0];
            if (currentActor.ActorId.ActorType == ActorType.Bot &&
                    currentActor.ActorId.Alignment == Alignment.Player)
            {
                CurrentWeaponImage.sprite =
                    currentActor.MeleeWeapon.WeaponThumbnail;
                OtherWeaponImage.sprite = EmptySprite;
            }
            else
            {
                if (currentActor.Fighter.CurrentWeapon != null &&
                    currentActor.ActorId.Alignment == Alignment.Player)
                {
                    CurrentWeaponImage.sprite =
                        currentActor.Fighter.CurrentWeapon.WeaponThumbnail;
                }
                else
                {
                    CurrentWeaponImage.sprite = EmptySprite;
                }

                if (currentActor.Fighter.OtherWeapon != null &&
                    currentActor.ActorId.Alignment == Alignment.Player)
                {
                    OtherWeaponImage.sprite =
                        currentActor.Fighter.OtherWeapon.WeaponThumbnail;
                }
                else
                {
                    OtherWeaponImage.sprite = EmptySprite;
                }
            }
        }
    }
}
