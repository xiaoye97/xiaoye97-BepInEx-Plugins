using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ZenFulcrum.EmbeddedBrowser;

namespace xiaoye97
{
    /// <summary>
    /// 此类为预制体上挂载的组件
    /// </summary>
    public class BrowserApp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public InputField inputField;
        public Browser browser;

        void Awake()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Application.streamingAssetsPath + "\\Cookies");
			if (!directoryInfo.Exists)
                directoryInfo.Create();
			if (string.IsNullOrEmpty(BrowserNative.ProfilePath))
                BrowserNative.ProfilePath = directoryInfo.FullName;
            browser.onNavStateChange += () => inputField.text = browser.Url;
        }

        public void ChangeUrl()
        {
            browser.Url = inputField.text;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            browser.EnableInput = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            browser.EnableInput = false;
        }
    }
}
