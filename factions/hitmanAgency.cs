using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Timers;
using GTANetworkAPI;
using rpg.Database;

namespace rpg
{

    public class hitmanAgency : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            NAPI.World.RequestIpl("gr_case10_bunkerclosed");
        }

        [Command("contract", "!{#CECECE}Syntax:~w~ /contract [name/playerid] [money]")]
        public void onContract(Player player, string username, int price)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<factionsManager.type>("faction") == factionsManager.type.hitmanAgency)
            {
                player.SendChatMessage("!{#CECECE}You can't place a contract because you are a hitman");
                return;
            }

            if (price < 10000 || price > 10000000)
            {
                player.SendChatMessage("!{#CECECE}Contract money must be at least $10,000, and not more then $10,000,000.");
                return;
            }

            Player target = playerManager.getPlayer(username);
            if (target == null)
            {
                player.SendChatMessage("!{#CECECE}Player not connected.");
                return;
            }
            else if (target == player)
            {
                player.SendChatMessage("!{#CECECE}You can't place a contract on yourself.");
                return;
            }
            else if (target.HasSharedData("faction") && target.GetSharedData<factionsManager.type>("faction") == factionsManager.type.hitmanAgency)
            {
                player.SendChatMessage("!{#CECECE}Cannot place contracts on hitmans.");
                return;
            }
            else if (playerManager.doesPlayerHasAdmin(target) < 0)
            {
                player.SendChatMessage("!{#CECECE}Cannot place contracts on admins.");
                return;
            }

            Player _targetContract = playerManager.getPlayer(username);
            contract targetContract = contracts.FindAll(a => a.target == _targetContract.Name).Find(b => b.Player == player.Name);
            if (targetContract == null)
            {
                contract contract = new contract();
                contract.Player = player.Name;
                contract.hitman = "none";
                contract.money = price;
                contract.target = target.Name;
                contract.accepted = false;
                contract.id = lastID;

                contracts.Add(contract);
                lastID++;
            }
            else
            {
                player.SendChatMessage("!{#CECECE}You already have a contract on that player.");
                return;
            }

            player.SendChatMessage("!{#CECECE}Contract placed! The hitmans will kill your target soon!");

            factionsManager.sendMessageToAll(factionsManager.type.hitmanAgency, $"{"!{#67AAB1}"}New hit available! Target: {target.Name}. Price: {price}.");
        }

        [Command("contracts")]
        public void onContracts(Player player)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<factionsManager.type>("faction") != factionsManager.type.hitmanAgency)
            {
                player.SendChatMessage("!{#FF6347}You are not a hitman.");
                return;
            }

            player.SendChatMessage("!{#8c0424}[Contracts]");
            player.SendChatMessage($"~w~Total targets: {contracts.Count}");
            player.SendChatMessage("!{#8c0424}-------------------");
        }

        [Command("mycontract")]
        public void onMyContract(Player player)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<factionsManager.type>("faction") != factionsManager.type.hitmanAgency)
            {
                player.SendChatMessage("!{#FF6347}You are not a hitman.");
                return;
            }

            if (player.HasData("contractId"))
                player.SendChatMessage($"{"!{#CECECE}"}You have a contract on {player.GetData<string>("contractName")}.");
            else
                player.SendChatMessage("~w~You don't have a contract.");
        }

        [Command("gethit")]
        public void getHit(Player player)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<factionsManager.type>("faction") != factionsManager.type.hitmanAgency)
            {
                player.SendChatMessage("!{#FF6347}You are not a hitman.");
                return;
            }

            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }

            if (player.HasData("contractId"))
            {
                player.SendChatMessage("~w~You already have a contract.");
                return;
            }

            if (contracts.Count == 0)
                player.SendChatMessage("~y~No contracts available.");

            foreach (contract contract in contracts)
            {
                Player target = playerManager.getPlayer(contract.target);
                if (target != null)
                {
                    factionsManager.sendMessageToAll(factionsManager.type.hitmanAgency, $"{"!{#67AAB1}"}{player.Name} received a contract. Target: Hidden.");
                    player.SendChatMessage("~y~You have a new contract (/mycontract).");

                    player.SetData("contractId", contract.id);
                    player.SetData("contractName", contract.target);

                    contracts.Find(s => s.id == contract.id).hitman = player.Name;
                    contracts.Find(s => s.id == contract.id).accepted = true;
                    break;
                }
            }
        }

        [Command("cancelhit")]
        public void onCancelHit(Player player)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<factionsManager.type>("faction") != factionsManager.type.hitmanAgency)
            {
                player.SendChatMessage("!{#FF6347}You are not a hitman.");
                return;
            }

            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }

            if (player.HasData("contractId"))
            {
                player.SendChatMessage("~w~You already have a contract.");
                return;
            }

            if (player.HasData("contractId"))
            {
                factionsManager.sendMessageToAll(factionsManager.type.hitmanAgency, $"{"!{#67AAB1}"}{player.Name} canceled his contract.");
                player.ResetData("contractId");
            }
            else
            {
                player.SendChatMessage("~w~You don't have a contract.");
            }
        }

        [Command("undercover")]
        public void onUndercover(Player player)
        {
            if (player.HasSharedData("faction") && player.GetSharedData<factionsManager.type>("faction") != factionsManager.type.hitmanAgency)
            {
                player.SendChatMessage("!{#FF6347}You are not a hitman.");
                return;
            }

            if (player.HasSharedData("inDuty") && !player.GetSharedData<bool>("inDuty"))
            {
                player.SendChatMessage("!{#CECECE}You are not on duty.");
                return;
            }

            if (!player.HasData("contractId"))
            {
                player.SendChatMessage("~w~You don't have a contract.");
                return;
            }

            if (player.HasSharedData("undercover") && player.GetSharedData<bool>("undercover"))
            {
                player.SetSharedData("undercover", false);
                player.SendChatMessage("~y~You are not undercover anymore. Anyone can see your name.");

            }
            else
            {
                player.SetSharedData("undercover", true);
                player.SendChatMessage("~y~You are now undercover. No one can see your name.");
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnectedAsync(Player player, DisconnectionType type, string reason)
        {

            if (player.HasSharedData("faction") && player.GetSharedData<int>("faction") == 3)
            {
                if (player.HasData("contractId"))
                {
                    contracts.Remove(contracts.Find(contract => contract.id == player.GetData<int>("contractId")));
                }
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeathAsync(Player player, Player killer, uint reason)
        {
            if (killer == null)
                return;

            if (player == null)
                return;

            if (killer.HasSharedData("faction") && killer.GetSharedData<factionsManager.type>("faction") == factionsManager.type.hitmanAgency && killer.HasData("contractId"))
            {
                var contract = contracts.Find(a => a.id == killer.GetData<int>("contractId"));
                if (player.HasSharedData("undercover") && !player.GetSharedData<bool>("undercover") && killer.Position.DistanceTo(player.Position) < 150) {
                    factionsManager.sendMessageToAll(factionsManager.type.hitmanAgency, $"{"!{#67AAB1}"}{killer.Name} failed to complete the contract on {player.Name} for ${contract.money}, distance: {player.Position.DistanceTo(killer.Position)}m. Fail reason: distance/not undercover");
                    contracts.Remove(contract);
                    killer.ResetData("contractId");
                    return;
                }
                else if (killer.Position.DistanceTo(player.Position) < 150)
                {
                    factionsManager.sendMessageToAll(factionsManager.type.hitmanAgency, $"{"!{#67AAB1}"}{killer.Name} failed to complete the contract on {player.Name} for ${contract.money}, distance: {player.Position.DistanceTo(killer.Position)}m. Fail reason: distance");
                    contracts.Remove(contract);
                    killer.ResetData("contractId");
                    return;
                }
                else if (player.HasSharedData("undercover") && !player.GetSharedData<bool>("undercover"))
                {
                    factionsManager.sendMessageToAll(factionsManager.type.hitmanAgency, $"{"!{#67AAB1}"}{killer.Name} failed to complete the contract on {player.Name} for ${contract.money}, distance: {player.Position.DistanceTo(killer.Position)}m. Fail reason: not undercover");
                    contracts.Remove(contract);
                    killer.ResetData("contractId");
                    return;
                }

                factionsManager.sendMessageToAll(factionsManager.type.hitmanAgency, $"{"!{#67AAB1}"}{killer.Name} has successfully completed the contract on {player.Name} for ${contract.money}, distance: {player.Position.DistanceTo(killer.Position)}m.");
                contracts.Remove(contract);
                killer.ResetData("contractId");
            }
        }

        public static void hitmanDuty(Player player)
        {
            if (player.HasData("positionBeforeEnteringHouse") || player.HasData("lastExitHq"))
            {
                if (player.HasSharedData("inDuty") && player.GetSharedData<bool>("inDuty"))
                {
                    player.SendChatMessage($"{"!{#afa1c4}"}*{getRankName(player.GetData<int>("factionRank"))} {player.Name} ({player.Value}) places his uniform and a sniper rifle in his locker.");
                    player.SetSharedData("inDuty", false);
                    return;
                }

                player.SendChatMessage($"{"!{#afa1c4}"}*{getRankName(player.GetData<int>("factionRank"))} {player.Name} ({player.Value}) takes his uniform and a sniper rifle from his locker.");
                player.GiveWeapon(WeaponHash.Switchblade, 1);
                player.GiveWeapon(WeaponHash.Pistol, 500);
                player.GiveWeapon(WeaponHash.Pumpshotgun_mk2, 500);
                player.GiveWeapon(WeaponHash.Heavysniper, 100);

                player.SetClothes(3, 4, 0);
                player.SetClothes(8, 31, 0);
                player.SetClothes(11, 142, 0);
                player.SetClothes(4, 105, 0);
                player.SetClothes(6, 61, 0);
                player.SetClothes(7, 38, 0);
                player.SetSharedData("inDuty", true);

            }
            else
            {
                player.SendChatMessage($"{"!{#CECECE}"} You need to be in HQ or a house.");
            }
        }

        public static string getRankName(int name)
        {
            string toReturn = "";

            switch (name)
            {
                case 1:
                    toReturn = "[1] Hired Killer";
                    break;
                case 2:
                    toReturn = "[2] Silent Assassin";
                    break;
                case 3:
                    toReturn = "[3] Contract Killer";
                    break;
                case 4:
                    toReturn = "[4] Iceman";
                    break;
                case 5:
                    toReturn = "[5] Executioner";
                    break;
                case 6:
                    toReturn = "* The Ghost";
                    break;
                case 7:
                    toReturn = "** Invisible Killer";
                    break;
            }

            return toReturn;
        }

        static int lastID = 0;

        public class contract
        {
            public int id { get; set; }
            public String hitman { get; set; }
            public String target { get; set; }
            public String Player { get; set; }

            public int money { get; set; }
            public bool accepted { get; set; }
            public contract() { }

            public contract(int id, int money, bool accepted, String hitman, String target, String Player)
            {
                this.id = id;
                this.hitman = hitman;
                this.target = target;
                this.Player = Player;
                this.money = money;
                this.accepted = accepted;
            }
        }

        static List<contract> contracts = new List<contract>();
    }
}