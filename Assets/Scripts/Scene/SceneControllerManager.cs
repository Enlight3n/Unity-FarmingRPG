using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneControllerManager : SingletonMonobehaviour<SceneControllerManager>
{
    private bool isFading;
    [SerializeField] private float fadeDuration = 1f; //fade的持续时间
    [SerializeField] private CanvasGroup faderCanvasGroup = null;
    [SerializeField] private Image faderImage = null;
    public SceneName startingSceneName;

    //只有这个函数是public的，从外部调用也是这个函数，传入欲场景名称和玩家要移动到的新位置即可
    public void FadeAndLoadScene(string sceneName,Vector3 spawnPosition)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScene(sceneName, spawnPosition));
        }
    }

    #region 私有函数
    
    //FadeAndSwitchScene作为处理的主力函数，负责场景转换的全部流程
    //我们把场景转换分作四个部分，分别是一切发生以前-卸载和加载场景之前-加载场景之后-一切搞定以后
    //这四个时刻恰好对应了四个事件，我们在这三个间隔中，又分别插入Fade(1f)，加载场景，Fade(0f)
    private IEnumerator FadeAndSwitchScene(string sceneName,Vector3 spawnPosition)
    {
        
        EventHandler.CallBeforeSceneUnloadFadeOutEvent();

        yield return StartCoroutine(Fade(1f));
        
        //保存场景数据
        SaveLoadManager.Instance.StoreCurrentSceneData();

        Player.Instance.gameObject.transform.position = spawnPosition;
        
        EventHandler.CallBeforeSceneUnloadEvent();

        //卸载场景
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        //加载场景
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));
        
        EventHandler.CallAfterSceneLoadEvent();
        
        //恢复场景数据
        SaveLoadManager.Instance.ReStoreCurrentSceneData();

        yield return StartCoroutine(Fade(0f));
        
        EventHandler.CallAfterSceneLoadFadeInEvent();
    }

    
    //加载给定名称的场景并将其设置为ActiveScene
    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
        Scene newlyLoadScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        SceneManager.SetActiveScene(newlyLoadScene);
    }
    
    //根据指定的finalAlpha，慢慢切换到目标值
    private IEnumerator Fade(float finalAlpha)
    {
        isFading = true;

        faderCanvasGroup.blocksRaycasts = true;
        
        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;

        while (!Mathf.Approximately(faderCanvasGroup.alpha,finalAlpha))
        {
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha, 
                fadeSpeed * Time.deltaTime);
            yield return null;
        }
        isFading = false;
        faderCanvasGroup.blocksRaycasts = false;
        
    }
    # endregion

    private IEnumerator Start()
    {
        //设置为黑布
        faderImage.color = new Color(0f, 0f, 0f, 1f);
        
        //这里是对image组件的alpha再做一次alpha值
        faderCanvasGroup.alpha = 1f;

        //加载场景农场
        yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName.ToString()));

        EventHandler.CallAfterSceneLoadEvent();
        
        SaveLoadManager.Instance.ReStoreCurrentSceneData();

        StartCoroutine(Fade(0f));
    }
}
