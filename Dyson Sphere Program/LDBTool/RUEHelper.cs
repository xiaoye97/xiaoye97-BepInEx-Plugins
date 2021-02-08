namespace xiaoye97
{
    public static class RUEHelper
    {
        public static void ShowData(IShowData data)
        {
            data.Show();
        }

        public static void ShowProto(Proto proto)
        {
            if (proto != null)
            {
                ShowItem item = new ShowItem(proto, $"{proto.GetType().Name} {proto.name.Translate()}");
                ShowData(item);
            }
        }

        public static UnityEngine.GUISkin GetRUESkin()
        {
            if(SupportsHelper.SupportsRuntimeUnityEditor)
            {
                return HarmonyLib.Traverse.Create(SupportsHelper._interfaceMakerType).Property("CustomSkin").GetValue<UnityEngine.GUISkin>();
            }
            return null;
        }
    }
}
