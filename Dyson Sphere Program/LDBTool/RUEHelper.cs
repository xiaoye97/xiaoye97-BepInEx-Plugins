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
    }
}
