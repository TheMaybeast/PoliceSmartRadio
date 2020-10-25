using Albo1125.Common.CommonLibrary;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliceSmartRadio.Actions
{
    // Unfinished as of the moment
    internal static class K9
    {
        private static bool onlyDuringTrafficStops = false;
        private static bool k9Busy = false;

        private static Blip k9VehicleBlip;
        private static Model k9Model = "a_c_shepherd";
        private static Vehicle suspectVehicle;
        private static Vehicle k9Vehicle;
        private static Ped k9;
        private static Ped dogHandler;
        private static Vector3 k9OutOfVehicle;
        private static int k9SeatIndex;

        public static bool available()
        {
            return !k9Busy && (!onlyDuringTrafficStops || Functions.GetCurrentPullover() != null);
        }

        public static void Main()
        {
            if (!available()) { return; }
            GameFiber.StartNew(k9MainLogic);
        }

        private static void k9MainLogic()
        {            
            suspectVehicle = Game.LocalPlayer.Character.IsInAnyVehicle(false) ? InCar() : OnFoot();
            if (!suspectVehicle.Exists()) { Game.DisplaySubtitle("No vehicle detected."); return; }
            Game.DisplayNotification("~b~" + PoliceSmartRadio.PlayerName + ": ~s~Requesting K-9 backup.");
            k9Vehicle = Functions.RequestBackup(Game.LocalPlayer.Character.Position, LSPD_First_Response.EBackupResponseType.Code2, LSPD_First_Response.EBackupUnitType.LocalUnit, string.Empty, false, false, 1);
            GameFiber.Yield();
            dogHandler = k9Vehicle.Driver;

            k9Vehicle.IsPersistent = true;

            dogHandler.MakeMissionPed();

            k9 = new Ped(k9Model, Vector3.Zero, 0);
            k9.MakeMissionPed();
            
            if (k9Vehicle.FreePassengerSeatsCount > 2 && k9Vehicle.GetFreeSeatIndex(1, 2) != null)
            {
                k9.WarpIntoVehicle(k9Vehicle, k9Vehicle.GetFreeSeatIndex(1, 2).GetValueOrDefault());
            }
            else
            {
                k9.WarpIntoVehicle(k9Vehicle, 2);
            }
            k9VehicleBlip = k9Vehicle.AttachBlip();
            k9VehicleBlip.Color = System.Drawing.Color.Blue;
            k9VehicleBlip.Flash(1000, 20000);
            k9SeatIndex = k9.SeatIndex;
            while (k9Vehicle.Speed != 0 && k9Vehicle.DistanceTo(Game.LocalPlayer.Character.Position) > 20f)
                GameFiber.Yield();
            GameFiber.Sleep(5000);
            dogHandler.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
            k9.Tasks.LeaveVehicle(LeaveVehicleFlags.None).WaitForCompletion(6000);
            k9OutOfVehicle = k9.Position;
            dogHandler.Tasks.AchieveHeading(ExtensionMethods.CalculateHeadingTowardsEntity(dogHandler, suspectVehicle)).WaitForCompletion(1000);
            GameFiber.Wait(1000);
            k9VehicleBlip.Delete();
            inspectVehicle();
            dogHandler.Tasks.EnterVehicle(k9Vehicle, 5000, -1).WaitForCompletion();
            dogHandler.Dismiss();
            k9Vehicle.Dismiss();

        }
        private static TupleList<float, float> K9_SuspectCarOffsets = new TupleList<float, float>()
        {
            new Tuple<float, float>(-2.3f, 0),
            new Tuple<float, float>(2, -100),
            new Tuple<float, float>(2.3f, 0),
            new Tuple<float, float>(2, 100),

        };
        private static void inspectVehicle()
        {
            Game.DisplaySubtitle("~b~K9 Handler: We'll let the dog do its thing. Please step back for a minute.");
            bool hasIndicated = false;

            dogHandler.Tasks.GoToOffsetFromEntity(suspectVehicle, -2.3f, 0, 2.5f).WaitForCompletion(10000);
            k9.Tasks.GoToOffsetFromEntity(suspectVehicle, -2.3f, 0, 2.5f).WaitForCompletion(10000);
            foreach (Tuple<float, float> off in K9_SuspectCarOffsets)
            {
                dogHandler.Tasks.GoToOffsetFromEntity(suspectVehicle, off.Item1, off.Item2, 2.5f).WaitForCompletion(3000);
                k9.Tasks.GoToOffsetFromEntity(suspectVehicle, off.Item1, off.Item2, 2.5f).WaitForCompletion(3000);
                if ((suspectVehicle.Metadata.hasNarcotics == 1 && PoliceSmartRadio.rnd.Next(2) == 0) || PoliceSmartRadio.rnd.Next(20) == 0 || suspectVehicle.HasDriver ? (Functions.IsPedCarryingContraband(suspectVehicle.Driver) && PoliceSmartRadio.rnd.Next(2) == 0) : false)
                {
                    k9.Tasks.PlayAnimation("creatures@rottweiler@indication@", "indicate_high", 8.0f, AnimationFlags.None).WaitForCompletion();
                    hasIndicated = true;
                }
            }
            if ((suspectVehicle.Metadata.hasNarcotics == 1 && PoliceSmartRadio.rnd.Next(2) == 0) || PoliceSmartRadio.rnd.Next(20) == 0 || (suspectVehicle.HasDriver ? Functions.IsPedCarryingContraband(suspectVehicle.Driver) : false && PoliceSmartRadio.rnd.Next(2) == 0) && !hasIndicated)
            {
                k9.Tasks.PlayAnimation("creatures@rottweiler@indication@", "indicate_high", 8.0f, AnimationFlags.None).WaitForCompletion();
                hasIndicated = true;
            }
            dogHandler.Tasks.FollowNavigationMeshToPosition(k9OutOfVehicle, k9Vehicle.Heading, 2.5f).WaitForCompletion(8000);
            k9.Tasks.FollowNavigationMeshToPosition(k9OutOfVehicle, k9Vehicle.Heading, 2.5f).WaitForCompletion(8000);
            k9.WarpIntoVehicle(k9Vehicle, k9SeatIndex);
            Game.DisplaySubtitle("~b~Handler: The dog " + (hasIndicated ? "indicated" : "did not indicate") + " for drugs.");
        }

        private static Vehicle InCar()
        {
            if (Functions.GetCurrentPullover() != null)
            {
                return Functions.GetPulloverSuspect(Functions.GetCurrentPullover()).CurrentVehicle;
            }
            Vector3 offSetPos = Game.LocalPlayer.Character.CurrentVehicle.GetOffsetPosition(Vector3.RelativeFront * 9f);
            Vehicle[] vehicleList = Game.LocalPlayer.Character.GetNearbyVehicles(10);
            foreach (Vehicle veh in vehicleList)
            {
                if (!veh.HasSiren)
                {
                    if (Vector3.Distance(offSetPos, veh.Position) < 7.5f)
                    {
                        return veh;
                    }
                }
            }
            return null;
        }
        private static Vehicle OnFoot()
        {
            if (Functions.GetCurrentPullover() != null)
            {
                return Functions.GetPulloverSuspect(Functions.GetCurrentPullover()).CurrentVehicle;
            }
            Vector3 offSetPos = Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeFront * 2.5f);
            Vehicle[] vehicleList = Game.LocalPlayer.Character.GetNearbyVehicles(10);
            foreach (Vehicle veh in vehicleList)
            {
                if (!veh.HasSiren)
                {
                    if (Vector3.Distance(offSetPos, veh.Position) < 4.0f)
                    {
                        return veh;
                    }
                }
            }
            return null;
        }
    }
}
