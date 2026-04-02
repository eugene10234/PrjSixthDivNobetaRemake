using Nobeta.Sam.Pathfinding;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using static UnityEngine.UI.Image;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// MageGhost movement actor.
    /// MG does not move — it only faces its target.
    /// Implements IMovementActor so shared states compile without changes.
    /// All movement methods are intentional no-ops.
    /// </summary>
    public class MGMovementActor : MonoBehaviour, IMovementActor
    {
        private Rigidbody _rb;
        private const float ROTATION_SPEED = 6f;
        public Transform muzzle;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.sleepThreshold = 0f;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationZ
                            | RigidbodyConstraints.FreezeRotationY
                            | RigidbodyConstraints.FreezePositionX
                            | RigidbodyConstraints.FreezePositionZ;
        }

        // ── IMovementActor ────────────────────────────────────────────────────

        public Vector3 Position => transform.position;

        public void LookAt(Vector3 target)
        {
            Vector3 lookDir = target - transform.position;
            lookDir.y = 0f; // stay upright, never tilt toward target
            if (lookDir.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, Time.fixedDeltaTime * ROTATION_SPEED);
        }

        public void AimMuzzleAt(Vector3 target)
        {
            Vector3 aimDir = target - muzzle.position;
            if (aimDir.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(aimDir);
            muzzle.rotation = Quaternion.Slerp(
                muzzle.rotation, targetRot, Time.fixedDeltaTime * ROTATION_SPEED);
        }


        public void Stop()
        {
            _rb.linearVelocity = Vector3.zero;
        }

        public void DeathMovement()
        {
            _rb.linearVelocity = new Vector3(0, 0.5f, 0);
        }

        // ── Intentional no-ops (MG does not move) ────────────────────────────

        public void MoveToward(Vector3 target) { }
        public void ApplyAvoidance(Vector3 dir) { }
        public void ClearPath() { }
        public void ReturnHome() { }

        public bool CheckReturnHome()
        {
            return true;
        }
    }
}
