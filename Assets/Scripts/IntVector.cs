using UnityEngine;
using System.Collections;

[System.Serializable]
public struct IntVector
{

    [SerializeField] private int _x, _y;

    public IntVector(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public static IntVector operator+ (IntVector a, IntVector b)
    {
        IntVector result = new IntVector();
        result.X = a.X + b.X;
        result.Y = a.Y + b.Y;
        return result;
    }

    public int X 
    {
        get { return _x; }
        set { _x = value; }
    }

    public int Y
    {
        get { return _y; }
        set { _y = value; }
    }

}
