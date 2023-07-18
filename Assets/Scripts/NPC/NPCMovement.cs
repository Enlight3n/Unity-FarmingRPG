using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NPCPath))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class NPCMovement : MonoBehaviour
{
    [HideInInspector] public SceneName npcCurrentScene;
    [HideInInspector] public SceneName npcTargetScene;
    [HideInInspector] public Vector3Int npcCurrentGridPosition;
    [HideInInspector] public Vector3Int npcTargetGridPosition;
    [HideInInspector] public Vector3 npcTargetWorldPosition;
    [HideInInspector] public Direction npcFacingDirectionAtDestination;

    private SceneName npcPreviousMovementStepScene;
    private Vector3Int npcNextGridPosition;
    private Vector3 npcNextWorldPosition;

    [Header("NPC Movement")]
    public float npcNormalSpeed = 2f;

    [SerializeField] private float npcMinSpeed = 1f;
    [SerializeField] private float npcMaxSpeed = 3f;
    private bool npcIsMoving = false;

    [HideInInspector] public AnimationClip npcTargetAnimationClip;

    [Header("NPC Animation")]
    [SerializeField] private AnimationClip blankAnimation = null;

    private Grid grid;
    private Rigidbody2D rigidBody2D;
    private BoxCollider2D boxCollider2D;
    private WaitForFixedUpdate waitForFixedUpdate;
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController;
    private int lastMoveAnimationParameter;
    private NPCPath npcPath;
    private bool npcInitialised = false;
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public bool npcActiveInScene = false;

    private bool sceneLoaded = false;

    private Coroutine moveToGridPositionRoutine;


    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        npcPath = GetComponent<NPCPath>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        //初始化 target world position, target grid position & target scene to current
        npcTargetScene = npcCurrentScene;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = transform.position;
    }


    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();

        SetIdleAnimation();
    }

    private void FixedUpdate()
    {
        if (sceneLoaded)
        {
            if (!npcIsMoving)
            {
                // set npc current and next grid position - to take into account the npc might be animating
                npcCurrentGridPosition = GetGridPosition(transform.position);
                npcNextGridPosition = npcCurrentGridPosition;

                if (npcPath.npcMovementStepStack.Count > 0)
                {
                    NPCMovementStep npcMovementStep = npcPath.npcMovementStepStack.Peek();

                    npcCurrentScene = npcMovementStep.sceneName;

                    // If NPC is in current scene then set NPC to active to make visible, pop the movement step off the stack and then call method to move NPC
                    if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
                    {
                        SetNPCActiveInScene();

                        npcMovementStep = npcPath.npcMovementStepStack.Pop();

                        npcNextGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;

                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);

                        MoveToGridPosition(npcNextGridPosition, npcMovementStepTime, TimeManager.Instance.GetGameTime());
                    }
                }
                // else if no more NPC movement steps
                else
                {
                    ResetMoveAnimation();

                    SetNPCFacingDirection();

                    SetNPCEventAnimation();
                }
            }
        }
    }
    
    private void InitialiseNPC()
    {
        // 根据npc是否在场景中决定是否活跃
        if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
        {
            SetNPCActiveInScene();
        }
        else
        {
            SetNPCInactiveInScene();
        }

        //获取npc当前所在的网格坐标
        npcCurrentGridPosition = GetGridPosition(transform.position);

        //设置坐标
        // Set Next Grid Position and Target Grid Position to current Grid Position
        npcNextGridPosition = npcCurrentGridPosition;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);

        //获取npc当前所在的世界坐标
        npcNextWorldPosition = GetWorldPosition(npcCurrentGridPosition);
    }
    
    //外部调用，设置npcScheduleEvent的属性到自己类中
    public void SetScheduleEventDetails(NPCScheduleEvent npcScheduleEvent)
    {
        npcTargetScene = npcScheduleEvent.toSceneName;
        npcTargetGridPosition = (Vector3Int)npcScheduleEvent.toGridCoordinate;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);
        npcFacingDirectionAtDestination = npcScheduleEvent.npcFacingDirectionAtDestination;
        npcTargetAnimationClip = npcScheduleEvent.animationAtDestination;
        ClearNPCEventAnimation();
    }

    #region 场景加载后，初始化npc
    
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloaded;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloaded;
    }
    
    private void AfterSceneLoad()
    {
        grid = GameObject.FindObjectOfType<Grid>();

        if (!npcInitialised)
        {
            InitialiseNPC();
            npcInitialised = true;
        }

        sceneLoaded = true;
    }

    private void BeforeSceneUnloaded()
    {
        sceneLoaded = false;
    }

    #endregion
    
    

    #region 设置npc事件动画

    

    

    private void SetNPCEventAnimation()
    {
        if (npcTargetAnimationClip != null)
        {
            ResetIdleAnimation();
            animatorOverrideController[blankAnimation] = npcTargetAnimationClip;
            animator.SetBool(Settings.eventAnimation, true);
        }
        else
        {
            animatorOverrideController[blankAnimation] = blankAnimation;
            animator.SetBool(Settings.eventAnimation, false);
        }
    }

    public void ClearNPCEventAnimation()
    {
        animatorOverrideController[blankAnimation] = blankAnimation;
        animator.SetBool(Settings.eventAnimation, false);

        // Clear any rotation on npc
        transform.rotation = Quaternion.identity;
    }

    private void SetNPCFacingDirection()
    {
        ResetIdleAnimation();

        switch (npcFacingDirectionAtDestination)
        {
            case Direction.up:
                animator.SetBool(Settings.idleUp, true);
                break;

            case Direction.down:
                animator.SetBool(Settings.idleDown, true);
                break;

            case Direction.left:
                animator.SetBool(Settings.idleLeft, true);
                break;

            case Direction.right:
                animator.SetBool(Settings.idleRight, true);
                break;

            case Direction.none:
                break;

            default:
                break;
        }
    }
    
    #endregion
    

    #region 设置npc是否在场景中活跃

    

    

    public void SetNPCActiveInScene()
    {
        spriteRenderer.enabled = true;
        boxCollider2D.enabled = true;
        npcActiveInScene = true;
    }

    public void SetNPCInactiveInScene()
    {
        spriteRenderer.enabled = false;
        boxCollider2D.enabled = false;
        npcActiveInScene = false;
    }

    #endregion

    #region 获取网格坐标和世界坐标
    
    /// <summary>
    /// returns the grid position given the worldPosition
    /// </summary>
    private Vector3Int GetGridPosition(Vector3 worldPosition)
    {
        if (grid != null)
        {
            return grid.WorldToCell(worldPosition);
        }
        else
        {
            return Vector3Int.zero;
        }
    }

    /// <summary>
    ///  returns the world position (centre of grid square) from gridPosition
    /// </summary>
    public Vector3 GetWorldPosition(Vector3Int gridPosition)
    {
        Vector3 worldPosition = grid.CellToWorld(gridPosition);

        // Get centre of grid square
        return new Vector3(worldPosition.x + Settings.gridCellSize / 2f, worldPosition.y + Settings.gridCellSize / 2f,
            worldPosition.z);
    }

    #endregion
    
    
    

    #region npc移动到给定的网格坐标

    

    
    private void MoveToGridPosition(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime)
    {
        moveToGridPositionRoutine = StartCoroutine(MoveToGridPositionRoutine(gridPosition, npcMovementStepTime, gameTime));
    }

    private IEnumerator MoveToGridPositionRoutine(Vector3Int gridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime)
    {
        Debug.Log(npcMovementStepTime);
        
        npcIsMoving = true;

        SetMoveAnimation(gridPosition);

        npcNextWorldPosition = GetWorldPosition(gridPosition);

        // If movement step time is in the future, otherwise skip and move NPC immediately to position
        if (npcMovementStepTime > gameTime)
        {
            //calculate time difference in seconds
            float timeToMove = (float)(npcMovementStepTime.TotalSeconds - gameTime.TotalSeconds);

            // Calculate speed
            float npcCalculatedSpeed = Vector3.Distance(transform.position, npcNextWorldPosition) / timeToMove / Settings.secondsPerGameSecond;

            //// If speed is at least npc min speed and less than npc max speed  then process, otherwise skip and move NPC immediately to position
            if (npcCalculatedSpeed >= npcMinSpeed && npcCalculatedSpeed <= npcMaxSpeed)
            {
                while (Vector3.Distance(transform.position, npcNextWorldPosition) > Settings.pixelSize)
                {
                    Vector3 unitVector = Vector3.Normalize(npcNextWorldPosition - transform.position);
                    Vector2 move = new Vector2(unitVector.x * npcCalculatedSpeed * Time.fixedDeltaTime, unitVector.y * npcCalculatedSpeed * Time.fixedDeltaTime);

                    rigidBody2D.MovePosition(rigidBody2D.position + move);

                    yield return waitForFixedUpdate;
                }
            }
        }

        rigidBody2D.position = npcNextWorldPosition;
        npcCurrentGridPosition = gridPosition;
        npcNextGridPosition = npcCurrentGridPosition;
        npcIsMoving = false;
    }
    
    #endregion

    #region 设置npc相关动画
    
    private void SetMoveAnimation(Vector3Int gridPosition)
    {
        // Reset idle animation
        ResetIdleAnimation();

        // Reset move animation
        ResetMoveAnimation();

        // 获取npc下一格应该走的网格坐标
        Vector3 toWorldPosition = GetWorldPosition(gridPosition);

        // get vector
        Vector3 directionVector = toWorldPosition - transform.position;

        if (Mathf.Abs(directionVector.x) >= Mathf.Abs(directionVector.y))
        {
            // Use left/right animation
            if (directionVector.x > 0)
            {
                animator.SetBool(Settings.walkRight, true);
            }
            else
            {
                animator.SetBool(Settings.walkLeft, true);
            }
        }
        else
        {
            //Use up/down animation
            if (directionVector.y > 0)
            {
                animator.SetBool(Settings.walkUp, true);
            }
            else
            {
                animator.SetBool(Settings.walkDown, true);
            }
        }
    }

    private void SetIdleAnimation()
    {
        animator.SetBool(Settings.idleDown, true);
    }

    private void ResetMoveAnimation()
    {
        animator.SetBool(Settings.walkRight, false);
        animator.SetBool(Settings.walkLeft, false);
        animator.SetBool(Settings.walkUp, false);
        animator.SetBool(Settings.walkDown, false);
    }

    private void ResetIdleAnimation()
    {
        animator.SetBool(Settings.idleRight, false);
        animator.SetBool(Settings.idleLeft, false);
        animator.SetBool(Settings.idleUp, false);
        animator.SetBool(Settings.idleDown, false);
    }
    
    #endregion
}