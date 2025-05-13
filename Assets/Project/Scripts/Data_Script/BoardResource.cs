using UnityEngine;

[CreateAssetMenu(fileName = "BoardResource", menuName = "Board Resource")]
public class BoardResource : ScriptableObject
{
    private static BoardResource _instance;
    public static BoardResource instance
    {
        get
        {
            if (_instance == null) _instance = Resources.Load<BoardResource>("BoardResource");
            return _instance;
        }
        private set { _instance = value; }
    }

    public Color[] blockColors;
    public Color[] wallColors;
}
