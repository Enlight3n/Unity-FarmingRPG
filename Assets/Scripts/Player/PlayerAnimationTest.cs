using UnityEngine;

public class PlayerAnimationTest : MonoBehaviour
{
    public float inputX;
    public float inputY;
    public bool isWalking;
    public bool isRunning;
    public bool isCarrying;
    public bool isIdle;
    
    public ToolEffect toolEffect;
    
    public bool isUsingToolRight;
    public bool isUsingToolLeft;
    public bool isUsingToolUp;
    public bool isUsingToolDown;
    public bool isLiftingToolRight;
    public bool isLiftingToolLeft;
    public bool isLiftingToolUp;
    public bool isLiftingToolDown;
    
    public bool isSwingingToolRight;
    public bool isSwingingToolLeft;
    public bool isSwingingToolUp;
    public bool isSwingingToolDown;
    
    public bool isPickingRight;
    public bool isPickingLeft;
    public bool isPickingUp;
    public bool isPickingDown;
    
    public bool idleUp;
    public bool idleDown;
    public bool idleRight;
    public bool idleLeft;
    

    private void Update()
    {
        EventHandler.CallMovementEvent(inputX, inputY,
            isWalking, isRunning, isIdle, isCarrying,
            toolEffect,
            isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
            isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
            isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
            isSwingingToolRight, isSwingingToolLeft, 
            isSwingingToolUp, isSwingingToolDown,
            idleUp, idleDown,idleRight, idleLeft);
    }
}
