using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;
using System.Reflection;

namespace PoliceSmartRadio
{
    internal class Main : Plugin
    {
        public Main()
        {

        }

        public override void Finally()
        {

        }

        public override void Initialize()
        {
            Game.Console.Print("PoliceSmartRadio " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ", developed by Albo1125, loaded successfully!");
            Game.Console.Print("Special thanks to FinKone for the inspiration and OfficerSquare for the default UI.");
            Game.Console.Print("Please go on duty to start Police SmartRadio.");

            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
        }

        internal static Version Albo1125CommonVer = new Version("6.6.3.0");
        internal static Version MadeForGTAVersion = new Version("1.0.2060.1");
        internal static float MinimumRPHVersion = 0.51f;
        internal static string[] AudioFilesToCheckFor = new string[] { "Plugins/LSPDFR/PoliceSmartRadio/Audio/ButtonScroll.wav", "Plugins/LSPDFR/PoliceSmartRadio/Audio/ButtonSelect.wav",
            "Plugins/LSPDFR/PoliceSmartRadio/Audio/PlateCheck/TargetPlate1.wav", "Plugins/LSPDFR/PoliceSmartRadio/Audio/PanicButton.wav" };
        internal static Version MadeForLSPDFRVersion = new Version("0.4.8");
        internal static string[] OtherFilesToCheckFor = new string[] { "Plugins/LSPDFR/Traffic Policer.dll", "Plugins/LSPDFR/PoliceSmartRadio/Config/GeneralConfig.ini",
            "Plugins/LSPDFR/PoliceSmartRadio/Config/ControllerConfig.ini", "Plugins/LSPDFR/PoliceSmartRadio/Config/KeyboardConfig.ini", "Plugins/LSPDFR/PoliceSmartRadio/Config/DisplayConfig.ini",
            "Plugins/LSPDFR/PoliceSmartRadio/Config/PanicButton.ini"};

        internal static Version TrafficPolicerVersion = new Version("6.14.0.0");
        internal static string[] conflictingFiles = new string[] { "Plugins/LSPDFR/PoliceRadio.dll" };

        internal static string FileID = "15354";
        internal static string DownloadURL = "http://www.lcpdfr.com/files/file/15354-police-smartradio-the-successor-to-police-radio/";
        internal static string PluginName = "Police SmartRadio";
        internal static string Path = "Plugins/LSPDFR/PoliceSmartRadio.dll";

        public static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                GameFiber.StartNew(delegate
                {
                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveAssemblyEventHandler);
                    while (!IsLSPDFRPluginRunning("Traffic Policer"))
                    {
                        GameFiber.Yield();
                    }
                    PoliceSmartRadio.Initialise();
                });
            }
        }

        public static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                AssemblyName an = assembly.GetName();
                if (an.Name.ToLower() == Plugin.ToLower())
                {
                    if (minversion == null || an.Version.CompareTo(minversion) >= 0) { return true; }
                }
            }
            return false;
        }

        public static Assembly ResolveAssemblyEventHandler(object sender, ResolveEventArgs args)
        {
            foreach (Assembly assembly in LSPD_First_Response.Mod.API.Functions.GetAllUserPlugins())
            {
                if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower()))
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}
