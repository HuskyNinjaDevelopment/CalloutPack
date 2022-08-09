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
    [CalloutProperties("Public Intoxication", "HuskyNinja","v1.0")]
    internal class PublicIntoxication : Callout
    {
        public readonly Random rng = new Random();
        public List<Vector3> barLocations = new List<Vector3>()
        {
            new Vector3(498.53f, -1539f, 29.27f),
            new Vector3(-554.8f, 285.28f, 82.18f),
            new Vector3(-1392.61f, -587.24f, 30.25f),
            new Vector3(1215.87f, -413.62f, 67.82f),
            new Vector3(252.23f, -1012.42f, 29.27f),
            new Vector3(226.25f, 302.41f, 105.53f),
            new Vector3(2186.15f, -394.55f, 13.43f)
        };

        public List<string> clipSets = new List<string>()
        {
            "MOVE_M@DRUNK@MODERATEDRUNK",
            "MOVE_M@DRUNK@MODERATEDRUNK_HEAD_UP",
            "MOVE_M@DRUNK@SLIGHTLYDRUNK",
            "MOVE_M@DRUNK@VERYDRUNK"
        };

        public Vector3 barLocation;
        public Ped drunk;
        public PedData drunkData;
        public int notDrunkChance;
        public PlayerData playerData;
        public string clipset;

        public PublicIntoxication()
        {
            barLocation = barLocations.SelectRandom();
            clipset = clipSets.SelectRandom();

            API.GetSafeCoordForPed(barLocation.X, barLocation.Y, barLocation.Z, true, ref barLocation, 16);

            InitInfo(barLocation);
            ShortName = "Public Intoxication";
            ResponseCode = 2;
            StartDistance = 165f;
            CalloutDescription = $"An Intoxicated person has been reported in the {World.GetZoneLocalizedName(barLocation)} area.";
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

            playerData = Utilities.GetPlayerData();
            notDrunkChance = rng.Next(0, 101);

            drunk = await SpawnPed(RandomUtils.GetRandomPed(), barLocation);
            drunkData = await drunk.GetData();
            DrunkQuestions();
            drunk.AttachBlip();

            if(notDrunkChance <= 80)
            {
                double bac = (double)rng.Next(10, 20) / 100;
                drunkData.BloodAlcoholLevel = bac;
                drunk.SetData(drunkData);
            }

            API.RequestAnimSet(clipset);
            while (!API.HasAnimSetLoaded(clipset)) { await BaseScript.Delay(50); }
            API.SetPedMovementClipset(drunk.Handle, clipset, 0.2f);

            drunk.Task.WanderAround();

            await Task.FromResult(0);
        }

        public void DrunkQuestions()
        {
            if(notDrunkChance <= 80)
            {
                PedQuestion q1 = new PedQuestion();
                q1.Question = String.Format("Hello {0}, I'm Officer {1}", drunkData.Gender == Gender.Male ? "Sir" : "Ma'am", playerData.DisplayName);
                q1.Answers = new List<string>()
                {
                    "Nice to meet you officer.",
                    "~y~*Suspect stares at you confused*",
                    "Piss off pig. ~y~*hiccups*",
                    "~y~*Suspect burbs loudly in your face*"
                };

                PedQuestion q2 = new PedQuestion();
                q2.Question = "How are you feeling right now?";
                q2.Answers = new List<string>()
                {
                    "Like I’m on top of the world!",
                    "I was better till you got here...",
                    "Just fine officer, thank you...",
                    "A little dizzy, I think I might be getting sick."
                };

                PedQuestion q3 = new PedQuestion();
                q3.Question = "Do you have any medical conditions?";
                q3.Answers = new List<string>()
                {
                    "No, healthy as an ox.",
                    "Too many to count!",
                    "I’m allergic to bees.",
                    "~y~*Suspect declines to answer your question*"
                };

                PedQuestion q4 = new PedQuestion();
                q4.Question = "Have you had anything to drink today?";
                q4.Answers = new List<string>()
                {
                    "I don’t believe you can ask me that, thanks to Obama",
                    "You mean my medicine? Yeah plenty. ~y~*hiccups*",
                    "Nope, not a drop.",
                    "I may have had a few sips from the bottle."
                };

                PedQuestion q5 = new PedQuestion();
                q5.Question = "Pedestrain Obersvations";
                q5.Answers = new List<string>()
                {
                    "~y~*Suspect speech slurred slightly*",
                    "~y~*Suspect speech slurred mildly*",
                    "~y~*Suspect speech slurred heavily*",
                    "~y~*Suspect stumbling excessively. Appears to be having trouble standing*",
                    "~y~*Intoxication signs subtle & unclear*",
                };

                PedQuestion[] questions = new PedQuestion[] { q1, q2, q3, q4, q5 };
                AddPedQuestions(drunk, questions);
            }
            else
            {
                PedQuestion q1 = new PedQuestion();
                q1.Question = String.Format("Hello {0}, I'm Officer {1}", drunkData.Gender == Gender.Male ? "Sir" : "Ma'am", playerData.DisplayName);
                q1.Answers = new List<string>()
                {
                    "Hello officer. Nice to meet you.",
                    "Is there a problem officer?",
                    String.Format("Hello officer, my name is {0}. What seems to be the problem?", drunkData.FirstName),
                    "~y~*Suspect nods at you*"
                };

                PedQuestion q2 = new PedQuestion();
                q2.Question = "How are you feeling right now?";
                q2.Answers = new List<string>()
                {
                    "Fine, thank you for asking. ~y~*Suspect appears confused*",
                    "I’m alright. How about yourself?",
                    "A little tired but fine thank you.",
                    "~y~*Suspect stares at you confused*"
                };

                PedQuestion q3 = new PedQuestion();
                q3.Question = "Do you have any medical conditions?";
                q3.Answers = new List<string>()
                {
                    "I don’t think I feel comfortable answering that.",
                    "I have Parkinson’s disease.",
                    "I have a form of Tourette’s syndrome.",
                    "Too many to count!"
                };

                PedQuestion q4 = new PedQuestion();
                q4.Question = "Have you had anything to drink today?";
                q4.Answers = new List<string>()
                {
                    "No Officer, why do you ask?",
                    "I had a glass of wine a little while ago but that’s about it.",
                    "No, Im a Mormon. We don’t partake in the devils nectar.",
                    "Might have a few beers later but no I haven’t drank anything yet. ~y~*Laughs*"
                };

                PedQuestion q5 = new PedQuestion();
                q5.Question = "Observe Pedestrian";
                q5.Answers = new List<string>()
                {
                    "~y~*Suspect shaking slightly & uncontrollably**",
                    "~y~*Suspect slurring speech slightly*",
                    "~y~*Signs unclear*",
                    "~y~*Slight twitching noticeable*",
                };

                PedQuestion[] questions = new PedQuestion[] { q1, q2, q3, q4, q5 };
                AddPedQuestions(drunk, questions);
            }
        }
    }
}
