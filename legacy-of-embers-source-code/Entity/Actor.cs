using Cinemachine;
using Game.Building;
using Game.Combat;
using Game.Core;
using Game.Inventories;
using Game.Movement;
using Game.Graphics;
using UnityEngine;

namespace Game.Entity
{
    /// <summary>
    /// Defines essential properties of all actors.
    /// </summary>
    public class Actor : MonoBehaviour
    {
        public static event TurnStarted TurnStarted;

        public static event TurnEnded TurnEnded;

        public event ShowCoverIndicator ShowCoverIndicator;

        public event Crouch Crouch;

        [Header("Actor Setup")]

        [Tooltip("Actor identifier.")]
        public ActorIDObject ActorId;

        [Tooltip("Actor Stats.")]
        public ActorStatsObject ActorStats;

        [Tooltip("Free look camera associated with this actor.")]
        public CinemachineFreeLook ActorCam;

        [Tooltip("Physical collider of the actor.")]
        public Collider ActorCollider;

        [Header("Combat")]

        [Tooltip("Transform used to determine if the actor's Fighter " +
            "component is in cover.")]
        public Transform TargetPoint;

        [Tooltip("The actor's melee weapon prefab.")]
        public WeaponObject MeleeWeapon;

        [Tooltip("The actor's primary weapon prefab.")]
        public WeaponObject PrimaryWeapon;

        [Tooltip("The actor's secondary weapon prefab.")]
        public WeaponObject SecondaryWeapon;

        [Header("Weapon Slots")]

        [Tooltip("Transform representing the position and rotation of a " +
            "handgun relative to humanoid graphics.")]
        public Transform HandgunSlot;

        [Tooltip("Transform representing the position and rotation of a " +
            "rifle relative to humanoid graphics.")]
        public Transform RifleSlot;

        [Tooltip("Transform representing the position and rotation of a " +
            "long rifle relative to humanoid graphics.")]
        public Transform LongRifleSlot;

        [Tooltip("Transform representing the position and rotation of a " +
            "melee weapon relative to humanoid graphics.")]
        public Transform MeleeSlot;

        [Header("Inventories")]

        [Tooltip("Inventory Magnet of the actor. Can be safely left null.")]
        public InventoryMagnet InventoryMagnet;

        [Header("Audio Sources")]

        [Tooltip("AudioSource for building noises.")]
        public AudioSource BuilderAudio;

        [Tooltip("AudioSource for weapon noises.")]
        public AudioSource WeaponAudio;

        [Tooltip("AudioSource for vocal noises.")]
        public AudioSource VocalAudio;

        [Header("Audio Clips - Construction")]
        public AudioClip ConstructionClip;
        public AudioClip ConstructionClipAlt;

        [Header("Audio Clips - Humanoid")]
        public AudioClip HurtClip;
        public AudioClip HurtClipAlt;
        public AudioClip DieClip;
        public AudioClip DieClipAlt;
        public AudioClip JumpClip1;
        public AudioClip JumpClip1Alt;
        public AudioClip JumpClip2;
        public AudioClip JumpClip2Alt;
        public AudioClip DrawWeaponClip;
        public AudioClip HolsterWeaponClip;

        [Header("Audio Clips - Bot")]
        public AudioClip BotDieClip;
        public AudioClip MetalImpact;
        public AudioClip MetalImpactAlt;
        public AudioClip WoodImpact;

        /// <summary>
        /// Builder component of the actor.
        /// </summary>
        public Builder Builder { get; private set; }

        /// <summary>
        /// Fighter component of the actor.
        /// </summary>
        public Fighter Fighter { get; private set; }

        /// <summary>
        /// Mover component of the actor.
        /// </summary>
        public Mover Mover { get; private set; }

        /// <summary>
        /// Vaulter component of the actor. Allows actor to hop over logs.
        /// </summary>
        public Vaulter Vaulter { get; private set; }

        /// <summary>
        /// Animation component of the actor.
        /// </summary>
        public HumanoidAnimationHandler animationHandler { get; private set; }

        /// <summary>
        /// Result of random choice between both hurt audio clips.
        /// </summary>
        private AudioClip humanoidHurtClip;

        /// <summary>
        /// Result of random choice between both die audio clips.
        /// </summary>
        private AudioClip humanoidDieClip;

        /// <summary>
        /// Collection of jump audio clips to be randomly drawn from.
        /// </summary>
        private AudioClip[] humanoidJumpClips;

        /// <summary>
        /// Collection of bot hurt audio clips to be randomly drawn from.
        /// </summary>
        private AudioClip[] botHurtAudioClips;

        // ----- Monobehaviour Methods -----------------------------------------

        private void Awake()
        {
            Builder = GetComponent<Builder>();
            Fighter = GetComponent<Fighter>();
            Mover = GetComponent<Mover>();
            Vaulter = GetComponent<Vaulter>();
            animationHandler = GetComponent<HumanoidAnimationHandler>();

            botHurtAudioClips = new AudioClip[3]
            {
                MetalImpact, MetalImpactAlt, WoodImpact
            };

            bool useBaseVocals = Random.value > 0.5f;
            if (useBaseVocals)
            {
                humanoidHurtClip = HurtClip;
                humanoidDieClip = DieClip;
                humanoidJumpClips = new AudioClip[2]
                {
                    JumpClip1, JumpClip2
                };
            }
            else
            {
                humanoidHurtClip = HurtClipAlt;
                humanoidDieClip = DieClipAlt;
                humanoidJumpClips = new AudioClip[2]
                {
                    JumpClip1Alt, JumpClip2Alt
                };
            }

            InitializeBuilder();
            InitializeFighter();
            InitializeMover();

            TurnManager.Instance.RegisterActor(this);
        }

        private void OnEnable()
        {
            if (animationHandler != null)
            {
                animationHandler.Fighter = Fighter;

                Crouch += animationHandler.Crouch;

                Mover.UpdateSpeed += animationHandler.UpdateSpeed;

                Fighter.UpdateWeaponType += animationHandler.UpdateWeaponType;
                Fighter.DrawWeapon += animationHandler.DrawWeapon;
                Fighter.HolsterWeapon += animationHandler.HolsterWeapon;
                Fighter.FighterDied += animationHandler.Die;
                Fighter.DamageTaken += animationHandler.TakeDamage;
                Fighter.StartMelee += animationHandler.StartMeleeAttack;

                if (Builder != null)
                {
                    Builder.BuildStarted += animationHandler.Build;
                }
            }

            Fighter.FighterDied += OnDeath;
            Fighter.DamageTaken += PlayHurtAudio;
            Fighter.DrawWeapon += PlayDrawWeaponClip;
            Fighter.HolsterWeapon += PlayHolsterWeaponClip;

            if (animationHandler != null && Vaulter != null)
            {
                Vaulter.VaultStarted += animationHandler.StartVault;
                Vaulter.VaultStarted += PlayJumpAudio;
            }
        }

        private void OnDisable()
        {
            TurnEnded = null;
            TurnStarted = null;
            Crouch = null;
        }

        // ----- Actor Methods - Public ----------------------------------------

        /// <summary>
        /// Shows a shield above the actor's head, indicating that those trying
        /// to attack it will incur an accuracy and damage penalty.
        /// </summary>
        public void DisplayCoverIndicator()
        {
            ShowCoverIndicator?.Invoke();
        }

        /// <summary>
        /// Defines what the actor does at the end of its turn.
        /// </summary>
        public void EndTurn()
        {
            Crouch?.Invoke();
            Mover?.EndTurn();
            TurnEnded?.Invoke(ActorId, false);
        }

        /// <summary>
        /// Invokes the crouch delegate to signal when the actor should begin to
        /// crouch.
        /// </summary>
        public void StartCrouch()
        {
            Crouch?.Invoke();
        }

        /// <summary>
        /// Defines what the actor does at the start of its turn.
        /// </summary>
        public void StartTurn()
        {
            TurnStarted?.Invoke(ActorId, true);
            Builder?.StartTurn();
            Fighter?.StartTurn();
            Mover?.StartTurn();
        }

        // ----- Actor Methods - Private ---------------------------------------

        /// <summary>
        /// Sets the actor ID and actor stats of the Builder component, if the 
        /// actor has one.
        /// </summary>
        private void InitializeBuilder()
        {
            if (Builder != null)
            {
                Builder.ActorId = ActorId;
                Builder.ActorStats = ActorStats;
                Builder.BuilderAudio = BuilderAudio;
                Builder.ConstructionClip = ConstructionClip;
                Builder.ConstructionClipAlt = ConstructionClipAlt;
            }
        }

        /// <summary>
        /// Sets the actor ID and actor stats of the Fighter component, if the 
        /// actor has one.
        /// </summary>
        private void InitializeFighter()
        {
            if (Fighter != null)
            {
                Fighter.ActorId = ActorId;
                Fighter.ActorStats = ActorStats;

                Fighter.MeleeWeapon = MeleeWeapon;
                Fighter.CurrentWeapon = PrimaryWeapon;
                Fighter.OtherWeapon = SecondaryWeapon;

                Fighter.TargetPoint = TargetPoint;

                Fighter.HandgunSlot = HandgunSlot;
                Fighter.RifleSlot = RifleSlot;
                Fighter.LongRifleSlot = LongRifleSlot;
                Fighter.MeleeSlot = MeleeSlot;

                Fighter.WeaponAudio = WeaponAudio;
            }
        }

        /// <summary>
        /// Sets the actor ID and actor stats of the Mover component, if the 
        /// actor has one.
        /// </summary>
        private void InitializeMover()
        {
            if (Mover != null)
            {
                Mover.ActorId = ActorId;
                Mover.ActorStats = ActorStats;
            }
        }

        /// <summary>
        /// Deregisters the actor from the turn system and disables the actor.
        /// </summary>
        private void OnDeath()
        {
            TurnManager.Instance.DeregisterActor(this);
            Mover.NavMeshAgent.enabled = false;
            ActorCollider.enabled = false;
            if (ActorId.ActorType == ActorType.Bot)
            {
                VocalAudio.pitch = Random.Range(0.95f, 1.05f);
                VocalAudio.PlayOneShot(BotDieClip, 1.0f);
                gameObject.SetActive(false);
            }
            else
            {
                VocalAudio.pitch = Random.Range(0.95f, 1.05f);
                VocalAudio.PlayOneShot(humanoidDieClip, 1.5f);
            }
            if (ActorId.ActorType == ActorType.Player)
            {
                GameManager.Instance.EndGame(false);
            }
        }

        /// <summary>
        /// Plays the audio clip for the actor drawing a weapon.
        /// </summary>
        private void PlayDrawWeaponClip()
        {
            WeaponAudio.pitch = Random.Range(0.95f, 1.05f);
            WeaponAudio.PlayOneShot(DrawWeaponClip, 0.5f);
        }

        /// <summary>
        /// Plays the audio clip for the actor holstering a weapon.
        /// </summary>
        private void PlayHolsterWeaponClip()
        {
            WeaponAudio.pitch = Random.Range(0.95f, 1.05f);
            WeaponAudio.PlayOneShot(HolsterWeaponClip, 0.5f);
        }

        /// <summary>
        /// Plays audio clip for when the actor takes damage.
        /// </summary>
        private void PlayHurtAudio()
        {
            if (ActorId.ActorType == ActorType.Bot)
            {
                AudioClip botHurtClip =
                    botHurtAudioClips[Random.Range(0, botHurtAudioClips.Length)];
                VocalAudio.pitch = Random.Range(0.85f, 1.15f);
                VocalAudio.PlayOneShot(botHurtClip, 1.0f);
            }
            else
            {
                VocalAudio.pitch = Random.Range(0.85f, 1.15f);
                if (!VocalAudio.isPlaying)
                {
                    VocalAudio.PlayOneShot(humanoidHurtClip, 0.75f);
                }
            }
        }

        /// <summary>
        /// Plays audio clip for when the actor jumps.
        /// </summary>
        private void PlayJumpAudio()
        {
            VocalAudio.pitch = Random.Range(0.95f, 1.05f);
            AudioClip clipToPlay =
                humanoidJumpClips[Random.Range(0, humanoidJumpClips.Length)];
            if (!VocalAudio.isPlaying)
            {
                VocalAudio.PlayOneShot(clipToPlay, 0.75f);
            }
        }
    }
}
