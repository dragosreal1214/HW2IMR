using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRI 3.x (daca ai XRI 2.x, sterge linia asta)

/// <summary>
/// Sageata (dart). Pune-l pe obiectul care are Rigidbody + XR Grab Interactable.
/// Varful sagetii trebuie sa fie pe axa +Z (sageata albastra in Scene view).
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class Dart : MonoBehaviour
{
    [Tooltip("Dupa cate secunde revine sageata pe masa dupa ce s-a infipt in tinta")]
    [SerializeField] private float respawnAfterHit = 2f;
    [Tooltip("Dupa cate secunde revine sageata daca a ratat tinta")]
    [SerializeField] private float respawnAfterMiss = 4f;
    [Tooltip("Optional: Trail Renderer pentru efect in zbor")]
    [SerializeField] private TrailRenderer trail;

    public bool HasScored { get; private set; }
    public bool InFlight { get; private set; }

    private Rigidbody rb;
    private XRGrabInteractable grab;
    private Vector3 startPos;
    private Quaternion startRot;
    private Coroutine respawnRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        startPos = transform.position;
        startRot = transform.rotation;

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);

        if (trail != null) trail.emitting = false;
    }

    private void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (respawnRoutine != null) { StopCoroutine(respawnRoutine); respawnRoutine = null; }
        HasScored = false;
        InFlight = false;
        if (trail != null) { trail.Clear(); trail.emitting = false; }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        InFlight = true;
        if (trail != null) trail.emitting = true;
        respawnRoutine = StartCoroutine(RespawnAfter(respawnAfterMiss));
    }

    private void FixedUpdate()
    {
        // Bonus: sageata se orienteaza dupa directia de zbor
        // (Unity 2022 si mai vechi: foloseste rb.velocity in loc de rb.linearVelocity)
        if (InFlight && !rb.isKinematic && rb.linearVelocity.sqrMagnitude > 1f)
            rb.MoveRotation(Quaternion.LookRotation(rb.linearVelocity));
    }

    /// <summary>Apelat de DartTarget cand sageata loveste tinta.</summary>
    public void StickAt(Vector3 point)
    {
        HasScored = true;
        InFlight = false;
        if (trail != null) trail.emitting = false;

        rb.isKinematic = true;          // ramane infipta
        transform.position = point;
        grab.enabled = false;           // nu poate fi apucata cat timp e infipta

        if (respawnRoutine != null) StopCoroutine(respawnRoutine);
        respawnRoutine = StartCoroutine(RespawnAfter(respawnAfterHit));
    }

    private IEnumerator RespawnAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        Respawn();
    }

    private void Respawn()
    {
        respawnRoutine = null;
        if (grab.isSelected) return;    // e deja in mana jucatorului

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = startPos;
        rb.rotation = startRot;
        transform.SetPositionAndRotation(startPos, startRot);

        HasScored = false;
        InFlight = false;
        grab.enabled = true;
        if (trail != null) { trail.Clear(); trail.emitting = false; }
    }
}
