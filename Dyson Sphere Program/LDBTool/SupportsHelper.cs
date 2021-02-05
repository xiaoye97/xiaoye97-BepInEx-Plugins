using System;

namespace xiaoye97
{
    public static class SupportsHelper
    {
        private static readonly Type _runtimeUnityEditorType = Type.GetType("RuntimeUnityEditor.Bepin5.RuntimeUnityEditor5, RuntimeUnityEditor.Bepin5", false);
        public static bool SupportsRuntimeUnityEditor { get; }

        static SupportsHelper()
        {
            SupportsRuntimeUnityEditor = _runtimeUnityEditorType != null;
        }
    }
}
