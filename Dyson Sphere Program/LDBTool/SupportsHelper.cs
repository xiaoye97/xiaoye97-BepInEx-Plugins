using System;

namespace xiaoye97
{
    public static class SupportsHelper
    {
        private static readonly Type _runtimeUnityEditorType = Type.GetType("RuntimeUnityEditor.Bepin5.RuntimeUnityEditor5, RuntimeUnityEditor.Bepin5", false);
        internal static readonly Type _interfaceMakerType = Type.GetType("RuntimeUnityEditor.Core.UI.InterfaceMaker, RuntimeUnityEditor.Core", false);
        public static bool SupportsRuntimeUnityEditor { get; }

        static SupportsHelper()
        {
            SupportsRuntimeUnityEditor = _runtimeUnityEditorType != null;
        }
    }

    public interface ISkin
    {
        UnityEngine.GUISkin GetSkin();
    }
}
