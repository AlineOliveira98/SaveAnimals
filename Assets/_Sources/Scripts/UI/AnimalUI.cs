using UnityEngine;
using UnityEngine.UI;

public class AnimalUI : MonoBehaviour
{
    [SerializeField] private AnimalType animalType;
    [SerializeField] private Image animalIcon;
    [SerializeField] private GameObject deathState;
    [SerializeField] private GameObject savedState;

    public AnimalType AnimalType { get => animalType; }

    public void Setup(Sprite icon)
    {
        animalIcon.sprite = icon;
    }

    public void UpdateState(bool isDead)
    {
        deathState.SetActive(isDead);
        savedState.SetActive(!isDead);
    }

    public void RevivedState()
    {
        deathState.SetActive(false);
    }
}
