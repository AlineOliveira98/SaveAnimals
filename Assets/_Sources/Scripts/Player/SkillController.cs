using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class SkillController : MonoBehaviour
{
    public static SkillController Instance;

    [SerializeField] private AudioClip collectedAudio;
    [SerializeField] private SkillHandler[] skills;

    public SerializedDictionary<SkillType, SkillHandler> skillsDic = new();

    public static Action<SkillType> OnSkillAcquired;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        skillsDic.Clear();

        for (int i = 0; i < skills.Length; i++)
        {
            skillsDic.Add(skills[i].skillType, skills[i]);
        }
    }

    void Start()
    {
        ApplySkill(SkillType.Dash);
    }

    public void CollectSkill(SkillType skillType)
    {
        var skill = skillsDic[skillType];
        skill.currentAmount++;

        if (skill.currentAmount >= skill.amountToAcquire)
        {
            ApplySkill(skillType);
        }

        AudioController.PlaySFX(collectedAudio);

        if (skillType == SkillType.Clone)
        {
            InventoryUI.Instance.GetBlueFlower(skillsDic[skillType].currentAmount);
        }
        else if (skillType == SkillType.Push)
        {
            InventoryUI.Instance.GetPinkFlower();
        }
    }

    private SkillHandler GetSkillHandler(SkillType skillType)
    {
        return skillsDic[skillType];
    }

    public bool HasSkill(SkillType skillType)
    {
        return GetSkillHandler(skillType).acquired;
    }

    public void ApplySkill(SkillType skillType)
    {
        var skill = GetSkillHandler(skillType);
        skill.acquired = true;
        OnSkillAcquired?.Invoke(skillType);
    }
}

[Serializable]
public class SkillHandler
{
    public SkillType skillType;
    public int amountToAcquire;
    public int currentAmount;
    public bool acquired;
}

public enum SkillType
{
    Dash,
    Push,
    Invisible,
    Clone
}