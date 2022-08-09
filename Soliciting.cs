using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalloutPack
{
    [CalloutProperties("Soliciting", "HuskyNinja", "v1.0")]
    internal class Soliciting : Callout
    {
        public readonly Random rng = new Random();
        public List<Vector3> areas = new List<Vector3>()
        {
            new Vector3(799.82f, -1015.11f, 26.03f),
            new Vector3(806.16f, -1096.57f, 28.72f)
        };
        public Vector3 suspectLocation;
        public Vector3 vehicleLocation;

        public List<PedHash> pedHashes = new List<PedHash>()
        {
            PedHash.Hooker01SFY,
            PedHash.Hooker02SFY,
            PedHash.Hooker03SFY
        };

        public Ped suspect, driver;
        public PedData suspectData;

        public Vehicle veh;
        public float heading;
        public int unk1;

        public bool undercoverMission = false;
        public bool followMission = false;

        public int rollChance;
        public Soliciting()
        {
            suspectLocation = areas.SelectRandom();
            rollChance = rng.Next(0, 101);

            if(rollChance >= 60)
            {
                undercoverMission = true;
            }
            
            if(rollChance <= 35)
            {
                followMission = true;
            }

            //Find road spot on road
            bool foundPos = API.GetNthClosestVehicleNodeWithHeading(suspectLocation.X, suspectLocation.Y, suspectLocation.Z, 3, ref vehicleLocation, ref heading, ref unk1, 9, 3.0f, 2.5f);
            while (!foundPos)
            {
                BaseScript.Delay(10);
                foundPos = API.GetNthClosestVehicleNodeWithHeading(suspectLocation.X, suspectLocation.Y, suspectLocation.Z, 3, ref vehicleLocation, ref heading, ref unk1, 9, 3.0f, 2.5f);
            }
            //Get boundry
            bool isRoadside = API.GetRoadBoundaryUsingHeading(vehicleLocation.X, vehicleLocation.Y, vehicleLocation.Z, heading, ref vehicleLocation);
            if (!isRoadside)
            {
                foundPos = API.GetNthClosestVehicleNodeWithHeading(suspectLocation.X, suspectLocation.Y, suspectLocation.Z, 3, ref vehicleLocation, ref heading, ref unk1, 9, 3.0f, 2.5f);
                isRoadside = API.GetRoadBoundaryUsingHeading(vehicleLocation.X, vehicleLocation.Y, vehicleLocation.Z, heading, ref vehicleLocation);
            }

            InitInfo(vehicleLocation);
            ShortName = "Soliciting";
            StartDistance = 165f;
            ResponseCode = 1;
            CalloutDescription = $"We have reports of some soliciting happening in the {World.GetZoneLocalizedName(suspectLocation)} area.";
        }

        public override async Task<bool> CheckRequirements()
        {
            bool startCallout = false;
            if(World.CurrentDayTime.Hours >= 21 || World.CurrentDayTime.Hours <= 5)
            {
                startCallout = true;
            }

            await Task.FromResult(0);

            return startCallout;
        }
        public override async Task OnAccept()
        {
            InitBlip();
            UpdateData();
            if (undercoverMission) { CalloutInfo(); }

            await Task.FromResult(0);
        }
        public override async void OnStart(Ped closest)
        {
            base.OnStart(closest);

            suspect = await SpawnPed(pedHashes.SelectRandom(), suspectLocation);
            suspectData = await suspect.GetData();
            suspect.AttachBlip();

            SuspectQuestions();

            suspect.BlockPermanentEvents = true;
            suspect.AlwaysKeepTask = true;

            if(rng.Next(0,101) >= 50)
            {
                API.TaskStartScenarioInPlace(suspect.Handle, "WORLD_HUMAN_PROSTITUTE_HIGH_CLASS", -1, true);
            }
            else
            {
                API.TaskStartScenarioInPlace(suspect.Handle, "WORLD_HUMAN_PROSTITUTE_LOW_CLASS", -1, true);
            }

            if(undercoverMission)
            {
                suspect.AttachedBlip.Color = BlipColor.Blue;

                veh = await SpawnVehicle(RandomUtils.GetVehicleHashesForClass(VehicleClass.Coupes).SelectRandom(), vehicleLocation, heading);
                driver = await SpawnPed(RandomUtils.GetRandomPed(), veh.Position);

                driver.BlockPermanentEvents = true;
                driver.AlwaysKeepTask = true;
                driver.AttachBlip();

                driver.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);

                driver.RelationshipGroup = (RelationshipGroup)"AMBIENT_GANG_WEICHENG";
                suspect.RelationshipGroup = (RelationshipGroup)"AMBIENT_GANG_WEICHENG";

                while (World.GetDistance(Game.PlayerPed.Position, suspect.Position) > 65f) { await BaseScript.Delay(50); }
                ShowNetworkedNotification("You'll be able to ~y~listen~s~ to the conversation between the undercover cop and the customer.", "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "~y~Callout Information~s~", 1f);
                veh.SoundHorn(250);
                await BaseScript.Delay(250);
                veh.SoundHorn(250);

                Vector3 passengerDoor = API.GetWorldPositionOfEntityBone(veh.Handle, API.GetEntityBoneIndexByName(veh.Handle, "seat_pside_f"));
                API.TaskFollowNavMeshToCoord(suspect.Handle, passengerDoor.X, passengerDoor.Y, passengerDoor.Z, 0.25f, -1, 0.001f, true, 0f);

                await BaseScript.Delay(5000);

                while(suspect.IsWalking) { await BaseScript.Delay(50); }

                suspect.Task.TurnTo(driver);
                driver.Task.LookAt(suspect);

                await BaseScript.Delay(750);
                API.SetVehicleDoorsLocked(veh.Handle, 0);
                API.RollDownWindow(veh.Handle, (int)VehicleWindowIndex.FrontRightWindow);
                API.TaskStartScenarioInPlace(suspect.Handle, "PROP_HUMAN_BUM_SHOPPING_CART", -1, true);
                ShowDialog("~b~Undercover Cop~s~: Hey there, looking for a good time?", 6500, 1f);

                int currentTime = API.GetGameTimer();

                while(API.GetGameTimer() - currentTime < 6500) { await BaseScript.Delay(50); }
                ShowDialog("~r~Suspect~s~: I sure am! Get in, let's go find a spot.", 6500, 1f);

                suspect.Task.EnterVehicle(veh,VehicleSeat.Passenger);

                while(!suspect.IsInVehicle()) { await BaseScript.Delay(50); }

                driver.Task.CruiseWithVehicle(veh, 10f, 395);

                await BaseScript.Delay(6500);
                ShowDialog("~b~Undercover Cop~s~: You've got ~g~cash~s~ right?", 6500, 1f);

                await BaseScript.Delay(6500);
                ShowDialog("~r~Suspect~s~: I've got ~g~$250~s~. Will that be enough?", 3500, 1f);

                await BaseScript.Delay(3500);
                ShowDialog("~b~Undercover Cop~s~: Ya that's enough, hand over the ~g~money~s~ and lets find a spot to park.", 6500, 1f);

                await BaseScript.Delay(6500);
                ShowDialog("~r~Suspect~s~: Here you go. ~y~*sounds of bills being counted*~s~", 6500, 1f);

                await BaseScript.Delay(6500);
                ShowDialog("~b~Undercover Cop~s~: It's all here thanks. That will get you the ~g~Dirty Sanchez~s~ special.", 8500, 1f);

                await BaseScript.Delay(8500);
                ShowDialog("~r~Suspect~s~: ~g~Dirty Sanchez~s~ sounds amazing! Do you know any places to park around here?", 6500, 1f);

                await BaseScript.Delay(6500);
                ShowDialog("~b~Undercover Cop~s~: There's a spot right around the corner that is perfect.", 6500, 1f);
                bool onTrafficStop = Utilities.IsPlayerPerformingTrafficStop();
                while (!onTrafficStop) { onTrafficStop = Utilities.IsPlayerPerformingTrafficStop(); await BaseScript.Delay(10); }
                ShowDialog("~r~Suspect~s~: Oh shit! It's the cops! What are we going to do?!", 6500, 1f);

                await BaseScript.Delay(6500);
                ShowDialog("~b~Undercover Cop~s~: You're under arrest for the solicitation of a prostitute! Exit the vehicle and put your hands in the air!", 6500, 1f);

                await BaseScript.Delay(2500);
                driver.Task.LeaveVehicle();
                suspect.Task.LeaveVehicle();
                await BaseScript.Delay(500);
                driver.Task.HandsUp(-1);
                suspect.Weapons.Give(WeaponHash.CombatPistol, 50, true, true);
                suspect.Task.AimAt(driver, -1);

                await BaseScript.Delay(3500);
                ShowDialog("~s~Suspect~s~: Don't shoot!", 6500, 1f);

                while(!driver.IsCuffed) { await BaseScript.Delay(10); }
                suspect.Task.ClearAllImmediately();
                suspect.Task.Wait(-1);

                ShowDialog("~b~Undercover Cop~s~: Officer come speak to me when you have a minute.", 10000, 1f);
            }

            if(followMission)
            {
                veh = await SpawnVehicle(RandomUtils.GetVehicleHashesForClass(VehicleClass.Coupes).SelectRandom(), vehicleLocation, heading);
                driver = await SpawnPed(RandomUtils.GetRandomPed(), veh.Position);
                driver.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);
                driver.AttachBlip();
                driver.RelationshipGroup = (RelationshipGroup)"AMBIENT_GANG_WEICHENG";
                suspect.RelationshipGroup = (RelationshipGroup)"AMBIENT_GANG_WEICHENG";

                while (World.GetDistance(Game.PlayerPed.Position, suspect.Position) > 25f) { await BaseScript.Delay(50); }
                veh.SoundHorn(250);
                await BaseScript.Delay(250);
                veh.SoundHorn(250);

                Vector3 passengerDoor = API.GetWorldPositionOfEntityBone(veh.Handle, API.GetEntityBoneIndexByName(veh.Handle, "seat_pside_f"));
                API.TaskFollowNavMeshToCoord(suspect.Handle, passengerDoor.X, passengerDoor.Y, passengerDoor.Z, 0.25f, -1, 0.001f, true, 0f);

                await BaseScript.Delay(5000);

                while (suspect.IsWalking) { await BaseScript.Delay(50); }

                suspect.Task.TurnTo(driver);
                driver.Task.LookAt(suspect);

                await BaseScript.Delay(750);
                API.SetVehicleDoorsLocked(veh.Handle, 0);
                API.RollDownWindow(veh.Handle, (int)VehicleWindowIndex.FrontRightWindow);
                API.TaskStartScenarioInPlace(suspect.Handle, "PROP_HUMAN_BUM_SHOPPING_CART", -1, true);
                ShowDialog("~r~Prostitute~s~: Hey there, looking for a good time?", 6500, 1f);

                int currentTime = API.GetGameTimer();

                while (API.GetGameTimer() - currentTime < 6500) { await BaseScript.Delay(50); }
                ShowDialog("~r~Suspect~s~: I sure am! Get in, let's go find a spot.", 6500, 1f);

                suspect.Task.EnterVehicle(veh, VehicleSeat.Passenger);

                while (!suspect.IsInVehicle()) { await BaseScript.Delay(50); }

                driver.Task.CruiseWithVehicle(veh, 10f, 395);
            }

            await BaseScript.Delay(0);
        }

        public override void OnCancelBefore()
        {
            base.OnCancelBefore();

            try
            {
                driver.RelationshipGroup = null;
                suspect.RelationshipGroup = null;
            }
            catch { }
        }

        public async void CalloutInfo()
        {
            ShowNetworkedNotification("An undercover officer has request your help in a ~y~Prostitution Sting~s~.", "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "~y~Callout Information~s~", 1f);

            await BaseScript.Delay(7000);

            ShowNetworkedNotification("~y~Listen~s~ for the phrase ~g~Dirty Sanchez~s~ before making the arrest.", "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "~y~Callout Information~s~", 1f);

            await Task.FromResult(0);
        }

        public void SuspectQuestions()
        {
            PedQuestion q1 = new PedQuestion();

            if(undercoverMission)
            {
                q1.Question = "Great work out there officer.";
                q1.Answers = new List<string>()
                {

                };
            }
            
            if(followMission)
            {
                q1.Question = "Want to tell me what's going on here?";
                q1.Answers = new List<string>()
                {

                };
            }

            if(!undercoverMission && !followMission)
            {
                q1.Question = "We've had reports of Prostitutes working in this area.";
                q1.Answers = new List<string>()
                {

                };
            }
        }
    }
}
