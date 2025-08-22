using UnityEngine;
using UnityEngine.UI;

public class AnimalUI : MonoBehaviour
{
    [SerializeField] private Image animalIcon;
    [SerializeField] private GameObject deathState;
    [SerializeField] private GameObject savedState;

    public void Setup(Sprite icon)
    {
        animalIcon.sprite = icon;
    }

    public void UpdateState(bool isDead)
    {
        deathState.SetActive(isDead);
        deathState.SetActive(!isDead);
    }
}
