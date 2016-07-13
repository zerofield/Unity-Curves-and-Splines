using UnityEngine;
using System.Collections;
using System;


public enum BezierControlPointMode
{
    Free,
    Aligned,
    Mirrored
}


public class BezierSpline : MonoBehaviour
{


    [SerializeField]
    private Vector3[] points;

    [SerializeField]
    private BezierControlPointMode[] modes;

    public int CurveCount
    {
        get
        {
            return (points.Length - 1) / 3;
        }
    }

    public int ControlPointCount
    {
        get
        {
            return points.Length;
        }
    }

    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }


    public void SetControlPoint(int index, Vector3 point)
    {
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if (index > 0)
            {
                points[index - 1] += delta;
            }

            if (index + 1 < points.Length)
            {
                points[index + 1] += delta;
            }
        }

        points[index] = point;
        EnforceMode(index);
    }

    public Vector3 GetPoint(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetPoint(
            points[i], points[i + 1], points[i + 2], points[i + 3], t));
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return transform.TransformPoint(Bezier.GetFirstDerivative(
            points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    public void Reset()
    {
        points = new Vector3[]
        {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };

        modes = new BezierControlPointMode[] {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free
        };

    }

    public void AddCurve()
    {
        Vector3 point = points[points.Length - 1];
        Array.Resize(ref points, points.Length + 3);
        point.x += 1;
        points[points.Length - 3] = point;
        point.x += 1;
        points[points.Length - 2] = point;
        point.x += 1;
        points[points.Length - 1] = point;

        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];

        //管理添加3个节点之前最后一个顶点
        EnforceMode(points.Length - 4);
    }

    public BezierControlPointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }

    public void SetControlPointMode(int index, BezierControlPointMode mode)
    {
        modes[(index + 1) / 3] = mode;
        EnforceMode(index);
    }

    void EnforceMode(int index)
    {
        Debug.Log("EnforceMode");
        int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode = GetControlPointMode(index);

        if (mode == BezierControlPointMode.Free ||
            modeIndex == 0 || modeIndex == modes.Length - 1) //对于第一个和最后一个点不需要控制点，因为没有另外一半可以控制
        {
            Debug.Log("mode: " + mode);
            return;
        }
        //0 1 | 2 3 4 | 5 6 7 | 8 9 // point index
        //0 0 | 1 1 1 | 2 2 2 | 3 3 // mode index

        int middleIndex = modeIndex * 3;

        int fixedIndex, enforcedIndex;

        //根据index 相对 middleIndex的位置决定去约束哪个点
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            enforcedIndex = middleIndex + 1;
        }
        else
        {
            fixedIndex = middleIndex + 1;
            enforcedIndex = middleIndex - 1;
        }

        Vector3 middle = points[middleIndex];
        Vector3 enforcedTangent = middle - points[fixedIndex];

        if (mode == BezierControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }

        points[enforcedIndex] = middle + enforcedTangent;

        Vector3 v1 = points[enforcedIndex] - middle;
        Vector3 v2 = points[fixedIndex] - middle;

        Debug.Log(v1 + ", " + v2);

    }

}
