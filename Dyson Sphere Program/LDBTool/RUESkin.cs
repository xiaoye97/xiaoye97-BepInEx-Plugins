using UnityEngine;
using RuntimeUnityEditor.Core.UI;

namespace xiaoye97
{
    public class RUESkin : ISkin
    {
        public GUISkin GetSkin()
        {
            return InterfaceMaker.CustomSkin;
        }
    }
}
