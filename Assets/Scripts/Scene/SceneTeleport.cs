using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneTeleport : MonoBehaviour
{
    [SerializeField] private SceneName sceneNameGoto = SceneName.Scene1_Farm;
    [SerializeField] private Vector3 scenePositionGoto;
    

    private void OnTriggerStay2D(Collider2D col)
    {
        Player player = col.GetComponent<Player>();

        if (player != null)
        {
            //如果设置的前往的x轴约为0，保留为玩家的x，否则使用设置的x
            float xPosition = Mathf.Approximately(scenePositionGoto.x, 999f)
                ? player.transform.position.x
                : scenePositionGoto.x;
            
            //如果设置的前往的y轴约为0，保留为玩家的y，否则使用设置的y
            float yPosition = Mathf.Approximately(scenePositionGoto.y, 999f)
                ? player.transform.position.y
                : scenePositionGoto.y;


            SceneControllerManager.Instance.FadeAndLoadScene(sceneNameGoto.ToString(),
                new Vector3(xPosition, yPosition, 0f));
        }
    }
}
