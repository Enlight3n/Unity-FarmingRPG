using UnityEngine;
//这个脚本添加给玩家
public class TriggerObsecuringItemFader : MonoBehaviour
{
   private void OnTriggerEnter2D(Collider2D collision)
   {
      ObsecuringItemFader[] obsecuringItemFader = 
         collision.gameObject.GetComponentsInChildren<ObsecuringItemFader>();
      
      if (obsecuringItemFader.Length > 0)
      {
         for (int i = 0; i < obsecuringItemFader.Length; i++)
         {
            obsecuringItemFader[i].FadeOut();
         }
      }
   }
   private void OnTriggerExit2D(Collider2D collision)
   {
      ObsecuringItemFader[] obsecuringItemFader = 
         collision.gameObject.GetComponentsInChildren<ObsecuringItemFader>();
      
      if (obsecuringItemFader.Length > 0)
      {
         for (int i = 0; i < obsecuringItemFader.Length; i++)
         {
            obsecuringItemFader[i].FadeIn();
         }
      }
   }
}
