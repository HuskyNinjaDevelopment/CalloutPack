using CitizenFX.Core;
using FivePD.API;
using FivePD.API.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalloutPack
{
    [CalloutProperties("Domestic Disturbance", "HuskyNinja", "v1.0")]
    internal class DomesticDisturbance : Callout
    {
        public List<Vector3> calloutLocations = new List<Vector3>()
        {
            new Vector3(-15.22f, -1446.51f, 30.65f),
            new Vector3(15.52f, -1446.72f, 30.54f),
            new Vector3(-61.85f, -1451.84f, 32.12f),
            new Vector3(-152.18f, -1690.15f, 32.87f),
            new Vector3(-162.94f, -1629.06f, 33.63f),
            new Vector3(-125.07f, -1653.92f, 32.56f),
            new Vector3(-130.90f, -1662.49f, 32.56f),
            new Vector3(127.52f, -1856.69f, 24.86f),
            new Vector3(330.24f, -1939.90f, 24.77f),
            new Vector3(386.48f, -1885.88f, 25.69f),
            new Vector3(-155.22f, 121.45f, 70.23f),
            new Vector3(326.49f, -123.43f, 67.88f),
            new Vector3(-1131.16f, 378.54f, 70.73f),
            new Vector3(-1219.74f, 503.07f, 95.67f),
            new Vector3(-1172.02f, 565.39f, 101.83f),
            new Vector3(-1105.79f, 590.91f, 104.36f),
            new Vector3(-842.92f, 497.10f, 90.62f),
            new Vector3(-820.45f, 481.14f, 90.17f),
            new Vector3(-739.91f, 442.91f, 106.85f),
            new Vector3(-675.32f, 503.49f, 110.06f),
            new Vector3(-613.14f, 463.39f, 108.85f),
            new Vector3(-578.92f, 405.07f, 100.66f),
            new Vector3(-533.25f, 456.98f, 103.19f)
        };

        public Ped victim, suspect;
        public PedData vicData, susData;

        public Vector3 calloutCoords;

        public DomesticDisturbance()
        {
            calloutCoords = calloutLocations.SelectRandom();

            InitInfo(calloutCoords);
            ShortName = "Domestic Disturbance";
            ResponseCode = 2;
            CalloutDescription = $"A Domestic Disturbance has been reported in the {World.GetZoneLocalizedName(calloutCoords)} area. Head over there and make sure everything is alright.";
            StartDistance = 150f;
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
            victim = await SpawnPed(RandomUtils.GetRandomPed(), calloutCoords.Around(0.5f));
            vicData = await victim.GetData();

            suspect = await SpawnPed(RandomUtils.GetRandomPed(), calloutCoords.Around(0.5f));
            susData = await suspect.GetData();

            await Task.FromResult(0);
        }

        public void VictimQuestions()
        {
            PedQuestion q1 = new PedQuestion();
            q1.Question = "Can you tell me what's going on here";
            q1.Answers = new List<string>(){
                $"~y~*{vicData.LastName} timidly remains silent*~s~",
                $"~y~*{vicData.LastName} stares at the ground, proceeds to shed a tear*~s~",
                "We got into an argument.",
                "Nothing officer."
            };

            PedQuestion q2 = new PedQuestion();
            q2.Question = $"*Examine {vicData.LastName} for signs of abuse*";
            q2.Answers = new List<string>()
            {
                "~y~Examination~s~: Bruising to upper left area of face",
                "~y~Examination~s~: No visible markings found.",
                "~y~Examination~s~: Minor scratches on both hands visible.",
                "~y~Examination~s~: Slight bruising and redness to neck visible."
            };

            PedQuestion q3 = new PedQuestion();
            q3.Question = "Ask about markings, if any are present";
            q3.Answers = new List<string>()
            {
                "I can’t live like this anymore.",
                "~y~*person begins crying*~s~",
                $"~y~*person looks towards {susData.LastName}, appears scared*~s~",
                $"~y~*{vicData.LastName} quickly attempts to hide markings*~s~"
            };

            PedQuestion q4 = new PedQuestion();
            q4.Question = "Conclude investigation: Arrest";
            q4.Answers = new List<string>()
            {
                $"~y~*{vicData.LastName} appears to be terrified*~s~",
                string.Format("{0} didn't do anything!", susData.Gender == Gender.Male ? "He" : "She"),
                "Are you kidding me?",
                string.Format("~y~*{0} frantically looks around, confused and scared*~s~", vicData.LastName)
            };

            PedQuestion q5 = new PedQuestion();
            q5.Question = "Conclude investigation: Release";
            q5.Answers = new List<string>()
            {
                "Thank you officer.",
                "~y~*You hand them a card for a domestic abuse hotline*~s~",
                "~y~*With tears in their eyes they nod at you*~s~",
                string.Format("~y~*{0} quickly and nervously walks away*~s~", vicData.Gender == Gender.Male ? "He" : "She")
            };
        }

        public void SuspectQuestions()
        {
            PedQuestion q1 = new PedQuestion();
            q1.Question = "Can you tell me what’s going on here?";
            q1.Answers = new List<string>(){
                "There’s nothing going on officer.",
                $"~y~*Person looks angrily at {vicData.LastName}.*~s~",
                "Why are you guys even here?",
                "Don’t worry about it. It’s a settled issue."
            };

            PedQuestion q2 = new PedQuestion();
            q2.Question = $"~y~*Inspect {susData.LastName}'s hands for marks.*~s~";
            q2.Answers = new List<string>(){
                "~y~Notice~s~: Redness to back of right hand.",
                "~y~Notice~s~: No markings visible.",
                "~y~Notice~s~: Minor redness to neck.",
                "~y~Notice~s~: Small laceration to right check. Unclear if defensive wound.",
            };

            PedQuestion q3 = new PedQuestion();
            q3.Question = "Ask about markings, if any are present";
            q3.Answers = new List<string>(){
                "It’s nothing officer, I’m fine.",
                "We got a little carried away. It’s done now, you can leave.",
                "Happened the other day at work.",
                "I’m clumsy... Must have fallen."
            };

            PedQuestion q4 = new PedQuestion();
            q4.Question = "Conclude investigation: Arrest";
            q4.Answers = new List<string>(){
                "Are you serious?!? I didn’t do anything!",
                "Fuck you pig, stay away from me!",
                string.Format("For what?!?!? I didn’t even touch {0}!!", vicData.Gender == Gender.Male ? "him" : "her"),
                "This is bullshit…"
            };

            PedQuestion q5 = new PedQuestion();
            q5.Question = "Conclude investigation: Release";
            q5.Answers = new List<string>()
            {
                "Thank you officer, won’t happen again.",
                $"~y~*{susData.LastName} nods at you*~s~",
                "Cool...",
                $"~y~*{susData.LastName} shows signs of relief*~s~"
            };
        }
    }
}
