//这个脚本是用来存储一些枚举字段的，因而无需类，且均为public
public enum AnimationName
{
    idleDown,
    idleUp,
    idleRight,
    idleLeft,
    walkUp,
    walkDown,
    walkRight,
    walkLeft,
    runUp,runDown,runRight,runLeft,
    useToolUp, useToolDown,useToolRight,useToolLeft,
    swingToolUp,swingToolDown,swingToolRight,swingToolLeft,
    liftToolUp,liftToolDown,liftToolRight,liftToolLeft,
    holdToolUp,holdToolDown,holdToolRight,holdToolLeft,
    pickDown,pickUp,pickRight,pickLeft,
    count
}
public enum CharacterPartAnimator
{
    body,arms,hair,tool,hat,count
}

public enum PartVariantColour
{
    none,count
}

public enum PartVariantType
{
    none,carry,hoe,pickaxe,axe,scythe,wateringCan,count
}
public enum ToolEffect
{
    none,
    watering
}

public enum Direction
{
    up,
    down,
    left,
    right,
    none
}

public enum InventoryLocation
{
    player,
    chest,
    count
}
public enum ItemType
{
    Seed,
    Commodity,
    Watering_tool,
    Hoeing_tool,
    Chopping_tool,
    Breaking_tool,
    Reaping_tool,
    Collecting_tool,
    Reapable_scenery,
    Furniture,
    none,
    count,
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter,
    none,
    count
}

public enum SceneName
{
    Scene1_Farm,
    Scene2_Field,
    Scene3_Cabin
}

public enum GridBoolProperty
{
    diggable,
    canDropItem,
    canPlaceFurniture,
    isPath,
    isNPCObstacle
}

public enum HarvestActionEffect
{
    deciduousLeavesFalling,
    pineConeFalling,
    choppingTreeTrunk,
    breakingStone,
    reaping,
    none
}

public enum Facing
{
    none,
    front,
    back,
    right
}

public enum Weather
{
    dry,
    raining,
    snowing,
    none,
    count
}

public enum SoundName
{
    none = 0,
    effectFootstepSoftGround = 10,
    effectFootstepHardGround = 20,
    effectAxe = 30,
    effectPickaxe = 40,
    effectScythe = 50,
    effectHoe = 60,
    effectWateringCan = 70,
    effectBasket = 80,
    effectPickupSound = 90,
    effectRustle = 100,
    effectTreeFalling = 110,
    effectPlantingSound = 120,
    effectPluck = 130,
    effectStoneShatter = 140,
    effectWoodSplinters = 150,
    ambientCountryside1 = 1000,
    ambientCountryside2 = 1010,
    ambientIndoors1 = 1020,
    musicCalm3 = 2000,
    musicCalm1 = 2010
}