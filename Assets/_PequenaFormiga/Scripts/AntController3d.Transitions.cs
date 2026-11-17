using UnityEngine;

public partial class AntController3d
{
    private void TryTransitionToTop()
    {
        if (!TryDetectTopSurface(out RaycastHit topHit))
        {
            Debug.Log("Transição cancelada: nenhum chão encontrado.");
            return;
        }

        // Calcula quanto a formiga precisa subir
        // para posicionar a base do collider sobre o chão.
        float clearance = 0.02f;

        float halfHeight =
            capsule.height * 0.5f * transform.lossyScale.y;

        float radius =
            capsule.radius * transform.lossyScale.x;

        float supportDistance = halfHeight + radius;

        Vector3 targetPosition =
            topHit.point
            + Vector3.up * (supportDistance + clearance)
            - (transform.TransformPoint(capsule.center) - transform.position);

        // Posiciona a formiga sobre a plataforma.
        rb.position = targetPosition;

        // Encerra a escalada e restaura a gravidade.
        isClimbing = false;
        rb.useGravity = previousUseGravity;

        rb.linearVelocity = Vector3.zero;
        currentHorizontalVelocity = Vector3.zero;

        inputDirection = Vector3.zero;
        moveInput = Vector2.zero;

        IsRunning = false;
        requireInputRelease = true;

        // Restaura a orientação terrestre.
        if (visual != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(
                -climbNormal,
                Vector3.up
            ).normalized;

            if (forward.sqrMagnitude > 0.001f)
            {
                visual.rotation = Quaternion.LookRotation(
                    forward,
                    Vector3.up
                );
            }
        }

        Debug.Log(
            $"Transição concluída! Plataforma: {topHit.collider.name}"
        );
    }
}
