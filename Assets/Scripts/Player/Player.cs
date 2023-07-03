
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class Player : SingletonMonobehaviour<Player>
{
    private WaitForSeconds afterUseToolAnimationPause;
    private WaitForSeconds useToolAnimationPause;
    private WaitForSeconds liftToolAnimationPause;
    private WaitForSeconds afterLiftToolAnimationPause;
    private bool playerToolUseDisabled = false;
    
    private AnimationOverrides animationOverrides;
    
    private GridCursor gridCursor;
    
    private List<CharacterAttribute> characterAttributeCustomisationList;
    [SerializeField] private SpriteRenderer equippedItemSpriteRenderer = null;
    private CharacterAttribute armsCharacterAttribute;
    private CharacterAttribute toolCharacterAttribute;

    //移动参数
    private float xInput;
    private float yInput;
    private bool isWalking;
    private bool isRunning;
    private bool isCarrying = false;
    private bool isIdle;
    private ToolEffect toolEffect = ToolEffect.none;
    private bool isUsingToolRight;
    private bool isUsingToolLeft;
    private bool isUsingToolUp;
    private bool isUsingToolDown;
    private bool isLiftingToolRight;
    private bool isLiftingToolLeft;
    private bool isLiftingToolUp;
    private bool isLiftingToolDown;
    private bool isSwingingToolRight;
    private bool isSwingingToolLeft;
    private bool isSwingingToolUp;
    private bool isSwingingToolDown;
    private bool isPickingRight;
    private bool isPickingLeft;
    private bool isPickingUp;
    private bool isPickingDown;

    private Camera mainCamera;
    
    
    private Rigidbody2D _rigidbody2D;

    //用来实现保存玩家位置的功能
#pragma warning disable 414
    private Direction _playerDirection;
#pragma warning restore 414
    
    
    private float _movementSpeed;

    //属性
    private bool playerInputIsDisabled = false;
    public bool PlayerInputIsDisabled
    {
        get => playerInputIsDisabled;
        set => playerInputIsDisabled = value;
    }

    protected override void Awake()
    {
        base.Awake();

        _rigidbody2D = GetComponent<Rigidbody2D>();

        mainCamera = Camera.main;

        animationOverrides = GetComponentInChildren<AnimationOverrides>();

        armsCharacterAttribute =
            new CharacterAttribute(CharacterPartAnimator.arms, PartVariantColour.none, PartVariantType.none);
        characterAttributeCustomisationList = new List<CharacterAttribute>();
    }

    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
        
        //从settings中赋值
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
    }

    private void Update()
    {
        if (!PlayerInputIsDisabled)
        {
            ResetAnimationTriggers(); //重置动画中使用工具部分的参数

            PlayerMovementInput(); //移动输入，根据键盘输入的情况设定动画参数

            PlayerWalkInput(); //检查是否行走，若按住shift，则改变人物移动速度和动画参数

            PlayerClickInput();
            
            EventHandler.CallMovementEvent(xInput, yInput,
                isWalking, isRunning, isIdle, isCarrying,
                toolEffect,
                isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
                isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
                isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
                isSwingingToolRight, isSwingingToolLeft, 
                isSwingingToolUp, isSwingingToolDown,
                false, false,false, false);
        }

        PlayerTestInput(); //开发者测试功能
    }
    
    private void FixedUpdate()
    {
        PlayerMovement();
    }

    
    
    
    #region 点击放置物体的函数

    private void PlayerClickInput()
    {
        if (!playerToolUseDisabled)
        {
            if (Input.GetMouseButton(0))
            {
                if (gridCursor.CursorIsEnabled)
                {
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();

                    Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();

                    ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
                }
            }
        }
        
    }

    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        ResetMovement(); //玩家放置东西时会停一下
        
        //传入玩家的光标网格位置和玩家网格位置的相对关系，获得动作动画的释放方位
        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);

        //获取光标网格位置的详细信息
        GridPropertyDetails gridPropertyDetails =
            GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        //获取当前选择的物体的详细信息
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);
        if (itemDetails != null)
        {
            //看当前选择的物品的类型，对不同的类型实现不同的方法（考虑是否可被释放，光标是否有效）
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputSeed(itemDetails);
                    }
                    break;
                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }
                    break;
                case ItemType.Watering_tool:
                case ItemType.Hoeing_tool:
                    ProcessPlayerClickInputTool(gridPropertyDetails, itemDetails, playerDirection);
                    break;
                case ItemType.none:
                    break;
                case ItemType.count:
                    break;
                default:
                    break;
            }
        }
    }
    
    //传入玩家的光标网格位置和玩家网格位置的相对关系，获得动作动画的释放方位
    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        if (cursorGridPosition.x > playerGridPosition.x)
        {
            return Vector3Int.right;
        }
        else if(cursorGridPosition.x < playerGridPosition.x)
        {
            return Vector3Int.left;
        }
        else if(cursorGridPosition.y > playerGridPosition.y)
        {
            return Vector3Int.up;
        }
        else
        {
            return Vector3Int.down;
        }
    }
    #endregion

    #region 处理不同类型物品放置效果的方法
    
    private void ProcessPlayerClickInputSeed(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }
    
    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        switch (itemDetails.itemType)
        {
            case ItemType.Hoeing_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;
            case ItemType.Watering_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;
            default:
                break;
        }
    }

   

    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        toolCharacterAttribute.partVariantType = PartVariantType.hoe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        if (playerDirection == Vector3Int.right)
        {
            isUsingToolRight = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            isUsingToolLeft = true;
        }
        else if (playerDirection==Vector3Int.up)
        {
            isUsingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            isUsingToolDown = true;
        }

        yield return useToolAnimationPause;

        if (gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0;
        }

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY,
            gridPropertyDetails);
        
        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);

        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }
    
    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        // Trigger animation
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;
        
        toolCharacterAttribute.partVariantType = PartVariantType.wateringCan;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        // TODO: 这里要确保水壶里有水
        toolEffect = ToolEffect.watering;

        if (playerDirection == Vector3Int.right)
        {
            isLiftingToolRight = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            isLiftingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.up)
        {
            isLiftingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            isLiftingToolDown = true;
        }

        yield return liftToolAnimationPause;

        // 设置为已经浇过水
        if (gridPropertyDetails.daysSinceWatered == -1)
        {
            gridPropertyDetails.daysSinceWatered = 0;
        }

        /*
         突然发现，即使禁用下面的SetGridPropertyDetails方法，程序同样正常运行，
         本来以为会出现，场景切换后未能正常保存浇水网格的GridPropertyDetails的情况
         究其原因，应该是这个gridPropertyDetails是引用类型，在赋值的之后虽然经过反复传递，但还是会直接改变原来字典中的值
         因此，下面或许这个方法没有执行的必要
         */
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY,
            gridPropertyDetails);

        GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);
        
        yield return afterLiftToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }
    
    #endregion

    
    
    #region 处理玩家移动的相关函数

    private void PlayerMovement()
    {
        Vector2 move = new Vector2(xInput *  _movementSpeed * Time.deltaTime, 
            yInput *  _movementSpeed * Time.deltaTime);
        _rigidbody2D.MovePosition(_rigidbody2D.position + move);
    }

    private void ResetAnimationTriggers()
    {
        isUsingToolRight = false;
        isUsingToolLeft = false;
        isUsingToolUp = false;
        isUsingToolDown = false;
        isLiftingToolRight = false;
        isLiftingToolLeft = false;
        isLiftingToolUp = false;
        isLiftingToolDown = false;
        isSwingingToolRight = false;
        isSwingingToolLeft = false;
        isSwingingToolUp = false;
        isSwingingToolDown = false;
        isPickingRight = false;
        isPickingLeft = false;
        isPickingUp = false;
        isPickingDown = false;
        toolEffect = ToolEffect.none;
    }

    private void PlayerMovementInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        
        if (yInput != 0 && xInput != 0)
        {
            xInput = xInput * 0.7f;
            yInput = yInput * 0.7f;
        }

        if (yInput != 0 || xInput != 0)
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            _movementSpeed = Settings.runningSpeed;
            
            if (xInput < 0)
            {
                _playerDirection = Direction.left;
            }
            else if (xInput > 0)
            {
                _playerDirection = Direction.right;
            }
            else if (yInput < 0)
            {
                _playerDirection = Direction.down;
            }
            else
            {
                _playerDirection = Direction.right;
            }
        }
        else if(yInput == 0 && xInput == 0)
        {
            isRunning = false;
            isWalking = false;
            isIdle = true;
        }
    }

    private void PlayerWalkInput()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isRunning = false;
            isWalking = true;
            isIdle = false;
            _movementSpeed = Settings.walkingSpeed;
        }
        else
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            _movementSpeed = Settings.runningSpeed;
        }
    }

    #endregion 
 

    
    //要禁用玩家移动，只需要调用一次下面的DisablePlayerInputAndResetMovement函数即可
    //要重新启用，只需要设置PlayerInputIsDisabled为false即可
    #region 处理玩家拖拽物品栏时禁用移动的相关函数

    private void ResetMovement()
    {
        xInput = 0f;
        yInput = 0f;
        isRunning = false;
        isWalking = false;
        isIdle = true;
    }
    
    public void DisablePlayerInputAndResetMovement()
    {
        DisablePlayerInput();
        ResetMovement();
        
        EventHandler.CallMovementEvent(xInput, yInput,
            isWalking, isRunning, isIdle, isCarrying,
            toolEffect,
            isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
            isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
            isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
            isSwingingToolRight, isSwingingToolLeft, 
            isSwingingToolUp, isSwingingToolDown,
            false, false,false, false);
    }

    public void DisablePlayerInput()
    {
        PlayerInputIsDisabled = true;
    }

    public void EnablePlayerInput()
    {
        PlayerInputIsDisabled = false;
    }
    
    #endregion


    
    #region 处理动画覆盖控制器的两个函数
    //清除举起物体的动画
    public void ClearCarriedItem()
    {
        equippedItemSpriteRenderer.sprite = null;
        equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);
        armsCharacterAttribute.partVariantType = PartVariantType.none;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(armsCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);
        isCarrying = false;
    }
    //显示举起物体的动画
    public void ShowCarriedItem(int itemCode)
    {
        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);

        if (itemDetails != null)
        {
            equippedItemSpriteRenderer.sprite = itemDetails.itemSprite;
            equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);

            armsCharacterAttribute.partVariantType = PartVariantType.carry;
            characterAttributeCustomisationList.Clear();
            characterAttributeCustomisationList.Add(armsCharacterAttribute);
            animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

            isCarrying = true;
        }
    }
    #endregion


    private void PlayerTestInput()
    {
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneControllerManager.Instance.FadeAndLoadScene(SceneName.Scene1_Farm.ToString(),transform.position);
        }
    }
    
    public Vector3 GetPlayerViewportPosition()
    {
        //Camera.main.WorldToScreenPoint()函数接收一个世界空间下的位置，返回其所在的屏幕空间位置，以及其相对于摄像机的深度信息
        return mainCamera.WorldToViewportPoint(transform.position);
    }

    //返回玩家的中心位置，因为玩家的轴心在脚底
    public Vector3 GetPlayerCentrePosition()
    {
        Vector3 tempP = transform.position;
        return new Vector3(tempP.x, tempP.y + Settings.playerCentreYOffset, tempP.z);
    }
}
