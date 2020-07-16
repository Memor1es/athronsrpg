using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace rpg
{
    public class dealerShip : Script
    {
        static Vector3 pos = new Vector3(-56.74555, -1096.729, 26.42234);
        [ServerEvent(Event.ResourceStart)]
        public void Dealership_Start()
        {
            NAPI.Task.Run(() =>
            {
                NAPI.Marker.CreateMarker(1, new Vector3(pos.X, pos.Y, pos.Z - 0.90f), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1f, 140, 4, 36, false, 0);
                NAPI.Blip.CreateBlip(545, pos, 1, 22, "DS Premium Deluxe");
                NAPI.TextLabel.CreateTextLabel($"[DS Premium Deluxe]", new Vector3(pos.X, pos.Y, pos.Z + 0.20f), 20.0f, 0.75f, 4, new Color(186, 186, 186), false);
                NAPI.TextLabel.CreateTextLabel($"Press F to browse the available vehicles.", pos, 20.0f, 0.75f, 4, new Color(186, 186, 186, 200), false);
            });
        }

        public static List<Vector3> CARSHOP_SPAWNS = new List<Vector3>()
        {
            new Vector3(-47.8021f, -1116.419f, 26.43427f),
            new Vector3(-50.66175f, -1116.753f, 26.4342f),
            new Vector3(-53.51776f, -1116.721f, 26.43449f),
            new Vector3(-56.41209f, -1116.901f, 26.43442f)
        };

      
        [RemoteEvent("purchaseVehicle")]
        public async void PurchaseVehicleEvent(Player player, String name, int vehiclePrice, int primary_color, int secondary_color)
        {

            VehicleHash vehicleHash = (VehicleHash)NAPI.Util.GetHashKey(name);
            bool nosuntsloturi = false;
            if (vehiclePrice > 0 && player.GetSharedData<int>("money") >= vehiclePrice)
            {
               
                for (int i = 0; i < CARSHOP_SPAWNS.Count; i++)
                {
                    bool spawnOccupied = false;
                    foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                    {
                        Vector3 vehPos = NAPI.Entity.GetEntityPosition(veh);
                        if (CARSHOP_SPAWNS[i].DistanceTo(vehPos) < 2.5f)
                        {
                            spawnOccupied = true;
                            break;
                        }
                    }
                    if (!spawnOccupied)
                    {
                        vehicleManager.vehicleModel vehModel = new vehicleManager.vehicleModel();
                        vehModel.model = name;
                        vehModel.rotation = 0;
                        vehModel.numberPlate = "LS-3145";
                        vehModel.primaryColor = primary_color;
                        vehModel.secondaryColor = secondary_color;
                        vehModel.spawnPosition = CARSHOP_SPAWNS[i];
                        vehModel.type = vehicleManager.type.personal;
                        vehModel.fuel = 100;
                        vehModel.kms = 0;
                        vehModel.owner = player.Name.ToLower();

                        int id = await vehicleManager.createVehicle(vehModel);


                        player.SendChatMessage("Car succesfully bought!");
                        player.Dimension = 0;
                        return;
                    }
                    nosuntsloturi = spawnOccupied;
                }
                if (nosuntsloturi)
                    player.SendChatMessage("No more parking space.");
            }


        }

    }
}
