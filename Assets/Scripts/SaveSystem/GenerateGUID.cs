using UnityEngine;

[ExecuteAlways] //在编辑器中也运行
public class GenerateGUID : MonoBehaviour
{
    
    [SerializeField] private string _gUID = "";
    public string GUID
    {
        get => _gUID;
        set => _gUID = value;
    }


    private void Awake()
    {
        //如果对象是游戏世界的一部分，则为 true。
        if (!Application.IsPlaying(gameObject))
        {
            if (_gUID == "")
            {
                _gUID = System.Guid.NewGuid().ToString();
            }
        }
    }
}
