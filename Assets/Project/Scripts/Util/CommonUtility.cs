using UnityEngine;

public static class CommonUtility
{
    public static Vector3 GetPointerWorldPos()
    {
        const float planeYHeight = 0.5f;
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 pos = Input.mousePosition;
        pos.z = 30;
        pos = Camera.main.ScreenToWorldPoint(pos);

        Vector3 dir = (pos - cameraPos).normalized;
        float factor = 1 / Vector3.Dot(dir, Vector3.down);
        float length = (cameraPos.y - planeYHeight) * factor;
        pos = cameraPos + (dir * length);

        return pos;
    }

    public static Color ToBlockColor(this ColorType colorType)
    {
        return BoardResource.instance.blockColors[(int)colorType];
    }
    public static Color ToWallColor(this ColorType colorType)
    {
        return BoardResource.instance.wallColors[(int)colorType];
    }
    public static Color ToRawColor(this ColorType colorType)
    {
        switch (colorType)
        {
            case ColorType.None:
                return Color.white;
            case ColorType.Red:
                return Color.red;
            case ColorType.Orange:
                return new(1, 0.6f, 0);
            case ColorType.Yellow:
                return Color.yellow;
            case ColorType.Gray:
                return Color.gray;
            case ColorType.Purple:
                return new(0.7f, 0, 1);
            case ColorType.Beige:
                return new(0.9f, 0.9f, 0.7f);
            case ColorType.Blue:
                return Color.blue;
            case ColorType.Green:
                return Color.green;
        }
        return Color.white;
    }

    public static Color WithAlpha(this Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }

    public static Vector2 ToVector(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector2.up;
            case Direction.Down:
                return Vector2.down;
            case Direction.Left:
                return Vector2.left;
            case Direction.Right:
                return Vector2.right;
        }
        return Vector2.zero;
    }

    public static Direction Invert(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
        }
        return direction;
    }

    public static Quaternion ToRotation(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Quaternion.Euler(0, 180, 0);
            case Direction.Down:
                return Quaternion.identity;
            case Direction.Left:
                return Quaternion.Euler(0, 90, 0);
                break;
            case Direction.Right:
                return Quaternion.Euler(0, -90, 0);
        }
        return Quaternion.identity;
    }

    public static bool IsVertical(this Direction direction)
    {
        return direction == Direction.Up || direction == Direction.Down;
    }

    public static bool IsHorizontal(this Direction direction)
    {
        return direction == Direction.Right || direction == Direction.Left;
    }

    public static Vector3 ToXZ(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public static bool IsNearlyEqual(this float a, float b, float tolerance = 0.1f)
    {
        return a > b - tolerance && a < b + tolerance;
    }

    public static bool IsBetween(this float value, float a, float b, float tolerance = 0)
    {
        float min = Mathf.Min(a, b) - tolerance;
        float max = Mathf.Max(a, b) + tolerance;
        return value > min && value < max;
    }
}
