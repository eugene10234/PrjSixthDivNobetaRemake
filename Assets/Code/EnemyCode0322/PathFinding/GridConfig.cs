using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridConfig", menuName = "Scriptable Objects/GridConfig")]
public class GridConfig : ScriptableObject
{
    public int width;
    public int length;
    public float size;
    public Vector3 origin;
    public List <Vector3> oriPoints = new List <Vector3>();
}
