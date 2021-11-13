using UnityEngine;
using SkySwordKill.Next;

namespace RollSystem
{
    public static class DataEx
    {
        public static Color[] QualityColors = new Color[]
        {
            new Color(0.84705883f, 0.84705883f, 0.7921569f),
            new Color(0.7019608f, 0.8509804f, 0.31764707f),
            new Color(0.44313726f, 0.85882354f, 1f),
            new Color(0.9372549f, 0.43529412f, 1f),
            new Color(1f, 0.6156863f, 0.2627451f),
            new Color(1f, 0.45490196f, 0.3019608f)
        };

        public static T GetAsset<T>(string path) where T : Object
        {
            Object asset;
            if (Main.Instance.resourcesManager.TryGetAsset(path, out asset))
            {
                return asset as T;
            }
            else
            {
                return null;
            }
        }
    }
}
