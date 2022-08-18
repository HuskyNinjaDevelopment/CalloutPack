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
    [CalloutProperties("Noise Complaint", "HuskyNinja", "v1.0")]
    internal class NoiseComplaint : Callout
    {
        public readonly Random rng = new Random();

        public string speakerHash = "prop_speaker_07";
        public int speaker1, speaker2;

        public Ped host, neighbor;
        public PedData hostData, neighborData;
        public List<Ped> attendees = new List<Ped>();

        public struct Dance
        {
            public string dict;
            public string name;

            public Dance(string animDict, string animName)
            {
                dict = animDict;
                name = animName;
            }
        }
        public List<Dance> dances = new List<Dance>()
        {
            new Dance("special_ped@mountain_dancer@monologue_3@monologue_3a","mnt_dnc_buttwag"),
            new Dance("move_clown@p_m_zero_idles@", "fidget_short_dance"),
            new Dance("move_clown@p_m_two_idles@", "fidget_short_dance"),
            new Dance("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_11_buttwiggle_b_laz"),
            new Dance("timetable@tracy@ig_5@idle_a", "idle_a"),
            new Dance("timetable@tracy@ig_8@idle_b", "idle_d"),
            new Dance("anim@amb@casino@mini@dance@dance_solo@female@var_b@", "med_center"),
            new Dance("anim@amb@casino@mini@dance@dance_solo@female@var_b@", "high_center"),
            new Dance("anim@mp_player_intcelebrationfemale@the_woogie", "the_woogie")
        };

        public List<PedHash> neighborHashes = new List<PedHash>()
        {
            PedHash.Bevhills02AFM,
            PedHash.Business02AFM,
            PedHash.Soucent01AFM,
            PedHash.Bevhills01AFY,
            PedHash.Bevhills02AFY,
            PedHash.Bevhills03AFY,
            PedHash.Bevhills04AFY,
            PedHash.Business01AFY,
            PedHash.Business02AFY,
            PedHash.Hipster01AFY,
            PedHash.Soucent03AFY,
        };
        public List<PedHash> partyGoerHashes = new List<PedHash>()
        {
            PedHash.Bevhills01AFM,
            PedHash.Beach01AFM,
            PedHash.FatBla01AFM,
            PedHash.Soucent02AFM,
            PedHash.Beach01AFY,
            PedHash.Eastsa02AFY,
            PedHash.Genhot01AFY,
            PedHash.Hipster02AFY,
            PedHash.Hipster03AFY,
            PedHash.Indian01AFY,
            PedHash.Soucent01AFY,
            PedHash.Vinewood02AFY,
            PedHash.Vinewood04AFY,
            PedHash.AfriAmer01AMM,
            PedHash.Bevhills02AMM,
            PedHash.Genfat01AMM,
            PedHash.Malibu01AMM,
            PedHash.Polynesian01AMY,
            PedHash.Polynesian01AMM,
            PedHash.Salton03AMM,
            PedHash.Skater01AMM,
            PedHash.Beach01AMY,
            PedHash.Beach02AMY,
            PedHash.Beachvesp01AMY,
            PedHash.Beachvesp02AMY,
            PedHash.Bevhills01AMY,
            PedHash.Eastsa02AMY,
            PedHash.Genstreet02AMY,
            PedHash.Hipster03AMY,
            PedHash.Ktown01AMY,
            PedHash.Stwhi01AMY,
            PedHash.Stwhi02AMY,
        };

        public struct CalloutData
        {
            public Vector3 hostCoords;
            public Vector3 neighborCoords;
            public Vector3 partyCoords;
            public Vector3 speaker1Coords;
            public Vector3 speaker2Coords;
            public float heading;

            public CalloutData(Vector3 hc, Vector3 nc, Vector3 pc, Vector3 s1, Vector3 s2, float direction)
            {
                hostCoords = hc;
                neighborCoords = nc;
                partyCoords = pc;
                speaker1Coords = s1;
                speaker2Coords = s2;
                heading = direction;
            }
        }
        public List<CalloutData> calloutLocations = new List<CalloutData>()
        {
            new CalloutData(new Vector3(938.35f, -665.49f, 58.11f), new Vector3(940.70f, -670.20f, 58.01f), new Vector3(930.46f, -665.08f, 58.15f), new Vector3(940.15f, -662.57f, 58.01f), new Vector3(938.05f, -659.91f, 58.01f), 350f),
            new CalloutData(new Vector3(1234.70f, -690.97f, 60.90f), new Vector3(1217.10f, -714.62f, 60.22f), new Vector3(1236.22f, -685.42f, 61.19f), new Vector3(1229.16f, -692.46f, 60.87f),new Vector3(1229.77f, -695.24f, 60.85f) , 103.82f),
            new CalloutData(new Vector3(1215.83f, -562.42f, 69.07f), new Vector3(1193.57f, -586.72f, 65.25f), new Vector3(1217.52f, -558.10f, 69.07f), new Vector3(1212.76f, -561.54f, 69.07f), new Vector3(1212.95f, -557.79f, 69.07f), 81.59f),
            new CalloutData(new Vector3(1028.90f, -486.89f, 63.95f), new Vector3(1055.85f, -483.72f, 63.84f), new Vector3(1021.47f, -492.18f, 63.95f), new Vector3(1031.26f, -490.87f, 63.92f), new Vector3(1030.68f, -494.66f, 63.92f), 258.14f),
            new CalloutData(new Vector3(-1267.72f, 612.56f, 139.91f), new Vector3(-1244.37f, 646.02f, 142.22f), new Vector3(-1272.10f, 606.75f, 139.28f), new Vector3(-1269.49f, 614.24f, 139.91f), new Vector3(-1272.14f, 617.62f, 139.91f), 311.82f),
            new CalloutData(new Vector3(-614.29f, 462.73f, 108.85f), new Vector3(-658.21f, 484.78f, 109.88f), new Vector3(-611.75f, 469.92f, 108.87f), new Vector3(-622.76f, 471.22f, 108.87f), new Vector3(-627.25f, 470.36f, 108.86f) , 7.89f),
            new CalloutData(new Vector3(-1661.03f, -419.67f, 41.62f), new Vector3(-1667.64f, -446.42f, 39.64f), new Vector3(-1663.48f, -414.06f, 42.01f), new Vector3(-1657.06f, -415.13f, 42.01f), new Vector3(-1653.70f, -418.15f, 42.06f), 218.34f)
        };
        public CalloutData callout;

        public NoiseComplaint()
        {
            callout = calloutLocations.SelectRandom();
            InitInfo(callout.partyCoords);
            StartDistance = 150f;
            ResponseCode = 1;
            ShortName = "Noise Complaint";
            CalloutDescription = $"A noise complaint has been reported in the {World.GetZoneLocalizedName(callout.partyCoords)} area.";
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

            API.RequestModel((uint)API.GetHashKey(speakerHash));
            while (!API.HasModelLoaded((uint)API.GetHashKey(speakerHash))) { await BaseScript.Delay(50); }

            speaker1 = API.CreateObject(API.GetHashKey(speakerHash), callout.speaker1Coords.X, callout.speaker1Coords.Y, callout.speaker1Coords.Z - 1f, true, true, true);
            speaker2 = API.CreateObject(API.GetHashKey(speakerHash), callout.speaker2Coords.X, callout.speaker2Coords.Y, callout.speaker2Coords.Z - 1f, true, true, true);

            API.SetEntityHeading(speaker1, callout.heading);
            API.SetEntityHeading(speaker2, callout.heading);

            host = await SpawnPed(partyGoerHashes.SelectRandom(), callout.hostCoords);
            host.AttachBlip();
            host.AttachedBlip.Color = BlipColor.Blue;
            hostData = await host.GetData();

            neighbor = await SpawnPed(neighborHashes.SelectRandom(), callout.neighborCoords);
            neighbor.AttachBlip();
            neighbor.AttachedBlip.Color = BlipColor.Green;
            neighborData = await neighbor.GetData();

            neighborData.FirstName = "Karen";
            neighbor.SetData(neighborData);

            NeighborQuestions();
            HostQuestions();

            int numberPartiers = rng.Next(5, 9);
            for(int i = 0; i < numberPartiers; i++)
            {
                Ped ped = await SpawnPed(partyGoerHashes.SelectRandom(), callout.partyCoords.Around(2.25f), (float)rng.Next(0,360));
                attendees.Add(ped);
                Dance dance = dances.SelectRandom();

                API.RequestAnimDict(dance.dict);
                while(!API.HasAnimDictLoaded(dance.dict)) { await BaseScript.Delay(5); }

                ped.Task.PlayAnimation(dance.dict, dance.name, 8.0f, -1, AnimationFlags.Loop);
            }

            if(!API.IsAudioSceneActive("MP_Reduce_Score_For_Emitters_Scene"))
            {
                API.StartAudioScene("MP_Reduce_Score_For_Emitters_Scene");
            }

            API.LinkStaticEmitterToEntity("SE_Script_Placed_Prop_Emitter_Boombox", speaker1);
            API.LinkStaticEmitterToEntity("SE_Script_Placed_Prop_Emitter_Boombox", speaker2);
            API.SetEmitterRadioStation("SE_Script_Placed_Prop_Emitter_Boombox", "RADIO_01_CLASS_ROCK");
            API.SetStaticEmitterEnabled("SE_Script_Placed_Prop_Emitter_Boombox", true);
            API.SetAudioSceneVariable("SE_Script_Placed_Prop_Emitter_Boombox", "volume", 1f);
            API.SetRadioStationMusicOnly("RADIO_01_CLASS_ROCK", true);

            API.RequestAnimDict("special_ped@mountain_dancer@monologue_3@monologue_3a");
            while (!API.HasAnimDictLoaded("special_ped@mountain_dancer@monologue_3@monologue_3a")) { await BaseScript.Delay(50); }

            host.Task.PlayAnimation("special_ped@mountain_dancer@monologue_3@monologue_3a", "mnt_dnc_buttwag", 8.0f, -1, AnimationFlags.Loop);
            neighbor.Task.StartScenario("WORLD_HUMAN_STAND_IMPATIENT", neighbor.Position);

            await Task.FromResult(0);
        }

        public override void OnCancelBefore()
        {
            base.OnCancelBefore();
            API.SetStaticEmitterEnabled("SE_Script_Placed_Prop_Emitter_Boombox", false);
            foreach (Ped ped in attendees)
            {
                ped.Task.ClearAllImmediately();
                ped.Task.WanderAround();
            }
        }

        public void NeighborQuestions()
        {
            PedQuestion q1 = new PedQuestion();
            q1.Question = "You called in the Noise Complaint?";
            q1.Answers = new List<string>()
            { 
                "Yes Officer that was me.",
                "That's correct.",
                "I sure did.",
                "About time you showed up!"
            };

            PedQuestion q2 = new PedQuestion();
            q2.Question = "How long has the party been going on?";
            q2.Answers = new List<string>()
            {
                $"{rng.Next(3,7)} hours! This is getting ridiculous! They haven't stopped dancing for even a minute!"
            };

            PedQuestion[] questions = new PedQuestion[] {q1, q2 };
            AddPedQuestions(neighbor, questions);
        }

        public void HostQuestions()
        {
            PedQuestion q1 = new PedQuestion();
            q1.Question = "Is this your home?";
            q1.Answers = new List<string>()
            {
                "Not technically, this is an AirBnB",
                "This is my friends house, he is currently out of town.",
                "I pay rent to live here, which would make it my home.",
                "Sure is, just moved in last week."
            };

            PedQuestion q2 = new PedQuestion();
            q2.Question = "We've gotten some noise complaints";
            q2.Answers = new List<string>()
            {
                "I bet it was my pain in the ass neighbors.",
                "Who called? This is bullshit, just having a party with some friends.",
                "Someone complained about the tunes? Not very cash money.",
                "I guess it may be a little loud but, its a party!"
            };

            PedQuestion q3 = new PedQuestion();
            q3.Question = "Do you mind turning it down?";
            q3.Answers = new List<string>()
            {
                "Ya no problem Officer.",
                "Sure, I can turn it down a bit.",
                "Ya I'll go handle that now.",
                "Hell no, we are just having a good time!",
            };

            PedQuestion[] questions = new PedQuestion[] { q1, q2, q3 };
            AddPedQuestions(host, questions);
        }
    }
}
