using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : SingletonMonobehaviour<Player>, ISaveable
{
    
    private WaitForSeconds useToolAnimationPause;
    private WaitForSeconds afterUseToolAnimationPause;
    
    private WaitForSeconds liftToolAnimationPause;
    private WaitForSeconds afterLiftToolAnimationPause;
    
    private WaitForSeconds pickAnimationPause;
    private WaitForSeconds afterPickAnimationPause;
    
    private bool playerToolUseDisabled = false;
    
    private AnimationOverrides animationOverrides;
    
    private GridCursor gridCursor;
    private Cursor cursor;
    
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
    private Direction playerDirection;
    
    private float _movementSpeed;

    //属性
    private bool playerInputIsDisabled = false;
    public bool PlayerInputIsDisabled
    {
        get => playerInputIsDisabled;
        set => playerInputIsDisabled = value;
    }
    
    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }


    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }


    protected override void Awake()
    {
        base.Awake();

        _rigidbody2D = GetComponent<Rigidbody2D>();

        mainCamera = Camera.main;

        animationOverrides = GetComponentInChildren<AnimationOverrides>();

        toolCharacterAttribute =
            new CharacterAttribute(CharacterPartAnimator.tool, PartVariantColour.none, PartVariantType.hoe);
        
        armsCharacterAttribute =
            new CharacterAttribute(CharacterPartAnimator.arms, PartVariantColour.none, PartVariantType.none);
        
        characterAttributeCustomisationList = new List<CharacterAttribute>();
        
        
        /*游戏保存相关*/
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();

        cursor = FindObjectOfType<Cursor>();
        
        //从settings中赋值
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
        pickAnimationPause = new WaitForSeconds(Settings.pickAnimationPause);
        afterPickAnimationPause = new WaitForSeconds(Settings.afterPickAnimationPause);
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        
        EventHandler.BeforeSceneUnloadFadeOutEvent -= DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent -= EnablePlayerInput;
    }


    private void OnEnable()
    {
        ISaveableRegister();

        EventHandler.BeforeSceneUnloadFadeOutEvent += DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;
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


    #region 接口实现

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave ISaveableSave()
    {
        // Delete saveScene for game object if it already exists
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // Create saveScene for game object
        SceneSave sceneSave = new SceneSave();
        // Create Vector3 Dictionary
        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();
        //  Create String Dictionary
        sceneSave.stringDictionary = new Dictionary<string, string>();

        
        // Add Player position to Vector3 dictionary
        Vector3Serializable vector3Serializable =
            new Vector3Serializable(transform.position.x, transform.position.y, transform.position.z);
        sceneSave.vector3Dictionary.Add("playerPosition", vector3Serializable);
        
        // Add Current Scene Name to string dictionary
        sceneSave.stringDictionary.Add("currentScene", SceneManager.GetActiveScene().name);
        
        // Add Player Direction to string dictionary
        sceneSave.stringDictionary.Add("playerDirection", playerDirection.ToString());

        // 把玩家相关的数据都存在PersistentScene里
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }


    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            // Get save data dictionary for scene
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                // Get player position
                if (sceneSave.vector3Dictionary != null &&
                    sceneSave.vector3Dictionary.TryGetValue("playerPosition", out Vector3Serializable playerPosition))
                {
                    transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
                }

                // Get String dictionary
                if (sceneSave.stringDictionary != null)
                {
                    // Get player scene
                    if (sceneSave.stringDictionary.TryGetValue("currentScene", out string currentScene))
                    {
                        SceneControllerManager.Instance.FadeAndLoadScene(currentScene, transform.position);
                    }

                    // Get player direction
                    if (sceneSave.stringDictionary.TryGetValue("playerDirection", out string playerDir))
                    {
                        bool playerDirFound = Enum.TryParse<Direction>(playerDir, true, out Direction direction);

                        if (playerDirFound)
                        {
                            playerDirection = direction;
                            SetPlayerDirection(playerDirection);
                        }
                    }
                }
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // Nothing required here since the player is on a persistent scene;
    }


    public void ISaveableRestoreScene(string sceneName)
    {
        // Nothing required here since the player is on a persistent scene;
    }


    private void SetPlayerDirection(Direction playerDirection)
    {
        switch (playerDirection)
        {
            case Direction.up:
                // set idle up trigger
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false);

                break;

            case Direction.down:
                // set idle down trigger
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false);
                break;

            case Direction.left:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false);
                break;

            case Direction.right:
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true);
                break;

            default:
                // set idle down trigger
                EventHandler.CallMovementEvent(0f, 0f, false, false, false, false, ToolEffect.none, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false);

                break;
        }
    }
    

    #endregion

    
    
    
    #region 点击放置、使用物体的函数
    private void PlayerClickInput()
    {
        if (!playerToolUseDisabled)
        {
            if (Input.GetMouseButton(0))
            {
                if (gridCursor.CursorIsEnabled || cursor.CursorIsEnabled)
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
                        ProcessPlayerClickInputSeed(gridPropertyDetails, itemDetails);
                    }
                    break;
                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }
                    break;
                case ItemType.Breaking_tool:
                case ItemType.Watering_tool:
                case ItemType.Reaping_tool:
                case ItemType.Hoeing_tool:
                case ItemType.Chopping_tool:
                case ItemType.Collecting_tool:
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

    #region (private)处理不同类型物品放置、使用效果的方法

    #region 种子的放置和使用

    private void ProcessPlayerClickInputSeed(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        //判断是否可以种地的
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid && gridPropertyDetails.daysSinceDug > -1 &&
            gridPropertyDetails.seedItemCode == -1)
        {
            PlantSeedAtCursor(gridPropertyDetails, itemDetails);
        }
        else if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    private void PlantSeedAtCursor(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        if (GridPropertiesManager.Instance.GetCropDetails(itemDetails.itemCode) != null)
        {
            //更新gridPropertyDetails
            gridPropertyDetails.seedItemCode = itemDetails.itemCode;
            gridPropertyDetails.growthDays = 0;

            //显示作物
            GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

            //从物品栏移除
            EventHandler.CallRemoveSelectedItemFromInventoryEvent();
            
            AudioManager.Instance.PlaySound(SoundName.effectPlantingSound);
        }
    }
    #endregion


    #region 货物的放置
    private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }
    #endregion

    
    #region 工具的使用
    
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
            case ItemType.Reaping_tool:
                if (cursor.CursorPositionIsValid)
                {
                    playerDirection = GetPlayerDirection(cursor.GetWorldPositionForCursor(), GetPlayerCentrePosition());
                    ReapInPlayerDirectionAtCursor(itemDetails, playerDirection);
                }
                break;
            case ItemType.Collecting_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    CollectInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;
            case ItemType.Chopping_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    ChopInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;
            case ItemType.Breaking_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    BreakInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;
            default:
                break;
        }
    }

    


    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        AudioManager.Instance.PlaySound(SoundName.effectHoe);
        
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
        AudioManager.Instance.PlaySound(SoundName.effectWateringCan);
        
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
         因此，下面这个方法或许没有执行的必要
         */
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY,
            gridPropertyDetails);

        GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);
        
        yield return afterLiftToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }
    
    private void ReapInPlayerDirectionAtCursor(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(ReapInPlayerDirectionAtCursorRoutine(itemDetails, playerDirection));
    }

    private IEnumerator ReapInPlayerDirectionAtCursorRoutine(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Set tool animation to scythe in override animation
        toolCharacterAttribute.partVariantType = PartVariantType.scythe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        // Reap in player direction
        UseToolInPlayerDirection(itemDetails, playerDirection);

        yield return useToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }
    
    private void UseToolInPlayerDirection(ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        if (Input.GetMouseButton(0))
        {
            switch (equippedItemDetails.itemType)
            {
                case ItemType.Reaping_tool:
                    if (playerDirection == Vector3Int.right)
                    {
                        isSwingingToolRight = true;
                    }
                    else if (playerDirection == Vector3Int.left)
                    {
                        isSwingingToolLeft = true;
                    }
                    else if (playerDirection == Vector3Int.up)
                    {
                        isSwingingToolUp = true;
                    }
                    else if (playerDirection == Vector3Int.down)
                    {
                        isSwingingToolDown = true;
                    }
                    break;
            }

            // 定义用于碰撞检测的正方形的中心点
            Vector2 point =
                new Vector2(
                    GetPlayerCentrePosition().x + (playerDirection.x * (equippedItemDetails.itemUseRadius / 2f)),
                    GetPlayerCentrePosition().y + playerDirection.y * (equippedItemDetails.itemUseRadius / 2f));

            // 定义用于碰撞检测的正方形的大小
            Vector2 size = new Vector2(equippedItemDetails.itemUseRadius, equippedItemDetails.itemUseRadius);

            //获取位于给定正方形中的，具有2D碰撞体的Item组件
            Item[] itemArray =
                HelperMethods.GetComponentsAtBoxLocationNonAlloc<Item>(Settings.maxCollidersToTestPerReapSwing, point,
                    size, 0f);

            int reapableItemCount = 0;

            // 遍历接受到的物体
            for (int i = itemArray.Length - 1; i >= 0; i--)
            {
                if (itemArray[i] != null)
                {
                    // 如果可收获则摧毁物体
                    if (InventoryManager.Instance.GetItemDetails(itemArray[i].ItemCode).itemType == ItemType.Reapable_scenery)
                    {
                        // 粒子位置
                        Vector3 effectPosition = new Vector3(itemArray[i].transform.position.x,
                            itemArray[i].transform.position.y + Settings.gridCellSize / 2f,
                            itemArray[i].transform.position.z);

                        //调用显示收获后的粒子系统的事件
                        EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.reaping);
                        
                        AudioManager.Instance.PlaySound(SoundName.effectScythe);
                        
                        Destroy(itemArray[i].gameObject);

                        reapableItemCount++;
                        if (reapableItemCount >= Settings.maxTargetComponentsToDestroyPerReapSwing)
                            break;
                    }
                }
            }
        }
    }

    private void CollectInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails,
        Vector3Int playerDirection)
    {
        AudioManager.Instance.PlaySound(SoundName.effectBasket);

        StartCoroutine(CollectInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }


    private IEnumerator CollectInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails,
        ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return pickAnimationPause;

        
        yield return afterPickAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void ProcessCropWithEquippedItemInPlayerDirection(Vector3Int playerDirection,
        ItemDetails equippedItemDetails, GridPropertyDetails gridPropertyDetails)
    {
        //看玩家拿着的物体是不是采集工具，是的话更改动画
        switch (equippedItemDetails.itemType)
        {
            case ItemType.Breaking_tool:
            case ItemType.Chopping_tool:

                if (playerDirection == Vector3Int.right)
                {
                    isUsingToolRight = true;
                }
                else if (playerDirection == Vector3Int.left)
                {
                    isUsingToolLeft = true;
                }
                else if (playerDirection == Vector3Int.up)
                {
                    isUsingToolUp = true;
                }
                else if (playerDirection == Vector3Int.down)
                {
                    isUsingToolDown = true;
                }
                break;


            case ItemType.Collecting_tool:

                if (playerDirection == Vector3Int.right)
                {
                    isPickingRight = true;
                }
                else if (playerDirection == Vector3Int.left)
                {
                    isPickingLeft = true;
                }
                else if (playerDirection == Vector3Int.up)
                {
                    isPickingUp = true;
                }
                else if (playerDirection == Vector3Int.down)
                {
                    isPickingDown = true;
                }
                break;

            case ItemType.none:
                break;
        }
        
        //获取这个网格中的作物
        Crop crop = GridPropertiesManager.Instance.GetCropObjectAtGridLocation(gridPropertyDetails);

        //如果crop脚本不空，且拿着的是采集工具，执行作物的corp脚本处理采集的方法
        //（包括累计采集次数判断是否采集完成，决定生成果实的数量和位置，重置网格属性，销毁作物，生成果实）
        if (crop != null)
        {
            switch (equippedItemDetails.itemType)
            {
                case ItemType.Breaking_tool:
                case ItemType.Chopping_tool:
                    crop.ProcessToolAction(equippedItemDetails, isUsingToolRight, isUsingToolLeft, isUsingToolDown,
                        isUsingToolUp);
                    break;

                case ItemType.Collecting_tool:
                    crop.ProcessToolAction(equippedItemDetails, isPickingRight, isPickingLeft, isPickingDown,
                        isPickingUp);
                    break;
            }
        }
    }

    private void ChopInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails,
        Vector3Int playerDirection)
    {
        AudioManager.Instance.PlaySound(SoundName.effectAxe);
        
        //触发动画
        StartCoroutine(ChopInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator ChopInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails,
        ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // 设置动画覆盖控制器
        toolCharacterAttribute.partVariantType = PartVariantType.axe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        // 使用动画以后
        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void BreakInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails,
        Vector3Int playerDirection)
    {
        AudioManager.Instance.PlaySound(SoundName.effectPickaxe);

        StartCoroutine(BreakInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator BreakInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails,
        ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;


        toolCharacterAttribute.partVariantType = PartVariantType.pickaxe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;
        
        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }


    #endregion
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
                playerDirection = Direction.left;
            }
            else if (xInput > 0)
            {
                playerDirection = Direction.right;
            }
            else if (yInput < 0)
            {
                playerDirection = Direction.down;
            }
            else
            {
                playerDirection = Direction.right;
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
    #region 禁用玩家移动的相关函数

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

    private void DisablePlayerInput()
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

    private Vector3Int GetPlayerDirection(Vector3 cursorPosition, Vector3 playerPosition)
    {
        if (cursorPosition.x > playerPosition.x && cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
                                                && cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f))
        {
            return Vector3Int.right;
        }
        else if (cursorPosition.x < playerPosition.x && cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
                                                     && cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)
                )
        {
            return Vector3Int.left;
        }
        else if (cursorPosition.y > playerPosition.y)
        {
            return Vector3Int.up;
        }
        else
        {
            return Vector3Int.down;
        }
    }
    
    //返回玩家的中心位置，因为玩家的轴心在脚底
    public Vector3 GetPlayerCentrePosition()
    {
        Vector3 tempP = transform.position;
        return new Vector3(tempP.x, tempP.y + Settings.playerCentreYOffset, tempP.z);
    }
}
