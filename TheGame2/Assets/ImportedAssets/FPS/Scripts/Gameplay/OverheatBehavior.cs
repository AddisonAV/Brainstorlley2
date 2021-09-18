using UnityEngine;
using System.Collections.Generic;
using Unity.FPS.Game;
using System.Collections;

namespace Unity.FPS.Gameplay
{


	public class OverheatBehavior : MonoBehaviour
    {
		private bool isThrowingGranade;
		[Header("Granade Options")]
		public int granadeCount = 0;
		public float grenadeSpawnDelay = 0.4f;

		[Header("Bullet Settings")]
		private float lastFired = 0;
		public float fireRate;
		[Tooltip("How much force is applied to the bullet when shooting.")]
		public float bulletForce = 400.0f;
		[Tooltip("How long after reloading that the bullet model becomes visible " +
			"again, only used for out of ammo reload animations.")]
		public float showBulletInMagDelay = 0.6f;
		[Tooltip("The bullet model inside the mag, not used for all weapons.")]
		public SkinnedMeshRenderer bulletInMagRenderer;


		[Header("Gun Sounds")]
		public AudioClip shootSound;
		public AudioClip takeOutSound;
		public AudioClip holsterSound;
		public AudioClip reloadSoundOutOfAmmo;
		public AudioClip reloadSoundAmmoLeft;
		public AudioClip aimSound;


		[Header("Muzzleflash Settings")]
		public bool randomMuzzleflash = false;
		//min should always bee 1
		private int minRandomValue = 1;

		[Range(2, 25)]
		public int maxRandomValue = 5;

		private int randomMuzzleflashValue;

		public bool enableMuzzleflash = true;
		public ParticleSystem muzzleParticles;
		public bool enableSparks = true;
		public ParticleSystem sparkParticles;
		public int minSparkEmission = 1;
		public int maxSparkEmission = 7;

		[Header("Prefabs")]
		public Transform bulletPrefab;
		public Transform casingPrefab;
		public Transform grenadePrefab;

		[Header("Spawnpoints")]
		//Array holding casing spawn points 
		//(some weapons use more than one casing spawn)
		//Casing spawn point array
		public Transform casingSpawnPoint;
		//Bullet prefab spawn from this point
		public Transform bulletSpawnPoint;

		public Transform grenadeSpawnPoint;

		private bool isInspecting = false;

		[Header("Muzzleflash Light Settings")]
		public Light muzzleflashLight;
		public float lightDuration = 0.02f;


		public bool autoReload;
		//Delay between shooting last bullet and reloading
		public float autoReloadDelay;
		//Check if reloading
		private bool isReloading;
		private int currentAmmo;
		//Totalt amount of ammo
		[Tooltip("How much ammo the weapon should have.")]
		public int ammo;
		//Check if out of ammo
		private bool outOfAmmo;

		//sonzin de recarga
		[Header("Audio Sauces")]
		public AudioSource mainAudioSource;
		public AudioSource shootAudioSource;

		//animadorzin
		Animator anim;

		//Holstering weapon
		private bool hasBeenHolstered = false;
		//If weapon is holstered
		private bool holstered;
		//Check if running
		private bool isRunning;
		//Check if aiming
		private bool isAiming;
		//Check if walking
		private bool isWalking;
		//Check if inspecting weapon
		private IEnumerator GrenadeSpawnDelay()
		{

			//Wait for set amount of time before spawning grenade
			yield return new WaitForSeconds(grenadeSpawnDelay);
			//Spawn grenade prefab at spawnpoint
			Instantiate(grenadePrefab,
				grenadeSpawnPoint.transform.position,
				grenadeSpawnPoint.transform.rotation);
		}

		private IEnumerator AutoReload()
		{
			//Wait set amount of time
			yield return new WaitForSeconds(autoReloadDelay);

			if (outOfAmmo == true)
			{
				//Play diff anim if out of ammo
				anim.Play("Reload Out Of Ammo", 0, 0f);

				mainAudioSource.clip = reloadSoundOutOfAmmo;
				mainAudioSource.Play();

				//If out of ammo, hide the bullet renderer in the mag
				//Do not show if bullet renderer is not assigned in inspector
				if (bulletInMagRenderer != null)
				{
					bulletInMagRenderer.GetComponent
					<SkinnedMeshRenderer>().enabled = false;
					//Start show bullet delay
					StartCoroutine(ShowBulletInMag());
				}
			}
			//Restore ammo when reloading
			currentAmmo = ammo;
			outOfAmmo = false;
		}

		//Reload
		private void Reload()
		{

			if (!isReloading)
			{
				//Play diff anim if out of ammo
				anim.Play("Reload Out Of Ammo", 0, 0f);
				mainAudioSource.clip = reloadSoundOutOfAmmo;
				mainAudioSource.Play();

				//If out of ammo, hide the bullet renderer in the mag
				//Do not show if bullet renderer is not assigned in inspector
				if (bulletInMagRenderer != null)
				{
					bulletInMagRenderer.GetComponent
					<SkinnedMeshRenderer>().enabled = false;
					//Start show bullet delay
					StartCoroutine(ShowBulletInMag());
				}
			}
			else
			{
				//Play diff anim if ammo left
				anim.Play("Reload Ammo Left", 0, 0f);

				mainAudioSource.Play();

				//If reloading when ammo left, show bullet in mag
				//Do not show if bullet renderer is not assigned in inspector
				if (bulletInMagRenderer != null)
				{
					bulletInMagRenderer.GetComponent
					<SkinnedMeshRenderer>().enabled = true;
				}
			}
			//Restore ammo when reloading
			currentAmmo = ammo;
			outOfAmmo = false;
			isReloading = false;
		}

		//Enable bullet in mag renderer after set amount of time
		private IEnumerator ShowBulletInMag()
		{

			//Wait set amount of time before showing bullet in mag
			yield return new WaitForSeconds(showBulletInMagDelay);
			bulletInMagRenderer.GetComponent<SkinnedMeshRenderer>().enabled = true;
		}

		//Show light when shooting, then disable after set amount of time
		private IEnumerator MuzzleFlashLight()
		{

			muzzleflashLight.enabled = true;
			yield return new WaitForSeconds(lightDuration);
			muzzleflashLight.enabled = false;
		}

		//Check current animation playing
		private void AnimationCheck()
		{

			//Check if reloading
			//Check both animations
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("Reload Out Of Ammo") ||
				anim.GetCurrentAnimatorStateInfo(0).IsName("Reload Ammo Left"))
			{
				isReloading = true;
			}
			else
			{
				isReloading = false;
			}
			//Check if throwing granade
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("GrenadeThrow")) 
				isThrowingGranade = true;
			else 
				isThrowingGranade = false;

			//Check if inspecting weapon
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("Inspect"))
			{
				isInspecting = true;
			}
			else
			{
				isInspecting = false;
			}
		}
		[System.Serializable]
        public struct RendererIndexData
        {
            public Renderer Renderer;
            public int MaterialIndex;

            public RendererIndexData(Renderer renderer, int index)
            {
                this.Renderer = renderer;
                this.MaterialIndex = index;
            }
        }

        [Header("Visual")] [Tooltip("The VFX to scale the spawn rate based on the ammo ratio")]
        public ParticleSystem SteamVfx;

        [Tooltip("The emission rate for the effect when fully overheated")]
        public float SteamVfxEmissionRateMax = 8f;

        //Set gradient field to HDR
        [GradientUsage(true)] [Tooltip("Overheat color based on ammo ratio")]
        public Gradient OverheatGradient;

        [Tooltip("The material for overheating color animation")]
        public Material OverheatingMaterial;

        [Header("Sound")] [Tooltip("Sound played when a cell are cooling")]
        public AudioClip CoolingCellsSound;

        [Tooltip("Curve for ammo to volume ratio")]
        public AnimationCurve AmmoToVolumeRatioCurve;


        WeaponController m_Weapon;
        AudioSource m_AudioSource;
        List<RendererIndexData> m_OverheatingRenderersData;
        MaterialPropertyBlock m_OverheatMaterialPropertyBlock;
        float m_LastAmmoRatio;
        ParticleSystem.EmissionModule m_SteamVfxEmissionModule;

        void Awake()
        {
			anim = GetComponent<Animator>();
			var emissionModule = SteamVfx.emission;
            emissionModule.rateOverTimeMultiplier = 0f;

            m_OverheatingRenderersData = new List<RendererIndexData>();
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == OverheatingMaterial)
                        m_OverheatingRenderersData.Add(new RendererIndexData(renderer, i));
                }
            }

            m_OverheatMaterialPropertyBlock = new MaterialPropertyBlock();
            m_SteamVfxEmissionModule = SteamVfx.emission;

            m_Weapon = GetComponent<WeaponController>();
            DebugUtility.HandleErrorIfNullGetComponent<WeaponController, OverheatBehavior>(m_Weapon, this, gameObject);

            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.clip = CoolingCellsSound;
            m_AudioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponOverheat);
        }

        void FixedUpdate()
        {

			if (Input.GetKeyDown(KeyCode.R) && !isReloading && !isInspecting)
			{
				//Reload
				Reload();
			}

			if (Input.GetMouseButton(0) && !outOfAmmo && !isReloading && !isInspecting && !isRunning)
			{
				//Shoot automatic
				if (Time.time - lastFired > 1 / fireRate)
				{
					lastFired = Time.time;

					//Remove 1 bullet from ammo
					currentAmmo -= 1;

					shootAudioSource.clip = shootSound;
					shootAudioSource.Play();

					if (!isAiming) //if not aiming
					{

						anim.Play("Fire", 0, 0f);
						//If random muzzle is false
						if (!randomMuzzleflash &&
							enableMuzzleflash == true)
						{
							muzzleParticles.Emit(1);
							//Light flash start
							StartCoroutine(MuzzleFlashLight());
						}
						else if (randomMuzzleflash == true)
						{
							//Only emit if random value is 1
							if (randomMuzzleflashValue == 1)
							{
								if (enableSparks == true)
								{
									//Emit random amount of spark particles
									sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
								}
								if (enableMuzzleflash == true)
								{
									muzzleParticles.Emit(1);
									//Light flash start
									StartCoroutine(MuzzleFlashLight());
								}
							}
						}
					}
					else //if aiming
					{

						anim.Play("Aim Fire", 0, 0f);

						//If random muzzle is false
						if (!randomMuzzleflash)
						{
							muzzleParticles.Emit(1);
							//If random muzzle is true
						}
						else if (randomMuzzleflash == true)
						{
							//Only emit if random value is 1
							if (randomMuzzleflashValue == 1)
							{
								if (enableSparks == true)
								{
									//Emit random amount of spark particles
									sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
								}
								if (enableMuzzleflash == true)
								{
									muzzleParticles.Emit(1);
									//Light flash start
									StartCoroutine(MuzzleFlashLight());
								}
							}
						}
					}

					//Spawn bullet from bullet spawnpoint
					var bullet = (Transform)Instantiate(
						bulletPrefab,
						bulletSpawnPoint.transform.position,
						bulletSpawnPoint.transform.rotation);

					//Add velocity to the bullet
					bullet.GetComponent<Rigidbody>().velocity =
						bullet.transform.forward * bulletForce;

					//Spawn casing prefab at spawnpoint
					Instantiate(casingPrefab,
						casingSpawnPoint.transform.position,
						casingSpawnPoint.transform.rotation);
				}
			}
			AnimationCheck();
			// visual smoke shooting out of the gun
			float currentAmmoRatio = m_Weapon.CurrentAmmoRatio;
            if (currentAmmoRatio != m_LastAmmoRatio)
            {
                m_OverheatMaterialPropertyBlock.SetColor("_EmissionColor",
                    OverheatGradient.Evaluate(1f - currentAmmoRatio));

                foreach (var data in m_OverheatingRenderersData)
                {
                    data.Renderer.SetPropertyBlock(m_OverheatMaterialPropertyBlock, data.MaterialIndex);
                }

                m_SteamVfxEmissionModule.rateOverTimeMultiplier = SteamVfxEmissionRateMax * (1f - currentAmmoRatio);
            }

			//Play knife attack 1 animation when Q key is pressed
			if (Input.GetKeyDown(KeyCode.Q) && !isInspecting)
			{
				anim.Play("Knife Attack 1", 0, 0f);
			}
			//Play knife attack 2 animation when F key is pressed
			if (Input.GetKeyDown(KeyCode.F) && !isInspecting)
			{
				anim.Play("Knife Attack 2", 0, 0f);
			}

			//Throw grenade when pressing G key
			if (Input.GetKeyDown(KeyCode.G) && !isInspecting && granadeCount > 0 && !isThrowingGranade)
			{
				StartCoroutine(GrenadeSpawnDelay());
				//Play grenade throw animation
				anim.Play("GrenadeThrow", 0, 0.0f);
				granadeCount -= 1;
			}

			// cooling sound
			if (CoolingCellsSound)
            {
                if (!m_AudioSource.isPlaying
                    && currentAmmoRatio != 1
                    && m_Weapon.IsWeaponActive
                    && m_Weapon.IsCooling)
                {
                    m_AudioSource.Play();
                }
                else if (m_AudioSource.isPlaying
                         && (currentAmmoRatio == 1 || !m_Weapon.IsWeaponActive || !m_Weapon.IsCooling))
                {
                    m_AudioSource.Stop();
                    return;
                }

                m_AudioSource.volume = AmmoToVolumeRatioCurve.Evaluate(1 - currentAmmoRatio);
            }

            m_LastAmmoRatio = currentAmmoRatio;
        }
    }
}