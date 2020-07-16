using GTANetworkAPI;
using rpg.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace rpg
{
    class vehicleManager : Script
    {
        public enum type
        {
            free,
            personal,
            bus,
            farmer,
            pizza,
            garbage,
            dmv,
            trucker
        }
        [ServerEvent(Event.ResourceStart)]
        public async Task VehicleManagerStartAsync()
        {
            await databaseManager.selectQuery($"SELECT * FROM vehicles", (DbDataReader reader) =>
            {
                if (reader.HasRows)
                {

                    vehicleModel veh = new vehicleModel();
                    veh.id = (int)reader["id"];
                    veh.owner = (string)reader["owner"];
                    veh.model = (string)reader["model"];
                    veh.type = (type)(int)reader["type"];
                    veh.days = (int)(DateTime.Now - (DateTime)reader["creationDate"]).TotalDays;
                    veh.spawnPosition = NAPI.Util.FromJson<Vector3>((string)reader["spawnPosition"]);
                    veh.primaryColor = (int)reader["primaryColor"];
                    veh.secondaryColor = (int)reader["secondaryColor"];
                    veh.numberPlate = (string)reader["numberPlate"];
                    veh.fuel = (int)reader["fuel"];
                    veh.kms = (int)reader["kms"];
                    veh.insurances = (int)reader["insurances"];
                    veh.rotation = (float)reader["rotation"];
                    veh.locked = (bool)reader["locked"];


                    vehicles.Add(veh);
                }
            }).Execute();

            foreach (var vehicle in vehicles)
            {
                if (vehicle.type != type.personal)
                    spawnVehicle(vehicle);
            }
            var timer_vehicles = new System.Timers.Timer();
            timer_vehicles.Interval = 15000;
            timer_vehicles.Elapsed += timerVehicle;
            timer_vehicles.Start();
        }
       
        [Command("enginestatus")]
        public void sda(Player player)
        {
            if (player.Vehicle != null)
            {
                player.SendChatMessage("engine status" + player.Vehicle.EngineStatus);
                player.SendChatMessage("sv engine status" + player.Vehicle.GetSharedData<bool>("setEngine"));
            }
        }
        public static void spawnVehicle(vehicleModel vehicleModel)
        {
            NAPI.Task.Run(() =>
            {
                Vehicle vehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(vehicleModel.model), vehicleModel.spawnPosition, 0f, vehicleModel.primaryColor, vehicleModel.secondaryColor, vehicleModel.numberPlate, 255, false, false);
                vehicle.SetSharedData("invincible", true);
                vehicle.NumberPlate = vehicleModel.numberPlate;
                vehicle.PrimaryColor = vehicleModel.primaryColor;
                vehicle.SecondaryColor = vehicleModel.secondaryColor;
                vehicle.Rotation = new Vector3(0, 0, vehicleModel.rotation);
                vehicle.SetData("id", vehicleModel.id);
                vehicle.SetData("owner", vehicleModel.owner);
                vehicle.SetData("type", vehicleModel.type);
                vehicle.SetData("insurances", vehicleModel.insurances);
                vehicle.SetSharedData("fuel", vehicleModel.type == type.personal ? (float)vehicleModel.fuel : 100.0);
                vehicle.SetSharedData("kms", (float)vehicleModel.kms);
                vehicle.SetSharedData("locked", vehicleModel.locked);
                vehicle.SetData("serverId", last_vehicle_server_id);
                vehicle.SetSharedData("setEngine", false);
                vehicle.EngineStatus = false;
            });
            last_vehicle_server_id++;
        }

        public static async Task<int> createVehicle(vehicleModel vehicleModel)
        {
            vehicleModel.id = await databaseManager.updateQuery($@"INSERT INTO vehicles (owner, model, type, spawnPosition, primaryColor, secondaryColor, numberPlate, rotation) VALUES ('{vehicleModel.owner}', '{vehicleModel.model}','{(int)vehicleModel.type}','{NAPI.Util.ToJson(vehicleModel.spawnPosition)}', '{vehicleModel.primaryColor}', '{vehicleModel.secondaryColor}', '{vehicleModel.numberPlate}', '{vehicleModel.rotation}')").Execute();
            vehicles.Add(vehicleModel);
            spawnVehicle(vehicleModel);
            return vehicleModel.id;
        }

        [Command("addvehicle")]
        public async void AddVehicle(Player player, string model, vehicleManager.type type, int color1, int color2, string numberPlate)
        {

            float rotation = 0.0F;
            if (player.Vehicle != null)
                rotation = player.Vehicle.Rotation.Z;
            else
                rotation = player.Heading;
            var vehmodel = new vehicleModel(model, type, color1, color2, numberPlate, rotation, numberPlate);
            vehmodel.spawnPosition = player.Position;
           
            int id = await createVehicle(vehmodel);
            player.SendChatMessage($"Vehicle with {id} was created at the current position.");
        }
        [ServerEvent(Event.VehicleDeath)]
        public async void OnVehicleDeath(Vehicle veh)
        {
            int insurances = veh.HasData("id") && veh.HasData("insurances") ? veh.GetData<int>("insurances") : -1;
            if (insurances != -1)
            {
                int ins = veh.GetData<int>("insurances");
                ins -= 1; if (ins <= 0) ins = 0;
                var localVeh = vehicles.Find(a => a.id == veh.GetData<int>("id"));
                localVeh.insurances = ins;
                await databaseManager.updateQuery($"UPDATE vehicles SET insurances = '{ins}' WHERE id = '{veh.GetData<int>("id")}'").Execute();
               
            }

            NAPI.Task.Run(() =>
            {
                veh.Delete();
            }, 1000);
       
        }
     
        [RemoteEvent("onPlayerBuyInsurance")]
        public void onPlayerBuyInsurance(Player player, int vehId)
        {
            buyInsurance(player, vehId);
        }
        public static async void buyInsurance(Player player, int vehId)
        {
            var localVeh = vehicles.Find(veh => veh.id == vehId);
            if (localVeh == null)
            {
                player.SendChatMessage("Invalid vehicle.");
                return;
            }
            if (localVeh.insurances < 5 && await player.takeMoneyAsync(500))
            {
                localVeh.insurances += 1;
                var serverVeh = getVehicleById(vehId);
                if (serverVeh != null)
                {
                    serverVeh.SetData("insurances", localVeh.insurances);
                }
                await databaseManager.updateQuery($"UPDATE vehicles SET insurances = '{localVeh.insurances}' WHERE id = '{vehId}'").Execute();
                player.SendChatMessage("Insurance bought.");
            }
            else
                player.SendChatMessage("You don't have enough money or you already have to many.");
        }
        [ServerEvent(Event.PlayerEnterVehicleAttempt)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatID)
        {
            if (vehicle != null)
            {
                vehicle.SetSharedData("invincible", false);
         
            }
            if ( vehicle.GetData<vehicleManager.type>( "type" ) != vehicleManager.type.dmv && seatID == 0 && !player.hasLicense(mainLicense.license.vehicle))
            {
                player.SendChatMessage("!{#CECECE}You need a driver's license.");
                player.WarpOutOfVehicle();
            }
        }
        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            if (vehicle != null)
            {
                if (vehicle.Occupants.Count == 0)
                   vehicle.SetSharedData("invincible", true);
                if (vehicle.HasData("type") && vehicle.GetData<type>("type") == type.garbage)
                {
                    vehicle.SetData("lastTimeUsed", DateTime.Now);
                    vehicle.SetData("lastDriver", player);
                }
                
            }
        }
        [Command("engine", "Syntax: /engine.")]
        public void StartEngine(Player player)
        {

            playerStartEngine(player);

        }

        public void playerStartEngine(Player player)
        {
            if (player.Vehicle != null && player.VehicleSeat == 0)
            {
                if ( player.Vehicle.HasData("type") && player.Vehicle.GetData<type>("type") == type.personal &&  player.Vehicle.HasData("insurances") && player.Vehicle.GetData<int>("insurances") <= 0)
                {
                    player.SendChatMessage("You can't use this car because it does not have the insurance paid.");
                    return;
                }
                if (player.Vehicle.HasSharedData("fuel") && player.Vehicle.GetSharedData<float>("fuel") <= 0.0)
                {
                    player.SendChatMessage("No fuel.");
                    return;

                }
                player.Vehicle.EngineStatus = !player.Vehicle.EngineStatus;

                if (player.Vehicle.EngineStatus)
                {

                    player.Vehicle.SetSharedData("setEngine", true);
                    foreach (Player players in NAPI.Player.GetPlayersInRadiusOfPlayer(30, player))
                    {
                        
                        if (player.HasSharedData("undercover") && player.GetSharedData<bool>("undercover"))
                            players.SendChatMessage("!{#afa1c4}* " + $"An unknown hitman starts the engine of his {char.ToUpper(player.Vehicle.DisplayName[0]) + player.Vehicle.DisplayName.Substring(1).ToLower() }.");
                        else if (player.Vehicle.DisplayName != null)
                            players.SendChatMessage("!{#afa1c4}* " + player.Name + $"[{player.Value}] starts the engine of his {char.ToUpper(player.Vehicle.DisplayName[0]) + player.Vehicle.DisplayName.Substring(1).ToLower() }.");
                    }
                }
                else
                {
                    player.Vehicle.SetSharedData("setEngine", false);
                    foreach (Player players in NAPI.Player.GetPlayersInRadiusOfPlayer(30, player))
                    {
                        if (player.HasSharedData("undercover") && player.GetSharedData<bool>("undercover"))
                            players.SendChatMessage("!{#afa1c4}* " + $"An unknown hitman stops the engine of his {char.ToUpper(player.Vehicle.DisplayName[0]) + player.Vehicle.DisplayName.Substring(1).ToLower() }.");
                        else if (player.Vehicle.DisplayName != null)
                            players.SendChatMessage("!{#afa1c4}* " + player.Name + $"[{player.Value}] stops the engine of his {char.ToUpper(player.Vehicle.DisplayName[0]) + player.Vehicle.DisplayName.Substring(1).ToLower() }.");
                    }
                }
            }
        }

        [RemoteEvent("startEngine")]
        public void StartEngineOn2(Player player)
        {
            playerStartEngine(player);
        }

        [Command("inv")]
        public void inv(Player player)
        {
            if (player.Vehicle != null)
                player.Vehicle.SetSharedData("invincible", true);
        }
        [Command("lock")]
        public void lockCommand(Player player)
        {
            if (player.Vehicle != null  && player.VehicleSeat == 0)
            {
                player.Vehicle.Locked = !player.Vehicle.Locked;
                if (player.Vehicle.Locked)
                    player.SendChatMessage("Vehicle locked.");
                else
                    player.SendChatMessage("Vehicle unlocked.");

                player.Vehicle.SetSharedData("locked", player.Vehicle.Locked);
            }
            else
            {
                var closestVehicle = getClosestVehicle(player);
                if (closestVehicle != null && closestVehicle.GetData<string>("owner") == player.Name)
                {
                    closestVehicle.Locked = !closestVehicle.Locked;
                    closestVehicle.SetSharedData("locked", closestVehicle.Locked);
                }
                else
                {
                    player.SendChatMessage("You don't have a car nearby.");
                }
            }
        }
        [Command("vre")]
        public void vre(Player client)
        {
            if (client.Vehicle != null)
            {
                client.Vehicle.Delete();
            }
        }
        [Command("vehicles", Alias = "v")]
        public void onPlayerVehicles(Player player)
        {
            DateTime old = DateTime.Now;
            List<vehicleModel> vehList = vehicles.FindAll(vehicle => vehicle.type == type.personal && vehicle.owner == player.Name);
            Console.WriteLine(NAPI.Util.ToJson(vehList));
            player.TriggerEvent("onPlayerOpenVehicleMenu", NAPI.Util.ToJson(vehList));

        }
        [RemoteEvent("onPlayerSpawnVehicle")]
        public void onPlayerSpawnVehicle(Player player, int id)
        {
            var vehicle = NAPI.Pools.GetAllVehicles().Find(a => a.HasData("id") && a.GetData<int>("id") == id);
            if (vehicle == null)
            {
                spawnVehicle(vehicles.Find(a => a.id == id));
                player.SendChatMessage("Vehicle spawned.");
            }
            else
                player.SendChatMessage("Vehicle already spawned.");
        }
        [RemoteEvent("onPlayerFindVehicle")]
        public void onPlayerFindVehicle(Player player, int id)
        {
            var vehicle = NAPI.Pools.GetAllVehicles().Find(a => a.HasData("id") && a.GetData<int>("id") == id);
            if (vehicle == null)
            {
                player.SendChatMessage("Vehicle doesn't exist.");
                return;
            }
            player.TriggerEvent("playerFindVehicle", vehicle.Position);
            player.SendChatMessage("~y~A waypoint was set to your car's location.");
        }
        public static Vehicle getClosestVehicle(Player player, float distance = 1.5f)
        {
            Vehicle vehicle = null;
            foreach (var veh in NAPI.Pools.GetAllVehicles())
            {
                if (player.Position.DistanceTo(veh.Position) < distance)
                {
                    vehicle = veh;
                    distance = player.Position.DistanceTo(veh.Position);
                }

            }

            return vehicle;
        }

        public void lockPersonal(Player player)
        {

        }
        public static Vehicle getVehicleById(int vehicleId)
        {
            return NAPI.Pools.GetAllVehicles().Find(veh => veh.HasData("id") && veh.GetData<int>("id") == vehicleId);
        }

        public static async Task<List<vehicleModel>> loadAllVehiclesPerPlayer(string username, bool db = false)
        {
            List<vehicleModel> vehicleList = new List<vehicleModel>();
            if (db)
            {
                await databaseManager.selectQuery($"SELECT * FROM vehicles WHERE username = '{username}'", (DbDataReader reader) =>
                {
                    if (reader.HasRows)
                    {

                        vehicleModel veh = new vehicleModel();
                        veh.id = (int)reader["id"];
                        veh.owner = (string)reader["owner"];
                        veh.model = (string)reader["model"];
                        veh.type = (type)(int)reader["type"];

                        veh.spawnPosition = NAPI.Util.FromJson<Vector3>((string)reader["spawnPosition"]);
                        veh.primaryColor = (int)reader["primaryColor"];
                        veh.secondaryColor = (int)reader["secondaryColor"];
                        veh.numberPlate = (string)reader["numberPlate"];

                        vehicleList.Add(veh);
                    }
                }).Execute();
            }
            else
                vehicleList = vehicles.FindAll(veh => veh.owner == username);
            return vehicleList;
        }

        public static async Task<List<vehicleModel>> loadAllVehicles()
        {
            List<vehicleModel> vehicleList = new List<vehicleModel>();

            await databaseManager.selectQuery($"SELECT * FROM vehicles", (DbDataReader reader) =>
            {
                if (reader.HasRows)
                {

                    vehicleModel veh = new vehicleModel();
                    veh.id = (int)reader["id"];
                    veh.owner = (string)reader["owner"];
                    veh.model = (string)reader["model"];
                    veh.kms = (int)reader["kms"];
                    veh.fuel = (int)reader["fuel"];
                    veh.type = (type)(int)reader["type"];
                    veh.spawnPosition = NAPI.Util.FromJson<Vector3>((string)reader["spawnPosition"]);
                    veh.primaryColor = (int)reader["primaryColor"];
                    veh.secondaryColor = (int)reader["secondaryColor"];
                    veh.numberPlate = (string)reader["numberPlate"];

                    vehicleList.Add(veh);
                }
            }).Execute();
            return vehicleList;
        }

        public static async Task<vehicleModel> getVehicleModelById(int id, bool db = false)
        {
            vehicleModel veh = new vehicleModel();
            if (db)
            {
                await databaseManager.selectQuery($"SELECT * FROM vehicles WHERE id = '{id}' LIMIT 1", (DbDataReader reader) =>
                {
                    if (reader.HasRows)
                    {
                        veh.id = (int)reader["id"];
                        veh.owner = (string)reader["owner"];
                        veh.model = (string)reader["model"];
                        veh.type = (type)(int)reader["type"];
                        veh.spawnPosition = NAPI.Util.FromJson<Vector3>((string)reader["spawnPosition"]);
                        veh.primaryColor = (int)reader["primaryColor"];
                        veh.secondaryColor = (int)reader["secondaryColor"];
                        veh.numberPlate = (string)reader["numberPlate"];
                    }
                }).Execute();
            }
            else
                veh = vehicles.Find(vehicle => vehicle.id == id);
            return veh;
        }
        public static Vehicle GetClosestVehicle(Player player, float distance = 2.5f)
        {
            Vehicle vehicle = null;
            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
            {
                Vector3 vehPos = veh.Position;
                uint vehicleDimension = NAPI.Entity.GetEntityDimension(veh);
                float distanceVehicleToPlayer = player.Position.DistanceTo(vehPos);
                if (distanceVehicleToPlayer < distance && player.Dimension == vehicleDimension)
                {
                    distance = distanceVehicleToPlayer;
                    vehicle = veh;

                }
            }
            return vehicle;
        }
        static int last_vehicle_server_id = 0;


        public static Player getDriver(Vehicle vehicle)
        {
            return NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.Vehicle == vehicle);
        }
        [Command("gotocar")]
        public void gotocar(Player player, int id)
        {
            Vehicle veh = getVehicleById(id);
            if (veh == null)
            {
                player.SendChatMessage("Invalid vehicle sql id.");
                return;
            }
            player.Position = veh.Position;
        }

        static void timerVehicle(object sender, System.Timers.ElapsedEventArgs e)
        {


            try
            {
                DateTime old = DateTime.Now;
                foreach (vehicleModel vehicle in vehicles)
                {


                    Vehicle veh = getVehicleById(vehicle.id);

                    if (veh == null)
                    {
                        spawnVehicle(vehicle);
                        continue;
                    }

                    var driver = getDriver(veh);
                    if (veh != null && veh.EngineStatus && driver != null && veh.HasSharedData("kms") && veh.HasSharedData("fuel"))
                    {
                        if (veh.HasData("lastSavedPosition"))
                        {
                            float kms = veh.GetSharedData<float>("kms");
                            float distance = veh.Position.DistanceTo(veh.GetData<Vector3>("lastSavedPosition"));
                            float fuel_consumed = distance / 300;
                            float fuel = veh.GetSharedData<float>("fuel");

                            float fuel_consumed_result = fuel - fuel_consumed;

                            if (fuel_consumed_result <= 0.0f)
                            {
                                fuel_consumed_result = 0.0f;


                                driver.SendChatMessage("You ran out of fuel.");
                                veh.EngineStatus = false;
                                return;

                            }
                            //driver.SendChatMessage("ai fuel " + fuel_consumed_result);
                            veh.SetSharedData("fuel", fuel_consumed_result);
                            veh.SetSharedData("kms", kms + (distance / 500));
                        }
                        veh.SetData("lastSavedPosition", veh.Position);


                    }



                    if (veh.HasData("lastTimeUsed") && (DateTime.Now - veh.GetData<DateTime>("lastTimeUsed")).TotalMinutes > 5)
                    {
                        if (veh.HasData("type"))
                        {
                            if (veh.GetData<type>("type") == type.garbage && veh.HasData("lastDriver"))
                            {
                                var lastDriver = veh.GetData<Player>("lastDriver");
                                if (lastDriver != null && lastDriver.HasSharedData("job") && lastDriver.GetSharedData<playerManager.job>("job") == playerManager.job.garbage)
                                {
                                    lastDriver.SendChatMessage("You failed the job because you left your vehicle for more than 5 minutes.");
                                }
                            }
                            NAPI.Task.Run(() =>
                            {
                                veh.Delete();
                            });
                        }
                    }

                }

                Console.WriteLine($"VEHICLE MANAGER TOOK {(DateTime.Now - old).TotalSeconds} ms {vehicles.Count} vehicles");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
     
        static List<vehicleModel> vehicles = new List<vehicleModel>();

        public class vehicleModel
        {
            public int id { get; set; }
            public string owner { get; set; }
            public string model { get; set; }
            public type type { get; set; }
            public int fuel { get; set; }
            public bool locked { get; set; }
            public int kms { get; set; }
            public int days { get; set; }
            public int insurances { get; set; }
            public Vector3 spawnPosition { get; set; }
            public int primaryColor { get; set; }
            public int secondaryColor { get; set; }
            public float rotation { get; set; }
            public string numberPlate { get; set; }
            public vehicleModel() { }

            public vehicleModel(string model, vehicleManager.type type, int color1, int color2, string numberPlate, float rotation, string owner = "AdmBot")
            {
                this.model = model; this.type = type; this.primaryColor = color1; this.secondaryColor = color2; this.numberPlate = numberPlate; this.owner = owner; this.rotation = rotation;
            }
        }
    }
}
