using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    /// <summary>
    /// 获取盒子上具有中心点、大小和角度的T型分量。如果至少找到了一个组件并且在列表中返回了找到的组件，则返回true
    /// </summary>
    public static bool GetComponentsAtBoxLocation<T>(out List<T> listComponentsAtBoxPosition, Vector2 point,
        Vector2 size, float angle)
    {
        bool found = false;

        List<T> componentList = new List<T>();

        Collider2D[] collider2DArray = Physics2D.OverlapBoxAll(point, size, angle);

        for (int i = 0; i < collider2DArray.Length; i++)
        {
            T tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();

            if (tComponent != null)
            {
                found = true;
                componentList.Add(tComponent);
            }
            else
            {
                tComponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tComponent != null)
                {
                    found = true;
                    componentList.Add(tComponent);
                }
            }
        }

        listComponentsAtBoxPosition = componentList;

        return found;
    }
    
    /// <summary>
    /// 在positionToCheck获取类型T的组件。如果至少找到一个组件，并且找到的组件在componentAtPositionList中返回，则返回true
    /// </summary>
    public static bool GetComponentsAtCursorLocation<T>(out List<T> componentsAtPositionList, Vector3 positionToCheck)
    {
        bool found = false;

        List<T> componentList = new List<T>();

        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(positionToCheck);

        // Loop through all colliders to get an object of type T

        T tComponent = default(T);

        for (int i = 0; i < collider2DArray.Length; i++)
        {
            tComponent = collider2DArray[i].gameObject.GetComponentInParent<T>();
            if (tComponent != null)
            {
                found = true;
                componentList.Add(tComponent);
            }
            else
            {
                tComponent = collider2DArray[i].gameObject.GetComponentInChildren<T>();
                if (tComponent != null)
                {
                    found = true;
                    componentList.Add(tComponent);
                }
            }
        }

        componentsAtPositionList = componentList;

        return found;
    }
    
    /// <summary>
    /// Returns array of components of type T at box with centre point and size and angle.
    /// The numberOfCollidersToTest for is passed as a parameter. Found components are returned in the array.
    /// 返回类型为T的组件的数组，位于具有中心点、大小和角度的盒子上。numberOfCollidersToTest作为参数传入。找到的组件将在数组中返回。
    /// </summary>
    public static T[] GetComponentsAtBoxLocationNonAlloc<T>(int numberOfCollidersToTest, Vector2 point, Vector2 size, float angle)
    {
        Collider2D[] collider2DArray = new Collider2D[numberOfCollidersToTest];

        Physics2D.OverlapBoxNonAlloc(point, size, angle, collider2DArray);

        T tComponent = default(T);

        T[] componentArray = new T[collider2DArray.Length];

        for (int i = collider2DArray.Length - 1; i >= 0; i--)
        {
            if (collider2DArray[i] != null)
            {
                tComponent = collider2DArray[i].gameObject.GetComponent<T>();

                if (tComponent != null)
                {
                    componentArray[i] = tComponent;
                }
            }
        }

        return componentArray;
    }
}
