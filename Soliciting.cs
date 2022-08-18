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
            new Vector3(806.16f, -1096.57f, 28.72f),
            new Vector3(-400.30f, 110.85f, 65.45f),
            new Vector3(-116.88f, 237.31f, 97.13f),
            new Vector3(16.32f, 239.87f, 109.56f),
            new Vector3(342.65f, 124.73f, 103f),
            new Vector3(173.15f, -150.19f, 46.47f),
            new Vector3(100.98f, -118.22f, 57.19f),
            new Vector3(-558.01f, 141.30f, 63.01f),
            new Vector3(-1453.02f, -479.08f, 34.71f),
            new Vector3(-1300.36f, -529.40f, 32.88f),
            new Vector3(-1245.58f, -592.32f, 27.18f),
            new Vector3(-1262.56f, -1058.08f, 8.35f),
            new Vector3(-791.40f, -1111.96f, 10.65f),
            new Vector3(-946.53f, -1199.52f, 5.17f),
            new Vector3(509.13f, -1696.46f, 29.21f),
            new Vector3(465.88f, -1815.41f, 27.80f),
            new Vector3(367.49f, -1914.03f, 24.56f),
            new Vector3(235.87f, -2056.80f, 17.95f),
            new Vector3(-149.98f, -1728.44f, 30.03f),
            new Vector3(59.22f, -1890.16f, 21.58f),
            new Vector3(14.78f, -1663.92f, 29.25f)
        };
        public Vector3 prostituteLocation;
        public Vector3 vehicleLocation;

        public List<PedHash> pedHashes = new List<PedHash>()
        {
            PedHash.Hooker01SFY,
            PedHash.Hooker02SFY,
            PedHash.Hooker03SFY
        };

        public Ped prostitute, driver;
        public PedData prostituteData;

        public Vehicle veh;
        public float heading;
        public int unk1;

        public bool undercoverMission = false;
        public bool followMission = false;

        public int rollChance;
        public Soliciting()
        {
            prostituteLocation = areas.SelectRandom();
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
            bool foundPos = API.GetNthClosestVehicleNodeWithHeading(prostituteLocation.X, prostituteLocation.Y, prostituteLocation.Z, 3, ref vehicleLocation, ref heading, ref unk1, 9, 3.0f, 2.5f);
            while (!foundPos)
            {
                BaseScript.Delay(10);
                foundPos = API.GetNthClosestVehicleNodeWithHeading(prostituteLocation.X, prostituteLocation.Y, prostituteLocation.Z, 3, ref vehicleLocation, ref heading, ref unk1, 9, 3.0f, 2.5f);
            }
            //Get boundry
            bool isRoadside = API.GetRoadBoundaryUsingHeading(vehicleLocation.X, vehicleLocation.Y, vehicleLocation.Z, heading, ref vehicleLocation);
            if (!isRoadside)
            {
                API.GetNthClosestVehicleNodeWithHeading(prostituteLocation.X, prostituteLocation.Y, prostituteLocation.Z, 3, ref vehicleLocation, ref heading, ref unk1, 9, 3.0f, 2.5f);
                isRoadside = API.GetRoadBoundaryUsingHeading(vehicleLocation.X, vehicleLocation.Y, vehicleLocation.Z, heading, ref vehicleLocation);
            }

            InitInfo(vehicleLocation);
            ShortName = "Soliciting";
            StartDistance = 165f;
            ResponseCode = 1;
            CalloutDescription = $"We have reports of some soliciting happening in the {World.GetZoneLocalizedName(prostituteLocation)} area.";
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

            prostitute = await SpawnPed(pedHashes.SelectRandom(), prostituteLocation);
            prostituteData = await prostitute.GetData();
            prostitute.AttachBlip();

            prostitute.BlockPermanentEvents = true;
            prostitute.AlwaysKeepTask = true;

            ProstituteQuestions();

            if(rng.Next(0,101) >= 50)
            {
                API.TaskStartScenarioInPlace(prostitute.Handle, "WORLD_HUMAN_PROSTITUTE_HIGH_CLASS", -1, true);
            }
            else
            {
                API.TaskStartScenarioInPlace(prostitute.Handle, "WORLD_HUMAN_PROSTITUTE_LOW_CLASS", -1, true);
            }

            if(undercoverMission)
            {
                prostitute.AttachedBlip.Color = BlipColor.Blue;

                veh = await SpawnVehicle(RandomUtils.GetVehicleHashesForClass(VehicleClass.Coupes).SelectRandom(), vehicleLocation, heading);
                driver = await SpawnPed(RandomUtils.GetRandomPed(), veh.Position);

                driver.BlockPermanentEvents = true;
                driver.AlwaysKeepTask = true;
                driver.AttachBlip();
                driver.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);
                driver.RelationshipGroup = (RelationshipGroup)"AMBIENT_GANG_WEICHENG";
                prostitute.RelationshipGroup = (RelationshipGroup)"AMBIENT_GANG_WEICHENG";

                DriverQuestions();

                while (World.GetDistance(Game.PlayerPed.Position, prostitute.Position) > 65f) { await BaseScript.Delay(50); }
                ShowNetworkedNotification("You'll be able to ~y~listen~s~ to the conversation between the undercover cop and the prostitute.", "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "~y~Callout Information~s~", 1f);
                veh.SoundHorn(250);
                await BaseScript.Delay(250);
                veh.SoundHorn(250);

                Vector3 passengerDoor = API.GetWorldPositionOfEntityBone(veh.Handle, API.GetEntityBoneIndexByName(veh.Handle, "seat_pside_f"));
                API.TaskFollowNavMeshToCoord(prostitute.Handle, passengerDoor.X, passengerDoor.Y, passengerDoor.Z, 0.25f, -1, 0.001f, true, 0f);

                await BaseScript.Delay(5000);

                while(prostitute.IsWalking) { await BaseScript.Delay(50); }

                prostitute.Task.TurnTo(driver);
                driver.Task.LookAt(prostitute);

                await BaseScript.Delay(750);
                API.SetVehicleDoorsLocked(veh.Handle, 0);
                API.RollDownWindow(veh.Handle, (int)VehicleWindowIndex.FrontRightWindow);
                API.TaskStartScenarioInPlace(prostitute.Handle, "PROP_HUMAN_BUM_SHOPPING_CART", -1, true);
                ShowDialog("~b~Undercover Cop~s~: Hey there, looking for a good time?", 6500, 1f);

                int currentTime = API.GetGameTimer();

                while(API.GetGameTimer() - currentTime < 6500) { await BaseScript.Delay(50); }
                ShowDialog("~r~Driver~s~: I sure am! Get in, let's go find a spot.", 6500, 1f);

                prostitute.Task.EnterVehicle(veh,VehicleSeat.Passenger);

                while(!prostitute.IsInVehicle()) { await BaseScript.Delay(50); }

                driver.Task.CruiseWithVehicle(veh, 10f, 395);

                await BaseScript.Delay(6500);
                ShowDialog("~b~Undercover Cop~s~: You've got ~g~cash~s~ right?", 6500, 1f);

                await BaseScript.Delay(6500);
                ShowDialog("~r~Driver~s~: I've got ~g~$250~s~. Will that be enough?", 3500, 1f);

                await BaseScript.Delay(3500);
                ShowDialog("~b~Undercover Cop~s~: Ya that's enough, hand over the ~g~money~s~ and lets find a spot to park.", 6500, 1f);

                await BaseScript.Delay(6500);
                ShowDialog("~r~Driver~s~: Here you go. ~y~*sounds of bills being counted*~s~", 6500, 1f);

                await BaseScript.Delay(6500);
                ShowDialog("~b~Undercover Cop~s~: It's all here thanks. That will get you the ~g~Dirty Sanchez~s~ special.", 8500, 1f);

                await BaseScript.Delay(8500);
                ShowDialog("~r~Driver~s~: ~g~Dirty Sanchez~s~ sounds amazing! Do you know any places to park around here?", 6500, 1f);

                await BaseScript.Delay(6500);
                ShowDialog("~b~Undercover Cop~s~: There's a spot right around the corner that is perfect.", 6500, 1f);
                bool onTrafficStop = Utilities.IsPlayerPerformingTrafficStop();
                while (!onTrafficStop) { onTrafficStop = Utilities.IsPlayerPerformingTrafficStop(); await BaseScript.Delay(10); }
                ShowDialog("~r~Driver~s~: Oh shit! It's the cops! What are we going to do?!", 6500, 1f);

                await BaseScript.Delay(6500);
                ShowDialog("~b~Undercover Cop~s~: You're under arrest for the solicitation of a prostitute! Exit the vehicle and put your hands in the air!", 6500, 1f);

                await BaseScript.Delay(2500);
                driver.Task.LeaveVehicle();
                prostitute.Task.LeaveVehicle();
                await BaseScript.Delay(500);
                driver.Task.HandsUp(-1);
                prostitute.Weapons.Give(WeaponHash.CombatPistol, 50, true, true);
                prostitute.Task.AimAt(driver, -1);

                await BaseScript.Delay(3500);
                ShowDialog("~r~Driver~s~: Don't shoot!", 3500, 1f);

                await BaseScript.Delay(3500);
                ShowDialog("~b~Undercover Cop~s~: Cuff him ~b~Officer~s~.", 4500, 1f);

                while(!driver.IsCuffed) { await BaseScript.Delay(10); }
                prostitute.Task.ClearAllImmediately();
                prostitute.Task.Wait(-1);

                ShowDialog("~b~Undercover Cop~s~: Officer come speak to me when you have a minute.", 10000, 1f);
            }

            if(followMission)
            {
                veh = await SpawnVehicle(RandomUtils.GetVehicleHashesForClass(VehicleClass.Coupes).SelectRandom(), vehicleLocation, heading);
                driver = await SpawnPed(RandomUtils.GetRandomPed(), veh.Position);
                driver.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);
                driver.AttachBlip();
                driver.RelationshipGroup = (RelationshipGroup)"AMBIENT_GANG_WEICHENG";
                prostitute.RelationshipGroup = (RelationshipGroup)"AMBIENT_GANG_WEICHENG";

                DriverQuestions();

                while (World.GetDistance(Game.PlayerPed.Position, prostitute.Position) > 55f) { await BaseScript.Delay(50); }
                veh.SoundHorn(250);
                await BaseScript.Delay(250);
                veh.SoundHorn(250);

                Vector3 passengerDoor = API.GetWorldPositionOfEntityBone(veh.Handle, API.GetEntityBoneIndexByName(veh.Handle, "seat_pside_f"));
                API.TaskFollowNavMeshToCoord(prostitute.Handle, passengerDoor.X, passengerDoor.Y, passengerDoor.Z, 0.25f, -1, 0.001f, true, 0f);

                await BaseScript.Delay(5000);

                while (prostitute.IsWalking) { await BaseScript.Delay(50); }

                prostitute.Task.TurnTo(driver);
                driver.Task.LookAt(prostitute);

                await BaseScript.Delay(750);
                API.SetVehicleDoorsLocked(veh.Handle, 0);
                API.RollDownWindow(veh.Handle, (int)VehicleWindowIndex.FrontRightWindow);
                API.TaskStartScenarioInPlace(prostitute.Handle, "PROP_HUMAN_BUM_SHOPPING_CART", -1, true);
                ShowDialog("~r~Prostitute~s~: Hey there, looking for a good time?", 6500, 1f);

                //I have no idea why I decied to wait 6500 ms this way instead of the Delay function
                int currentTime = API.GetGameTimer();
                while (API.GetGameTimer() - currentTime < 6500) { await BaseScript.Delay(50); }
                ShowDialog("~r~Prostitute~s~: I sure am! Get in, let's go find a spot.", 6500, 1f);

                prostitute.Task.EnterVehicle(veh, VehicleSeat.Passenger);

                while (!prostitute.IsInVehicle()) { await BaseScript.Delay(50); }

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
                prostitute.RelationshipGroup = null;
            }
            catch { }
        }

        public async void CalloutInfo()
        {
            ShowNetworkedNotification("An ~b~undercover officer~s~ has requested your help in a ~y~Prostitution Sting~s~.", "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "~y~Callout Information~s~", 1f);

            await BaseScript.Delay(7000);

            ShowNetworkedNotification("~y~Listen~s~ for the phrase ~g~Dirty Sanchez~s~ before making the arrest.", "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "~y~Callout Information~s~", 1f);

            await Task.FromResult(0);
        }
        
        public void DriverQuestions()
        {
            if(undercoverMission)
            {
                PedQuestion q1 = new PedQuestion();
                q1.Question = "You're being arrested for Solicitation of Prositution.";
                q1.Answers = new List<string>()
                {
                    "Was this a setup?!",
                    "This is bullshit, I was just trying to have a good time!",
                    "I want to speak with my lawyer.",
                    "This is entrapment!"
                };

                PedQuestion[] questions = new PedQuestion[] {q1};
                AddPedQuestions(driver, questions);
            }

            if (followMission)
            {
                PedQuestion q1 = new PedQuestion();
                q1.Question = "How do you know this woman?";
                q1.Answers = new List<string>()
                {
                    "I just give her a ride every now and then.",
                    "We met through a friend.",
                    "She is my massage therapist.",
                    "She's my mom's friend. Just giving her a ride home."
                };

                PedQuestion q2 = new PedQuestion();
                q2.Question = "How long have your known her?";
                q2.Answers = new List<string>()
                {
                    String.Format("About {0} months.", rng.Next(2,10))
                };

                PedQuestion q3 = new PedQuestion();
                q3.Question = "Can you tell me her name?";
                q3.Answers = new List<string>()
                {
                    "I'm not sure. We don't know each other that well.",
                    $"It's {prostituteData.FirstName}.",
                    "I think it's Donna.",
                    "I forget... I have a really bad memory."
                };

                PedQuestion q4 = new PedQuestion();
                q4.Question = "~y~Attempt Confession~s~";
                q4.Answers = new List<string>()
                {
                    "~y~*driver denies all accusations*",
                    "I want a lawyer.",
                    "Fuck off pig...",
                    "~y~*driver confesses to soliciting a prostitute*"
                };

                PedQuestion[] questions = new PedQuestion[] { q1, q2, q3, q4 };
                AddPedQuestions(driver, questions);
            }
        }

        public void ProstituteQuestions()
        {
            if (!undercoverMission && !followMission)
            {
                PedQuestion q1 = new PedQuestion();
                q1.Question = "We've had some reports of Soliciting in the area.";
                q1.Answers = new List<string>()
                {
                    "Why are you telling me this?",
                    "What's soliciting?",
                    "Sounds illegal, I'll keep my eyes open for any soliciting going on.",
                    "The oldest profession is illegal? Since when?"
                };

                PedQuestion q2 = new PedQuestion();
                q2.Question = "Can you tell me what you're doing here?";
                q2.Answers = new List<string>()
                {
                    "I'm waiting for the bus.",
                    "Just hanging out on the sidewalk. Is that illegal?",
                    "Minding my own damn business like you should.",
                    "Waiting for a friend to come pick me up."
                };

                PedQuestion[] questions = new PedQuestion[] { q1, q2 };
                AddPedQuestions(prostitute, questions);
            }
        }
    }
}
