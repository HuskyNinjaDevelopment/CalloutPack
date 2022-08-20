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
    [CalloutProperties("Abandonded Vehicle", "HuskyNinja", "v1.0")]
    internal class AbandondedVehicle : Callout
    {
        public readonly Random rng = new Random();

        public Vehicle veh;
        public VehicleData vehData;

        public Vector3 vehCoords;

        public float heading;
        public int unk1;

        public List<Vector3> carTorchLocations = new List<Vector3>()
        {
            new Vector3(1390.56f, -761.90f, 66.87f),
            new Vector3(705f, -291.22f, 59.18f),
            new Vector3(613.15f, -885.36f, 11.17f),
            new Vector3(697.71f, -1159.39f, 24.29f),
            new Vector3(1222.52f, -2176.63f, 41.82f),
            new Vector3(-1228.82f, -2039.08f, 13.54f),
            new Vector3(-1193.88f, -1485.63f, 4.38f),
            new Vector3(-1697.74f, -896.77f, 8.09f),
            new Vector3(-1538.43f, 341.27f, 86.5f),
            new Vector3(-1221.95f, -645.87f, 40.36f),
            new Vector3(-1555.36f, -993.54f, 13.02f)
        };

        public bool carTorched = false;

        public AbandondedVehicle()
        {
            //Determine which variation of the callout will be played out. 1.Someone Left the car 2.Stolen vehicle burned and left
            
            if (rng.Next(0, 100) >= 75)
            {
                carTorched = true;
                vehCoords = carTorchLocations.SelectRandom();
            }
            else
            {
                Vector3 playerPos = Game.PlayerPed.Position;

                //Find road spot on road
                bool foundPos = API.GetNthClosestVehicleNodeWithHeading(playerPos.X, playerPos.Y, playerPos.Z, rng.Next(500, 750), ref vehCoords, ref heading, ref unk1, 9, 3.0f, 2.5f);
                while (!foundPos)
                {
                    BaseScript.Delay(10);
                    foundPos = API.GetNthClosestVehicleNodeWithHeading(playerPos.X, playerPos.Y, playerPos.Z, rng.Next(500, 750), ref vehCoords, ref heading, ref unk1, 9, 3.0f, 2.5f);
                }
                //Get boundry
                bool isRoadside = API.GetRoadBoundaryUsingHeading(vehCoords.X, vehCoords.Y, vehCoords.Z, heading, ref vehCoords);
                if (!isRoadside)
                {
                    API.GetNthClosestVehicleNodeWithHeading(playerPos.X, playerPos.Y, playerPos.Z, rng.Next(500, 750), ref vehCoords, ref heading, ref unk1, 9, 3.0f, 2.5f);
                    isRoadside = API.GetRoadBoundaryUsingHeading(vehCoords.X, vehCoords.Y, vehCoords.Z, heading, ref vehCoords);

                }
            }

            InitInfo(vehCoords);
            ShortName = "Abandonded Vehicle";
            ResponseCode = 1;
            CalloutDescription = $"An abandonded vehicle has been reported in the {World.GetZoneLocalizedName(vehCoords)} area.";
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
            
            if(carTorched)
            {
                API.RequestModel((uint)API.GetHashKey("prop_rub_carwreck_14"));
                while(!API.HasModelLoaded((uint)API.GetHashKey("prop_rub_carwreck_14"))) { await BaseScript.Delay(10); }

                int car = API.CreateObject(API.GetHashKey("prop_rub_carwreck_14"), vehCoords.X, vehCoords.Y, vehCoords.Z - 1.20f, true, true, true);

                while(World.GetDistance(vehCoords, Game.PlayerPed.Position) > 75f) { await BaseScript.Delay(10); }

                for (int i = 0; i < 25; i++)
                {
                    Vector3 coords = vehCoords.Around(0.35f);
                    int fire = API.StartScriptFire(coords.X, coords.Y, coords.Z, 25, true);
                }

                while(World.GetDistance(vehCoords, Game.PlayerPed.Position) > 20f) { await BaseScript.Delay(10); }
                API.AddExplosion(vehCoords.X, vehCoords.Y, vehCoords.Z, 34, 0f, true, false, 1.0f);

                int blip = API.AddBlipForEntity(car);
                API.SetBlipSprite(blip, 436);
            }
            else
            {
                veh = await SpawnVehicle(RandomUtils.GetRandomVehicle(VehicleClass.Compacts), vehCoords, heading);
                vehData = await veh.GetData();

                vehData.Flag = "Abandonded Vehicle";
                veh.SetData(vehData);

                ShowNetworkedNotification("The ~y~Abandonded Vehicle~s~ has been ~r~marked~s~.", "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "~y~Callout Update~s~", 1f);
                veh.AttachBlip();
                veh.AttachedBlip.Sprite = (BlipSprite)326;
            }

            await Task.FromResult(0);
        }
    }
}
