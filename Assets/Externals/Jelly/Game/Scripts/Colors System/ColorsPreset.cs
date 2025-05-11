using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.JellyMerge
{
    [CreateAssetMenu(fileName = "Colors Preset", menuName = "Data/Colors System/Colors Preset")]
    public class ColorsPreset : ScriptableObject
    {
        public Color borders = Color.white;
        public Color backplane = Color.white;

        [Space(5)]
        public Color color1 = Color.white;
        public Color color2 = Color.white;
        public Color color3 = Color.white;
    }
}