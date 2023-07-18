using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "so_SceneSoundsList", menuName = "Scriptable Objects/Sounds/Scene Sounds List")]
public class SO_SceneSoundsList : ScriptableObject
{
    [SerializeField]
    public List<SceneSoundsItem> sceneSoundsDetails;
}