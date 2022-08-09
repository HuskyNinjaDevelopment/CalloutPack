using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalloutPack
{
    [CalloutProperties("Dead Animal in Road", "HuskyNinja", "v1.0")]
    internal class DeadAnimalInRoad : Callout
    {
        public readonly Random rng = new Random();
        public Vector3 calloutCoords;
        public PedHash animalHash;
        public List<PedHash> animalList = new List<PedHash>()
        {
            PedHash.Boar,
            PedHash.Coyote,
            PedHash.Deer,
            PedHash.MountainLion,
        };
        public Ped animal;
        public int density, flags;

        public DeadAnimalInRoad()
        {
            animalHash = animalList.SelectRandom();
            bool foundNode = API.GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, rng.Next(350,500), ref calloutCoords, 1, 1, 1);
            while (!foundNode)
            {
                foundNode = API.GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, rng.Next(350, 500), ref calloutCoords, 1, 1, 1);      
            }

            bool gotProperties = API.GetVehicleNodeProperties(calloutCoords.X, calloutCoords.Y, calloutCoords.Z, ref density, ref flags);
            if(gotProperties && density > 4 && density < 12)
            {
                API.GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, rng.Next(350,500), ref calloutCoords, 1, 1, 1);
                gotProperties = API.GetVehicleNodeProperties(calloutCoords.X, calloutCoords.Y, calloutCoords.Z, ref density, ref flags);
            }    

            InitInfo(calloutCoords);
            ShortName = "Animal In Roadway";
            ResponseCode = 2;
            StartDistance = 175f;
            CalloutDescription = $"A dead animal is blocking the roadway in the {World.GetZoneLocalizedName(calloutCoords)} area. Head over there and call Animal Control to the scene.";
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
            animal = await SpawnPed(animalHash, calloutCoords);
            animal.Kill();

            await Task.FromResult(0);
        }
    }
}
