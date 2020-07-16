using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using rpg.Database;
using System.Threading;

namespace rpg
{
    public class playerManager : Script
    {
        public enum job
        {
            none,
            armsdealer,
            busdriver,
            farmer,
            garbage,
            fisher,
            pizza,
            trucker
        }
     
        [ServerEvent(Event.ResourceStart)]
        public void playerManagerStart()
        {
            System.Timers.Timer timer2 = new System.Timers.Timer();
            timer2.Interval = 2000;
            timer2.Elapsed += timer2_Elapsed;
            timer2.Start();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 3000;
            timer.Elapsed += timer_Elapsed;
             timer.Start();


            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    try
                    {



                        updateAllPlayerData();
                        Console.WriteLine($"[{DateTime.Now.ToString()}] Updated player data for {NAPI.Pools.GetAllPlayers().Count} players. Next will be in 60 seconds at {DateTime.Now.AddSeconds(60).ToString()}");
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Ai luat muita " + e);
                    }
                        Thread.Sleep(60000);
                }
            }).Start();
        }
        [Command("calcangle")]
        public void ca(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            player.SendChatMessage("heading: " + player.Heading);
            player.SendChatMessage("degress heading: " + player.Heading * (180.0 / Math.PI));
        }
        public static Player getPlayer(string targetString)
        {
            return int.TryParse(targetString, out int targetId) ? NAPI.Pools.GetAllPlayers().Find(player => player.Value == targetId) : NAPI.Pools.GetAllPlayers().Find(player => player.Name.ToLower() == targetString.ToLower());
        }
        public static void updateAllPlayerData()
        {
            foreach (var player in NAPI.Pools.GetAllPlayers())
            {
                if(player.HasSharedData("logged"))
                {
                    accountController.databaseUpdate(player);
                }
            }
        }
        public static int doesPlayerHasAdmin(Player player)
        {
            if (player.HasSharedData("admin"))
                return player.GetSharedData<int>("admin");
            return 0;
        }

        [RemoteEvent("updateTempSeconds")]
        public void onPlayerSendTempSeconds(Player player, int seconds)
        {
            player.SetSharedData("tempSeconds", seconds);
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnect(Player client, DisconnectionType type, string reason)
        {
            //accountController.databaseUpdate(client);

        }
        public static int getXpAmount(int playedTime)
        {
            int minutes = playedTime / 60;
            Random rnd = new Random();
            int randomXP = rnd.Next(50, 75);
           
            float final = 0.0F;
            if (minutes >= 15 && minutes < 30)
            {
                final = (float)(randomXP * 1.1);
            }
            else if (minutes >= 30 && minutes < 50)
            {
                final = (float)(randomXP * 1.3);
            }
            else if (minutes >= 50)
            {
                final = (float)(randomXP * 1.5);
            }
            else
            {
                final = (float)(randomXP);
            }
            return (int)final;
        }
        public static int xpToLevel(int xp)
        {
            return (int)(0.08 * Math.Sqrt(xp));
        }
        [Command("stats")]
        public void onStats(Player player)
        {
            int house = player.GetData<int>("house");
            string __toReturn = "";
            switch (house)
            {
                case 0:
                    __toReturn = "None";
                    break;
            }
           


            int phonenumber = player.GetData<int>("phoneNumber");
            string toReturnNumber = "";
            if (phonenumber == -1)
                toReturnNumber = "None";
            else
                toReturnNumber = phonenumber.ToString();

            var job = jobs.jobList.Find(job => job.type == player.GetSharedData<playerManager.job>("job"));
            var faction =  factionsManager.factions.Find(faction => faction.type == player.GetSharedData<factionsManager.type>("faction"));
            player.SendChatMessage($"{"!{#8c0424}"}General: ~w~{player.Name} ({player.Value}) | played: {(player.GetData<int>("seconds") / 60) / 60} hours | Phone no.: {toReturnNumber} | job: {(job != null ? job.name : "None")}");
            player.SendChatMessage($"{"!{#8c0424}"}Account: ~w~level {player.GetData<int>("level")} ({player.GetData<int>("xp")}/NaN experience)");
            player.SendChatMessage($"{"!{#8c0424}"}Economy: ~w~money: ${player.GetSharedData<int>("money")} (cash) | house: {__toReturn}");
            player.SendChatMessage($"{"!{#8c0424}"}Faction: ~w~{( faction != null ? faction.name : "Civilian" )}, 0/3 faction warns");
        }


        [Command("id", "!{#CECECE}Syntax:~w~ /id [name/playerid]")]
        public void onId(Player player, string username)
        {
            Player target = playerManager.getPlayer(username);
            if (target == null)
            {
                if (username.Length >= 3)
                {
                    var posiblePlayers = NAPI.Pools.GetAllPlayers().FindAll(player => player.Name.ToLower().Contains(username));
                    if (posiblePlayers.Count == 0)
                    {
                        player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                        return;
                    }
                    posiblePlayers.ForEach(a =>
                    {
                        string factionName = factionsManager.getFactionName(a.GetSharedData<factionsManager.type>("faction"));

                        a.SendChatMessage($"({a.Value}) {a.Name} | Level: {a.GetData<int>("level")} | Faction: {factionName} (rank {a.GetData<int>("factionRank")}) | Ping: {a.Ping}");
                    });
                }
                else
                {
                    player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                }
           
                return;
            }

            var faction = factionsManager.getFactionName(target.GetSharedData<factionsManager.type>("faction"));

            player.SendChatMessage($"({target.Value}) {target.Name} | Level: {target.GetData<int>("level")} | Faction: {faction} (rank {player.GetData<int>("factionRank")}) | Ping: {target.Ping}");
        }
        [Command("time")]
        public void onTime(Player player)
        {
            player.SendChatMessage($"~w~The current time is {DateTime.Now.Hour}:{DateTime.Now.Minute} ({DateTime.Now.Second} seconds).");
            player.SendChatMessage($"~w~Since last payday: {player.GetSharedData<int>("tempSeconds")} seconds.");

            foreach (Player players in NAPI.Player.GetPlayersInRadiusOfPlayer(30, player))
                players.SendChatMessage($"{"!{#afa1c4}"}* {player.Name} raises his hand and looks down at his watch.");
        }
        [Command("quit", Alias ="q")]
        public void onPlayerQuit(Player player)
        {
            player.TriggerEvent("onPlayerQuit");
        }
        [Command("accept")]
        public async void onPlayerAccept(Player player, string type)
        {
            switch (type)
            {
                case "ticket":
                    {
                        if (player.HasData("needToPayTicket") && player.HasData("needToPayTicketId"))
                        {
                            var amount = player.GetData<int>("needToPayTicket");
                            if (!await player.takeMoneyAsync(amount))
                                player.SendChatMessage("{!#CECECE} You don't have enought money to pay the ticket.");
                            else
                            {
                                var cop = playerManager.getPlayer(player.GetData<int>("needToPayTicketId").ToString());
                                foreach (Player players in NAPI.Player.GetPlayersInRadiusOfPlayer(30, player))
                                    players.SendChatMessage($"{"!{#afa1c4"}{player.Name} ({player.Value}) paid the ticket to {cop.Name} worth ${amount}.");
                                player.SendChatMessage($"{"!{#CECECE}"} You have paid the ticket worth ${amount} {cop.Name} issued you.");
                            await cop.giveMoney(amount);
                                player.ResetData("needToPayTicket"); player.ResetData("needToPayTicketId");
                            }
                        }
                    }
                    break;
                default:
                    {
                        player.SendChatMessage($"{"!{#CECECE}"} Invalid option.");
                        break;
                    }
            }
  

        }
        static int last_hour = -1;
        static async void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (DateTime.Now.Minute == 60 && DateTime.Now.Hour != last_hour)
                {
                     
                    foreach (Player player in NAPI.Pools.GetAllPlayers())
                    {

                        if (player.HasSharedData("logged") && player.GetSharedData<bool>("logged"))
                        {
                            int secondsUntilPayDay = player.GetSharedData<int>("tempSeconds");

                            int moneyToRecieve = 1000 - 13 * player.GetData<int>("level");
                            player.SetSharedData("money", player.GetSharedData<int>("money") + moneyToRecieve);



                            player.SetData("seconds", ((player.GetData<int>("seconds") + secondsUntilPayDay)));
                            var oldlevel = player.GetData<int>("level");
                            int total_ore = (player.GetData<int>("seconds") / 60) / 60;
                            int xpPrimit = getXpAmount(secondsUntilPayDay);

                            player.SetData("xp", player.GetData<int>("xp") + xpPrimit);
                            var lvl = xpToLevel(player.GetData<int>("xp"));
                            if (oldlevel != lvl)
                                player.SendChatMessage("~y~You now have level " + lvl);
                            //  player.SendChatMessage($"old level {player.GetData<int>("level")} new level {lvl}");
                            player.SendChatMessage("Your paycheck has arrived!");
                            player.SendChatMessage("!{#67AAB1}---------------------------------------------------------------------------------------------------------");
                            player.SetData("level", lvl);
                            player.SendChatMessage($"{"!{#CECECE}"}Paycheck: ${moneyToRecieve} | Balance: ${player.GetSharedData<int>("money")} | Received experience: {xpPrimit}");
                            player.SendChatMessage($"{"!{#CECECE}"}This hour you got: {secondsUntilPayDay / 60} minutes ({secondsUntilPayDay} seconds) | Total hours: {total_ore} | Total experience: {player.GetData<int>("xp")} ");

                            player.SendChatMessage("!{#67AAB1}---------------------------------------------------------------------------------------------------------");
                            player.SetSharedData("tempSeconds", 0);

                            accountController.databaseUpdate(player);
                        }
                    }
                    await house.onPayDayWork( );
                    last_hour = DateTime.Now.Hour;
                }
            
            }
           catch(Exception exc)
            {
                Console.WriteLine(exc);
            }

        }
        static void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                foreach (var player in NAPI.Pools.GetAllPlayers())
                {
                    if (player.HasData("findPlayer"))
                    {
                        var target = NAPI.Pools.GetAllPlayers().Find(a => player.HasData("findPlayer") && a.Value == player.GetData<int>("findPlayer"));
                        if (target != null)
                        {
                            player.TriggerEvent("findPlayer", target.Value, target.Position);
                        }
                        else
                            player.ResetData("findPlayer");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
