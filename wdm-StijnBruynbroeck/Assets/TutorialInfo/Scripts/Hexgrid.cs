using UnityEngine;

public class Hexgrid : MonoBehaviour
{

    [field: SerializeField] public int Width { get; private set; }

[field: SerializeField] public int Height { get; private set; }
[field:SerializeField]public float Hexsize{ get; private set; }

}

public enum HexOrientation
{
    FlatTop,
    PointyTop
}