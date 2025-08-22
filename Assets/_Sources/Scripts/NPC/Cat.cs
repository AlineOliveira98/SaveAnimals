using UnityEngine;

public class Cat : Animal
{
    public override void Collect()
    {
        if (IsDead || IsSaved || LockedInteraction) return;

        IsSaved = true;

        GameController.Instance.SaveAnimal(this);
        OnAnimalSaved?.Invoke(AnimalType);
    }
}