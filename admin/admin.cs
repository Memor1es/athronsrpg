using GTANetworkAPI;
using rpg.Database;
using System;
using System.Threading.Tasks;
using static rpg.business;

namespace rpg
{
    public class admin : Script
    {
       
        public void SendMessageToAdmins(string message)
        {
            NAPI.Pools.GetAllPlayers().FindAll(player => playerManager.doesPlayerHasAdmin(player) > 0).ForEach(player => { player.SendChatMessage($"{message}"); });
        }

        public void SendMessageToAdminsAdmCmd(string message)
        {
            NAPI.Pools.GetAllPlayers().FindAll(player => playerManager.doesPlayerHasAdmin(player) > 0).ForEach(player => { player.SendChatMessage($"{"!{#EFB646}"}AdmCmd: {message}"); });
        }

        public void SendMessageToAdminsPromotionAdmin(string message)
        {
            NAPI.Pools.GetAllPlayers().FindAll(player => playerManager.doesPlayerHasAdmin(player) > 0).ForEach(player => { player.SendChatMessage($"{"!{#FFC266}"}* {message}"); });
        }

        public void SendMessageToAdminsPromotionHelper(string message)
        {
            NAPI.Pools.GetAllPlayers().FindAll(player => playerManager.doesPlayerHasAdmin(player) > 0).ForEach(player => { player.SendChatMessage($"{"!{#CC6633}"}* {message}"); });
        }

        public static void checkAdmin(Player player)
        {
            if (player.HasSharedData("admin") && player.GetSharedData<int>("admin") > 0)
                player.SendChatMessage($"{"!{#A9C4E4}"}~w~You are a level {player.GetSharedData<int>("admin")} admin.");
        }

        [Command("makeadmin", "Syntax: /makeadmin <Name/Playerid> <Admin Level>", SensitiveInfo = false)]
        public async void onSetAdmin(Player player, string username, int level)
        {
            if (playerManager.doesPlayerHasAdmin(player) < 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            var target = playerManager.getPlayer(username);

            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }

            player.SetSharedData("admin", level);

            if (level <= 0)
            {
                target.SendChatMessage($"~y~You have been demoted to level {level} admin.");
                player.SendChatMessage($"~y~You have demoted {target.Name}[{target.Value}] to level {level} admin.");

                SendMessageToAdminsPromotionAdmin($"Admin {player.Name}[{player.Value}] set {target.Name}[{target.Value}]'s admin level to {level}.");
            }
            else
            {
                target.SendChatMessage($"~y~You have been promoted to level {level} admin.");
                player.SendChatMessage($"~y~You have promoted {target.Name}[{target.Value}] to level {level} admin.");

                SendMessageToAdminsPromotionAdmin($"Admin {player.Name}[{player.Value}] set {target.Name}[{target.Value}]'s admin level to {level}.");
            }

            await databaseManager.updateQuery($"UPDATE accounts SET admin = '{target.GetSharedData<int>("admin")}' WHERE username = '{target.Name}'").Execute();
        }

        [Command("makehelper", "Syntax: /makehelper <Name/Playerid> <Helper Level>")]
        public async void onSetHelper(Player player, string username, int level)
        {
            if (playerManager.doesPlayerHasAdmin(player) < 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            foreach (Player target in NAPI.Pools.GetAllPlayers())
            {
                if (playerManager.doesPlayerHasAdmin(target) > 0)
                {
                    player.SendChatMessage($"{"!{#CEF0AC}"}{target.Name}[{player.Value}] is already an admin.");
                    return;
                }

                if (target.Name.ToLower() == username.ToLower() || target.Value.ToString() == username.ToLower())
                {
                    player.SetSharedData("helper", level);

                    if (level <= 0)
                    {
                        target.SendChatMessage($"{"!{#33CCFF}"}Administrator {player.Name}[{player.Value}] has removed you from the helper team.");
                        player.SendChatMessage($"{"!{#33CCFF}"}You have removed {target.Name}[{target.Value}] from the helper team.");

                        SendMessageToAdminsPromotionHelper($"Admin {player.Name}[{player.Value}] set {target.Name}[{target.Value}]'s helper level to {level}.");
                    }
                    else
                    {
                        target.SendChatMessage($"{"!{#33CCFF}"}Administrator {player.Name}[{player.Value}] has promoted you to a level {level} helper.");
                        player.SendChatMessage($"{"!{#33CCFF}"}You have promoted {target.Name}[{target.Value}] to helper level {level}.");

                        SendMessageToAdminsPromotionHelper($"Admin {player.Name}[{player.Value}] set {target.Name}[{target.Value}]'s helper level to {level}.");
                    }

                    await databaseManager.updateQuery($"UPDATE accounts SET helper = '{target.GetSharedData<int>("admin")}' WHERE username = '{player.Name}'").Execute();
                }
                else
                {
                    player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                    return;
                }
            }
        }

        [Command("aaa2")]
        public void onGotoAAA2(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            player.Position = new Vector3(-993.296, -3147.131, 13.944);
            player.Dimension = 1337;
            player.SendChatMessage($"Welcome to LS Airport (in virtual world).");
        }

        [Command("gotoa")]
        public void onGotoA(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            player.Position = new Vector3(-993.296, -3147.131, 13.944);
            player.Dimension = 0;
            player.SendChatMessage($"Welcome to LS Airport.");
        }

        [Command("coords")]
        public void GetCoords(Player player)
        {
            player.SendChatMessage($"Coords: {player.Position.X}, {player.Position.Y}, {player.Position.Z} | VW: {player.Dimension}");
        }

        [Command("gotocoords", "Syntax: /gotocoords <x> <y> <z>")]
        public void GotoCoords(Player player, float x, float y, float z)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            player.Position = new Vector3(x, y, z);
            SendMessageToAdminsAdmCmd($"{player.Name}[{player.Value}] used /gotocoords.");
        }

        [Command("spawncar", "Syntax: /spawncar <model> <numberPlate>")]
        public void onSpawnCar(Player player, string carModel, string numberPlate = "")
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            Random random = new Random();
            Vehicle vehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(carModel), player.Position, 0, random.Next(0, 159), random.Next(0, 159), numberPlate, 255, false, false); vehicle.SetSharedData("invincible", true);
            vehicle.Dimension = player.Dimension;
            vehicle.NumberPlate = numberPlate;
            vehicle.SetSharedData("setEngine", false);
            vehicle.EngineStatus = false;   
           player.SetIntoVehicle(vehicle, 0);
            player.SendChatMessage($"You spawned a(n) {char.ToUpper(carModel[0]) + carModel.Substring(1).ToLower()}");
            SendMessageToAdminsAdmCmd($"{player.Name} has spawned a(n) {char.ToUpper(carModel[0]) + carModel.Substring(1).ToLower()}.");
        }

        

        [Command("arepair")]
        public void onARepair(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            if (player.IsInVehicle && player.Vehicle != null)
                player.Vehicle.Repair();
        }
        [Command("passive")]
        public void onPlayerTogglePassiveMode(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            var toggle = player.HasData("passiveMode") ? player.GetData<bool>("passiveMode") : false; toggle = !toggle;
            player.SetData("passiveMode", toggle);
            if (toggle)
            {
                SendMessageToAdmins($"{"!{#EFB646}"}{player.Name} [id: {new accountController(player).sqlid}] is now in passive mode.");
                player.TriggerEvent("togglePassiveMode", toggle);
            }
            else
            {
                SendMessageToAdmins($"{"!{#EFB646}"}{player.Name} [id: {new accountController(player).sqlid}] is no longer in passive mode.");
                player.TriggerEvent("togglePassiveMode", toggle);
            }
        }
       
        [Command("set", "!{#CECECE}Syntax: ~w~/set <Name/Playerid> <Choose an item> <Value> !{#CECECE}Items: Money, Materials, Job, Group")]
        public async void onSet(Player player, string username, string type, dynamic args1 = null, dynamic args2 = null, dynamic args3 = null)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            Player target = playerManager.getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }
   
            switch (type)
            {
                case "Money":
                case "money":
                    int money = 0;
                    if (!int.TryParse(args1, out money))
                    {
                        player.SendChatMessage($"{"!{#CECECE}"}Invalid amount.");
                        return;
                    }
                  
                    target.SendChatMessage($"~r~Admin {player.Name}[{player.Value}] has given you {money}$.");
                    SendMessageToAdmins($"{"!{#F03337}"}Warning: Admin {player.Name}[{player.Value}] sent ${money} to {target.Name}[{target.Value}].");
                    await target.giveMoney(money);
                    break;
                case "License":
                case "license":

                    mainLicense.license license = 0; int hours = 0;
                    if (mainLicense.license.TryParse(args1, true, out license))
                    {
                        if (!int.TryParse(args2, out hours))
                        {
                            player.SendChatMessage($"{"!{#CECECE}"}Invalid amount.");
                            return;
                        }

                        if (hours > 0)
                            await target.setLicense(license, hours);
                        else
                           await target.removeLicense(license);

                        target.SendChatMessage($"~r~Admin {player.Name} has changed your license  {Enum.GetName(typeof(mainLicense.license), license)} to {hours} hours.");
                        SendMessageToAdmins($"{"!{#EFB646}"}{target.Name} [id: {new accountController(target).sqlid}]'s {Enum.GetName(typeof(mainLicense.license), license)} was set to {hours} hours(( Admin {player.Name}[id: {new accountController(player).sqlid}] ))");
                    }
                    else
                    {
                        player.SendChatMessage($"{"!{#CECECE}"}Invalid license.");
                    }
                    break;
                case "Materials":
                case "materials":
                    int amount = 0;
                    if (!int.TryParse(args1, out amount))
                    {
                        player.SendChatMessage($"{"!{#CECECE}"}Invalid amount.");
                        return;
                    }
                    target.SetSharedData("materials", target.GetSharedData<int>("materials") + amount);
                    target.SendChatMessage($"~r~Admin {player.Name}[{player.Value}] has given you {amount}$.");
                    SendMessageToAdmins($"{"!{#F03337}"}Warning: Admin {player.Name}[{player.Value}] sent {amount} materials to {target.Name}[{target.Value}].");
                    await databaseManager.updateQuery($"UPDATE accounts SET materials = '{target.GetSharedData<int>("materials")}' WHERE username = '{target.Name}'").Execute();
                    break;

                case "Job":
                case "job":
                    playerManager.job job = 0; 
                    if (playerManager.job.TryParse(args1, true, out job))
                    {
                        target.SetSharedData("job", job);
                        target.SendChatMessage($"~r~Admin {player.Name} has changed your job to {Enum.GetName(typeof(playerManager.job), job)}.");
                        SendMessageToAdmins($"{"!{#EFB646}"}{target.Name} [id: {new accountController(target).sqlid}]'s job was set to {Enum.GetName(typeof(playerManager.job), job)} (( Admin {player.Name}[id: {new accountController(player).sqlid}] ))");
                        await databaseManager.updateQuery($"UPDATE accounts SET job = '{(int)job}' WHERE username = '{target.Name}'").Execute();
                    }
                    else
                    {
                        player.SendChatMessage($"{"!{#CECECE}"}Invalid job.");
                    }
                    break;

                case "Group":
                case "group":
                    factionsManager.type faction = 0; 
                    if (factionsManager.type.TryParse(args1, true, out faction))
                    {
                     
                        target.SetSharedData("faction", faction);
                        target.SendChatMessage($"~r~Admin {player.Name} has set your group to {Enum.GetName(typeof(factionsManager.type), faction)}.");
                        SendMessageToAdmins($"{"!{#EFB646}"}{target.Name} [id: {new accountController(target).sqlid}]'s group was set to {Enum.GetName(typeof(factionsManager.type), faction)} (( Admin {player.Name}[id: {new accountController(player).sqlid}] ))");
                        await databaseManager.updateQuery($"UPDATE accounts SET faction = '{(int)faction}' WHERE username = '{target.Name}'").Execute();
                    }
                    else
                    {
                        player.SendChatMessage($"{"!{#CECECE}"}Invalid faction.");
                    }
                    break;

                case "Grouprank":
                case "grouprank":
                    int rank = 0;
                    if (!int.TryParse(args1, out rank))
                    {
                        player.SendChatMessage($"{"!{#F03337}"}Invalid group rank.");
                        return;
                    }
                    if (target.HasSharedData("faction") && target.GetSharedData<factionsManager.type>("faction") == factionsManager.type.civilian)
                    {
                        player.SendChatMessage($"{"!{#CECECE}"}Target does not have a group.");
                        return;
                    }
                    target.SetData("factionRank", rank);
                    target.SendChatMessage($"~r~Admin {player.Name} has set your group rank to {rank}.");
                    SendMessageToAdmins($"{"!{#EFB646}"}{target.Name} [id: {new accountController(target).sqlid}]'s group rank was set to {rank} (( Admin {player.Name}[id: {new accountController(player).sqlid}] ))");
                    await databaseManager.updateQuery($"UPDATE accounts SET factionRank = '{rank}' WHERE username = '{target.Name}'").Execute();
                    break;

                case "vw":
                case "Vw":
                    int vw = 0;
                    if (!int.TryParse(args1, out vw))
                    {
                        player.SendChatMessage("");
                        return;
                    }
                    target.SendChatMessage($"~r~Admin {player.Name} has set your virtual world to {vw}.");
                    SendMessageToAdminsAdmCmd($"Admin {player.Name}[{player.Value}] has changed {target.Name}[{target.Value}]'s virtual world to {vw}.");
                    target.Dimension = (uint)vw;
                    break;
            }
        }

        [Command("setwv", "!{#CECECE}Syntax: ~w~/setvw [id]")]
        public void dda(Player player, int wv)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            player.Dimension = (uint)wv;
        }
        [Command("gotohq", "!{#CECECE}Syntax: ~w~/gotohq [faction hq]")]
        public void onGotoHq(Player player, int hq)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            var faction = factionsManager.factions.Find(f => f.id == hq);
            if (faction == null)
            {
                player.SendChatMessage($"{"!{#CECECE}"}Invalid hq id.");
                return;
            }
            player.Position = faction.enterHq;

            SendMessageToAdminsAdmCmd($"{player.Name} used /gotohq {hq}.");
        }

        [Command("goto", "!{#CECECE}Syntax:~w~ /goto [name/playerid]")]
        public void onGoto(Player player, string username)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            Player target = playerManager.getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }

            player.Dimension = target.Dimension;
            player.Position = target.Position;

            if (target.Vehicle != null)
            {
                player.SetIntoVehicle(target.Vehicle, 1);
            }

            player.SendChatMessage($"{"!{#CECECE}"}You have been teleported to {target.Name}.");
            SendMessageToAdminsAdmCmd($"{player.Name}[{player.Value}] used /goto.");
        }

        [Command("gethere", "Syntax: /gethere [name/playerid]")]
        public void onGetHere(Player player, string username)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            Player target = playerManager.getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }

            target.Dimension = player.Dimension;
            target.Position = player.Position;

            player.SendChatMessage($"{"!{#CECECE}"}You have teleported {target.Name} to you.");
            SendMessageToAdminsAdmCmd($"Admin {player.Name}[{player.Value}] used /gethere.");
        }

      

        [Command("slap", "!{#CECECE}Syntax:~w~ /slap [name/playerid]")]
        public void onSlap(Player player, string username)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            Player target = playerManager.getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }

            target.WarpOutOfVehicle();
            target.StopAnimation();
            target.Position = new Vector3(target.Position.X, target.Position.Y, target.Position.Z + 2);
            SendMessageToAdminsAdmCmd($"Admin {player.Name}[{player.Value}] slapped {target.Name}[{player.Value}].");
        }

        [Command("spawnweapon", "!{#CECECE}Syntax:~w~ /spawnweapon [name/playerid] [weapon]")]
        public void onSpawnWeapon(Player player, string username, WeaponHash hash)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            Player target = playerManager.getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }

            target.GiveWeapon(hash, 1000);
            SendMessageToAdminsAdmCmd($"Admin {player.Name}[{player.Value}] used /spawnweapon and gave {target.Name}[{player.Value}] {hash}.");
        }

        [Command("aclear", "!{#CECECE}Syntax:~w~ /aclear [name/playerid]")]
        public async void onAClearAsync(Player player, string username)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            Player target = playerManager.getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }

            player.SendChatMessage($"{"!{#CECECE}"}You cleared {target.Name}'s wanted.");
            SendMessageToAdminsAdmCmd($"Admin {player.Name}[{player.Value}] cleared {target.Name}[{target.Value}]'s wanted.");

            target.SetSharedData("wantedLevel", 0);
            await databaseManager.updateQuery($"UPDATE accounts SET wanted = '0' WHERE username = '{target.Name}'").Execute();
            await databaseManager.updateQuery($"DELETE FROM mdc WHERE suspect = '{target.Name}'").Execute();
        }
        [Command("gotobusiness")]
        public void onPlayergoBusiness(Player player, int id)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            model business = businessList.Find(biz => biz.bizId == id);
            if (business == null)
            {
                player.SendChatMessage("Invalid business id.");
                return;
            }
            SendMessageToAdminsAdmCmd($"Admin {player.Name} teleported to business {id}.");
            player.Position = business.enterPos;
            player.SendChatMessage("You have been teleported to business " + id+ " .");
        }
        [Command("find")]
        public void find(Player player, string username)
        {
            var target = playerManager.getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}Player not connected.");
                return;
            }
            else if (target == player)
            {
                player.SendChatMessage( "!{#CECECE}You can't find youself.");
                return;
            }

            player.SetData("findPlayer", target.Value);
            player.SendChatMessage($"~y~The checkpoint will show {target.Name}'s location. Distance to the player: {player.Position.DistanceTo(target.Position)}m.");
        }

        [Command("killcheckpoint", Alias = "killcp")]
        public void onKillCheckpoint(Player player)
        {
            player.ResetData("findPlayer");
            player.SendChatMessage("~w~You have disabled the checkpoint.");
        }

        [Command("test")]
        public void test(Player player, int slot, int drawable, int texture)
        {
            player.SetClothes(slot, drawable, texture);
        }
    }
}