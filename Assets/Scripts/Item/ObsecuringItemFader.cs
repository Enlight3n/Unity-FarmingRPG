using System.Collections;
using UnityEngine;


/// <summary>
/// 这个脚本挂载到需要控制渐隐渐显的物体上
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ObsecuringItemFader : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }
    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine());
    }
    public void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    /// <summary>
    /// 将物体的透明度变为1f
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeInRoutine()
    {
        float currentAlpha = _spriteRenderer.color.a;
        float distance = 1f - currentAlpha;
        
        while (1f - currentAlpha > 0.01f)
        {
            currentAlpha += distance / Settings.fadeInSeconds * Time.deltaTime;
            _spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }
        
        _spriteRenderer.color = new Color(1f, 1f, 1f,1f);

    }
    
    /// <summary>
    /// 将物体的透明度变为targetAlpha
    /// </summary>

    private IEnumerator FadeOutRoutine()
    {
        float currentAlpha = _spriteRenderer.color.a;
        float distance = currentAlpha - Settings.targetAlpha;

        while (currentAlpha - Settings.targetAlpha > 0.01f)
        {
            //计算物体每帧需要减少的透明度，以实现物体逐渐淡出的效果。
            currentAlpha = currentAlpha - distance / Settings.fadeOutSeconds * Time.deltaTime;
            
            _spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            
            yield return null;
        }

        _spriteRenderer.color = new Color(1f, 1f, 1f, Settings.targetAlpha);
    }
}
