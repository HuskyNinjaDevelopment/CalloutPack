using CitizenFX.Core;
using FivePD.API;
using FivePD.API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalloutPack
{
    [CalloutProperties("Tow Truck Altercation", "HuskyNinja", "v1.0")]
    internal class TowTruckAltercation : Callout
    {
        public PedHash towDriverHash = PedHash.Autoshop02SMM;
        public VehicleHash towTruckHash = VehicleHash.TowTruck;

        public List<Vector3> vehicleLocations = new List<Vector3>()
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
        public Vector3 calloutCoords;

        public Vehicle towTruck, vehicle;
        public Ped towDriver, driver;

        public TowTruckAltercation()
        {
            calloutCoords = vehicleLocations.SelectRandom();
            InitInfo(calloutCoords);
            StartDistance = 175f;
            ResponseCode = 2;
            ShortName = "Tow Truck Altercation";
            CalloutDescription = "A Tow Truck Driver has called 911 because the owner of the vehicle he is towing came back and started attacking him for doing his job. Head to the scene and help the driver.";
        }

        public override async Task OnAccept()
        {
            InitBlip(50f);
            UpdateData();
            await Task.FromResult(0);
        }

        public override async void OnStart(Ped closest)
        {
            base.OnStart(closest);

            towTruck = await SpawnVehicle(towTruckHash, calloutCoords.Around(3f));
            vehicle = await SpawnVehicle(RandomUtils.GetRandomVehicle(VehicleClass.Compacts), calloutCoords.Around(3f));

            driver = await SpawnPed(RandomUtils.GetRandomPed(), new Vector3(vehicle.Position.X + 1f, vehicle.Position.Y,vehicle.Position.Z));
            towDriver = await SpawnPed(towDriverHash, new Vector3(towTruck.Position.X + 1f, towTruck.Position.Y, towTruck.Position.Z));

            DriverQuestions();
            TowTruckDriverQuestions();

            driver.Task.TurnTo(towDriver);
            await BaseScript.Delay(500);
            towDriver.Task.TurnTo(driver);

            while(World.GetDistance(driver.Position, Game.PlayerPed.Position) > 30f) { await BaseScript.Delay(50); }
            driver.Task.FightAgainst(towDriver);
            ShowDialog("I'm not going to let you tow my car.", 8000, 1f);

            await Task.FromResult(0);
        }

        private void TowTruckDriverQuestions()
        {
            PedQuestion q1 = new PedQuestion();
            q1.Question = "What's going on here?";
            q1.Answers = new List<string>() { "I got a request to remove this vehicle from the premisis by the property owner." };

            PedQuestion q2 = new PedQuestion();
            q2.Question = "Why were you two fighting?";
            q2.Answers = new List<string>() { String.Format("I got here and tried to hook up the vehicle, this person ran over and started attacking me.") };

            PedQuestion[] questions = new PedQuestion[] { q1, q2 };
            AddPedQuestions(towDriver, questions);
        }

        private void DriverQuestions()
        {
            PedQuestion q1 = new PedQuestion();
            q1.Question = "What's going on here?";
            q1.Answers = new List<string>()
            {
                "This jerk is trying to tow my vehicle!",
                "I came back after 15 minutes to find my car getting towed!",
                "I'm not letting them tow my car!",
            };

            PedQuestion q2 = new PedQuestion();
            q2.Question = "Why were you two fighting?";
            q2.Answers = new List<string>() { 
                "I had to do something, they were trying to tow my car!",
                "They wouldn't listent to me when I told them to stop.",
                "I was upset this asshole was trying to take my car.",
            };

            PedQuestion[] questions = new PedQuestion[] { q1, q2 };
            AddPedQuestions(driver, questions);
        }
    }
}
