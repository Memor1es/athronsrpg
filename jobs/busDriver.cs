using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using rpg.Database;

namespace rpg
{
    class busJob : Script
    {
        public class point
        {
            public Vector3 position { get; set; }
            public string name { get; set; }
            public point() { }
            public point(Vector3 position, string name) { this.position = position; this.name = name; }
        }

        public class busRoute
        {
            public int id { get; set; }
            public List<point> stations { get; set; }
            public string name { get; set; }
            public  busRoute(){}
            public busRoute(int id) { this.id = id; }
          }

        static List<busRoute> routes = new List<busRoute>();
        [ServerEvent(Event.ResourceStart)]
        public async void onResourceStart()
        {
            NAPI.Task.Run(() =>
            {
                NAPI.TextLabel.CreateTextLabel($"~w~ID: ~r~{(int)playerManager.job.busdriver}\n~w~Job: ~r~Bus Driver\n~w~Use ~r~/getjob ~w~to get this job.", new Vector3(451.14493, -659.4379, 28.48024), 15, 1, 4, new Color(0, 162, 255, 255));
                jobs.jobList.Add(new jobs.jobModels(playerManager.job.busdriver, "Bus Driver", new Vector3(451.14493, -659.4379, 28.48024)));

                NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("s_m_m_pilot_01"), new Vector3(451.14493, -659.4379, 28.48024), 310f, false, true, true);

            });
            routes = await db.readBusRoutes();
        }
        [Command("createroute")]
        public void createRoute(Player client, string name)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (client.HasData("createdBusRoute")) { client.SendChatMessage("Esti deja intr-un route creation."); return; };
 
            var rotue = new busRoute();
            rotue.name = name;
            rotue.stations = new List<point>();
            client.SetData("createdBusRoute", rotue);
 
        }
        [Command("routes")]
        public void showRoutes(Player client)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            client.SendChatMessage("Rutele sunt:");
            foreach (var item in routes)
            {
                client.SendChatMessage($"ID: {item.id} NAME: {item.name} STATIONS: {item.stations.Count}");
            }
        }
        [Command("showstations")]
        public void showStations(Player client, int id)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            var stations = routes.Find(s => s.id == id).stations;
            if (stations == null)
            {
                client.SendChatMessage("nu exista.");
                return;
            }
            for (int i = 0; i < stations.Count; i++)
                client.SendChatMessage($"ID: {i} NAME: {stations[i].name} POSITION: {NAPI.Util.ToJson(stations[i].position)}");
        
            
        }
        [Command("deleteroute")]
        public void deleteRoute(Player client)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!client.HasData("createdBusRoute")) { client.SendChatMessage("Nu esti intr-un route creation."); return; };

            client.ResetData("createdBusRoute");

        }


        [Command("addpoint")]
        public void addPoint(Player client, string name)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!client.HasData("createdBusRoute")) { client.SendChatMessage("Nu esti intr-un route creation."); return; };
            var route = client.GetData<busRoute>("createdBusRoute");
            var point = new point(client.Position, name);
            route.stations.Add(point);
            client.SetData("createdBusRoute",route);
            client.TriggerEvent("debug:CreateBusPoint", point);
        }

        [Command("points")]
        public void getPoints(Player client)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!client.HasData("createdBusRoute")) { client.SendChatMessage("Nu esti intr-un route creation."); return; };
            var route = client.GetData<busRoute>("createdBusRoute");
            for (int i = 0; i < route.stations.Count; i++)
            {
                var item = route.stations[i];
                client.SendChatMessage($"ID: {i} NAME : {item.name}, POSITION : {NAPI.Util.ToJson(item.position)}");
            }
           
          
        }

        [Command("deletepoint")]
        public void deletePoint(Player client, int id)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!client.HasData("createdBusRoute")) { client.SendChatMessage("Nu esti intr-un route creation."); return; };

            var route = client.GetData<busRoute>("createdBusRoute");
           
            route.stations.Remove(route.stations[id]);
            client.SetData("createdBusRoute", route);
        }

        [Command("finishroute")]
        public async void finishRoute(Player client)
        {
            if (playerManager.doesPlayerHasAdmin(client) <= 0)
            {
                client.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!client.HasData("createdBusRoute")) { client.SendChatMessage("Nu esti intr-un route creation."); return; };
            var route = client.GetData<busRoute>("createdBusRoute");
            await db.createBusRoute(route);
            client.ResetData("createdBusRoute");
          
            client.SendChatMessage("Ai creaat ruta cu success.");
        }
        [Command("startbus")]
        public void startBus(Player client, int cursa)
        {
            var route = routes.Find(a => a.id == cursa);

            client.SetSharedData("inJob", true);
            client.SetSharedData("job", 5);
            
            client.TriggerEvent("job:startBus", route);
        }
        [Command("busmenu")]
        public void busmenu(Player client)
        {
            client.TriggerEvent("job:showBusMenu", routes.Select(l => l.name).ToList());

        }
        [RemoteEvent("job:busStart")]
        public void busStart(Player client, string route)
        {
            var selectedRoute = routes.Find(r => r.name == route);
            client.SetSharedData("inJob", true);
            client.SetSharedData("job", 5);

            client.TriggerEvent("job:startBus", selectedRoute);
         
        }
       
        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player client, Vehicle vehicle, sbyte seatID)
        {
            if (vehicle.HasData("type") && vehicle.GetData<vehicleManager.type>("type") == vehicleManager.type.bus)
            {
                if (jobs.doesPlayerHasJob(client) != playerManager.job.busdriver)
                {
                    client.SendChatMessage("!{#2CB6D0}You are not a Bus Driver.");
                    client.WarpOutOfVehicle();
                    return;
                }

                client.TriggerEvent("job:showBusMenu", routes.Select(l => l.name).ToList());

                vehicle.SetData("in_use", true);
                vehicle.SetData("need_respawn", false);
                client.SetSharedData("inJob", true);
            }

        }
        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player client, Vehicle vehicle)
        {
            if (vehicle == null)
                return;

            if (vehicle.HasData("in_use")  && vehicle.GetData<bool>("in_use") && vehicle.HasData("type") && vehicle.GetData<vehicleManager.type>("type") == vehicleManager.type.bus)
            {
                vehicle.SetData("in_use", false);
                vehicle.SetData("need_respawn", true);
                client.SetSharedData("inJob", false);
            }

        }
        public class db
        {
            public static async Task<int> createBusRoute(busRoute gs)
            {
                return await databaseManager.updateQuery($"INSERT INTO busroutes (name, points) VALUES ('{gs.name}', '{NAPI.Util.ToJson(gs.stations)}')").Execute();
            }
            public static async Task<List<busRoute>> readBusRoutes()
            {
                List<busRoute> routes = new List<busRoute>();
                await databaseManager.selectQuery("SELECT * FROM busroutes", (DbDataReader reader) =>
                {
                    busRoute route = new busRoute();
                    route.name = (string)reader["name"];
                    route.stations = NAPI.Util.FromJson<List<point>>((string)reader["points"]);
                    route.id = (int)reader["id"];
                    routes.Add(route);
                }).Execute();
                return routes;
            }

            
        }


    }
}
