using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 所有的作物都附上此脚本组件
/// </summary>
public class Crop : MonoBehaviour
{
    private int harvestActionCount = 0; //标明已在这个作物上收割了几次，比如砍了三次树，但树要五次才能砍倒
    
    [HideInInspector] public Vector2Int cropGridPosition; //记录这个作物是在哪个网格上
    
    public void ProcessToolAction(ItemDetails equippedItemDetails)
    {
        GridPropertyDetails gridPropertyDetails =
            GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);

        if (gridPropertyDetails == null)
            return;

        // Get seed item details
        ItemDetails seedItemDetails = InventoryManager.Instance.GetItemDetails(gridPropertyDetails.seedItemCode);
        if (seedItemDetails == null)
            return;

        // Get crop details
        CropDetails cropDetails = GridPropertiesManager.Instance.GetCropDetails(seedItemDetails.itemCode);
        if (cropDetails == null)
            return;


        // Get required harvest actions for tool
        int requiredHarvestActions = cropDetails.RequiredHarvestActionsForTool(equippedItemDetails.itemCode);
        if (requiredHarvestActions == -1)
            return; // this tool can't be used to harvest this crop


        // Increment harvest action count
        harvestActionCount += 1;

        // Check if required harvest actions made
        if (harvestActionCount >= requiredHarvestActions)
            HarvestCrop(cropDetails, gridPropertyDetails);
    }

    private void HarvestCrop(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        // Delete crop from grid properties
        gridPropertyDetails.seedItemCode = -1;
        gridPropertyDetails.growthDays = -1;
        gridPropertyDetails.daysSinceLastHarvest = -1;
        gridPropertyDetails.daysSinceWatered = -1;

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        HarvestActions(cropDetails, gridPropertyDetails);
    }

    private void HarvestActions(CropDetails cropDetails, GridPropertyDetails gridPropertyDetails)
    {
        SpawnHarvestedItems(cropDetails);

        Destroy(gameObject);
    }

    private void SpawnHarvestedItems(CropDetails cropDetails)
    {
        // Spawn the item(s) to be produced
        for (int i = 0; i < cropDetails.cropProducedItemCode.Length; i++)
        {
            int cropsToProduce;

            // Calculate how many crops to produce
            if (cropDetails.cropProducedMinQuantity[i] == cropDetails.cropProducedMaxQuantity[i] ||
                cropDetails.cropProducedMaxQuantity[i] < cropDetails.cropProducedMinQuantity[i])
            {
                cropsToProduce = cropDetails.cropProducedMinQuantity[i];
            }
            else
            {
                cropsToProduce = Random.Range(cropDetails.cropProducedMinQuantity[i],
                    cropDetails.cropProducedMaxQuantity[i] + 1);
            }

            //选择生成位置，直接在玩家位置生成/随机位置生成，比如采摘就是在玩家位置，而伐木就是随机位置
            for (int j = 0; j < cropsToProduce; j++)
            {
                Vector3 spawnPosition;
                if (cropDetails.spawnCropProducedAtPlayerPosition)
                {
                    //  Add item to the players inventory
                    InventoryManager.Instance.AddItem(InventoryLocation.player, cropDetails.cropProducedItemCode[i]);
                }
                else
                {
                    // Random position
                    spawnPosition = new Vector3(transform.position.x + Random.Range(-1f, 1f),
                        transform.position.y + Random.Range(-1f, 1f), 0f);
                    SceneItemsManager.Instance.InstantiateSceneItem(cropDetails.cropProducedItemCode[i], spawnPosition);
                }
            }
        }
    }
}
