using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
   private void OnTriggerEnter2D(Collider2D collision)
   {
      Item item = collision.GetComponent<Item>();

      if (item != null)
      {
         ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(item.ItemCode);

         if (itemDetails.canBePickedUp == true)
         {
            InventoryManager.Instance.AddItem(InventoryLocation.player, item, collision.gameObject);
            
            AudioManager.Instance.PlaySound(SoundName.effectPickupSound);
         }
      }
   }
}
