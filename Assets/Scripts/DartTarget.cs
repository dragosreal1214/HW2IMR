using System.Collections;
using UnityEngine;

/// <summary>
/// Tinta de darts. Pune-l pe un Cylinder turtit (scale 1, 0.02, 1) rotit pe X = 90,
/// astfel incat fata lui sa fie spre jucator. Scorul depinde de distanta fata de centru.
/// </summary>
public class DartTarget : MonoBehaviour
{
    [System.Serializable]
    public struct Ring
    {
        [Range(0f, 1f)] public float maxNormalizedRadius; // 0 = centru, 1 = marginea tintei
        public int points;
        public string label;
        public Color color;
    }

    [Header("Scor (de la centru spre margine)")]
    [SerializeField] private Ring[] rings =
    {
        new Ring { maxNormalizedRadius = 0.10f, points = 100, label = "BULLSEYE!", color = Color.red },
        new Ring { maxNormalizedRadius = 0.30f, points = 50,  label = "Excelent",  color = new Color(1f, 0.5f, 0f) },
        new Ring { maxNormalizedRadius = 0.55f, points = 25,  label = "Bine",      color = Color.yellow },
        new Ring { maxNormalizedRadius = 0.80f, points = 10,  label = "OK",        color = Color.green },
        new Ring { maxNormalizedRadius = 1.00f, points = 5,   label = "La limita", color = Color.cyan },
    };

    [Tooltip("Raza tintei in metri. 0 = calculata automat din scale (pentru Cylinder)")]
    [SerializeField] private float radiusOverride = 0f;

    [Header("Feedback vizual")]
    [SerializeField] private Renderer flashRenderer;
    [SerializeField] private float flashDuration = 0.25f;
    [SerializeField] private ParticleSystem hitParticlesPrefab;
    [SerializeField] private ScorePopup popupPrefab;
    [SerializeField] private AudioSource hitSound;

    private Color originalColor;

    private void Awake()
    {
        if (flashRenderer == null) flashRenderer = GetComponent<Renderer>();
        if (flashRenderer != null) originalColor = flashRenderer.material.color;
    }

    private float Radius => radiusOverride > 0f ? radiusOverride : transform.lossyScale.x * 0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        Dart dart = collision.rigidbody != null ? collision.rigidbody.GetComponent<Dart>() : null;
        if (dart == null || dart.HasScored || !dart.InFlight) return;

        Vector3 hitPoint = collision.GetContact(0).point;

        // Distanta de la centru, masurata in planul tintei (transform.up = normala fetei)
        Vector3 offset = Vector3.ProjectOnPlane(hitPoint - transform.position, transform.up);
        float normalized = offset.magnitude / Radius;

        Ring ring = GetRing(normalized);
        dart.StickAt(hitPoint);
        ScoreManager.Instance?.AddScore(ring.points, ring.label);
        PlayFeedback(hitPoint, ring);
    }

    private Ring GetRing(float normalized)
    {
        foreach (Ring r in rings)
            if (normalized <= r.maxNormalizedRadius) return r;
        return rings[rings.Length - 1];
    }

    private void PlayFeedback(Vector3 point, Ring ring)
    {
        if (flashRenderer != null) StartCoroutine(Flash(ring.color));

        if (hitParticlesPrefab != null)
        {
            ParticleSystem ps = Instantiate(hitParticlesPrefab, point, Quaternion.LookRotation(transform.up));
            var main = ps.main;
            main.startColor = ring.color;
            ps.Play();
            Destroy(ps.gameObject, 2f);
        }

        if (popupPrefab != null)
        {
            ScorePopup popup = Instantiate(popupPrefab, point + transform.up * 0.1f, Quaternion.identity);
            popup.Init($"+{ring.points}\n{ring.label}", ring.color);
        }

        if (hitSound != null) hitSound.Play();
    }

    private IEnumerator Flash(Color color)
    {
        flashRenderer.material.color = color;
        yield return new WaitForSeconds(flashDuration);
        flashRenderer.material.color = originalColor;
    }
}
