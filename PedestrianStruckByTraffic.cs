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
    [CalloutProperties("Pedestrian Struck By Traffic","HuskyNinja","v1.0")]
    internal class PedestrianStruckByTraffic : Callout
    {
        public Vector3 calloutLocation;
        public float heading;
        public int unk1;

        public Vehicle veh;
        public VehicleData vehData;

        public Ped victim, driver;
        public Ped victimData, driverData;
        
        public PedestrianStruckByTraffic()
        {
            Vector3 randomPOS = Game.PlayerPed.Position.Around(500f);

            //Find road spot on road
            bool foundPos = API.GetNthClosestVehicleNodeWithHeading(randomPOS.X, randomPOS.Y, randomPOS.Z, 3, ref calloutLocation, ref heading, ref unk1, 9, 3.0f, 2.5f);
            while (!foundPos)
            {
                BaseScript.Delay(50);
                randomPOS = Game.PlayerPed.Position.Around(500f);
                foundPos = API.GetNthClosestVehicleNodeWithHeading(randomPOS.X, randomPOS.Y, randomPOS.Z, 3, ref calloutLocation, ref heading, ref unk1, 9, 3.0f, 2.5f);
            }
            //Get boundry
            bool isRoadside = API.GetRoadBoundaryUsingHeading(calloutLocation.X, calloutLocation.Y, calloutLocation.Z, heading, ref calloutLocation);
            if (!isRoadside)
            {
                randomPOS = Game.PlayerPed.Position.Around(500f);
                foundPos = API.GetNthClosestVehicleNodeWithHeading(randomPOS.X, randomPOS.Y, randomPOS.Z, 3, ref calloutLocation, ref heading, ref unk1, 9, 3.0f, 2.5f);
                isRoadside = API.GetRoadBoundaryUsingHeading(calloutLocation.X, calloutLocation.Y, calloutLocation.Z, heading, ref calloutLocation);
            }

            InitInfo(calloutLocation);
            ShortName = "Pedestrian Struck by Vehicle";
            ResponseCode = 3;
            StartDistance = 165f;
            CalloutDescription = "A Pedestrain has been struck by a vehicle. Head to the area and give aid to the victim.";
        }

        public override async Task OnAccept()
        {
            InitBlip(150f);
            UpdateData();

            await Task.FromResult(0);
        }

        public override async void OnStart(Ped closest)
        {
            base.OnStart(closest);

            API.RequestAnimDict("mini@cpr@char_b@cpr_def");
            while(API.HasAnimDictLoaded("mini@cpr@char_b@cpr_def")) {
                API.RequestAnimDict("mini@cpr@char_b@cpr_def"); 
                await BaseScript.Delay(10);
            }

            veh = await SpawnVehicle(RandomUtils.GetVehicleHashesForClass(VehicleClass.Coupes).SelectRandom(), calloutLocation, heading);
            vehData = await veh.GetData();

            driver = await SpawnPed(RandomUtils.GetRandomPed(), veh.Position);
            driver.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);

            Vector3 offset = veh.Position + (veh.ForwardVector * 3f);

            victim = await SpawnPed(RandomUtils.GetRandomPed(), offset);
            await BaseScript.Delay(1000);
            victim.Task.PlayAnimation("mini@cpr@char_b@cpr_def", "cpr_pumpchest_idle", 8.0f, -1, AnimationFlags.Loop);
            victim.BlockPermanentEvents = true;
            victim.AlwaysKeepTask = true;

            veh.AttachBlip();

            await Task.FromResult(0);
        }
    }
}
