using UnityEngine;

public partial class AntController3d
{
    [Header("Climbing")]
    private Vector3 climbUp;
    private Vector3 climbRight;
    
    [SerializeField] private float climbSpeed = 2f;
    [SerializeField] private float climbDetectionDistance = 1.5f;
    private bool isClimbing;
    private Vector3 climbNormal;
    private bool previousUseGravity;

    private Collider climbSurfaceCollider;

    private void TryStartClimbing()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        Vector3 direction = visual != null
            ? visual.forward
            : transform.forward;

        if (!Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            climbDetectionDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            return;
        }

        if (hit.collider.transform.IsChildOf(transform))
            return;

        isClimbing = true;

        climbNormal = hit.normal;
        climbSurfaceCollider = hit.collider;

        // Define a vertical da escalada usando a vertical do mundo.
        climbUp = Vector3.ProjectOnPlane(
            Vector3.up,
            climbNormal
        ).normalized;

        // Define o eixo lateral sobre a superfície.
        climbRight = Vector3.Cross(
            climbNormal,
            climbUp
        ).normalized;

        previousUseGravity = rb.useGravity;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        currentHorizontalVelocity = Vector3.zero;

        // A frente do modelo aponta para cima da superfície,
        // enquanto o eixo superior local fica alinhado à normal.
        if (visual != null)
        {
            visual.rotation = Quaternion.LookRotation(
                climbUp,
                climbNormal
            );
        }
    }

    private void Climb()
    {
        bool hasContact = UpdateSurfaceContact();

        Vector3 movement =
            climbRight * moveInput.x +
            climbUp * moveInput.y;

        movement = Vector3.ProjectOnPlane(movement, climbNormal);

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        Vector3 targetPosition =
            rb.position +
            movement * climbSpeed * Time.fixedDeltaTime;

        // Se perdeu contato, só permite movimentos
        // que mantenham a formiga sobre a parede.
        if (!hasContact && movement.sqrMagnitude > 0.001f)
        {
            if (!HasWallAtPosition(targetPosition))
            {
                movement = Vector3.zero;
            }
        }

        // Mantém a proteção das bordas laterais.
        if (hasContact && Mathf.Abs(moveInput.x) > 0.01f)
        {
            if (!HasWallAtPosition(targetPosition))
            {
                movement = climbUp * moveInput.y;

                movement = Vector3.ProjectOnPlane(
                    movement,
                    climbNormal
                );

                if (movement.sqrMagnitude > 1f)
                    movement.Normalize();
            }
        }

        rb.linearVelocity = Vector3.zero;

        rb.MovePosition(
            rb.position +
            movement * climbSpeed * Time.fixedDeltaTime
        );

        if (movement.sqrMagnitude > 0.001f && visual != null)
        {
            visual.rotation = Quaternion.LookRotation(
                movement.normalized,
                climbNormal
            );
        }
    }

    private bool UpdateSurfaceContact()
    {
        Vector3 colliderCenter =
            transform.TransformPoint(capsule.center);

        Vector3 origin =
            colliderCenter + climbNormal * 0.05f;

        Vector3 direction = -climbNormal;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction,
            surfaceCheckDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform) ||
                hit.collider.transform == transform)
            {
                continue;
            }

            if (hit.collider != climbSurfaceCollider)
                continue;

            climbNormal = hit.normal;

            return true;
        }

        return false;
    }
}
