using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Enemy;
using System.Threading;
using UnityEngine;

public class MGHitBox_Tick : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _muzzle;
    [SerializeField] private GameObject _visualBeam; // A Cylinder or Capsule child
    [SerializeField] private LayerMask _wallMask;
    public EnemyStat enemyStat;

    [Header("FX")]
    public GameObject PreAttackFX;
    public GameObject AttackFX;

    private BoxCollider _hitArea;
    private IEventBus _eventBus;
    private bool _isFiring;
    private float _tickTimer = 1f;
    private const float MAX_DISTANCE = 50f;
    private const float BEAM_WIDTH = 0.2f;

    private void Awake()
    {
        _eventBus = EventBusProvider.GetEventBus();
        _hitArea = GetComponent<BoxCollider>();

        if (_hitArea != null)
        {
            _hitArea.isTrigger = true;
            _hitArea.enabled = false;
        }

        if (_visualBeam != null) _visualBeam.SetActive(false);
        AttackFX.SetActive(false);
    }

    private void Update()
    {
        if (!_isFiring) return;

        UpdateBeamDimensions();
    }

    private void UpdateBeamDimensions()
    {
        // 1. Raycast to find current length
        float currentLength;
        if (Physics.Raycast(_muzzle.position, _muzzle.forward, out RaycastHit hit, MAX_DISTANCE, _wallMask))
            currentLength = hit.distance;
        else
            currentLength = MAX_DISTANCE;

        // 2. Adjust the Visual Mesh
        if (_visualBeam != null)
        {
            // Position the mesh at the center of the beam segment
            _visualBeam.transform.position = _muzzle.position + (_muzzle.forward * (currentLength / 2f));

            // Align rotation with muzzle
            _visualBeam.transform.rotation = _muzzle.rotation * Quaternion.Euler(90, 0, 0); // Cylinders are Y-up by default

            // Scale: X/Z are width, Y is height (Length / 2 because primitive cylinder is 2 units tall)
            _visualBeam.transform.localScale = new Vector3(BEAM_WIDTH, currentLength / 2f, BEAM_WIDTH);
        }

        // 3. Adjust the Collider
        if (_hitArea != null)
        {
            _hitArea.size = new Vector3(BEAM_WIDTH, BEAM_WIDTH, currentLength);
            _hitArea.center = new Vector3(0, 0, currentLength / 2f);

            // Ensure the collider object matches muzzle position/rotation
            transform.position = _muzzle.position;
            transform.rotation = _muzzle.rotation;
        }
    }

    public async UniTask StartAttackFX()
    {
        bool success = await StartAttackSequence(this.destroyCancellationToken);
        if (success)
        {
            AttackActivation();
        }
    }

    private void AttackActivation()
    {
        _isFiring = true;
        if (_visualBeam != null) _visualBeam.SetActive(true);
        AttackFX.SetActive(true);
        _hitArea.enabled = true;
    }

    private async UniTask<bool> StartAttackSequence(CancellationToken token)
    {
        try
        {
            PreAttackFX.SetActive(true);
            await UniTask.Delay(500, cancellationToken: token);
            PreAttackFX.SetActive(false);
            return true;
        }
        catch (System.OperationCanceledException)
        {
            Cleanup();
            return false;
        }
    }

    public void EndAttackFX()
    {
        _isFiring = false;
        Cleanup();
    }

    private void Cleanup()
    {
        if (_visualBeam != null) _visualBeam.SetActive(false);
        AttackFX.SetActive(false);
        PreAttackFX.SetActive(false);
        if (_hitArea != null) _hitArea.enabled = false;
    }

    // --- Damage Logic ---

    private void OnTriggerEnter(Collider other) => TryDamage(other);

    private void OnTriggerStay(Collider other)
    {
        _tickTimer -= Time.deltaTime;
        if (_tickTimer <= 0)
        {
            TryDamage(other);
            _tickTimer = 1f;
        }
    }

    private void TryDamage(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _eventBus.Publish(new PlayerDamagedEvent
            {
                Damage = Mathf.FloorToInt(enemyStat.AttackPower),
                hitPosition = other.ClosestPoint(transform.position)
            });
        }
    }
}
