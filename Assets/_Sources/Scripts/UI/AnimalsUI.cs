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
        Animal.OnAnimalRevived += AnimalRevived;
    }

    void OnDisable()
    {
        Animal.OnAnimalSaved -= AnimalSaved;
        Animal.OnAnimalDeath -= AnimalDied;
        Animal.OnAnimalRevived -= AnimalRevived;
    }

    private void Start()
    {
        animalsDict.Clear();

        foreach (var animal in animalsUI)
        {
            animalsDict.Add(animal.AnimalType, animal);
        }

        foreach (var animal in GameController.Instance.AllAnimals)
        {
            if (!animalsDict.ContainsKey(animal.AnimalType)) continue;

            animalsDict[animal.AnimalType].Setup(animal.Icon);
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

        animalsDict[type].RevivedState();
    }
}
