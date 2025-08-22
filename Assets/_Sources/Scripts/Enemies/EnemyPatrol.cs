using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float atkAnimationDuration = 0.2f;

    [Header("Patrol")]
    [SerializeField] private bool waitToStartPatrol;
    [SerializeField][Min(3)] private float rangePatrol = 5f;
    [SerializeField] private float rangeToEnterChasePlayer = 3f;
    [SerializeField] private float rangeToEnterChaseAnimals = 2f;
    [SerializeField] private float rangeToOutChase = 4f;
    [SerializeField] private float rangeAttack = 1f;
    [SerializeField] private float stoppedTime = 2f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float dashDuration = 0.03f;
    [SerializeField] private float dashCooldown = 0.5f;

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyVisual visual;
    [SerializeField] private Enemy enemy;
    [SerializeField] private AudioClip dashAudio;

    private Vector2 NavMeshTarget;
    private Player cachedPlayer;
    private Transform player;
    private Transform clone;
    private float checkCooldown = 0.3f;
    private float checkTimer;
    private float sqrRangeToEnterPlayer;
    private float sqrRangeToExit;

    public bool IsAttacking { get; set; }
    public bool IsKnockback { get; set; }
    public Transform TargetFind { get; private set; }
    public NavMeshAgent Agent => agent;
    public float RangeAttack => rangeAttack;

    private static List<Animal> allAnimals = new();
    private float waitTimer;
    

    public IDash Dash { get; private set; }
    private bool CanMove => GameController.GameStarted && !GameController.GameIsOver && !GameController.GameIsPaused;

    void Start()
    {
        if (allAnimals.Count <= 0) allAnimals = GameController.Instance.AllAnimals;
        
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        Dash = new BasicDash(enemy.EnemyMovement.Rig, dashSpeed, dashDuration, dashCooldown, agent);

        player = GameObject.FindWithTag("Player")?.transform;
        clone = GameObject.FindAnyObjectByType<PlayerClone>(FindObjectsInactive.Include)?.transform;

        sqrRangeToEnterPlayer = rangeToEnterChasePlayer * rangeToEnterChasePlayer;
        sqrRangeToExit = rangeToOutChase * rangeToOutChase;
    }

    void Update()
    {
        visual.SetDashing(Dash.IsDashing);
        if(agent != null) visual.SetRunning(agent.velocity != Vector3.zero);

        (Dash as BasicDash).LockDash = IsKnockback;

        if (!CanMove || Dash.IsDashing || IsAttacking || IsKnockback)
        {
            if (agent != null && agent.enabled) agent.SetDestination(transform.position);
            return;
        }

        if (TargetFind == player)
        {
            if (cachedPlayer == null)
                cachedPlayer = player.GetComponent<Player>();

            if (cachedPlayer.IsInvisible)
            {
                TargetFind = null;
                waitToStartPatrol = true;
                if(agent != null && agent.enabled) agent.SetDestination(transform.position);
                return;
            }
        }

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkCooldown)
        {
            checkTimer = 0f;
            UpdateTarget();
        }

        if (TargetFind != null)
        {
            if (Dash.CanDash && !IsKnockback && !IsAttacking)
            {
                var randomDash = Random.Range(0, 2);
                if (randomDash == 0)
                {
                    Vector2 dashDirection = (TargetFind.position - transform.position).normalized;
                    Dash.TryDash(dashDirection);
                    visual.TriggerDash();
                    AudioController.PlaySFX(dashAudio);
                }
                else
                {
                    Dash.CooldownDash();
                }
            }
        }

        if (TargetFind != null)
        {
            agent.SetDestination(TargetFind.position);
            visual.SetDirection(TargetFind.position);

            float distanceToTarget = Vector3.Distance(transform.position, TargetFind.position);
            if (distanceToTarget < rangeAttack && !IsAttacking)
            {
                AttackRoutine();
            }
        }
        else if (!waitToStartPatrol)
        {
            Patrol();
        }
    }

    private async UniTask AttackRoutine()
    {
        IsAttacking = true;
        agent.SetDestination(transform.position);
        visual.SetAttack();

        await Task.Delay((int)(atkAnimationDuration * 1000));

        if (TargetFind.TryGetComponent(out IDamageable damageable))
        {
            float distanceToTarget = Vector3.Distance(transform.position, TargetFind.position);
            if (distanceToTarget < rangeAttack)
            {
                damageable.TakeDamage(enemy.Damage);

                if (damageable.IsDead)
                {
                    IsAttacking = false;
                }
            }
        }

        IsAttacking = false;
    }

    private void Patrol()
    {
        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= stoppedTime)
            {
                UpdateRandomPoint();
                agent.SetDestination(NavMeshTarget);
                visual.SetDirection(NavMeshTarget);
                waitTimer = 0f;
            }
        }
    }

    private void UpdateTarget()
    {
        // Verifica Clone (prioridade máxima)
        if (clone != null && clone.gameObject.activeInHierarchy &&
            (clone.position - transform.position).sqrMagnitude <= sqrRangeToEnterPlayer)
        {
            if (TargetFind != clone)
            {
                TargetFind = clone;
                waitToStartPatrol = false;
            }
            return;
        }

        // Verifica Player
        if (player != null)
        {
            cachedPlayer = player.GetComponent<Player>();
            if (!cachedPlayer.IsInvisible &&
                (player.position - transform.position).sqrMagnitude <= sqrRangeToEnterPlayer)
            {
                if (TargetFind != player)
                {
                    TargetFind = player;
                    waitToStartPatrol = false;
                }
                return;
            }
        }

        // Verifica Animal
        Transform animalTarget = GetClosestAnimal();
        if (animalTarget != null)
        {
            if (TargetFind != animalTarget)
            {
                TargetFind = animalTarget;
                waitToStartPatrol = false;
            }
            return;
        }

        // Checa se deve sair da perseguição
        if (TargetFind != null && (TargetFind.position - transform.position).sqrMagnitude > sqrRangeToExit)
        {
            TargetFind = null;
            waitToStartPatrol = true;
        }
    }

    private Transform GetClosestAnimal()
    {
        float sqrRange = rangeToEnterChaseAnimals * rangeToEnterChaseAnimals;
        Transform closest = null;
        float closestDist = float.MaxValue;

        foreach (var npc in allAnimals)
        {
            if (npc == null) continue;

            float sqrDist = (npc.transform.position - transform.position).sqrMagnitude;
            if (sqrDist <= sqrRange && sqrDist < closestDist)
            {
                closest = npc.transform;
                closestDist = sqrDist;
            }
        }

        return closest;
    }

    private void UpdateRandomPoint()
    {
        bool isInsideNavMesh;

        while (true)
        {
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * rangePatrol;
            isInsideNavMesh = NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas);

            if (isInsideNavMesh)
            {
                NavMeshTarget = hit.position;
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangeAttack);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangeToEnterChasePlayer);
        Gizmos.DrawWireSphere(transform.position, rangeToEnterChaseAnimals);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangeToOutChase);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangePatrol);

        #if UNITY_EDITOR
        Vector3 screenPosDet = HandleUtility.WorldToGUIPoint(transform.position + Vector3.right * rangeToEnterChasePlayer);
        Vector3 screenPosAni = HandleUtility.WorldToGUIPoint(transform.position + Vector3.right * rangeToEnterChaseAnimals);
        Vector3 screenPosAtk = HandleUtility.WorldToGUIPoint(transform.position + Vector3.right * rangeToOutChase);
        Vector3 screenPosAlt = HandleUtility.WorldToGUIPoint(transform.position + Vector3.right * rangeAttack);
        Vector3 screenPosPatrol = HandleUtility.WorldToGUIPoint(transform.position + Vector3.right * rangePatrol);

        Handles.BeginGUI();

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = Color.black; 
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.padding = new RectOffset(4, 4, 2, 2);
        style.normal.background = Texture2D.whiteTexture; 

        GUI.backgroundColor = new Color(1f, 1f, 1f, 1f);

        GUI.Label(new Rect(screenPosAlt.x - 40, screenPosAlt.y - 10, 80, 20), "Attack", style);
        GUI.Label(new Rect(screenPosDet.x - 40, screenPosDet.y - 35, 80, 20), "ChasePlayer", style);
        GUI.Label(new Rect(screenPosAni.x - 40, screenPosAni.y - 55, 100, 20), "ChaseAnimals", style);
        GUI.Label(new Rect(screenPosAtk.x - 40, screenPosAtk.y - 75, 80, 20), "ToOutChase", style);
        GUI.Label(new Rect(screenPosPatrol.x - 40, screenPosPatrol.y - 95, 80, 20), "Patrol", style);

        Handles.EndGUI();
        #endif
    }
}