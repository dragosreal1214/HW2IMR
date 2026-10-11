using TMPro;
using UnityEngine;

/// <summary>
/// Text 3D care apare la impact, urca, se intoarce spre jucator si dispare.
/// Prefab: GameObject cu componenta TextMeshPro (3D Object > Text - TextMeshPro, NU cel de UI).
/// </summary>
public class ScorePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private float riseSpeed = 0.4f;

    private float elapsed;
    private Color baseColor = Color.white;

    private void Awake()
    {
        if (text == null) text = GetComponent<TextMeshPro>();
    }

    public void Init(string message, Color color)
    {
        text.text = message;
        baseColor = color;
        text.color = color;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        Camera cam = Camera.main;
        if (cam != null)
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

        Color c = baseColor;
        c.a = Mathf.Clamp01(1f - elapsed / lifetime);
        text.color = c;

        if (elapsed >= lifetime) Destroy(gameObject);
    }
}
