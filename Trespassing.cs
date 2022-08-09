using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalloutPack
{
    [CalloutProperties("Trespassing", "HuskyNinja", "v1.0")]
    internal class Trespassing : Callout
    {
        public PedHash securityHash = PedHash.Security01SMM;
        public Ped guard, trespasser;
        
        //Callout locations
        public struct CalloutLocation
        {
            public Vector3 guardCoords;
            public Vector3 trespasserCoords;
            public float guardHeading;
            public string name;

            public CalloutLocation(Vector3 guard, Vector3 trespass, float heading, string businessName)
            {
                guardCoords = guard;
                trespasserCoords = trespass;
                guardHeading = heading;
                name = businessName;
            }
        }
        public List<CalloutLocation> calloutLocations = new List<CalloutLocation>()
        {
            new CalloutLocation(new Vector3(1168.47f, -1335.84f, 34.9f), new Vector3(1151.69f, -1325.38f, 34.69f), 281.38f, "L T Weld Supply Co"),
            new CalloutLocation(new Vector3(1210.31f, -1283.36f, 35.38f), new Vector3(1203.86f, -1267.16f, 35.23f), 141.29f, "Warehouse"),
            new CalloutLocation(new Vector3(939.67f, -1490.62f, 30.03f), new Vector3(941.41f, -1498.72f, 30.17f), 169.82f, "Scrap Yard"),
            new CalloutLocation(new Vector3(868.19f, -1642.2f, 30.34f), new Vector3(894.38f, -1655.54f, 30.18f), 109.54f, "Fridgit Cold Storage Warehouse"),
            new CalloutLocation(new Vector3(965.69f, -1932.46f, 31.12f), new Vector3(1001.75f, -1919.47f, 31.15f), 342.8f, "Grand Banks Steel Inc"),
            new CalloutLocation(new Vector3(729.66f, -1973.39f, 29.29f), new Vector3(734.1f, -1947.22f, 29.29f), 253.77f, "Substation"),
            new CalloutLocation(new Vector3(849.39f, -2434f, 28.01f), new Vector3(881.34f, -2411.34f, 28.04f), 200.33f, "Zalinsky Supply & MFR Corp"),
            new CalloutLocation(new Vector3(774.08f, -2474.04f, 20.15f), new Vector3(955.48f, -2543.67f, 28.3f), 266.87f, "Cypress Warehouses"),
            new CalloutLocation(new Vector3(1384.92f, -2078.8f, 52f), new Vector3(1412.08f, -2047.46f, 52f), 28.86f, "Covington Engineering"),
            new CalloutLocation(new Vector3(1432.47f, -2317.2f, 66.88f), new Vector3(1447.4f, -2220.16f, 61.12f), 357.86f, "Oil Fields")
        };
        public CalloutLocation locationData;

        public Trespassing()
        {
            locationData = calloutLocations.SelectRandom();

            InitInfo(locationData.guardCoords);
            ResponseCode = 2;
            StartDistance = 175f;
            ShortName = "Trespassing";
            CalloutDescription = $"A security guard working at the {locationData.name} by {World.GetStreetName(locationData.guardCoords)} in the {World.GetZoneLocalizedName(locationData.guardCoords)} area";
        }
        
        public override async Task OnAccept()
        {
            InitBlip(100f);
            UpdateData();
            await Task.FromResult(0);
        }

        public override async void OnStart(Ped closest)
        {
            base.OnStart(closest);

            guard = await SpawnPed(securityHash, locationData.guardCoords);
            guard.Task.AchieveHeading(locationData.guardHeading);
            guard.AttachBlip();
            guard.AttachedBlip.Color = BlipColor.Blue;
            GuardQuestions();


            trespasser = await SpawnPed(RandomUtils.GetRandomPed(), locationData.trespasserCoords);
            trespasser.Task.WanderAround(locationData.trespasserCoords, 15f);

            API.RequestAnimDict("amb@code_human_police_investigate@idle_a");
            while(!Function.Call<bool>(Hash.REQUEST_ANIM_DICT, "amb@code_human_police_investigate@idle_a"))
            {
                await BaseScript.Delay(10);
            }

            guard.Task.PlayAnimation("amb@code_human_police_investigate@idle_a", "idle_a");

            await Task.FromResult(0);
        }

        public void GuardQuestions()
        {
            PedQuestion q1 = new PedQuestion();
            q1.Question = "Are you the one who called?";
            q1.Answers = new List<string>() {
                "Yes officer that was me.",
                "Yes sir.",
                "Yeah I called. Good response time.",
                "Yeah that was me. Took you long enough to get here."
            };

            PedQuestion q2 = new PedQuestion();
            q2.Question = "You have a trespasser?";
            q2.Answers = new List<string>()
            {
                "Yes sir.",
                "Yeah some jerk walked by me ignoring me completely.",
                "Yeah, they looked pretty suspicious so I hung back and called you guys.",
                "Yeah, they looked lost. Not my job to help them."
            };

            PedQuestion q3 = new PedQuestion();
            q3.Question = "Where did they go?";
            q3.Answers = new List<string>()
            {
                "They just walked away back there.",
                "I’m not sure, they have just been walking around for 20 minutes now.",
                "What I have to do your job now too? Go find them yourself.",
                "They are around here somewhere."
            };

            PedQuestion q4 = new PedQuestion();
            q4.Question = "Okay, I’ll go take a look.";
            q4.Answers = new List<string>()
            {
                "Thank you officer.",
                "Be quick about it. I don’t feel like hearing about this by my boss.",
                "I’d help but I have to stay here.",
                "Okay, be careful."
            };

            PedQuestion[] questions = new PedQuestion[] { q1, q2, q3, q4 };
            AddPedQuestions(guard, questions);
        }

        public void TrespasserQuestions()
        {
            PedQuestion q1 = new PedQuestion();
            q1.Question = "Hi there, how’s it going?";
            q1.Answers = new List<string>()
            {
                "No complaints.",
                "Leave me alone.",
                "Back off pig.",
                "It’s going just fine."
            };

            PedQuestion q2 = new PedQuestion();
            q2.Question = "What are you doing here?";
            q2.Answers = new List<string>()
            {
                "Minding my own business. How about you?",
                "I’m lost.",
                "Just going for a walk.",
                "Don’t worry about it."
            };

            PedQuestion q3 = new PedQuestion();
            q3.Question = "Are you aware this is private property?";
            q3.Answers = new List<string>()
            {
                "No, I had no idea.",
                "Yeah, I can read a sign…",
                "Can any one ever really own Mother Earth?",
                "I didn’t see any signs.",
                "I thought this was America!"
            };

            PedQuestion q4 = new PedQuestion();
            q4.Question = "You need to leave now please.";
            q4.Answers = new List<string>()
            {
                "No, I’m fine right here thank you.",
                "Whatever.",
                "Fine but I’ll be filling a complaint with your supervisor!",
                "Okay okay I’m leaving, chill.",
                "How do I get out of here?"
            };

            PedQuestion[] questions = new PedQuestion[] { q1, q2, q3, q4 };
            AddPedQuestions(trespasser, questions);
        }
    }
}
