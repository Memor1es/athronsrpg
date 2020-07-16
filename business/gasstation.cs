using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using rpg.Database;
using System.Threading.Tasks;

namespace rpg
{
    public class gasStation : Script
    {
        public class station
        {
            public Vector3 position { get; set; }
            public station() { }
            public  station(Vector3 position)
            {
                this.position = position;
      
            }
        }
        public class external
        {
            public List<station> stations { get; set; }
            public bool isShop { get; set; }
            public Vector3 center { get; set; }
            public Vector3 pedPosition { get; set; }
        }

       [ServerEvent(Event.ResourceStart)]
        public async void onResourceStart()
        {
           await db.loadAllGasStations();
            NAPI.Task.Run(() =>
            {
                
                foreach (var gasStation in business.getBusinessList(type.gasStation))
                {
                    business.createBusiness( gasStation );

                    ColShape gscolshape = NAPI.ColShape.CreateSphereColShape(gasStation.external.center, 40);
                    gscolshape.SetData("gasStation", 1);
                    gscolshape.SetData("bizid", gasStation.bizId);
                
               
                    Console.WriteLine("Gas Stations loaded.");
                }
            });
        }
 
       [Command("gastest")]
       public void sd(Player client)
        {
            if (client.Vehicle != null)
            client.TriggerEvent("testgs", client.Vehicle.GetSharedData<float>("fuel"));
        }
        [RemoteEvent("onPlayerBuyFuel")]
        public void onPlayerBuyFuel(Player client, float fuel, int price)
        {
            if (client.Vehicle != null)
            {
                client.Vehicle.SetSharedData("fuel", fuel);
                client.SendChatMessage($"You fed the car with {fuel} for {price}, now go and pay.");
                client.SetData("needToPayFuel", (client.HasData("needToPayFuel") ? client.GetData<int>("needToPayFuel") : 0 ) + price);
        
            }
        }
        [RemoteEvent("onPlayerPayFuel")]
        public async void onPlayerPayFuel(Player client, int bizId)
        {
            var price  =  client.GetData<int>( "needToPayFuel" );
            await client.takeMoneyAsync( price ) ;
             await business.utilities.addBalance( bizId, price );
            client.ResetData("needToPayFuel");
        }
        [Command("setskin")]
        public void sad(Player client, string id, PedHash ped)
        {
            var target = playerManager.getPlayer(id);
            if (target == null)
            {
                client.SendChatMessage("nu exista.");
                return;
            }
            target.SetSkin(ped);
        }
        [ServerEvent(Event.PlayerEnterColshape)]
        public void onPlayerEntercolShape(ColShape shape, Player player)
        {
            if (shape.HasData("gasStation") && shape.HasData("bizid"))
            {
                player.TriggerEvent("onPlayerEnterGasStation", business.businessList.Find(shop => shop.bizId == shape.GetData<int>("bizid")));
            }
          
        }
        [ServerEvent(Event.PlayerExitColshape)]
        public async void onPlayerExitcolShape(ColShape shape, Player player)
        {
            if (shape.HasData("gasStation") && shape.HasData("bizid"))
            {
                player.TriggerEvent("onPlayerExitGasStation");
                if (player.HasData("needToPayFuel") && player.GetData<int>("needToPayFuel") > 0)
                {
                    await policeDepartment.givePlayerWanted(player, null, 1, "Furt benzina", true);
                    player.SendChatMessage("~r~You left the gas station and got one wanted level for your crime.");
                }
            }

        }
       [Command("gassetped")]
       public void gasSetPed(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("gasCreation"))
            {

                player.SendChatMessage("Nu esti in crearea  unui gas station.");
                return;
            }

            var gasStation = player.GetData<business.model>("gasCreation");
            if (gasStation.external.isShop)
            {
                player.SendChatMessage("Nu poti seta pozitia pedului deoarece e un gasstaion de tip macasin.");
                return;
            }
            gasStation.external.pedPosition = player.Position;
            player.SendChatMessage("A fost setata pozitia pedului.");
         
        }

        [Command("gssetcenter")]
        public void gssetcentre(Player client)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!client.HasData("gasCreation"))
            {

                client.SendChatMessage("Nu esti in crearea  unui gas station.");
                return;
            }
            var gasStation = client.GetData<business.model>("gasCreation");
            gasStation.external.center = client.Position;
            client.SendChatMessage("a fost setatt centru.");
        }
        [Command("addstation")]
        public void addStation(Player client)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!client.HasData("gasCreation"))
            {

                client.SendChatMessage("Nu esti in crearea  unui gas station.");
                return;
            }
            var gasStation = client.GetData<business.model>("gasCreation");
            gasStation.external.stations.Add(new station(client.Position));

        }
        [Command("finishgs")]
        public async void onPlayerFinishGasStation(Player client)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!client.HasData("gasCreation"))
            {

                client.SendChatMessage("Nu esti in crearea  unui gas station.");
                return;
            }
            var gasStation = client.GetData<business.model>("gasCreation");
           if (gasStation.external.stations.Count == 0)
            {
                client.SendChatMessage("Nu ai setat statile.");
                return;
            }
           if (!gasStation.external.isShop && gasStation.external.pedPosition == new Vector3(0,0,0))
            {

                client.SendChatMessage("GasStationul este unull de tip alone la care trebuie sa setezio positia pedilui.");
                return;
            }

           await db.createGasStation(gasStation);
            client.SendChatMessage("A fost fnishat cu suces.");
        }
        [Command("addgs")]
        public void onPlayerAddGasStation(Player client, string name, bool isShop)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            business.model gasStation = new business.model();
            gasStation.enterPos = client.Position;
            gasStation.owner = "AdmBot";
            gasStation.external = new external();
            gasStation.external.stations = new List<station>();
            gasStation.external.isShop = isShop;
            gasStation.name = name;
            client.SetData("gasCreation", gasStation);
            client.SendChatMessage($"Gas statio {gasStation.bizId} created. Use /addstation {gasStation.bizId}.");
        }
        public class db
        {
            public static async Task<int> createGasStation(business.model gs)
            {
                return await databaseManager.updateQuery($"INSERT INTO business (owner, name, type, enterPos, external) VALUES ('{gs.owner}', '{gs.name}', '{(int)type.gasStation}', '{NAPI.Util.ToJson(gs.enterPos)}', '{NAPI.Util.ToJson(gs.external)}')").Execute();
            }
           

            public static async Task loadAllGasStations()
            {
         
                await databaseManager.selectQuery($"SELECT * FROM business WHERE type = '{(int)type.gasStation}'", (DbDataReader reader) =>
                {
                    business.model gs = new business.model(); ;
                    gs.owner = (string)reader["owner"];
                    gs.enterPos = (Vector3)NAPI.Util.FromJson<Vector3>(reader["enterPos"]);
                    gs.sale = (int)reader["sale"];
                    gs.external = NAPI.Util.FromJson<external>((string)reader["external"]);
                    gs.name = (string)reader["name"];
                    gs.bizId = (int)reader["id"];
                    gs.type = type.gasStation;
                    gs.balance = ( int ) reader[ "balance" ];
                    gs.type = type.gasStation;
                   
                    business.businessList.Add( gs );
                } ).Execute();

             
            }
        }
    }
}
