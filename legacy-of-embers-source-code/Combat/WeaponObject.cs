using Game.Core;
using Game.Graphics;
using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Data representation of a weapon that fighters can use to attack 
    /// eachother.
    /// </summary>
    [CreateAssetMenu(fileName = "new WeaponObject",
    menuName = "Data.../WeaponObject")]
    public class WeaponObject : ScriptableObject
    {
        [Header("Identification")]

        [Tooltip("Unique identifier for the weapon.")]
        public int WeaponID;

        [Tooltip("In-game name of the weapon.")]
        public string WeaponName;

        [Tooltip("Type of weapon")]
        public WeaponClass WeaponClass;

        [Tooltip("In-game description of the weapon.")]
        public string WeaponDescription;

        [Tooltip("The thumbnail to be displayed in the HUD when the weapon " +
            "is equipped.")]
        public Sprite WeaponThumbnail;

        [Header("Prefab")]
        public GameObject WeaponPrefab;

        [Header("Attack Behaviour")]

        [Tooltip("Type of attack this weapon uses.")]
        public AttackType AttackType;

        [Tooltip("How many projectiles are fired (used only with SingleFire, " +
            "BurstFire and SpreadFire attack types).")]
        public int Projectiles = 1;

        [Tooltip("How close to the target the user must be to attack it.")]
        public float AttackRange;

        [Tooltip("How long in between each shot. This should have no impact on " +
            "gameplay, it serves only to increase realism.")]
        public float RecoveryTime;

        [Header("Damage")]

        [Tooltip("Minimum amount of damage the weapon can deal.")]
        public int DamageMin;

        [Tooltip("Maximum amount of damage the weapon can deal.")]
        public int DamageMax;

        [Header("Criticals")]

        [Tooltip("Chance for each attack to deal critical damage, equal to " +
            "its would-be damage roll times the CriticalMultiplier.")]
        [Range(0, 1)]
        public float CriticalChance;

        [Tooltip("When a the firearm scores a critical, multiply its " +
        "would-be damage roll by this number to get total critical damage.")]
        public float CriticalMultiplier;

        [Header("Attacking Cover")]

        [Tooltip("Damage penalty incurred when this weapon attacks a " +
            "target that is in cover.")]
        public int CoverDamagePenalty = 5;

        [Tooltip("Accuracy penalty incurred when this weapon attacks a " +
            "target that is in cover.")]
        public float CoverAccuracyPenalty = 0.50f;

        [Header("Audio")]

        [Tooltip("Clip to be played each time the weapon is fired. Used if " +
            "this weapon is not a melee weapon.")]
        public AudioClip WeaponFireClip;

        [Tooltip("Clip to be played each time the weapon is fired. Used if " +
            "this weapon is a melee weapon.")]
        public AudioClip MeleeWeaponClip;

        ///<summary>
        /// Coroutine to attack the target with this weapon, based on data from 
        /// its associated WeaponObject.
        /// </summary>
        public IEnumerator Attack(ICombatTarget target,
            Vector3 weaponShotOrigin, float sqrDistanceFromTarget,
            WeaponGraphics graphics, Fighter fighter, AudioSource weaponAudio)
        {
            bool targetInCover =
                CoverManager.Instance.CheckCover(weaponShotOrigin,
                target.TargetPoint.position);

            if (targetInCover)
            {
                Debug.Log("Target in cover! Damage and accuracy reduced.");
            }

            // Attack Routine Selection
            switch (AttackType)
            {
                case AttackType.Melee:
                    yield return MeleeRoutine(target, weaponAudio);
                    break;
                case AttackType.SingleFire:
                    yield return SingleFireRoutine(target,
                        sqrDistanceFromTarget, targetInCover, graphics,
                        fighter, weaponAudio);
                    break;
                case AttackType.BurstFire:
                    yield return BurstFireRoutine(target,
                        sqrDistanceFromTarget, targetInCover, graphics,
                        fighter, weaponAudio);
                    break;
                case AttackType.SpreadFire:
                    yield return SpreadFireRoutine(target,
                        sqrDistanceFromTarget, targetInCover, graphics,
                        fighter, weaponAudio);
                    break;
            }

            // Recovery
            yield return new WaitForSeconds(RecoveryTime);
        }

        /// <summary>
        /// Returns a formatted string representing the summary of the weapon, 
        /// crafted from its various stats.
        /// </summary>
        public string GetWeaponSummary()
        {
            string weaponSummary = "";

            weaponSummary += "<b>" + WeaponName + "</b> \n";

            weaponSummary += "<i>" + WeaponDescription + "</i> \n\n";

            weaponSummary += "<b>" + "Average Base Damage: " +
                GetAverageDamage().ToString() + "</b> \n";

            string fireMode;
            switch (AttackType)
            {
                case AttackType.Melee:
                    fireMode = "Melee Attack \n";
                    break;
                case AttackType.SingleFire:
                    fireMode = "Fire Mode: Semi-Auto\n";
                    break;
                case AttackType.BurstFire:
                    fireMode = "Fire Mode: Burst (" + Projectiles + " shots)\n";
                    break;
                case AttackType.SpreadFire:
                    fireMode = "Fire Mode: Spread (" + Projectiles + " projectiles)\n";
                    break;
                default:
                    fireMode = "";
                    break;
            }
            weaponSummary += fireMode;
            weaponSummary += "Range: " + AttackRange + "m\n";
            weaponSummary += "Base Damage: " + DamageMin + "-" + DamageMax + "\n";
            weaponSummary += "Crit%: " + CriticalChance + " | Multiplier: " + CriticalMultiplier + "x";

            return weaponSummary;
        }

        /// <summary>
        /// Returns the average damage of the weapon based on its minimum
        /// damage, maximum damage, attack type, and projectile count.
        /// </summary>
        private float GetAverageDamage()
        {
            float averageDamage;
            if (AttackType == AttackType.Melee || AttackType == AttackType.SingleFire)
            {
                return averageDamage = (DamageMin + DamageMax) * 0.5f;
            }
            else if (AttackType == AttackType.BurstFire || AttackType == AttackType.SpreadFire)
            {
                return averageDamage = ((DamageMin + DamageMax) * 0.5f) *
                    Projectiles;
            }
            else
            {
                Debug.LogError("{{WeaponObject.cs}} Unrecognized " +
                    "AttackType passed to GetAverageDamage.");
                return averageDamage = 0.0f;
            }
        }

        /// <summary>
        /// Instantiates a prefab of this weapon parented to the passed 
        /// transform.
        /// </summary>
        /// <param name="parent">Transform this weapon will be parented 
        /// to.</param>
        public GameObject InstantiateWeaponPrefab(Transform parent)
        {
            if (WeaponPrefab != null)
            {
                GameObject weaponObject = Instantiate(WeaponPrefab, parent) as GameObject;
                weaponObject.SetActive(false);
                return weaponObject;
            }
            else
            {
                return null;
            }
        }

        ///<summary>
        /// Attack routine for burst shot weapons.
        /// </summary>
        private IEnumerator BurstFireRoutine(ICombatTarget target,
            float sqrDistanceFromTarget, bool targetInCover,
            WeaponGraphics graphics, Fighter fighter, AudioSource weaponAudio)
        {
            for (int i = 0; i < Projectiles; i++)
            {
                // Damage Calculation
                CalculateAttackDamage(out int damage, out bool critical,
                    sqrDistanceFromTarget, targetInCover);

                // Fire
                graphics.Fire();
                fighter.FireSignal();
                PlayWeaponFireAudio(weaponAudio);
                yield return new WaitForSeconds(0.2f);
                if (target.HealthReserve.Current > 0)
                {
                    target.TakeDamage(damage, critical);
                }
                yield return new WaitForSeconds(0.05f);
            }
        }

        /// <summary>
        /// Plays an audio clip when the weapon is fired.
        /// </summary>
        /// <param name="weaponAudio">AudioSource through which the clip will
        /// be played.</param>
        private void PlayWeaponFireAudio(AudioSource weaponAudio)
        {
            weaponAudio.pitch = Random.Range(0.95f, 1.05f);
            weaponAudio.PlayOneShot(WeaponFireClip, 1.0f);
        }

        /// <summary>
        /// Plays an audio clip when a melee weapon is swung.
        /// </summary>
        /// <param name="weaponAudio">AudioSource through which the clip will
        /// be played.</param>
        private void PlayMeleeAudio(AudioSource weaponAudio)
        {
            weaponAudio.pitch = Random.Range(0.90f, 1.10f);
            weaponAudio.PlayOneShot(MeleeWeaponClip, 1.0f);
        }

        ///<summary>
        /// Calculates damage for a single bullet or attack.
        /// </summary>
        private void CalculateAttackDamage(out int damage, out bool critical,
            float sqrDistanceFromTarget, bool targetInCover)
        {
            float accuracy = 1.0f;
            if (targetInCover)
            {
                accuracy -= CoverAccuracyPenalty;
            }

            // compares a random value to accuracy to see if a bullet hits.
            if (Random.value < accuracy)
            {
                int baseDamage = Random.Range(DamageMin, DamageMax + 1);
                critical = (Random.value <= CriticalChance);

                if (targetInCover)
                {
                    critical = false;
                }

                damage = critical ?
                    (int)Mathf.Ceil(baseDamage * CriticalMultiplier) :
                    baseDamage;

                if (targetInCover)
                {
                    damage -= CoverDamagePenalty;
                }
            }
            else
            {
                critical = false;
                damage = 0;
            }

            if (damage < 0) damage = 0;
        }

        ///<summary>
        /// Determines number of projectile hits based on distance for a spread
        /// fire weapon.
        /// </summary>
        private int CalculateSpreadProjectileCount(ICombatTarget target,
            float sqrDistanceFromTarget)
        {
            float sqrAttackRange = AttackRange * AttackRange;
            float distPercentage = (sqrDistanceFromTarget / sqrAttackRange);

            int numProjectiles;
            if (distPercentage <= 0.1)
            {
                numProjectiles = Projectiles;
            }
            else if (distPercentage > 0.1 && distPercentage <= 0.9)
            {
                numProjectiles =
                    (int)Mathf.Ceil(Projectiles * (1 - distPercentage));
            }
            else
            {
                numProjectiles = 1;
            }
            //Debug.Log("Num Projectiles: " + numProjectiles);
            return numProjectiles;
        }

        ///<summary>
        /// Attack routine for melee weapons.
        /// </summary>
        private IEnumerator MeleeRoutine(ICombatTarget target,
            AudioSource weaponAudio)
        {
            // Damage Calculation
            CalculateAttackDamage(out int damage, out bool critical, 0.0f,
                false);

            // Attack
            PlayMeleeAudio(weaponAudio);
            yield return new WaitForSeconds(0.5f);
            if (target.HealthReserve.Current > 0)
            {
                target.TakeDamage(damage, critical);
            }
        }

        ///<summary>
        /// Attack routine for single shot weapons.
        /// </summary>
        private IEnumerator SingleFireRoutine(ICombatTarget target,
            float sqrDistanceFromTarget, bool targetInCover,
            WeaponGraphics graphics, Fighter fighter, AudioSource weaponAudio)
        {
            // Damage Calculation
            CalculateAttackDamage(out int damage, out bool critical,
                sqrDistanceFromTarget, targetInCover);

            // Fire
            graphics.Fire();
            fighter.FireSignal();
            PlayWeaponFireAudio(weaponAudio);
            yield return new WaitForSeconds(0.2f);
            if (target.HealthReserve.Current > 0)
            {
                target.TakeDamage(damage, critical);
            }
        }

        ///<summary>
        /// Attack routing for spread shot weapons, like shotguns.
        /// </summary>
        private IEnumerator SpreadFireRoutine(ICombatTarget target,
            float sqrDistanceFromTarget, bool targetInCover,
            WeaponGraphics graphics, Fighter fighter, AudioSource weaponAudio)
        {
            // Projectile Count
            int projectiles = CalculateSpreadProjectileCount(target,
                sqrDistanceFromTarget);

            // Fire
            graphics.Fire();
            fighter.FireSignal();
            PlayWeaponFireAudio(weaponAudio);
            yield return new WaitForSeconds(0.2f);
            for (int i = 0; i < projectiles; i++)
            {
                // Damage Calculation
                CalculateAttackDamage(out int damage, out bool critical,
                    sqrDistanceFromTarget, targetInCover);
                if (target.HealthReserve.Current > 0)
                {
                    target.TakeDamage(damage, critical);
                }
            }
        }
    }
}
