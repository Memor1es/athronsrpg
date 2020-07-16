using GTANetworkAPI;
using rpg.Database;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace rpg
{
    class garbageMan : Script
    {
        static List<garbage> garbageList = new List<garbage>();
        [ServerEvent(Event.ResourceStart)]
        public async void onResourceStart()
        {
            NAPI.Task.Run(() =>
            {
                NAPI.TextLabel.CreateTextLabel($"~w~ID: ~r~{(int)playerManager.job.garbage}\n~w~Job: ~r~Garbage Man\n~w~Use ~r~/getjob ~w~to get this job.", new Vector3(-504.49496, -2204.3628, 6.8940945), 15, 1, 4, new Color(0, 162, 255, 255));
                jobs.jobList.Add(new jobs.jobModels(playerManager.job.garbage, "Garbage Man", new Vector3(-504.49496, -2204.3628, 6.0940945)));
                NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("s_m_y_garbage"), new Vector3(-504.49496, -2204.3628, 6.4), 310f, false, true, true);
             
            });
            _ = await databaseManager.selectQuery($"SELECT * FROM garbagepositions", (DbDataReader reader) =>
              {
                  garbageList.Add(new garbage(NAPI.Util.FromJson<Vector3>((string)reader["position"]), (int)reader["id"]));
              }).Execute();
        }
        public class garbage
        {
            public Vector3 position { get; set; }
            public int id { get; set; }
            public garbage(){}
            public garbage(Vector3 position, int id)
            {
                this.position = position;
                this.id = id;
            }
        }

        

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatID)
        {
            if (vehicle.GetData<vehicleManager.type>("type") == vehicleManager.type.garbage)
            {
                if (jobs.doesPlayerHasJob(player) != playerManager.job.garbage)
                {
                    player.SendChatMessage("!{#CECECE}This car can only be used by the garbage men.");
                    player.WarpOutOfVehicle();
                  
                }
                else if (player.HasSharedData("inJob") && !player.GetSharedData<bool>("inJob"))
                { 
                    player.SendChatMessage("~y~Use /collecttrash to start collecting the trash.");
                }
            }
            
        }
        [Command("editgarbage")]
        public void onEditGarbage(Player player)
        {
            player.SetData("inEditGarbage", true);
        }
        [Command("addgarbage")]
        public async void onAddGarbage(Player player)
        {
            if (!player.HasData("inEditGarbage"))
                return;

           await databaseManager.updateQuery($"INSERT INTO garbagepositions (position) VALUES ('{NAPI.Util.ToJson(player.Position)}')").Execute();
            player.SendChatMessage("A fost adaugat garbage-u");
            player.TriggerEvent("addPlayerGarbage", player.Position);
        }
        [Command("jobstatus")]
        public void ssd(Player player)
        {
            player.SendChatMessage((player.HasSharedData("inJob") ? player.GetSharedData<bool>("inJob") : false).ToString());
        }

        [Command("stoptrash")]
        public void onPlayerStopTrash(Player player)
        {
            player.TriggerEvent("stopGarbageJob");
            player.AddAttachment("trashBag", true);
            player.StopAnimation();
            player.SendChatMessage("You stopped the garbage job.");
        }
        [Command("collecttrash")]
        public void onCollectTrash(Player player)
        {
            if (jobs.doesPlayerHasJob(player) != playerManager.job.garbage)
            {
                player.SendChatMessage("!{#CEF0AC}You don't have the garbage man job!.");
                return;
            }
            if (player.HasSharedData("inJob") && player.GetSharedData<bool>("inJob"))
            {
                player.SendChatMessage("!{#CEF0AC}You are already in this job.");
                return;
            }
            if (player.Vehicle == null || player.Vehicle.GetData<vehicleManager.type>("type") != vehicleManager.type.garbage)
            {
                player.SendChatMessage("!{#CEF0AC}You must be in a garbage car to use this command.");
                return;
            }
          

            player.SetSharedData("inJob", true);
            player.TriggerEvent("startGarbageJob", garbageList, player.Vehicle);
        }
        [RemoteEvent("onPlayerLoadTrash")]
        public void onPlayerLoadTrash(Player player, int money)
        {
            player.AddAttachment("trashBag", true);
            player.StopAnimation();
            player.SetData("garbageBins", (player.HasData("garbageBins") ? player.GetData<int>("garbageBins") : 0) + 1);
            player.SetData("garbageMoney", (player.HasData("garbageMoney") ? player.GetData<int>("garbageMoney") : 0) + money);
        }

        [RemoteEvent("pickupTheTrash")]
        public void onPickupTheTrashAsync(Player player)
        {
            player.AddAttachment("trashBag", false);
            player.PlayAnimation("anim@move_m@trash", "idle", 49);
        }
         
        [Command("unloadtrash")]
        public async void onUnloadTrash(Player player)
        {
            if (jobs.doesPlayerHasJob(player) != playerManager.job.garbage)
            {
                player.SendChatMessage("!{#CEF0AC}You don't have the garbage man job!.");
                return;
            }

            if (player.Vehicle == null || (player.Vehicle != null && player.Vehicle.GetData<vehicleManager.type>("type") != vehicleManager.type.garbage))
            {
                player.SendChatMessage("!{#CEF0AC}You are not in a garbage car.");
                return;
            }
            if (player.Position.DistanceTo(new Vector3(-453.09668, -1713.526, 18.526152)) > 4)
            {
                player.SendChatMessage("You aren't at unload position.");
                return;
            }
            if (player.HasData("garbageBins"))
             {
                int bins = player.GetData<int>("garbageBins");
                if (bins >= 3 && player.HasData("garbageMoney"))
                {
                    int money = player.GetData<int>("garbageMoney");
                    await player.giveMoney(money);
                    player.SendChatMessage($"~g~(+) You received ${money} for {bins} bags of trash.");

                    player.SetSharedData("trashBags", 0);
                    player.SetSharedData("inJob", false);
                    player.ResetData("garbageBins");
                    player.ResetData("garbageMoney");
                    player.TriggerEvent("onPlayerUnloadTrash");
                }
                else
                    player.SendChatMessage("You need to have atleast 3 garbage bins.");
               
            }
            else
            {
                player.SendChatMessage("You don't have any garbage.");
            }
        }

      
       
    }
}
