using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

static class VectorExtensions
{
    /// <summary>
    /// Draws a cross at the vector's point. Default color being transparent becomes white.
    /// </summary>
    /// <param name="inPoint"></param>
    /// <param name="color"></param>
    /// <param name="inSize"></param>
    /// <param name="inSecondsDuration"></param>
    /// <param name="inDepthTest"></param>
    public static void DebugDrawCross(this Vector3 inPoint, Color color = default(Color), float inSize = 1, float inSecondsDuration = 1, bool inDepthTest = true, bool inIsUpNormal = true)
    {
        //Default color is transparent, which is useless
        if (color == default(Color))
            color = Color.green;

        float crossArmLength = inSize / 2;

        if (inIsUpNormal)
        {
            Debug.DrawLine(inPoint + crossArmLength * Vector3.forward, inPoint + crossArmLength * Vector3.back, color, inSecondsDuration, inDepthTest);
            Debug.DrawLine(inPoint + crossArmLength * Vector3.left, inPoint + crossArmLength * Vector3.right, color, inSecondsDuration, inDepthTest);
        }
        else
        {
            Debug.DrawLine(inPoint + crossArmLength * Vector3.up, inPoint + crossArmLength * Vector3.down, color, inSecondsDuration, inDepthTest);
            Debug.DrawLine(inPoint + crossArmLength * Vector3.left, inPoint + crossArmLength * Vector3.right, color, inSecondsDuration, inDepthTest);
        }
    }

    /// <summary>
    /// Draws a cross at the vector's point. Default color being transparent becomes white. We assume drawing for a 2D point is a (0,0,-1) normal.
    /// </summary>
    /// <param name="inPoint"></param>
    /// <param name="color"></param>
    /// <param name="inSize"></param>
    /// <param name="inSecondsDuration"></param>
    /// <param name="inDepthTest"></param>
    public static void DebugDrawCross(this Vector2 inPoint, Color color = default(Color), float inSize = 1, float inSecondsDuration = 1, bool inDepthTest = true)
    {
        DebugDrawCross((Vector3)inPoint, color, inSize, inSecondsDuration, inDepthTest, false);
    }
        
    public static bool IsWithinDistanceOf(this Vector3 inPoint, Vector3 inOtherPoint, float inMinDistance)
    {
        if (inMinDistance < 0) return false;

        return Vector3.SqrMagnitude(inPoint - inOtherPoint) <= inMinDistance * inMinDistance;
    }

    public static bool IsWithinDistanceOf(this Vector2 inPoint, Vector2 inOtherPoint, float inMinDistance)
    {
        return IsWithinDistanceOf((Vector3)inPoint, (Vector3)inOtherPoint, inMinDistance);
    }

    public static bool IsMagnitudeLargerThan(this Vector3 inVector, float inMinDistance)
    {
        if (inMinDistance < 0) return true;

        return Vector3.SqrMagnitude(inVector) > inMinDistance * inMinDistance;
    }

    public static bool IsMagnitudeLargerThan(this Vector2 inVector, float inMinDistance)
    {
        return IsMagnitudeLargerThan((Vector3)inVector, inMinDistance);
    }
}