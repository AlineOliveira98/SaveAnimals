using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI victimsCount;

    [Header("Animation")]
    [SerializeField] private float scaleFactor = 1.5f;
    [SerializeField] private float speed = 1f;

    void OnEnable()
    {
        GameController.OnAnimalSaved += UpdateVictimsCount;
        GameController.OnAnimalDied += UpdateVictimsCount;
    }

    void OnDisable()
    {
        GameController.OnAnimalSaved -= UpdateVictimsCount;
        GameController.OnAnimalDied -= UpdateVictimsCount;
    }

    void Start()
    {
        UpdateVictimsCount();

        victimsCount.transform.DOScale(Vector2.one * scaleFactor, speed).SetSpeedBased().SetLoops(-1, LoopType.Yoyo);
    }

    private void UpdateVictimsCount(Animal animal = null)
    {
        int amountAnimals = GameController.Instance.AnimalsCurrentNumber;
        string text = amountAnimals > 1 ? $"{amountAnimals}" : $"WATER THE SEED";
        victimsCount.text = $"{text}";
    }
}
