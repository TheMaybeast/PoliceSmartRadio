using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliceSmartRadio.Actions
{
    internal static class RunPedName
    {
        public static bool IsAvailable()
        {
            return GetNearestValidPed(allowPursuitPeds:true, subtitleDisplayTime:-1).Exists();
        }
        public static bool vc_main() { Main(); return true; }
        public static void Main()
        {
            Ped p = GetNearestValidPed();
            if (p.Exists())
            {
                if (Functions.HasPedBeenIdentified(p))
                {
                    Persona pers = Functions.GetPersonaForPed(p);
                    Game.DisplayNotification("~b~" + PoliceSmartRadio.PlayerName + "~s~: Dispatch, could you run a person check through for me? It's ~b~" + pers.FullName + "~s~, born on ~b~" + pers.Birthday.ToShortDateString() + "~s~.");
                    GameFiber.Wait(4000);
                    Functions.DisplayPedId(p, true);
                }
                else
                {
                    Game.DisplayNotification("~b~You need to identify the person before running a person check!");
                }
            }
        }

        private static Ped GetNearestValidPed(float Radius = 3.5f, bool allowPursuitPeds = false, int subtitleDisplayTime = 3000)
        {
            if (Game.LocalPlayer.Character.IsInAnyVehicle(false) || Game.LocalPlayer.Character.GetNearbyPeds(1).Length == 0) { return null; }
            Ped nearestped = Game.LocalPlayer.Character.GetNearbyPeds(1)[0];

            if (nearestped.RelationshipGroup == "COP")
            {
                if (Game.LocalPlayer.Character.GetNearbyPeds(2).Length >= 2) { nearestped = Game.LocalPlayer.Character.GetNearbyPeds(2)[1]; }
                if (nearestped.RelationshipGroup == "COP")
                {
                    return null;
                }
            }
            if (Vector3.Distance(Game.LocalPlayer.Character.Position, nearestped.Position) > Radius) { Game.DisplaySubtitle("Get closer to the ped", subtitleDisplayTime); return null; }
            if (!allowPursuitPeds && Functions.GetActivePursuit() != null)
            {
                if (Functions.GetPursuitPeds(Functions.GetActivePursuit()).Contains(nearestped)) { return null; }
            }
            if (!nearestped.IsHuman) { Game.DisplaySubtitle("Ped isn't human...", subtitleDisplayTime); return null; }
            if (Functions.IsPedGettingArrested(nearestped) && !Functions.IsPedArrested(nearestped)) { return null; }
            return nearestped;
        }
    }
}
