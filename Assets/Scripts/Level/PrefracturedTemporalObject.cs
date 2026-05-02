using System;
using FutureHeroQuest.Core;
using UnityEngine;

namespace FutureHeroQuest.Level
{
    /// <summary>
    /// Drives a pre-fractured object from semantic timeline state.
    /// Use OpenFracture or another editor tool to create the fragment root ahead of time.
    /// Runtime only swaps intact/fractured roots and optionally wakes fragment physics.
    /// </summary>
    public class PrefracturedTemporalObject : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private EventKind eventKind = EventKind.SetSemanticState;
        [SerializeField] private EventDirection direction = EventDirection.Bidirectional;
        [SerializeField] private string stateKey = "FractureState";
        [SerializeField] private string brokenValue = "Broken";
        [SerializeField] private string targetId;
        [SerializeField] private bool applyExistingStateOnEnable = true;
        [SerializeField] private bool resetOnMismatch;

        [Header("Visual Roots")]
        [SerializeField] private GameObject intactRoot;
        [SerializeField] private GameObject fracturedRoot;
        [SerializeField] private bool startIntact = true;

        [Header("Fragments")]
        [SerializeField] private bool autoCollectFragments = true;
        [SerializeField] private Rigidbody[] fragmentRigidbodies;
        [SerializeField] private Collider[] fragmentColliders;
        [SerializeField] private bool makeFragmentsKinematicUntilBroken = true;
        [SerializeField] private bool toggleFragmentColliders = true;
        [SerializeField] private bool useGravityWhenBroken = true;
        [SerializeField] private bool freezeConstraintsUntilBroken = true;

        [Header("Break Impulse")]
        [SerializeField] private bool applyBreakImpulse = true;
        [SerializeField, Min(0f)] private float impulseStrength = 1.2f;
        [SerializeField, Min(0f)] private float torqueStrength = 0.35f;
        [SerializeField, Range(0f, 1f)] private float jitterStrength = 0.25f;
        [SerializeField] private Vector3 impulseOriginOffset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private int impulseSeed = 12345;

        [Header("Effects")]
        [SerializeField] private AudioSource breakAudio;
        [SerializeField] private ParticleSystem breakParticles;

        private SemanticStateStore _store;
        private bool _isBroken;

        public bool IsBroken => _isBroken;

        private void Awake()
        {
            CollectFragmentsIfNeeded();

            if (startIntact)
                ApplyIntact();
            else
                ApplyBroken(false);
        }

        private void OnEnable()
        {
            _store = SemanticStateStore.EnsureInstance();
            _store.OnStateChanged += HandleStateChanged;

            if (applyExistingStateOnEnable && _store.TryGetState(stateKey, targetId, out string current))
            {
                Apply(IsBrokenValue(current), false);
            }
        }

        private void OnDisable()
        {
            if (_store != null)
                _store.OnStateChanged -= HandleStateChanged;
        }

        public void BroadcastBroken()
        {
            var store = SemanticStateStore.EnsureInstance();
            store.SendState(eventKind, direction, stateKey, brokenValue, targetId, transform.position);
            ApplyBroken(true);
        }

        [ContextMenu("Break Local")]
        public void BreakLocal()
        {
            ApplyBroken(true);
        }

        [ContextMenu("Reset To Intact")]
        public void ResetToIntact()
        {
            ApplyIntact();
        }

        [ContextMenu("Collect Fragments")]
        private void CollectFragmentsFromContext()
        {
            CollectFragments(true);
        }

        private void HandleStateChanged(string key, string value, TimelineEvent evt)
        {
            if (!string.Equals(key, stateKey, StringComparison.OrdinalIgnoreCase)) return;
            if (!string.IsNullOrWhiteSpace(targetId)
                && !string.Equals(evt.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Apply(IsBrokenValue(value), true);
        }

        private void Apply(bool broken, bool playEffects)
        {
            if (broken)
            {
                ApplyBroken(playEffects);
                return;
            }

            if (resetOnMismatch)
                ApplyIntact();
        }

        private void ApplyIntact()
        {
            _isBroken = false;
            SetActive(intactRoot, true);
            SetFragmentPhysics(false);
            SetActive(fracturedRoot, false);
        }

        private void ApplyBroken(bool playEffects)
        {
            bool wasBroken = _isBroken;
            _isBroken = true;

            SetActive(intactRoot, false);
            SetActive(fracturedRoot, true);
            SetFragmentPhysics(true);

            if (wasBroken || !playEffects) return;

            if (breakAudio != null) breakAudio.Play();
            if (breakParticles != null) breakParticles.Play();
            if (applyBreakImpulse) ApplyDeterministicImpulse();
        }

        private void SetFragmentPhysics(bool broken)
        {
            CollectFragmentsIfNeeded();

            if (fragmentRigidbodies != null)
            {
                foreach (Rigidbody rb in fragmentRigidbodies)
                {
                    if (rb == null) continue;

                    if (!broken && !rb.isKinematic) ClearVelocity(rb);
                    if (makeFragmentsKinematicUntilBroken) rb.isKinematic = !broken;
                    if (freezeConstraintsUntilBroken)
                        rb.constraints = broken ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;
                    rb.useGravity = broken && useGravityWhenBroken;
                    rb.detectCollisions = broken;
                }
            }

            if (!toggleFragmentColliders || fragmentColliders == null) return;
            foreach (Collider fragmentCollider in fragmentColliders)
            {
                if (fragmentCollider != null) fragmentCollider.enabled = broken;
            }
        }

        private void ApplyDeterministicImpulse()
        {
            if (fragmentRigidbodies == null || fragmentRigidbodies.Length == 0) return;

            var rng = new System.Random(impulseSeed);
            Vector3 origin = transform.TransformPoint(impulseOriginOffset);

            foreach (Rigidbody rb in fragmentRigidbodies)
            {
                if (rb == null) continue;

                Vector3 outward = rb.worldCenterOfMass - origin;
                if (outward.sqrMagnitude <= 0.0001f)
                    outward = DeterministicUnitVector(rng);
                else
                    outward.Normalize();

                Vector3 jitter = DeterministicUnitVector(rng) * jitterStrength;
                Vector3 forceDirection = (outward + jitter + Vector3.up * 0.15f).normalized;
                rb.AddForce(forceDirection * impulseStrength, ForceMode.Impulse);

                if (torqueStrength > 0f)
                    rb.AddTorque(DeterministicUnitVector(rng) * torqueStrength, ForceMode.Impulse);
            }
        }

        private void CollectFragmentsIfNeeded()
        {
            if (!autoCollectFragments) return;
            bool missingRigidbodies = fragmentRigidbodies == null || fragmentRigidbodies.Length == 0;
            bool missingColliders = fragmentColliders == null || fragmentColliders.Length == 0;
            if (missingRigidbodies || missingColliders) CollectFragments(false);
        }

        private void CollectFragments(bool logResult)
        {
            if (fracturedRoot == null) return;

            fragmentRigidbodies = fracturedRoot.GetComponentsInChildren<Rigidbody>(true);
            fragmentColliders = fracturedRoot.GetComponentsInChildren<Collider>(true);

            if (logResult)
            {
                Debug.Log(
                    $"[{nameof(PrefracturedTemporalObject)}] Collected {fragmentRigidbodies.Length} rigidbodies and {fragmentColliders.Length} colliders.",
                    this);
            }
        }

        private bool IsBrokenValue(string value)
        {
            return string.Equals(value?.Trim(), brokenValue?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static void SetActive(GameObject obj, bool active)
        {
            if (obj != null) obj.SetActive(active);
        }

        private static Vector3 DeterministicUnitVector(System.Random rng)
        {
            float z = (float)(rng.NextDouble() * 2.0 - 1.0);
            float angle = (float)(rng.NextDouble() * Math.PI * 2.0);
            float radius = Mathf.Sqrt(Mathf.Max(0f, 1f - z * z));
            return new Vector3(radius * Mathf.Cos(angle), z, radius * Mathf.Sin(angle));
        }

        private static void ClearVelocity(Rigidbody rb)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
        }
    }
}
