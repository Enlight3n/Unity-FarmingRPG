using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOverrides : MonoBehaviour
{
    //用来获取Player物体
    [SerializeField] private GameObject character = null; 
    
    //数据容器列表，用来存储赋值的数据容器
    [SerializeField] private SO_AnimationType[] soAnimationTypeArray = null;

    //将数据容器列表做成<SO_AnimationType.animationClip，SO_AnimationType>的字典
    private Dictionary<AnimationClip, SO_AnimationType> animationTypeDictionaryByAnimation;
    
    //将数据容器做成<key，SO_AnimationType>的字典
    private Dictionary<string, SO_AnimationType> animationTypeDictionaryByCompositeAttributeKey;

    private void Start()
    {
        //分配内存地址并将数据容器列表做成字典<SO_AnimationType.animationClip，SO_AnimationType>
        animationTypeDictionaryByAnimation = new Dictionary<AnimationClip, SO_AnimationType>();
        foreach (SO_AnimationType item in soAnimationTypeArray)
        {
            animationTypeDictionaryByAnimation.Add(item.animationClip, item);
        }
        
        //分配内存地址并将数据容器做成字典，<key，SO_AnimationType>
        animationTypeDictionaryByCompositeAttributeKey = new Dictionary<string, SO_AnimationType>();
        foreach (SO_AnimationType item in soAnimationTypeArray)
        {
            string key = item.characterPart.ToString() + item.partVariantColour.ToString() +
                         item.partVariantType.ToString() + item.animationName.ToString();

            animationTypeDictionaryByCompositeAttributeKey.Add(key, item); 
        }
    }

    
    //传入的参数为CharacterAttribute的列表
    public void ApplyCharacterCustomisationParameters(List<CharacterAttribute> characterAttributesList)
    {
        //遍历CharacterAttribute的列表，对于每一个CharacterAttribute执行同样的操作
        foreach (CharacterAttribute characterAttribute in characterAttributesList)
        {
            //键值对列表
            List<KeyValuePair<AnimationClip, AnimationClip>> animsKeyValuePairList =
                new List<KeyValuePair<AnimationClip, AnimationClip>>();

            
            /*寻找到和本次循环的CharacterAttribute的部位类型同名的Animator*/
            Animator currentAnimator = null;
            //获取该CharacterAttribute的部位类型
            string animatorSOAssetName = characterAttribute.characterPart.ToString();
            //获取Player下的子物体的全部Animator
            Animator[] animatorsArray = character.GetComponentsInChildren<Animator>();
            //遍历数组，寻找到和部位类型同名的Animator
            foreach (Animator animator in animatorsArray)
            {
                if (animator.name == animatorSOAssetName)
                {
                    currentAnimator = animator;
                    break;
                }
            }

            
            AnimatorOverrideController aoc = new AnimatorOverrideController(currentAnimator.runtimeAnimatorController);
            List<AnimationClip> animationsList = new List<AnimationClip>(aoc.animationClips);
            foreach (AnimationClip animationClip in animationsList)
            {
                SO_AnimationType so_AnimationType;

                bool foundAnimation =
                    animationTypeDictionaryByAnimation.TryGetValue(animationClip, out so_AnimationType);

                if (foundAnimation)
                {
                    string key = characterAttribute.characterPart.ToString() +
                                 characterAttribute.partVariantColour.ToString() +
                                 characterAttribute.partVariantType.ToString() +
                                 so_AnimationType.animationName.ToString();
                    
                    SO_AnimationType swapSO_AnimationType;

                    bool foundSwapAnimation =
                        animationTypeDictionaryByCompositeAttributeKey.TryGetValue(key, out swapSO_AnimationType);

                    if (foundSwapAnimation)
                    {
                        AnimationClip swapAnimationClip = swapSO_AnimationType.animationClip;

                        animsKeyValuePairList.Add(
                            new KeyValuePair<AnimationClip, AnimationClip>(animationClip, swapAnimationClip));
                    }
                }
            }
            
            aoc.ApplyOverrides(animsKeyValuePairList);
            currentAnimator.runtimeAnimatorController = aoc;
        }
    }
}
