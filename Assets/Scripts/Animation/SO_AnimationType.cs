using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///一个数据容器包含了一个替换后的动画Clip，这个动画本来的动作类型，这个动画使用的部位，替换后的颜色变化，替换后的动作类型
/// </summary>
[CreateAssetMenu(fileName = "so_AnimationType",menuName = "Scriptable Objects/Animation/Animation Type")]
public class SO_AnimationType : ScriptableObject
{
    //实际animation clip的引用
    public AnimationClip animationClip; 
    
    //animation名字的枚举，如AnimationName.IdleUp
    public AnimationName animationName; 
    
    //Animator组件附在的gameobject的名字，如arms
    public CharacterPartAnimator characterPart; 
    
    //允许指定动画类型的颜色变化。如"none"， "bronze "， "silver"， "gold"
    public PartVariantColour partVariantColour; 
    
    //变量类型，指定这个动画类型所指的变量，如"none"，"carry"，"hoe"，"pickaxe"，"axe"等
    public PartVariantType partVariantType; 
}
