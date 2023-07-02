using System;
using Cinemachine;
using UnityEngine;

public class SwitchConfineBoundingShape : MonoBehaviour
{
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SwitchBoundingShape;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SwitchBoundingShape;
    }
    /// <summary>
    /// 切换Cinemachine使用的碰撞器以便定义屏幕边缘
    /// </summary>
    private void SwitchBoundingShape()
    {
        //获取polygon collider
        PolygonCollider2D polygonCollider2D = GameObject.FindGameObjectWithTag(
            Tags.BoundsConfiner).GetComponent<PolygonCollider2D>();
   
        //获取cinemachineconfiner组件
        CinemachineConfiner cinemachineConfiner = GetComponent<CinemachineConfiner>();

        //将polygon的值传给cinemachineconfiner
        cinemachineConfiner.m_BoundingShape2D = polygonCollider2D;
        
        //清除缓存
        cinemachineConfiner.InvalidatePathCache();
    }
}
