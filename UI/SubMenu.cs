using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ChilloutButtonAPI.UI
{
    public class SubMenu
    {
        public static List<SubMenu> AllSubMenus = new List<SubMenu>();

        public GameObject gameObject;

        internal bool LastState;

        private Transform PageContent => gameObject.transform.Find("Scroll View/Viewport/Content");
        private static Transform MainPageContent => ChilloutButtonAPIMain.MainPage.gameObject.transform.Find("Scroll View/Viewport/Content");

        public SubMenu()
        {
            AllSubMenus.Add(this);
        }

        public void SetActive(bool state, bool IsQMClose = false)
        {
            if (!IsQMClose)
            {
                LastState = state;
            }

            gameObject.SetActive(state);
        }

        public SubMenu AddSubMenu(string Title, string ButtonText = null)
        {
            ButtonText ??= Title;

            var menu = new SubMenu
            {
                gameObject = Object.Instantiate(ChilloutButtonAPIMain.MainPage.gameObject, gameObject.transform.parent)
            };

            menu.gameObject.transform.Find("Scroll View/Viewport/Content").DestroyChildren(b => b.GetSiblingIndex() == 0, true);

            menu.gameObject.transform.localPosition = gameObject.transform.localPosition;
            menu.gameObject.transform.localRotation = gameObject.transform.localRotation;
            menu.SetActive(false);

            var BackButton = menu.gameObject.transform.Find("Scroll View/Viewport/Content/Back Button");
            BackButton.Find("Text (TMP) Title").GetComponent<TextMeshProUGUI>().text = Title;

            BackButton.Find("Text (TMP)").gameObject.SetActive(true);

            BackButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            BackButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                menu.SetActive(false);
                SetActive(true);
            });

            ChilloutButtonAPIMain.MainPage.AddButton(ButtonText, $"Enter The {Title} SubMenu.", () =>
            {
                ChilloutButtonAPIMain.MainPage.SetActive(false);
                menu.SetActive(true);
            });

            return menu;
        }

        public GameObject AddButton(string Text, string Tooltip, Action OnClick)
        {
            var CopiedButton = Object.Instantiate(MainPageContent.Find("Button").gameObject, PageContent);

            CopiedButton.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = Text;
            CopiedButton.GetOrAddComponent<ChilloutButtonAPIMain.ToolTipStore>().Tooltip = Tooltip;

            CopiedButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            CopiedButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnClick?.Invoke();
            });

            CopiedButton.SetActive(true);

            return CopiedButton;
        }

        public GameObject AddToggle(string Text, string Tooltip, Action<bool> OnToggle, bool DefaultState)
        {
            var CopiedToggle = Object.Instantiate(MainPageContent.Find("Toggle").gameObject, PageContent);

            CopiedToggle.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = Text;
            CopiedToggle.GetOrAddComponent<ChilloutButtonAPIMain.ToolTipStore>().Tooltip = Tooltip;

            CopiedToggle.GetComponent<Toggle>().onValueChanged = new Toggle.ToggleEvent();
            CopiedToggle.GetComponent<Toggle>().onValueChanged.AddListener(v =>
            {
                OnToggle?.Invoke(v);
            });

            CopiedToggle.GetComponent<Toggle>().isOn = DefaultState;

            CopiedToggle.SetActive(true);

            return CopiedToggle;
        }

        public GameObject AddSlider(string Text, string Tooltip, Action<float> OnSlide, float DefaultValue, float MinValue = 0f, float MaxValue = 1f)
        {
            var CopiedSlider = Object.Instantiate(MainPageContent.Find("Slider").gameObject, PageContent);

            CopiedSlider.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = Text;
            CopiedSlider.transform.Find("Slider").GetOrAddComponent<ChilloutButtonAPIMain.ToolTipStore>().Tooltip = Tooltip;

            CopiedSlider.transform.Find("Slider").GetComponent<Slider>().onValueChanged = new Slider.SliderEvent();
            CopiedSlider.transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(v =>
            {
                OnSlide?.Invoke(v);
            });

            CopiedSlider.transform.Find("Slider").GetComponent<Slider>().minValue = MinValue;
            CopiedSlider.transform.Find("Slider").GetComponent<Slider>().maxValue = MaxValue;
            CopiedSlider.transform.Find("Slider").GetComponent<Slider>().value = DefaultValue;

            CopiedSlider.SetActive(true);

            return CopiedSlider;
        }

        public GameObject AddLabel(string Text, string Tooltip = "")
        {
            var CopiedLabel = Object.Instantiate(MainPageContent.Find("Text (TMP)").gameObject, PageContent);

            CopiedLabel.GetComponent<TextMeshProUGUI>().text = Text;
            CopiedLabel.GetOrAddComponent<ChilloutButtonAPIMain.ToolTipStore>().Tooltip = Tooltip;

            CopiedLabel.SetActive(true);

            return CopiedLabel;
        }
    }
}
