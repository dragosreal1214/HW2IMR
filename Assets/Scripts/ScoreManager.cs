using TMPro;
using UnityEngine;

/// <summary>
/// Tine scorul total si actualizeaza UI-ul (TextMeshPro pe un Canvas World Space).
/// Pune-l pe un GameObject gol "ScoreManager" in scena.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI totalText;
    [SerializeField] private TextMeshProUGUI lastThrowText;

    private int total;
    private int throws;

    private void Awake()
    {
        Instance = this;
        UpdateUI(0, "-");
    }

    public void AddScore(int points, string zone)
    {
        total += points;
        throws++;
        UpdateUI(points, zone);
    }

    public void ResetScore()
    {
        total = 0;
        throws = 0;
        UpdateUI(0, "-");
    }

    private void UpdateUI(int last, string zone)
    {
        if (totalText != null)
            totalText.text = $"Scor: {total}\nAruncari: {throws}";
        if (lastThrowText != null)
            lastThrowText.text = throws == 0 ? "Arunca o sageata!" : $"Ultima: +{last} ({zone})";
    }
}
