using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.UI;
using ChilloutButtonAPI.UI;
using HarmonyLib;
using Libraries;
using MelonLoader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(ChilloutButtonAPI.ChilloutButtonAPIMain), "CVRMod_Plague", "1.0", "Plague")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace ChilloutButtonAPI
{
    public class ChilloutButtonAPIMain : MelonMod
    {
        public static event Action OnInit;

        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(AccessTools.Constructor(typeof(PlayerDescriptor)), null, new HarmonyMethod(typeof(ChilloutButtonAPIMain).GetMethod(nameof(OnPlayerJoined), BindingFlags.NonPublic | BindingFlags.Static)));

            HarmonyInstance.Patch(typeof(CVR_MenuManager).GetMethod(nameof(CVR_MenuManager.ToggleQuickMenu), AccessTools.all), null, new HarmonyMethod(typeof(ChilloutButtonAPIMain).GetMethod(nameof(OnQMStateChange), BindingFlags.NonPublic | BindingFlags.Static))); // Patch Method Setting Bool For QM Status; Use For Our UI To Sync
        }

        private static GameObject OurUIParent;
        public static SubMenu MainPage;

        private static void OnQMStateChange(bool __0)
        {
            MelonCoroutines.Start(RunMe());

            IEnumerator RunMe()
            {
                yield return new WaitForSeconds(0.2f);

                if (!HasInit)
                {
                    if (new AssetBundleLib() is var Bundle && Bundle.LoadBundle("ChilloutButtonAPI.universal ui.asset")) // This If Also Checks If It Successfully Loaded As To Prevent Further Exceptions
                    {
                        var obj = Bundle.Load<GameObject>("Universal UI.prefab");

                        var QM = GameObject.Find("Cohtml").transform.Find("QuickMenu");

                        OurUIParent = Object.Instantiate(obj);

                        OurUIParent.hideFlags = HideFlags.DontUnloadUnusedAsset;

                        OurUIParent.transform.SetParent(QM);

                        OurUIParent.transform.localPosition = new Vector3(0.65f, 0.062f, 0f);
                        OurUIParent.transform.localScale = new Vector3(0.0007f, 0.001f, 0.001f);

                        OurUIParent.transform.Find("Scroll View/Viewport/Content/Back Button/Text (TMP)").gameObject.SetActive(false);
                        OurUIParent.transform.Find("Scroll View/Viewport/Content/Back Button/Text (TMP) Title").GetComponent<TextMeshProUGUI>().text = "Mod UI";

                        OurUIParent.transform.Find("Scroll View/Viewport/Content/Slider").gameObject.AddComponent<SliderTextUpdater>();
                        OurUIParent.transform.Find("Tooltip").gameObject.AddComponent<TooltipHandler>();

                        MainPage = new SubMenu
                        {
                            gameObject = OurUIParent
                        };
                    }
                    else
                    {
                        MelonLogger.Error($"Failed Loading Bundle: {Bundle.error}");
                    }

                    HasInit = true;

                    OnInit += () =>
                    {
                        var menu = MainPage.AddSubMenu("Test SubMenu");

                        menu.AddButton("Test Button", "Test Button", () =>
                        {
                            MelonLogger.Msg("Button Clicked!");

                            CohtmlHud.Instance.ViewDropText("Category", "Headline", "Small");

                            ViewManager.Instance.openMenuKeyboard("");
                        });

                        menu.AddToggle("Test Toggle", "Test Toggle", (v) =>
                        {
                            MelonLogger.Msg($"Toggle Clicked! -> {v}");
                        }, true);

                        menu.AddSlider("Test Slider", "Test Slider", (v) =>
                        {
                            MelonLogger.Msg($"Slider Adjusted! -> {v}");
                        }, 0.5f, 0f, 1f);

                        menu.AddLabel("Test Label", "Test Label");
                    };

                    OnInit?.Invoke();
                }

                if (SubMenu.AllSubMenus.Any(o => o.LastState))
                {
                    foreach (var menu in SubMenu.AllSubMenus)
                    {
                        if (menu.gameObject.activeSelf != __0)
                        {
                            menu.SetActive(__0 && menu.LastState, true);
                        }
                    }
                }
                else
                {
                    if (MainPage.gameObject.activeSelf != __0)
                    {
                        MainPage.SetActive(__0);
                    }
                }
            }
        }

        private static bool HasInit = false;

        private static void OnPlayerJoined(PlayerDescriptor __instance)
        {
            MelonCoroutines.Start(RunMe());

            IEnumerator RunMe()
            {
                yield return new WaitForSeconds(1f);

                MelonLogger.Msg(!string.IsNullOrEmpty(__instance.userName) ? $"{__instance.userName} Joined" : "Local Player Init");

                yield break;
            }
        }

        internal class SliderTextUpdater : MonoBehaviour
        {
            private Slider SliderComp;
            private TextMeshProUGUI TextComp;

            void Start()
            {
                SliderComp = transform.Find("Slider").GetComponent<Slider>();
                TextComp = transform.Find("Slider/Text (TMP)").GetComponent<TextMeshProUGUI>();
            }

            void Update()
            {
                var val = SliderComp.value.ToString("0.0");

                if (TextComp.text != val)
                {
                    TextComp.text = val;
                }
            }
        }

        internal class TooltipHandler : MonoBehaviour
        {
            private TextMeshProUGUI TextComp;
            private GameObject OffsetParent;
            void Start()
            {
                TextComp = transform.GetComponentInChildren<TextMeshProUGUI>(true);
                OffsetParent = transform.Find("Offset Parent").gameObject;
            }

            void Update()
            {
                if (XRDevice.isPresent)
                {
                    // VR
                }
                else
                {
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out var hit)) // idk how the fuck to use this method
                    {
                        var store = hit.transform.GetComponent<ToolTipStore>();

                        if (store != null)
                        {
                            TextComp.text = store.Tooltip;
                            OffsetParent.SetActive(true);
                        }
                        else
                        {
                            OffsetParent.SetActive(false);
                        }
                    }
                    else
                    {
                        OffsetParent.SetActive(false);
                    }
                }
            }
        }

        internal class ToolTipStore : MonoBehaviour
        {
            public string Tooltip;
        }
    }

    internal static class Ex
    {
        internal static float GetBiggestVector(this Vector3 vec)
        {
            return Mathf.Max(Mathf.Max(vec.x, vec.y), vec.z);
        }

        internal static Vector3 Multiply(this Vector3 one, Vector3 two)
        {
            return new Vector3(one.x * two.x, one.y * two.y, one.z * two.z);
        }
    }
}