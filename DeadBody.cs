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
    [CalloutProperties("Dead Body","HuskyNinja","v1.0")]
    internal class DeadBody : Callout
    {
        public List<Vector3> calloutLocations = new List<Vector3>()
        {
            new Vector3(-1567.34f, 749.92f, 192.58f),
            new Vector3(-1573.06f, 771.45f, 189.19f),
            new Vector3(-1462.25f, 179.30f, 54.77f),
            new Vector3(-27.44f, -1307.06f, 29.56f),
            new Vector3(-570.42f, -1676.99f, 19.62f),
            new Vector3(379.06f, -1830.08f, 28.67f),
            new Vector3(124.96f, -1185.44f, 29.50f),
            new Vector3(-97.25f, -1001.56f, 21.28f),
            new Vector3(266.03f, -2430.39f, 8.04f),
            new Vector3(-1464.79f, -1092.01f, 0.29f)
        };
        public Vector3 calloutLocation;

        public Ped victim;

        public DeadBody()
        {
            calloutLocation = calloutLocations.SelectRandom();

            InitInfo(calloutLocation);
            ShortName = "Dead Body";
            CalloutDescription = $"A Dead Body has turned up in the {calloutLocation} area.";
            StartDistance = 165f;
            ResponseCode = 1;
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

            victim = await SpawnPed(RandomUtils.GetRandomPed(), calloutLocation);
            victim.Kill();

            await Task.FromResult(0);
        }
    }
}
