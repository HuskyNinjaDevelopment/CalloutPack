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
        public Ped player;
        public Vehicle veh;
        public VehicleData vehData;
        public Vector3 vehCoords;
        public float heading;
        public int unk1;
        public AbandondedVehicle()
        {
            player = Game.PlayerPed;
            Vector3 playerPos = player.Position;

            //Find road spot on road
            bool foundPos = API.GetNthClosestVehicleNodeWithHeading(playerPos.X, playerPos.Y, playerPos.Z, 750, ref vehCoords, ref heading, ref unk1, 9, 3.0f, 2.5f);
            while (!foundPos)
            {
                BaseScript.Delay(50);
                foundPos = API.GetNthClosestVehicleNodeWithHeading(playerPos.X, playerPos.Y, playerPos.Z, 750, ref vehCoords, ref heading, ref unk1, 9, 3.0f, 2.5f);
            }
            //Get boundry
            bool isRoadside = API.GetRoadBoundaryUsingHeading(vehCoords.X, vehCoords.Y, vehCoords.Z, heading, ref vehCoords);
            if(!isRoadside)
            {
                playerPos = player.Position;
                foundPos = API.GetNthClosestVehicleNodeWithHeading(playerPos.X, playerPos.Y, playerPos.Z, 750, ref vehCoords, ref heading, ref unk1, 9, 3.0f, 2.5f);
                API.GetClosestVehicleNodeWithHeading(vehCoords.X, vehCoords.Y, vehCoords.Z, ref vehCoords, ref heading, 5, 3.0f, 0);
                isRoadside = API.GetRoadBoundaryUsingHeading(vehCoords.X, vehCoords.Y, vehCoords.Z, heading, ref vehCoords);

            }
            InitInfo(vehCoords);
            ShortName = "Abandonded Vehicle";
            ResponseCode = 1;
            CalloutDescription = $"An abandonded vehicle has been reported in the {World.GetZoneLocalizedName(vehCoords)} area. Find the vehicle and get it towed.";
            StartDistance = 150f;
        }

        public override async Task OnAccept()
        {
            InitBlip(175f);
            UpdateData();
            await Task.FromResult(0);
        }

        public override async void OnStart(Ped closest)
        {
            base.OnStart(closest);

            veh = await SpawnVehicle(RandomUtils.GetRandomVehicle(VehicleClass.Compacts), vehCoords, heading);
            vehData = await veh.GetData();

            vehData.Flag = "Abandonded Vehicle";
            veh.SetData(vehData);

            ShowNetworkedNotification("The ~y~Abandonded Vehicle~s~ has been ~r~marked~s~.", "CHAR_CALL911", "CHAR_CALL911","Dispatch","~y~Callout Update~s~",1f);
            veh.AttachBlip();
            await Task.FromResult(0);
        }
    }
}
