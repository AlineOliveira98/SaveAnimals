using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [SerializeField] private int animalsAmoutSavedToVictory = 7;

    [Header("Audio")]
    [SerializeField] private AudioClip[] gameplayMusics;
    [SerializeField] private AudioClip animalSavedAudio;

    [Space(10)]
    [SerializeField] private SkillType skillTypeWhenSavedAnimals;
    [SerializeField] private DialogueSO DialogueLastAnimal;
    [SerializeField] private GameObject lastAnimal;
    [SerializeField] private EndingUI goodEndgame;
    [SerializeField] private BadEnding badEndgame;

    private int totalAnimals;

    public static bool GameStarted { get; private set; }
    public static bool GameIsOver { get; private set; }
    public static bool GameIsPaused { get; private set; }

    public int AnimalsCurrentNumber { get; private set; }
    public int AnimalsDied { get; private set; }
    public int AnimalsSaved { get; private set; }
    public Player Player { get; private set; }
    public bool HasAxe { get; private set; }
    public bool HasFeather { get; set; }
    public List<Animal> AllAnimals { get; private set; } = new();

    public static Action<Animal> OnAnimalSaved;
    public static Action<Animal> OnAnimalDied;
    public static Action OnDeadAnimalLimitReached;
    public static Action OnGameEnding;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Player = FindAnyObjectByType<Player>();
        AllAnimals = FindObjectsByType<Animal>(FindObjectsSortMode.None).ToList();
        totalAnimals = AllAnimals.Count();

        AnimalsDied = 0;
        AnimalsSaved = 0;
        AnimalsCurrentNumber = totalAnimals;

        GameStarted = false;
        GameIsOver = false;
        GameIsPaused = false;
    }

    void OnEnable()
    {
        DialogueLastAnimal.OnOptionSelected += DialogueAnswer;
    }

    void OnDisable()
    {
        DialogueLastAnimal.OnOptionSelected -= DialogueAnswer;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var animals = FindObjectsByType<Animal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var YggDrasil = FindAnyObjectByType<YggDrasil>();

            foreach (var item in animals)
            {
                item.Collect();
            }

            for (int i = 0; i < 7; i++)
            {
                YggDrasil.Watering();
            }

            HasAxe = true;
        }
    }

    private void DialogueAnswer(int option)
    {
        Debug.Log("option " + option);

        if (option == 0)
        {
            Debug.Log("First Ending");
            CameraController.Instance.SetCamera(CameraType.BadEndGame);
            BadEnding();
        }
        else if (option == 1)
        {
            Debug.Log("Second Ending");
            CameraController.Instance.SetCamera(CameraType.GoodEndGame);
            GoodEnding();
        }
    }

    void Start()
    {
        lastAnimal.SetActive(false);
    }

    public async void SaveAnimal(Animal animal)
    {
        AnimalsSaved++;
        AnimalsCurrentNumber--;
        OnAnimalSaved?.Invoke(animal);

        if (!SkillController.Instance.HasSkill(skillTypeWhenSavedAnimals))
        {
            SkillController.Instance.CollectSkill(skillTypeWhenSavedAnimals);
        }

        AudioController.PlaySFX(animalSavedAudio);

        if (AnimalsSaved >= totalAnimals)
        {
            GameOver();
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            DialogueService.StartDialogue(DialogueLastAnimal);
        }
    }

    public void EnableLastAnimal()
    {
        lastAnimal.SetActive(true);
    }

    public void KillAnimal(Animal animal)
    {
        AnimalsDied++;
        AnimalsCurrentNumber--;
        OnAnimalDied?.Invoke(animal);

        Debug.Log($"Animal Died: {animal.gameObject.name}");
    }

    public void StartGameplay()
    {
        GameStarted = true;
        var randomValue = UnityEngine.Random.Range(0, gameplayMusics.Length);
        AudioController.Instance.PlayMusic(gameplayMusics[randomValue]);
        CameraController.Instance.SetCamera(CameraType.Gameplay);
    }

    public void PauseGame(bool isPaused)
    {
        GameIsPaused = isPaused;
    }

    public void GameOver()
    {
        GameIsOver = true;
    }

    public void OpenChest()
    {
        HasAxe = true;
        InventoryUI.Instance.GetAxe();
        Debug.Log("Chest Opened");
    }

    public void GetFeather()
    {
        HasFeather = true;
        InventoryUI.Instance.GetFeather();
    }

    public void GoodEnding()
    {
        UIController.Instance.OpenPanel(PanelType.GoodEnding);
        Debug.Log("Good Ending");
        AudioController.Instance.StopMusic();
        goodEndgame.AnimEndGame();
    }

    public void BadEnding()
    {
        UIController.Instance.OpenPanel(PanelType.BadEnding);
        Debug.Log("Bad Ending");
        AudioController.Instance.StopMusic();
        badEndgame.AnimEndGame();
    }
}
