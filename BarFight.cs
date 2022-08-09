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
    [CalloutProperties("Bar Fight", "HuskyNinja", "v1.0")]
    internal class BarFight : Callout
    {
        public readonly Random rng = new Random();
        public List<Vector3> barLocations = new List<Vector3>()
        {
            new Vector3(498.53f, -1539f, 29.27f),
            new Vector3(-554.8f, 285.28f, 82.18f),
            new Vector3(-1392.61f, -587.24f, 30.25f),
            new Vector3(-1183.87f, -1527.96f, 4.38f),
            new Vector3(1215.87f, -413.62f, 67.82f),
            new Vector3(252.23f, -1012.42f, 29.27f),
            new Vector3(226.25f, 302.41f, 105.53f),
            new Vector3(2186.15f, -394.55f, 13.43f)
        };
        public List<String> juiceNames = new List<string>()
        {
            "Cabbage Digestive Juice",
            "Power Up Punch",
            "Mango Madness Smoothie",
            "Clamato Juice",
            "Wheat Grass Shake",
        };
        public List<PedHash> jPeds = new List<PedHash>()
        {
            PedHash.Musclbeac01AMY,
            PedHash.Musclbeac02AMY,
        };

        public List<string> clipSets = new List<string>()
        {
            "MOVE_M@DRUNK@MODERATEDRUNK",
            "MOVE_M@DRUNK@MODERATEDRUNK_HEAD_UP",
            "MOVE_M@DRUNK@SLIGHTLYDRUNK",
            "MOVE_M@DRUNK@VERYDRUNK"
        };

        public Vector3 barLocation;
        public Ped fighter1, fighter2;
        public PedData fighter2Data;
        public string jName;
        public string clipset;
        public BarFight()
        {
            barLocation = barLocations.SelectRandom();
            jName = juiceNames.SelectRandom();
            clipset = clipSets.SelectRandom();

            InitInfo(barLocation);
            ShortName = "Bar Fight";
            ResponseCode = 2;
            StartDistance = 175f;
            CalloutDescription = $"A Bar Fight has been reported in the {World.GetZoneLocalizedName(barLocation)} area.";
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

            API.RequestAnimSet(clipset);
            while (!API.HasAnimSetLoaded(clipset)) { await BaseScript.Delay(50); }

            if (barLocation == new Vector3(-1183.87f, -1527.96f, 4.38f))
            {
                fighter1 = await SpawnPed(jPeds.SelectRandom(), barLocation.Around(0.5f));
            }
            else
            {
                fighter1 = await SpawnPed(RandomUtils.GetRandomPed(), barLocation.Around(0.5f));
                API.SetPedMovementClipset(fighter1.Handle, clipset, 0.2f);
            }

            if (barLocation == new Vector3(-1183.87f, -1527.96f, 4.38f))
            {
                fighter2 = await SpawnPed(jPeds.SelectRandom(), barLocation.Around(0.5f));
            }
            else
            {
                fighter2 = await SpawnPed(RandomUtils.GetRandomPed(), barLocation.Around(0.5f));
                API.SetPedMovementClipset(fighter2.Handle, clipset, 0.2f);
                fighter2Data = await fighter2.GetData();
                double bac = (double)rng.Next(10, 20) / 100;
                fighter2Data.BloodAlcoholLevel = bac;
                fighter2.SetData(fighter2Data);
            }

            fighter1.Task.LookAt(fighter2);
            fighter2.Task.LookAt(fighter1);

            while(World.GetDistance(Game.PlayerPed.Position, fighter1.Position) > 25f) { await BaseScript.Delay(50); }

            if(barLocation == new Vector3(-1183.87f, -1527.96f, 4.38f)) { ShowDialog($"You made me ~y~spill~s~ my ~r~{jName}~s~! I'm gonna fuck you up!", 12500, 20f); }

            fighter1.Task.FightAgainst(fighter2);
            fighter2.Task.FightAgainst(fighter1);
            
            await Task.FromResult(0);
        }

        //Victim
        public void Fighter1Questions()
        {
            PedQuestion q1, q2, q3, q4;
            q1 = new PedQuestion();
            q2 = new PedQuestion();
            q3 = new PedQuestion();
            q4 = new PedQuestion();

            //Juice Bar
            if(barLocation == new Vector3(-1183.87f, -1527.96f, 4.38f))
            {
                q1.Question = "What's going on here?";
                q1.Answers = new List<string>()
                {
                    "I accidentally bumped into this guy then he started freaking out.",
                    $"I went to get another {juiceNames.SelectRandom()} then this guy starts going bananas.",
                    "This jackass got up in my face and started talking some shit.",
                    $"I was just trying to enjoy a nice {juiceNames.SelectRandom()}."
                };

                q2.Question = "Did you start this fight?";
                q2.Answers = new List<string>()
                {
                    "No chance, this asshole spilled his juice and is blaming me.",
                    "I'm not going to let some punk ass get away with acting like an asshole.",
                    "No. He started attacking me after spilling his juice.",
                    "He got in my face and started pushing me. I couldn't let it go."
                };
            }
            //Non Juice Bar
            else
            {
                q1.Question = "What's going on here?";
                q1.Answers = new List<string>()
                {
                    "I was just trying to enjoy a drink with my girlfriend.",
                    "I attempting to drink an ice cold beer after a long day at work.",
                    "Can't even get a beer in this city without shit going down.",
                    "I'm trying to defend myself from this drunk asshole."
                };

                q2.Question = "Did you start this fight?";
                q2.Answers = new List<string>()
                {
                    "No officer, I was just trying to relax after work.",
                    "Nope. He said I was looking at his girl, I don't even know who that is!",
                    "He started attacking me and I defended myself.",
                    "Not me, this drunk idiot came up to me and started punching me."
                };
            }

            PedQuestion[] questions = new PedQuestion[] { q1, q2, q3, q4 };
            AddPedQuestions(fighter1, questions);
        }

        //Suspect
        public void Fighter2Questions()
        {
            PedQuestion q1, q2;
            q1 = new PedQuestion();
            q2 = new PedQuestion();

            //Juice Bar
            if (barLocation == new Vector3(-1183.87f, -1527.96f, 4.38f))
            {
                q1.Question = "What's going on here?";
                q1.Answers = new List<string>()
                {
                    $"This asshole made me spill my {jName}! He's dead!",
                    "This jerk made the biggest mistake of his life. Now he's paying for it.",
                    $"This guy bumped into me. Made me spill my {jName}. Now I'm going to destroy him!",
                    "This guy fucked up. Now he's paying the price!",
                    "Making this chump regret ever bumping into me!",
                };

                q2.Question = "Did you start this fight?";
                q2.Answers = new List<string>()
                {
                    "I'm ending this fight thats for damn sure. He started this by bumping into me.",
                    "I may have hit him first, but he wasn't watching where he was going. Who's the real criminal here?",
                    "Techincally yes, but he had it coming. He bumped into ME.",
                    "I prefer to say that his face got in the way of my fist... Twice."
                };
            }
            //Non Juice Bar
            else
            {
                q1.Question = "What's going on here?";
                q1.Answers = new List<string>()
                {
                    "Huh?... Who are you? ~y~*person appears confused*~s~",
                    "I'm kicking this guys ass! ~y~*noticeable slurring of speech*~s~",
                    "Two men settling things like men!",
                    "I'm about to send this asshole to the hospital! ~y~*strong smell of alcohol*~s~"
                };

                q2.Question = "Did you start this fight?";
                q2.Answers = new List<string>()
                {
                    "He was hitting on my girlfriend. I had to kick his ass. ~y~*strong smell of alcohol*~s~",
                    "He started this by making me spill my beer.",
                    "This jerk was eyeballing me from across the bar. Can't let that slide. ~y~*noticeable slurring of speech*~s~",
                    "No way. This guy started talking shit and I punched him to shut him up."
                };
            }

            PedQuestion[] questions = new PedQuestion[] { q1, q2};
            AddPedQuestions(fighter2, questions);
        }
    }
}
