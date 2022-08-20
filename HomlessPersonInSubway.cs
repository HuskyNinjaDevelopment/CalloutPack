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
    [CalloutProperties("Homeless Person In Subway", "HuskyNinja", "v1.0")]
    internal class HomlessPersonInSubway : Callout
    {
        public readonly Random rng = new Random();

        public struct CalloutLocation
        {
            //Blip Coords
            public Vector3 blipCoords;

            //Spawnpoint Guard
            public Vector3 guardCoords;

            //Suspect Spwan Points
            public List<Vector3> spawnpoints;

            public CalloutLocation(Vector3 blip, Vector3 guard, List<Vector3> spawns)
            {
                blipCoords = blip;
                guardCoords = guard;
                spawnpoints = spawns;
            }
        }
        public List<CalloutLocation> calloutLocations = new List<CalloutLocation>()
        {
            //Rockford Hills
            new CalloutLocation(new Vector3(-826.51f, -112.88f, 37.5f), new Vector3(-829.26f, -110.45f, 27.96f), new List<Vector3>(){
                new Vector3(-845.72f, -109.74f, 28.19f),
                new Vector3(-856.97f, -112.05f, 28.18f),
                new Vector3(-851.76f, -122.78f, 28.18f),
                new Vector3(-841.54f, -131.15f, 28.18f),
                new Vector3(-823.85f, -125.45f, 28.18f),
                new Vector3(-838.66f, -151.10f, 19.95f)
            }),
            //Little Seoul
            new CalloutLocation(new Vector3(-490.04f, -697.33f, 33.24f), new Vector3(-493.24f, -726.66f, 23.90f), new List<Vector3>()
            {
                new Vector3(-486.63f, -732.46f, 23.90f),
                new Vector3(-470.19f, -732.05f, 23.90f),
                new Vector3(-469.51f, -701.58f, 20.03f),
                new Vector3(-468.49f, -690.80f, 20.03f),
                new Vector3(-488.15f, -690.45f, 20.03f),
                new Vector3(-502.59f, -680.99f, 20.03f),
                new Vector3(-532.61f, -673.05f, 11.81f),
                new Vector3(-474.67f, -670.95f, 11.81f)
            }),
            //Del Perro
            new CalloutLocation(new Vector3(-1369.29f, -527.72f, 30.31f), new Vector3(-1351.21f, -518.91f, 23.27f), new List<Vector3>()
            {
                new Vector3(-1344.50f, -510.91f, 23.27f),
                new Vector3(-1352.45f, -497.93f, 23.27f),
                new Vector3(-1361.26f, -483.22f, 23.27f),
                new Vector3(-1359.10f, -467.68f, 23.27f),
                new Vector3(-1338.51f, -487.92f, 15.05f),
                new Vector3(-1368.27f, -437.89f, 15.05f)
            }),
            //Burton
            new CalloutLocation(new Vector3(-244.92f, -336.11f, 29.98f), new Vector3(-246.91f, -310.42f, 21.63f), new List<Vector3>()
            {
                new Vector3(-256.54f, -295.73f, 21.63f),
                new Vector3(-277.93f, -308.14f, 18.29f),
                new Vector3(-279.69f, -331.86f, 18.29f),
                new Vector3(-294.47f, -295.61f, 10.06f),
                new Vector3(-294.68f, -362.34f, 10.06f)
            }),
            //Airport Parking Lot
            new CalloutLocation(new Vector3(-950.67f, -2338.82f, 5.01f), new Vector3(-916.61f, -2346.46f, -3.51f), new List<Vector3>(){
                new Vector3(-908.93f, -2338.23f, -3.51f),
                new Vector3(-902.84f, -2321.99f, -3.51f),
                new Vector3(-889.69f, -2313.16f, -3.51f),
                new Vector3(-894.35f, -2349.93f, -11.73f),
                new Vector3(-872.26f, -2287.36f, -11.73f)
            }),
            //Airport Entrance
            new CalloutLocation(new Vector3(-1037.14f, -2737.01f, 13.77f), new Vector3(-1024.29f, -2757.58f, 0.80f), new List<Vector3>()
            {
                new Vector3(-1015.24f, -2752.16f, 0.80f),
                new Vector3(-1027.13f, -2741.47f, 0.80f),
                new Vector3(-1047.00f, -2724.88f, 0.80f),
                new Vector3(-1065.28f, -2721.71f, 0.81f),
                new Vector3(-1080.28f, -2723.19f, 0.81f),
                new Vector3(-1062.08f, -2691.15f, -7.41f),
                new Vector3(-1105.31f, -2741.80f, -7.41f)
            })
        };
        public CalloutLocation calloutLocation;

        public List<PedHash> homlessHashes = new List<PedHash>()
        {
            PedHash.MilitaryBum,
            PedHash.Acult02AMO,
            PedHash.Acult02AMY,
            PedHash.Hippy01AMY,
            PedHash.Jesus01,
            PedHash.Rurmeth01AMM,
            PedHash.Rurmeth01AFY,
            PedHash.Salton01AMY,
            PedHash.Soucent03AMO,
            PedHash.Tramp01AMO,
            PedHash.Tramp01AMM,
            PedHash.TrampBeac01AFM,
            PedHash.Tramp01AFM,
            PedHash.Tramp01,
            PedHash.TrampBeac01AMM
        };
        public PedHash suspectHash;

        public Ped suspect, guard;
        public PedData suspectData;

        public List<string> clipSets = new List<string>()
        {
            "MOVE_M@DRUNK@MODERATEDRUNK",
            "MOVE_M@DRUNK@MODERATEDRUNK_HEAD_UP",
            "MOVE_M@DRUNK@SLIGHTLYDRUNK",
            "MOVE_M@DRUNK@VERYDRUNK"
        };
        public string clipset;

        public HomlessPersonInSubway()
        {
            calloutLocation = calloutLocations.SelectRandom();
            suspectHash = homlessHashes.SelectRandom();
            clipset = clipSets.SelectRandom();

            InitInfo(calloutLocation.blipCoords);
            ShortName = "Homeless Person in Subway";
            StartDistance = 150f;
            ResponseCode = 1;
            CalloutDescription = $"A Homeless Person is refusing to leave the Subway Station in {World.GetZoneLocalizedName(calloutLocation.blipCoords)}.";
        }

        public override async Task OnAccept()
        {
            InitBlip();
            UpdateData();

            await Task.FromResult(0);
        }

        public override async void OnStart(Ped closest)
        {
            base.OnStart(closest);

            suspect = await SpawnPed(suspectHash, calloutLocation.spawnpoints.SelectRandom());
            suspectData = await suspect.GetData();
            guard = await SpawnPed(PedHash.Security01SMM, calloutLocation.guardCoords);

            SuspectQuestions();

            API.RequestAnimSet(clipset);
            while (!API.HasAnimSetLoaded(clipset)) { await BaseScript.Delay(50); }
            API.SetPedMovementClipset(suspect.Handle, clipset, 0.2f);

            while (World.GetDistance(guard.Position, Game.PlayerPed.Position) > 5f) { await BaseScript.Delay(50); }
            suspect.AttachBlip();
            guard.PlayAmbientSpeech("GENERIC_HI");
            guard.Task.LookAt(Game.PlayerPed, -1);
            ShowDialog(String.Format("~b~Guard~s~: There is a homeless {0} refusing to leave further in the terminal.", suspectData.Gender == Gender.Male ? "man" : "woman"), 8500, 1f);

            suspect.Task.WanderAround();

            await Task.FromResult(0);
        }

        public void SuspectQuestions()
        {
            PedQuestion q1 = new PedQuestion();
            q1.Question = "The guard says you won't leave?";
            q1.Answers = new List<string>()
            {
                "I got no where to go man, at least it's dry down here.",
                "Why should I? Isn't this public property?",
                "I haven't even been down here that long. Let me stay just a bit longer.",
                "I can't go back under the overpass after what happened..."
            };

            PedQuestion q2 = new PedQuestion();
            q2.Question = String.Format("You can't stay here {0}.", suspectData.Gender == Gender.Male ? "sir" : "ma`am");
            q2.Answers = new List<string>()
            {
                "And what if I don't leave?",
                "What are you going to do to make me leave?",
                "You're going to have to arrest me to get me to leave.",
                "I'll be on my way. I don't want any trouble."
            };
        }
    }
}
