using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

/// <summary>
/// Represents a projectile explosion.  Damages enemy entities with an area effect.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ShellExplosion : MonoBehaviour, IProjectile
{
    /// <summary>
    /// Triggered when the projectile is destroyed.
    /// </summary>
    public UnityEvent DestroyedEvent { get { return _destroyedEvent; } }

    [SerializeField] private UnityEvent _destroyedEvent;

    /// <summary>
    /// The time in seconds before the shell is removed.
    /// </summary>
    [SerializeField] private float _maxLifeTime = 10f;

    /// <summary>
    /// The maximum distance away from the explosion tanks can be and are still affected.
    /// </summary>
    [SerializeField] private float _explosionRadius = 5f;
    
    // Optional.
    [SerializeField] private ParticleSystem _explosionParticlePrefab;
    [SerializeField] private AudioSource _explosionAudio;            

    private PlayerModel _owner;
    private Rigidbody _rigidbody;
    private int _areaDamage;

    public void Init(Entity creator)
    {
        // EARLY OUT ! //
        if(creator == null) return;

        _owner = creator.Owner;
        _areaDamage = creator.AreaAttackDamage;
    }
    
    private void Start ()
    {
        _rigidbody = GetComponent<Rigidbody>();
        Assert.IsNotNull(_rigidbody);

        // If it isn't destroyed by then, destroy the shell after it's lifetime.
        Destroy (gameObject, _maxLifeTime);
    }

    private void Update()
    {
        // Point the projectile to its direction of movement.
        if(_rigidbody != null && !Mathf.Approximately(_rigidbody.velocity.sqrMagnitude, 0f))
        {
            transform.forward = _rigidbody.velocity;
        }
    }

    private void OnTriggerEnter (Collider other)
    {
        // EARLY OUT! //
        // If the trigger isn't on an impact layer, ignore it.
        if(!CombatUtils.IsProjectileCollider(other.gameObject.layer)) return;

        // EARLY OUT! //
        // If the collider is a friendly entity, early out.
        if(CombatUtils.IsEntity(other.gameObject.layer) && !CombatUtils.IsEnemy(_owner, other)) return;

        // Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius.
        Collider[] colliders = Physics.OverlapSphere (transform.position, _explosionRadius, CombatUtils.EntityMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            Entity targetEntity = colliders[i].GetComponent<Entity> ();
            
            // If it's an enemy unit, do damage to it.
            if(CombatUtils.IsEnemy(_owner, targetEntity))
            {
                targetEntity.TakeDamage(_areaDamage);
            }
        }

        if(_explosionParticlePrefab != null)
        {
            var explosion = Utils.Instantiate(_explosionParticlePrefab, transform.position, transform.rotation);

            // Play the particle system.
            explosion.Play();
            
            // Once the particles have finished, destroy the gameobject they are on.
            Destroy (explosion.gameObject, explosion.main.duration);
        }

        if(_explosionAudio != null)
        {
            // Play the explosion sound effect.
            _explosionAudio.Play();
        }


        // Destroy the shell.
        Destroy (gameObject);
    }

    private void OnDestroy()
    {
        _destroyedEvent.Invoke();
        _destroyedEvent.RemoveAllListeners();
    }
}