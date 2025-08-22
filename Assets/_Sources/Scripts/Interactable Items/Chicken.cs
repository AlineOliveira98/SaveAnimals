using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Chicken : Animal
{
    [SerializeField] private float delayAnimation;

    public override async void Collect()
    {
        if (!GameController.Instance.HasFeather)
        {
            Debug.Log("You need a Feather");
            return;
        }

        Animator.SetTrigger("Break");

        await UniTask.Delay(TimeSpan.FromSeconds(delayAnimation));
        LockedInteraction = false;

        AnimalsUI.Instance.SetSaved(animalType);
        GameController.Instance.SaveAnimal(this);
    }
}