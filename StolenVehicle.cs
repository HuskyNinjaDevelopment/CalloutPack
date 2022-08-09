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
    [CalloutProperties("Stolen Vehicle", "HuskyNinja", "1.0")]
    internal class StolenVehicle : Callout
    {
        public readonly Random rng = new Random();
        public List<Vector3> victimLocations = new List<Vector3>()
        {
                new Vector3(-50.31f, -1837.34f, 27f), //Grove Street
                new Vector3(-34.04f, -1224.79f, 29f), //Under Olympic Freeway
                new Vector3(441.3f, -1155.27f, 30f), //Mission Row PL
                new Vector3(235.55f, -796.6f, 31f), //Pillbox Hill PL
                new Vector3(277.33f, -336.02f, 45.5f), //Alta PL
                new Vector3(-742.11f, -70.14f, 42.5f), //Rockford Hills PL
                new Vector3(-98.46f, 87.83f, 72.26f), //WV PL 1
                new Vector3(619.05f, 103.82f, 92.64f), //V PL 1
                new Vector3(642.90f, 200.29f, 97f), //V PL 2
                new Vector3(378.05f, 283.69f, 103.6f), //DV PL
                new Vector3(-203.66f, 309.59f, 96.71f), // WV PL 2
                new Vector3(-338.43f, 273.1f, 86.05f), //WV PL 3
                new Vector3(-568.40f, 323.69f, 84.99f), //WV PL 4
                new Vector3(-1672.83f, 79.07f, 64f), //Richman PL
                new Vector3(-1636.14f, -234.45f, -54.62f), //PB PL 1
                new Vector3(-2017.90f, -477.43f, 13.25f), //PB PL 2
                new Vector3(-1733.94f, -720.5f, 11.33f), //DP PL 1
                new Vector3(-1606.45f, -880.31f, 9.73f), //DP PL 2
                new Vector3(-194.86f, -2013.25f, 27.82f), //MB PL
                new Vector3(1184.65f, -1535.76f, 35.07f), //Hospital PG
        };
        public Vector3 victimCoords;
        public Vector3 vehicleCoords;
        public float heading;

        private Ped victim, suspect;
        private PedData victimData, suspectData;
        public Vehicle veh;
        public VehicleData vehData;

        public Blip searchArea;
        public int blipHandle;

        public int startTimer;
        public int unk1;

        public StolenVehicle()
        {
            victimCoords = victimLocations.SelectRandom();

            InitInfo(victimCoords);
            ResponseCode = 2;
            ShortName = "Stolen Vehicle";
            StartDistance = 165f;
            CalloutDescription = $"A vehicle has been reported stolen in {World.GetZoneLocalizedName(victimCoords)}. Travel to the area and investigate.";
        }

        public override async Task OnAccept()
        {
            InitBlip(35f);
            UpdateData();

            await Task.FromResult(0);
        }

        public override async void OnStart(Ped closest)
        {
            base.OnStart(closest);

            //Spawn Victim
            victim = await CreateVictim();

            victimData = new PedData();
            victimData = await victim.GetData();

            bool foundLocation = false;
            while (!foundLocation)
            {
                foundLocation = API.GetNthClosestVehicleNodeWithHeading(victimCoords.X, victimCoords.Y, victimCoords.Z, 350, ref vehicleCoords, ref heading, ref unk1, 9, 3.0f, 2.5f);
            }

            //Spawn Vehicle
            veh = await SpawnVehicle(RandomUtils.GetRandomVehicle(VehicleClass.Compacts), vehicleCoords, heading);
            vehData = await veh.GetData();
            suspect = await SpawnPed(RandomUtils.GetRandomPed(), vehicleCoords);
            suspectData = await suspect.GetData();
            suspect.BlockPermanentEvents = true;
            suspect.AlwaysKeepTask = true;
            suspect.Weapons.RemoveAll();

            victimData.FirstName = vehData.OwnerFirstName.ToString();
            victimData.LastName = vehData.OwnerLastName.ToString();

            vehData.Flag = "Stolen";
            veh.SetData(vehData);
            victim.SetData(victimData);

            VictimQuestions();
            SuspectQuestions();

            API.SetDriverAbility(suspect.Handle, 1.0f);
            API.SetDriverAggressiveness(suspect.Handle, 0.0f);
            API.SetPedIntoVehicle(suspect.Handle, suspect.Handle, -1);
            API.TaskVehicleDriveWander(suspect.Handle, veh.Handle, 18.0f, 536871359);

            while(World.GetDistance(victim.Position, Game.PlayerPed.Position) > 2f) { await BaseScript.Delay(50); }
            startTimer = API.GetGameTimer();
            while(API.GetGameTimer() - startTimer < 15000) { await BaseScript.Delay(500); }

            ShowNetworkedNotification("~r~Be advised~s~: BOLO was put out for the victims vehicle. ~y~Search~s~ the area.", "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "~y~Callout Update~s~", 1f);

            Marker.Delete();
            World.RemoveWaypoint();
            searchArea = World.CreateBlip(victim.Position, 350f);
            blipHandle = searchArea.Handle;
            searchArea.Alpha = 100;
            searchArea.Color = BlipColor.Yellow;

            int waitTime = rng.Next(25, 35);
            await BaseScript.Delay(waitTime * 1000);

            veh.AttachBlip();
            Tick += DrawInfo;

            ShowNetworkedNotification("~r~Be advised~s~: Vehicle matching BOLO description spotted in your area. Vehicle ~r~marked~s~.", "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "~y~BOLO Update~s~", 1f);

            await Task.FromResult(0);
        }

        public override void OnCancelBefore()
        {
            try
            {
                searchArea.Delete();
                veh.AttachedBlip.Delete();
            }
            catch { }
            base.OnCancelBefore();
        }
        public async Task<Ped> CreateVictim()
        {
            Ped victim = await SpawnPed(RandomUtils.GetRandomPed(), victimCoords);
            victim.BlockPermanentEvents = true;
            victim.AlwaysKeepTask = true;

            await Task.FromResult(0);
            return victim;
        }
        public async Task DrawInfo()
        {
            DrawText(0.35f, "~y~Callout Information~s~", 0.02f, 0.43f);
            DrawText(0.3f, $"~b~Name~s~: {victimData.FirstName} {victimData.LastName}", 0.0325f, 0.46f);
            DrawText(0.3f, $"~b~Vehicle Model~s~: {vehData.Name}", 0.0325f, 0.48f);
            DrawText(0.3f, $"~b~Vehicle Color~s~: {vehData.Color}", 0.0325f, 0.50f);
            DrawText(0.3f, $"~b~License Plate~s~: {vehData.LicensePlate}", 0.0325f, 0.52f);
            await Task.FromResult(0);
        }
        private void DrawText(float scale, string msg, float x, float y)
        {
            API.SetTextFont(0);
            API.SetTextProportional(false);
            API.SetTextScale(scale, scale);
            API.SetTextOutline();
            API.SetTextEntry("STRING");
            API.AddTextComponentString(msg);
            API.DrawText(x, y);
        }
        private void VictimQuestions()
        {
            double mintues = Math.Round(rng.Next(10, 51) / 5.0) * 5;

            PedQuestion q1 = new PedQuestion();
            q1.Question = "Were you the one who called?";
            q1.Answers = new List<string>()
            {
                "Yeah, some jerk stole my car!",
                "Yeah, my car was stolen.",
                "I did. Some one stole my car.",
                "Yep,  appears my car has been stolen."
            };

            PedQuestion q2 = new PedQuestion();
            q2.Question = "Can you describe the vehicle please.";
            q2.Answers = new List<string>()
            {
                $"It's a ~y~{vehData.Color}~s~ ~g~{vehData.Name}~s~ with the license plate ~b~{vehData.LicensePlate}~s~. I didn’t see who took it though."
            };

            PedQuestion q3 = new PedQuestion();
            q3.Question = "How long were you parked for?";
            q3.Answers = new List<string>()
            {
                $"{mintues} minutes or so.",
                $"Not long. I only ran up the street quick. Maybe {mintues} minutes.",
                "What, do you think I just sit here and count the minutes since I parked?",
                "Honestly I’m not sure. Not long at all."
            };

            PedQuestion[] questions = new PedQuestion[] { q1, q2, q3 };
            AddPedQuestions(victim, questions);
        }
        private void SuspectQuestions()
        {
            PedQuestion q1 = new PedQuestion();
            q1.Question = "Do you know why I pulled you over?";
            q1.Answers = new List<string>()
            {
                "Was I speeding?",
                "I know I wasn't speeding.",
                "~y~*remains silent*~s~",
                "You need to reach your ticket quota this month?",
                "You like hassling people.",
                "Why don't you tell me.",
                "I don't have time for this today..."
            };

            PedQuestion q2 = new PedQuestion();
            q2.Question = "License and Registration please.";
            q2.Answers = new List<string>() {
                "I'm borrowing this car, I can't find the registration.",
                "Don't you have better things to do with your time?",
                "Registration is around here somewhere I just can't find it.",
                "I just bought this car, haven't had a chance to register it.",
                "I don't have any registration..."
            };

            PedQuestion q3 = new PedQuestion();
            q3.Question = "This car has been reported stolen";
            q3.Answers = new List<string>()
            {
                "Fake news",
                "Shit...",
                "You've got to be kidding me.",
                "I'm just borrowing this car.",
                "That can't be good."
            };

            PedQuestion q4 = new PedQuestion();
            q4.Question = "I'm placing you under arrest.";
            q4.Answers = new List<string>() {
                "You've got to be kidding me.",
                "I want to speak with a lawyer.",
                "That's bullshit, I didn't do anything!",
                "Good luck proving anything!",
                "You better read me my rights!"
            };

            PedQuestion[] questions = new PedQuestion[]{ q1, q2, q3, q4 };
            AddPedQuestions(suspect, questions);
        }
    }
}
