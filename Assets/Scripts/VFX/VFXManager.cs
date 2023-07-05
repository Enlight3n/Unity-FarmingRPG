using System.Collections;
using UnityEngine;

public class VFXManager : SingletonMonobehaviour<VFXManager>
{

    private WaitForSeconds twoSeconds;
    [SerializeField] private GameObject reapingPrefab = null;
    [SerializeField] private GameObject deciduousLeavesFallingPrefab;
    [SerializeField] private GameObject choppingTreeTrunkPrefab;
    [SerializeField] private GameObject pineConeFallingPrefab;

    protected override void Awake()
    {
        base.Awake();

        twoSeconds = new WaitForSeconds(2f);

    }

    private void OnDisable()
    {
        EventHandler.HarvestActionEffectEvent -= displayHarvestActionEffect;
    }

    private void OnEnable()
    {
        EventHandler.HarvestActionEffectEvent += displayHarvestActionEffect;
    }

    private IEnumerator DisableHarvestActionEffect(GameObject effectGameObject, WaitForSeconds secondsToWait)
    {
        yield return secondsToWait;
        effectGameObject.SetActive(false);
    }

    private void displayHarvestActionEffect(Vector3 effectPosition, HarvestActionEffect harvestActionEffect)
    {
        switch (harvestActionEffect)
        {

            case HarvestActionEffect.reaping:
                GameObject reaping = PoolManager.Instance.ReuseObject(reapingPrefab, effectPosition, Quaternion.identity);
                reaping.SetActive(true);
                StartCoroutine(DisableHarvestActionEffect(reaping, twoSeconds));
                break;
            case HarvestActionEffect.deciduousLeavesFalling:
                GameObject deciduousLeavesFalling = PoolManager.Instance.ReuseObject(deciduousLeavesFallingPrefab,
                    effectPosition, Quaternion.identity);
                deciduousLeavesFalling.SetActive(true);
                StartCoroutine(DisableHarvestActionEffect(deciduousLeavesFalling, twoSeconds));
                break;
            case HarvestActionEffect.choppingTreeTrunk:
                GameObject choppingTreeTrunk = PoolManager.Instance.ReuseObject(choppingTreeTrunkPrefab,
                    effectPosition, Quaternion.identity);
                choppingTreeTrunk.SetActive(true);
                StartCoroutine(DisableHarvestActionEffect(choppingTreeTrunk, twoSeconds));
                break;
            case HarvestActionEffect.pineConeFalling:
                GameObject pineConesFalling = PoolManager.Instance.ReuseObject(pineConeFallingPrefab, effectPosition, Quaternion.identity);
                pineConesFalling.SetActive(true);
                StartCoroutine(DisableHarvestActionEffect(pineConesFalling, twoSeconds));
                break;
            case HarvestActionEffect.none:
                break;
            
            

            default:
                break;
        }
    }


}
