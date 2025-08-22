using UnityEngine;
using System.Collections.Generic;

public class AnimalsUI : MonoBehaviour
{
    [SerializeField] private AnimalUI[] animalsUI;

    private Dictionary<AnimalType, AnimalUI> animalsDict = new();

    void OnEnable()
    {
        Animal.OnAnimalSaved += AnimalSaved;
        Animal.OnAnimalDeath += AnimalDied;
        Animal.OnAnimalDeath += AnimalRevived;
    }

    void OnDisable()
    {
        Animal.OnAnimalSaved -= AnimalSaved;
        Animal.OnAnimalDeath -= AnimalDied;
        Animal.OnAnimalDeath -= AnimalRevived;
    }

    private void Start()
    {
        animalsDict.Clear();

        for (int i = 0; i < GameController.Instance.AllAnimals.Count; i++)
        {
            Animal animal = GameController.Instance.AllAnimals[i];

            if (animalsUI.Length <= i) break;

            animalsUI[i].Setup(animal.Icon);
            animalsDict.Add(animal.AnimalType, animalsUI[i]);
        }
    }


    public void AnimalSaved(AnimalType type)
    {
        if (!animalsDict.ContainsKey(type)) return;

        animalsDict[type].UpdateState(false);
    }

    public void AnimalDied(AnimalType type)
    {
        if (!animalsDict.ContainsKey(type)) return;

        animalsDict[type].UpdateState(true);
    }

    private void AnimalRevived(AnimalType type)
    {
        if (!animalsDict.ContainsKey(type)) return;

        animalsDict[type].UpdateState(false);
    }
}
