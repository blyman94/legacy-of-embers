using Game.Core;
using Game.Combat.Reserves;
using Game.Graphics;
using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Component that allows the actor to attack other fighters, take damage
    /// from other fighters, and die.
    /// </summary>
    [RequireComponent(typeof(HealthReserve))]
    public class Fighter : MonoBehaviour, ICombatTarget
    {
        // ----- Static Events -------------------------------------------------

        public static event UpdateAttackCount UpdateAttackCount;
        public static event RequestDamagePopup RequestDamagePopup;
        public static event WeaponSwitched WeaponSwitched;

        // ----- Events --------------------------------------------------------
        public event UpdateWeaponType UpdateWeaponType;
        public event DrawWeapon DrawWeapon;
        public event HolsterWeapon HolsterWeapon;
        public event StartMelee StartMelee;
        public event FighterDied FighterDied;
        public event DamageTaken DamageTaken;
        public event ShotFailed ShotFailed;
        public event Fire Fire;

        // ----- Fighter Properties -------------------------------------------

        /// <summary>
        /// Actor ID of the owning actor.
        /// </summary>
        public ActorIDObject ActorId { get; set; }

        /// <summary>
        /// Actor stats of the owning actor.
        /// </summary>
        public ActorStatsObject ActorStats { get; set; }

        /// <summary>
        /// Number of attacks this fighter has remaining in its current turn.
        /// </summary>
        public int Attacks
        {
            get
            {
                return attacks;
            }
            set
            {
                attacks = value;
                UpdateAttackCount?.Invoke(attacks);
            }
        }

        /// <summary>
        /// Reference to the current weapon the fighter has equipped.
        /// </summary>
        public WeaponObject CurrentWeapon { get; set; }

        /// <summary>
        /// Transform representing the position and rotation of a handgun
        /// relative to humanoid graphics.
        /// </summary>
        public Transform HandgunSlot { get; set; }

        /// <summary>
        /// Bool to determine if the actor is alive (currentHealth > 0).
        /// </summary>
        public bool IsAlive { get; private set; }

        /// <summary>
        /// Transform representing the position and rotation of a long rifle
        /// relative to humanoid graphics.
        /// </summary>
        public Transform LongRifleSlot { get; set; }

        /// <summary>
        /// Transform representing the position and rotation of a melee weapon
        /// relative to humanoid graphics.
        /// </summary>
        public Transform MeleeSlot { get; set; }

        /// <summary>
        /// Melee weapon of the fighter.
        /// </summary>
        public WeaponObject MeleeWeapon { get; set; }

        /// <summary>
        /// Reference to the other weapon the fighter has in their inventory.
        /// </summary>
        public WeaponObject OtherWeapon { get; set; }

        /// <summary>
        /// Transform representing the position and rotation of a rifle
        /// relative to humanoid graphics.
        /// </summary>
        public Transform RifleSlot { get; set; }

        /// <summary>
        /// Audio source through which to play weapon audio.
        /// </summary>
        public AudioSource WeaponAudio { get; set; }

        // ----- Interface Properties ------------------------------------------

        public Alignment Alignment
        {
            get
            {
                return ActorId.Alignment;
            }
        }

        public IReserve HealthReserve { get; set; }

        public Transform TargetPoint { get; set; }

        public Transform Transform
        {
            get
            {
                return transform;
            }
        }

        // ----- Private Variables ---------------------------------------------

        /// <summary>
        /// Number of attacks this fighter has remaining in its current turn.
        /// </summary>
        private int attacks;

        /// <summary>
        /// GameObject representing the Fighter's current weapon.
        /// </summary>
        private GameObject currentWeaponObject;

        /// <summary>
        /// Object containing the weapons graphics, including VFX and lighting.
        /// </summary>
        private WeaponGraphics graphics;

        /// <summary>
        /// GameObject representing the Fighter's melee weapon.
        /// </summary>
        private GameObject meleeWeaponObject;

        /// <summary>
        /// GameObject representing the Fighter's other weapon.
        /// </summary>
        private GameObject otherWeaponObject;

        /// <summary>
        /// True when the fighter is currently switching weapons.
        /// </summary>
        private bool switchingWeapon;

        // ----- Monobehaviour Methods -----------------------------------------

        public void Awake()
        {
            HealthReserve = GetComponent<HealthReserve>();
            IsAlive = true;
        }

        public void OnEnable()
        {
            HealthReserve.ReserveEmpty += Die;
        }

        private void OnDisable()
        {
            UpdateWeaponType = null;
            DrawWeapon = null;
            HolsterWeapon = null;
            Fire = null;
            DamageTaken = null;
            FighterDied = null;
            ShotFailed = null;
            StartMelee = null;
        }

        public void Start()
        {
            HealthReserve.Max = ActorStats.MaxHealth;
        }

        // ----- Fighter Methods, Public ---------------------------------------

        /// <summary>
        /// Inflicts damage on the fighter component of the RaycastHit passed.
        /// </summary>
        /// <param name="hitInfo">Raycast hit resulting from an action such
        /// as clicking the mouse.</param>
        public IEnumerator Attack(RaycastHit hitInfo)
        {
            ICombatTarget target =
                hitInfo.transform.GetComponent<ICombatTarget>();
            yield return StartCoroutine(Attack(target));
        }

        /// <summary>
        /// Inflicts damage on the ICombatTarget passed.
        /// </summary>
        /// <param name="target">Combat target to damage.</param>
        public IEnumerator Attack(ICombatTarget target)
        {
            if (target.Alignment != ActorId.Alignment)
            {
                if (Attacks > 0)
                {
                    if (target != null)
                    {
                        float sqrDistance = (transform.position -
                            target.Transform.position).sqrMagnitude;
                        if (sqrDistance <=
                            MeleeWeapon.AttackRange * MeleeWeapon.AttackRange)
                        {
                            yield return MeleeAttack(target, sqrDistance);
                        }
                        else if (CurrentWeapon != null &&
                            sqrDistance <=
                            CurrentWeapon.AttackRange * CurrentWeapon.AttackRange)
                        {
                            yield return Shoot(target, sqrDistance, graphics);
                        }
                        else
                        {
                            ShotFailed?.Invoke("Out of Range!");
                            yield break;
                        }
                    }
                    else
                    {
                        Debug.Log("Target does not have a Fighter component!");
                        yield break;
                    }
                }
                else
                {
                    ShotFailed?.Invoke("No More Attacks!");
                    yield break;
                }
            }
            else
            {
                ShotFailed?.Invoke("Cannot Attack Someone On Your Team!");
                yield break;
            }
        }

        /// <summary>
        /// Invokes the "Fire" event when called.
        /// </summary>
        public void FireSignal()
        {
            Fire?.Invoke();
        }

        /// <summary>
        /// Returns the squared attack range of this Fighter's current weapon.
        /// </summary>
        public float GetSqrAttackRange()
        {
            return (CurrentWeapon.AttackRange * CurrentWeapon.AttackRange);
        }

        /// <summary>
        /// Starts the fighter's turn by instantiating its weapons if they are 
        /// null and setting its maximum attack count.
        /// </summary>
        public void StartTurn()
        {
            if (MeleeWeapon != null)
            {
                meleeWeaponObject =
                    MeleeWeapon.InstantiateWeaponPrefab(MeleeSlot);
            }
            if (CurrentWeapon != null && currentWeaponObject == null)
            {
                switch (CurrentWeapon.WeaponClass)
                {
                    case WeaponClass.LongRifle:
                        currentWeaponObject =
                            CurrentWeapon.InstantiateWeaponPrefab(LongRifleSlot);
                        break;
                    case WeaponClass.Handgun:
                        currentWeaponObject =
                            CurrentWeapon.InstantiateWeaponPrefab(HandgunSlot);
                        break;
                    case WeaponClass.Rifle:
                        currentWeaponObject =
                            CurrentWeapon.InstantiateWeaponPrefab(RifleSlot);
                        break;
                    case WeaponClass.Melee:
                        break;
                    default:
                        Debug.LogError("{{Fighter.cs}} Unrecognized " +
                        "WeaponClass passed to StartTurn.");
                        break;
                }
                if (currentWeaponObject != null)
                {
                    graphics =
                        currentWeaponObject.GetComponent<WeaponGraphics>();
                }
            }

            if (OtherWeapon != null && otherWeaponObject == null)
            {
                switch (OtherWeapon.WeaponClass)
                {
                    case WeaponClass.LongRifle:
                        otherWeaponObject =
                            OtherWeapon.InstantiateWeaponPrefab(LongRifleSlot);
                        break;
                    case WeaponClass.Handgun:
                        otherWeaponObject =
                            OtherWeapon.InstantiateWeaponPrefab(HandgunSlot);
                        break;
                    case WeaponClass.Rifle:
                        otherWeaponObject =
                            OtherWeapon.InstantiateWeaponPrefab(RifleSlot);
                        break;
                    case WeaponClass.Melee:
                        break;
                    default:
                        Debug.LogError("{{Fighter.cs}} Unrecognized " +
                        "WeaponClass passed to StartTurn.");
                        break;
                }
            }

            UpdateWeaponType?.Invoke(CurrentWeapon.WeaponClass);
            if (currentWeaponObject != null)
            {
                currentWeaponObject.SetActive(true);
            }
            DrawWeapon?.Invoke();
            Attacks = ActorStats.MaxAttacksPerTurn;
        }

        /// <summary>
        /// Switches the fighters's current weapon to their other weapon, and 
        /// vice versa.
        /// </summary>
        public IEnumerator SwitchWeapon()
        {
            if (!switchingWeapon)
            {
                if (CurrentWeapon != null && OtherWeapon != null)
                {
                    switchingWeapon = true;

                    HolsterWeapon.Invoke();
                    yield return new WaitForSeconds(0.65f);
                    currentWeaponObject.SetActive(false);

                    WeaponObject current = CurrentWeapon;
                    CurrentWeapon = OtherWeapon;
                    OtherWeapon = current;

                    GameObject currentObject = currentWeaponObject;
                    currentWeaponObject = otherWeaponObject;
                    otherWeaponObject = currentObject;

                    graphics =
                        currentWeaponObject.GetComponent<WeaponGraphics>();

                    UpdateWeaponType?.Invoke(CurrentWeapon.WeaponClass);
                    currentWeaponObject.SetActive(true);
                    WeaponSwitched?.Invoke();
                    DrawWeapon.Invoke();
                }
                else
                {
                    Debug.Log("This actor does not have multiple weapons!");
                }
            }
            else
            {
                Debug.Log("Must finish switch action first!");
            }
            switchingWeapon = false;
        }

        // ----- Fighter Methods, Private --------------------------------------

        /// <summary>
        /// Attacks an enemy at very close range with the fighters's melee 
        /// weapon.
        /// </summary>
        /// <param name="target">ICombatTarget to attack.</param>
        /// <param name="sqrDistance">Squared distance from the target.</param>
        private IEnumerator MeleeAttack(ICombatTarget target, float sqrDistance)
        {
            yield return RotateTowardsTargetRoutine(target);
            Attacks -= 1;
            StartMelee?.Invoke();

            if (ActorId.ActorType != ActorType.Bot)
            {
                yield return new WaitForSeconds(0.25f);

                if (currentWeaponObject != null)
                {
                    currentWeaponObject.SetActive(false);
                }
                if (meleeWeaponObject != null)
                {
                    meleeWeaponObject.SetActive(true);
                }
                yield return new WaitForSeconds(0.75f);
            }

            yield return StartCoroutine(MeleeWeapon.Attack(target,
                HandgunSlot.position, sqrDistance, null, this, WeaponAudio));

            if (ActorId.ActorType != ActorType.Bot)
            {
                yield return new WaitForSeconds(0.75f);

                if (meleeWeaponObject != null)
                {
                    meleeWeaponObject.SetActive(false);
                }
                if (currentWeaponObject != null)
                {
                    currentWeaponObject.SetActive(true);
                }
            }
        }

        ///<summary>
        /// Uses a dot product to determine if the marksman component's
        /// transform is facing the combat target's transform. Rotates the 
        /// marksman component until it is facing the combat target's transform.
        /// </summary>
        private IEnumerator RotateTowardsTargetRoutine(ICombatTarget target)
        {
            Vector3 lookPos = new Vector3(target.Transform.position.x,
                transform.position.y, target.Transform.position.z);
            Vector3 lookDir = (lookPos - transform.position);
            float dot = Vector3.Dot(lookDir.normalized, transform.forward);

            while (dot < 0.999f)
            {
                Vector3 newDir =
                    Vector3.RotateTowards(transform.forward, lookDir,
                    Time.deltaTime * 10, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDir);
                dot = Vector3.Dot(lookDir.normalized, transform.forward);
                yield return null;
            }
        }

        /// <summary>
        /// Attacks the enemy with the current weapon.
        /// </summary>
        /// <param name="target">ICombatTarget to attack.</param>
        /// <param name="sqrDistance">Squared distance from the target.</param>        
        private IEnumerator Shoot(ICombatTarget target, float sqrDistance, WeaponGraphics graphics)
        {
            yield return RotateTowardsTargetRoutine(target);
            Attacks -= 1;
            yield return StartCoroutine(CurrentWeapon.Attack(target,
                HandgunSlot.position, sqrDistance, graphics, this,
                WeaponAudio));
        }

        // ----- Interface Methods ---------------------------------------------

        public void Die()
        {
            IsAlive = false;
            FighterDied?.Invoke();
        }

        public void TakeDamage(int damage, bool critical)
        {
            RequestDamagePopup?.Invoke(TargetPoint.position, damage, critical);
            if (HealthReserve.Current - damage > 0 && damage > 0)
            {
                DamageTaken?.Invoke();
            }
            HealthReserve.Modify(-damage);
        }
    }
}
