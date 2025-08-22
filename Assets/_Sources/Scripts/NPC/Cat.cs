using UnityEngine;

public class Cat : Animal
{
    public override void Collect()
    {
        if (IsDead || IsSaved || LockedInteraction) return;

        IsSaved = true;
        // gameObject.SetActive(false);

        AnimalsUI.Instance.SetSaved(animalType);
        GameController.Instance.SaveAnimal(this);
    }
}