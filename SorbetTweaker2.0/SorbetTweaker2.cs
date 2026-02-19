using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MSCLoader;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace SorbetTweaker2
{
    public class SorbetTweaker2 : Mod
    {
        public override string ID => "VrwPack"; // Your (unique) mod ID 
        public override string Name => "VrwPack"; // Your mod name
        public override string Author => "VRW"; // Name of the Author (your name)
        public override string Version => "1.0"; // Version
        public override string Description => "A Simple Mod For Adding Clock & Unlimited Fuel"; // Short description of your mod 
        public override byte[] Icon
        {
            get
            {
                Stream stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("SorbetTweaker2._0.icon.png");

                if (stream == null)
                    return null;

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Close();

                return buffer;
            }
        }
        public override Game SupportedGames => Game.MyWinterCar;

        public SettingsCheckBox sinffuel, Ginffuel, Kinffuel, isdebug, is12;
        SettingsSliderInt fontSize;
        FsmFloat clockM, sorbetFuelFloat, gifuFsm, kekmetFsm;
        FsmInt clockH;
        Canvas canvas;
        GameObject panel;
        Text time;
        bool errorFuel = false, isSoCache, isGiCache, isKeCache;
        float cachefuellevel, GifuCache, KeCache;

        private void debugs()
        {
            getFsmFuel();
        }

        string GetPath(GameObject obj)
        {
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return path;
        }

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.Update, Mod_Update);
            SetupFunction(Setup.ModSettings, Mod_Settings);
            SetupFunction(Setup.ModSettingsLoaded, Mod_SettingsLoaded);
            SetupFunction(Setup.OnSave, Mod_OnSave);
        }

        private void Mod_OnSave()
        {
            sorbetFuelFloat.Value = cachefuellevel;
            kekmetFsm.Value = KeCache;
            gifuFsm.Value = GifuCache;
        }

        private void Mod_SettingsLoaded()
        {
            //Nothing
        }

        private void Mod_Settings()
        {
            Settings.AddHeader("Vehicle Tweaker");
            sinffuel = Settings.AddCheckBox(
                "sinffuel",
                "Sorbet Unlimited Fuel",
                false
                );

            Ginffuel = Settings.AddCheckBox(
                "Ginffuel",
                "Gifu Unlimited Fuel",
                false
                );

            Kinffuel = Settings.AddCheckBox(
                "Kinffual",
                "Kekmet Unlimited Fuel",
                false
                );

            Settings.AddHeader("Clock Settings");
            fontSize = Settings.AddSlider("FontSize", "Font Size", 14, 99);
            SettingsButton change = Settings.AddButton("Save", changeFontSize);
            is12 = Settings.AddCheckBox(
                    "isPm",
                    "12 Hour Clock",
                    false);

            Settings.AddHeader("DEBUG");
            isdebug = Settings.AddCheckBox(
                "isdebug",
                "Debuging",
                false
                );
            SettingsButton debug = Settings.AddButton("Dump Fuel Data", debugs);
        }

        private string GetCurrentTime()
        {
            clockH = PlayMakerGlobals.Instance.Variables.FindFsmInt("GlobalHour");
            clockM = PlayMakerGlobals.Instance.Variables.FindFsmFloat("TimeRotationMinute");
            float minutef = (360f - clockM.Value) / 6f;
            int minute = Mathf.FloorToInt(minutef);

            float sec = (minutef - minute) * 60f;
            sec = Mathf.FloorToInt(sec);

            int Hour = clockH.Value;
            if (is12.GetValue())
            {
                if(clockH.Value >= 12)
                {
                    Hour -= 12;
                    return $"{Hour:00}:{minute:00}:{sec:00} PM";
                }else
                {
                    return $"{Hour:00}:{minute:00}:{sec:00} AM";
                }
            }
            if (clockH.Value == 24)
                Hour = 0;

            return $"{Hour:00}:{minute:00}:{sec:00}";
        }

        private GameObject CreatePanel(Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            GameObject panelGO = new GameObject("panel");
            panelGO.transform.SetParent(canvas.transform, false);

            Image img = panelGO.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);

            RectTransform rt = panelGO.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = new Vector2(220, 90);
            rt.anchoredPosition = Vector2.zero;

            return panelGO;
        }

        Text CreateText(GameObject parent, string txt, Vector2 pos)
        {
            GameObject go = new GameObject("Times");
            go.transform.SetParent(parent.transform, false);

            Text t = go.AddComponent<Text>();
            t.text = txt;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = 20;
            t.color = Color.white;

            RectTransform rt = t.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 30);
            rt.anchoredPosition = pos;

            return t;
        }

        private void Mod_OnLoad()
        {
            /*GameObject sorbetGo = GameObject.Find("SORBET(190-200psi)");
            if (sorbetGo == null)
            {
                ModConsole.Print("Sorbet root not found");
                return;
            }

            Transform fuelTankTf = sorbetGo.transform.Find("Simulation/FuelTankSorbett");
            if (fuelTankTf == null)
            {
                ModConsole.Print("FuelTankSorbet not found");
                error = true;
                return;
            }

            GameObject fuelTankGo = fuelTankTf.gameObject;
            ModConsole.Print("FuelTankSorbet accessed!");

            foreach(var fsm in fuelTankGo.GetComponents<PlayMakerFSM>())
            {
                ModConsole.Log(fsm.FsmName + " | " + fsm.FsmDescription);
            }

            PlayMakerFSM fuel = fuelTankGo.GetComponent<PlayMakerFSM>();
            fuelData = fuel.FsmVariables.GetFsmFloat("Data");

            if(fuelData == null)
            {
                ModConsole.LogError("fuelData not found");
            }

            ModConsole.Log($"fuelData found current value {fuelData.Value}");*/

            foreach (string s in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                ModConsole.Print(s);
            }


            //Clock UI
            GameObject canvasGO = new GameObject("VrwPackCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            UnityEngine.Object.DontDestroyOnLoad(canvasGO);

            panel = CreatePanel(
                new Vector2(0, 0),
                new Vector2(0, 0),
                new Vector2(0, 0)
            );
            panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, 20);
            time = CreateText(panel, GetCurrentTime(), new Vector2(0, 0));
            ModConsole.Log("succesfuly make Time UI");
            getFsmFuel();
            changeFontSize();
        }

        private void sorbetUnlimitedFuel()
        {
            if (sorbetFuelFloat == null)
            {
                ModConsole.LogError("Sorbet Fuel Float Not Found");
                errorFuel = true;
                return;
            }

            if (sinffuel.GetValue())
            {
                if (!isSoCache)
                {
                    cachefuellevel = sorbetFuelFloat.Value;
                    isSoCache = true;
                }

                sorbetFuelFloat.Value = 58f;
                if (isdebug.GetValue())
                    ModConsole.Log($"Sorbet Fuel | {sorbetFuelFloat.Value}");
            }
            else
            {
                if (isSoCache)
                    sorbetFuelFloat.Value = cachefuellevel;
                isSoCache = false;
            }
        }

        private void gifuUnlFuel()
        {
            if(gifuFsm == null)
            {
                ModConsole.LogError("Gifu Fuel FSM not found");
                errorFuel = true;
                return;
            }
            if (Ginffuel.GetValue())
            {
                if (!isGiCache)
                {
                    GifuCache = gifuFsm.Value;
                    isGiCache = true;
                }
                gifuFsm.Value = 130f;
                if (isdebug.GetValue())
                    ModConsole.Print($"Gifu Fuel | {gifuFsm.Value}");
            }
            else
            {
                if (isGiCache)
                    gifuFsm = GifuCache;
                isGiCache = false;
            }
        }

        private void kekmetUnlFuel()
        {
            if (gifuFsm == null)
            {
                ModConsole.LogError("Kekmet Fuel FSM not found");
                errorFuel = true;
                return;
            }
            if (Kinffuel.GetValue())
            {
                if (!isKeCache)
                {
                    KeCache = kekmetFsm.Value;
                    isKeCache = true;
                }
                kekmetFsm.Value = 65f;
                if (isdebug.GetValue())
                    ModConsole.Print($"Kekmet Fuel | {kekmetFsm.Value}");
            }
            else
            {
                if (isKeCache)
                    kekmetFsm = KeCache;
                isKeCache = false;
            }
        }

        private void getFsmFuel()
        {
            PlayMakerFSM[] fsms = GameObject.FindObjectsOfType<PlayMakerFSM>();
            ModConsole.Log($"Find FSM of {fsms.Length}");

            foreach(PlayMakerFSM fsm in fsms)
            {
                foreach(FsmFloat f in fsm.FsmVariables.FloatVariables)
                {
                    if (f.Name.ToLower().Contains("fuel"))
                    {
                        ModConsole.Log($"{GetPath(fsm.gameObject)} | {fsm.FsmName} = {f.Value}");
                        if (fsm.gameObject.name == "FuelTankSorbett")
                        { 
                            sorbetFuelFloat = f;
                            ModConsole.Log("Found Fuel Data "+sorbetFuelFloat.Value); 
                        }
                        if (fsm.gameObject.name == "FuelTankGifu" && f.Value > 0)
                        {
                            gifuFsm = f;
                            ModConsole.Log("Found Gifu Fuel");
                        }
                        if(fsm.gameObject.name == "FuelTankKekmet" && f.Value > 0)
                        {
                            kekmetFsm = f;
                            ModConsole.Log("Found Kekmet Fuel");
                        }
                    }
                }
            }
        }

        private void Mod_Update()
        {
            time.text = GetCurrentTime();
            if (!errorFuel)
            {
                sorbetUnlimitedFuel();
                gifuUnlFuel();
                kekmetUnlFuel();
            }
        }

        private void changeFontSize()
        {
            if (time != null)
                time.fontSize = fontSize.GetValue();
        }
    }
}
