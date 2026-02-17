using MSCLoader;
using UnityEngine;
using HutongGames.PlayMaker;
using System.Reflection;
using UnityEngine.UI;
using HutongGames.PlayMaker.Actions;
using System.Linq;

namespace SorbetTweaker2
{
    public class SorbetTweaker2 : Mod
    {
        public override string ID => "VrwPack"; // Your (unique) mod ID 
        public override string Name => "VrwPack"; // Your mod name
        public override string Author => "VRW"; // Name of the Author (your name)
        public override string Version => "0.1"; // Version
        public override string Description => "Sorbet Tweaker by Vrw"; // Short description of your mod 
        public override Game SupportedGames => Game.MyWinterCar;

        public SettingsCheckBox inffuel;
        SettingsSliderInt fontSize;
        SettingsSlider value;
        FsmFloat fuelData, clockM, sorbetFuelFloat;
        PlayMakerFSM sorbetFuelFsm;
        FsmInt clockH;
        Canvas canvas;
        GameObject panel;
        Text time;
        bool dumped = false, error = false, isCache;
        float cachefuellevel;

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
        }

        private void Mod_SettingsLoaded()
        {
            changeFontSize();
        }

        private void Mod_Settings()
        {
            Settings.AddHeader("Sorbet Tweaker");
            inffuel = Settings.AddCheckBox(
                "inffuel",
                "unlimited fuel",
                false
                );

            Settings.AddHeader("UI");
            fontSize = Settings.AddSlider("FontSize", "Font Size", 14, 99);
            SettingsButton change = Settings.AddButton("Save", changeFontSize);
            Settings.AddHeader("DEBUG");
            value = Settings.AddSlider("debugV", "Debug Value", 0f, 60f);
            SettingsButton debug = Settings.AddButton("Debug", debugs);
        }

        private string GetCurrentTime()
        {
            clockH = PlayMakerGlobals.Instance.Variables.FindFsmInt("GlobalHour");
            clockM = PlayMakerGlobals.Instance.Variables.FindFsmFloat("TimeRotationMinute");
            float minutef = (360f - clockM.Value) / 6f;
            int minute = Mathf.FloorToInt(minutef);

            float sec = (minutef - minute) * 60f;
            sec = Mathf.FloorToInt(sec);

            return $"{clockH:00}:{minute:00}:{sec:00}";
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
            GameObject sorbetGo = GameObject.Find("SORBET(190-200psi)");
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

            ModConsole.Log($"fuelData found current value {fuelData.Value}");


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
            //getFsmFuel();
            //getFuelData();
            getFsmFuel();
        }

        private void getFuelData()
        {
            var sorbet = GameObject.Find("SORBET(190-200psi)");
            Transform sim = sorbet.transform.Find("Simulation/FuelTankSorbett");
            if(sim == null)
            {
                ModConsole.LogError("SORBET/Simulation/FuelTankSorbet Not Found");
                return;
            }
            sorbetFuelFsm = sim.GetComponent<PlayMakerFSM>();

            if(sorbetFuelFsm == null)
            {
                ModConsole.LogError("Sorbet PlayMakerFSM Not Found");
                return;
            }
            sorbetFuelFloat = sorbetFuelFsm.FsmVariables.GetFsmFloat("Data");
            ModConsole.Log("Sorbet Fuel FSM Intialized | " + sorbetFuelFloat.Value);
        }

        private void unlimitedFuel()
        {
            //getFuelData();
            if (sorbetFuelFloat == null)
            {
                ModConsole.LogError("Sorbet Fuel Float Not Found");
                error = true;
                return;
            }

            if (inffuel.GetValue())
            {
                if (!isCache)
                {
                    cachefuellevel = sorbetFuelFloat.Value;
                    isCache = true;
                }

                sorbetFuelFloat.Value = 58f;
                ModConsole.Log($"Fuel Value: {sorbetFuelFloat.Value} | cache: {cachefuellevel}");
            }
            else
            {
                if (isCache)
                    sorbetFuelFloat.Value = cachefuellevel;
                isCache = false;
                //ModConsole.Log($"Fuel Value: {sorbetFuelFloat.Value}");
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
                            { sorbetFuelFloat = f; ModConsole.Log("Found Fuel Data "+sorbetFuelFloat.Value); }
                    }
                }
            }
        }

        private void Mod_Update()
        {
            time.text = GetCurrentTime();
            if(!error)
                unlimitedFuel();
        }

        private void changeFontSize()
        {
            if (time != null)
                time.fontSize = fontSize.GetValue();
        }
    }
}
