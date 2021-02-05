using RuntimeUnityEditor.Core.Inspector.Entries;

namespace xiaoye97
{
    public class ShowItem : IShowData
    {
        object obj;
        string name;
        public ShowItem(object obj, string name)
        {
            this.obj = obj;
            this.name = name;
        }

        public void Show()
        {
            InstanceStackEntry entry = new InstanceStackEntry(obj, name);
            RuntimeUnityEditor.Bepin5.RuntimeUnityEditor5.Instance.Inspector.Push(entry, true);
            RuntimeUnityEditor.Bepin5.RuntimeUnityEditor5.Instance.Show = true;
        }
    }
}
