using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LOAD : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }
}
