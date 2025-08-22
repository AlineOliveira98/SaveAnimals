using System.Collections.Generic;
using UnityEngine;

public class SafeArea : MonoBehaviour
{
    [SerializeField] private Transform[] safePoints;

    private List<Transform> availablePoints = new();

    void Start()
    {
        availablePoints = new(safePoints);
    }

    void OnEnable()
    {
        GameController.OnAnimalSaved += SetAnimalPosition;
    }

    void OnDisable()
    {
        GameController.OnAnimalSaved -= SetAnimalPosition;
    }

    private void SetAnimalPosition(Animal animal)
    {
        if (animal as Cat) return;
        
        int randomIndex = Random.Range(0, availablePoints.Count);
        Transform pointTransform = availablePoints[randomIndex];

        animal.gameObject.SetActive(true);
        animal.transform.position = pointTransform.position;

        availablePoints.RemoveAt(randomIndex);
    }
}
