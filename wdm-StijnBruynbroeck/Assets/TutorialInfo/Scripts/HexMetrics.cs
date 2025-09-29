using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexMetrics
{
    public static float OuterRadius(float hexSize)
    { return hexSize; }

    public static float InnerRadius(float hexSize)
    { return hexSize * 0.866025404f; }
}
