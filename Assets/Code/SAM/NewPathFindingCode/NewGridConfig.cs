using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGirdConfig", menuName = "Scriptable Objects/NewGirdConfig")]
public class NewGridConfig : ScriptableObject
{
    public int width;
    public int length;
    public float size;
    [SerializeField] private List <Vector3> origin = new List<Vector3>();
}
