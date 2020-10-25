using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace PoliceSmartRadio
{
    internal static class PoliceSmartRadio
    {
        public static string PlayerName = "NoNameSet";
        public static Random rnd = new Random();
        public static KeysConverter kc = new KeysConverter();
        public static bool buttonspassed = false;
        private static void MainLogic()
        {            
            DisplayHandler.InitialiseTextures(true);
            Actions.Panic.IniSetup();
            registerActions();      
        }

        private static void registerActions()
        {          
            API.Functions.AddActionToButton(Actions.RequestPit.Main, Actions.RequestPit.available, "pit");
            API.Functions.AddActionToButton(Actions.PlateChecker.Main, "platecheck");
            API.Functions.AddActionToButton(Actions.Panic.Main, "panic");
            API.Functions.AddActionToButton(Actions.RunPedName.Main, Actions.RunPedName.IsAvailable, "pedcheck");
            API.Functions.AddActionToButton(Actions.EndCall.Main, Functions.IsCalloutRunning, "endcall");
            API.Functions.AddActionToButton(Actions.K9.Main, Actions.K9.available, "k9");
            buttonspassed = true;
            Game.LogTrivial("All PoliceSmartRadio default buttons have been assigned actions.");            
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

        internal static void Initialise()
        {
            Game.LogTrivial("PoliceSmartRadio, developed by Albo1125, has been loaded successfully!");
            GameFiber.StartNew(delegate
            {                
                GameFiber.Wait(6000);
                Game.DisplayNotification("~b~PoliceSmartRadio~s~, developed by ~b~Albo1125, ~s~has been loaded ~g~successfully.");

            });
            MainLogic();
        }        
    }
}
