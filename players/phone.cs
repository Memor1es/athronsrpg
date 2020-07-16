using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using rpg.Database;
using static rpg.inventory;

namespace rpg
{
    public class phone : Script
    {
        public class callData
        {
            public string caller { get; set; }
            public string answerer { get; set; }
            public bool answared { get; set; }
        }


        static Random rand = new Random();
        static string GenPhone()
        {
            StringBuilder telNo = new StringBuilder(6);
            int number;

            number = rand.Next(10, 20);
            telNo = telNo.Append(number.ToString());

            number = rand.Next(20, 73);
            telNo = telNo.Append(number.ToString());

            number = rand.Next(50, 93);
            telNo = telNo.Append(number.ToString());

            return telNo.ToString();
        }
        public static Player GetPlayerByPhoneNumber(int number)
        {
            if (number == -1)
                return null;

            return NAPI.Pools.GetAllPlayers().Find(player => player.GetData<int>("phoneNumber") == number);

        }
        public static int GeneratePhoneNumber()
        {
            int random_number = -1;
            for (int i = 0; i < 10; i++)
            {
                random_number = Int32.Parse(GenPhone());
                if (NAPI.Pools.GetAllPlayers().Find(client => client.GetData<int>("phoneNumber") == random_number) == null)
                    break;

            }
            return random_number;
        }
        [Command("generatenumber")]
        public void GenerateNr(Player player)
        {
            player.SendChatMessage($"{GeneratePhoneNumber()}");
        }
        [Command("sms", GreedyArg = true)]
        public void SendSms(Player player, int phonenumber, string message)
        {

            if (inventory.hasPlayerItem(player, itemList.phone))
            {

                Player targetplayer = GetPlayerByPhoneNumber(phonenumber);

                if (targetplayer != null)
                {
                    targetplayer.SendChatMessage($"~y~SMS from {player.Name}: {message} ");
                    player.SendChatMessage($"{"!{D5EAFF}"}SMS sent to {targetplayer.Name} : {message} ");
                }
                else
                    player.SendChatMessage("Invalid phone number, make sure you typed it correctly. ");
            }
            else { player.SendChatMessage("You don't have a phone."); }


        }
        [Command("number")]
        public void CMDNumber(Player player, string id)
        {
            if (inventory.hasPlayerItem(player, itemList.phonebook))
            {
                var target = playerManager.getPlayer(id);//.GetPlayer(id);
                if (target != null)
                {
                    var number = target.HasData("phoneNumber") ? target.GetData<int>("phoneNumber") : -1;
                    if (number != -1)
                        player.SendChatMessage($"Player {target.Name} | Number: {number}");
                    else player.SendChatMessage("!{#CECECE}Invalid number.");
                }
                else
                    player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
            }
            else player.SendChatMessage("You don't have a phonebook.");
        }
        [Command("call")]
        public static void Call(Player player, int phonenumber)
        {
            if (inventory.hasPlayerItem(player, itemList.phone))
            {
                var number = GetPhoneNumber(player);
                if (number != 1)
                {
                    Player targetplayer = GetPlayerByPhoneNumber(phonenumber);

                    if (targetplayer != null)
                    {
                        targetplayer.SendChatMessage($"{player.Name} is calling you. Type /p to answer.");
                        callData call = new callData();
                        call.answerer = targetplayer.Name;
                        call.caller = player.Name;
                        call.answared = false;
                        targetplayer.SetData("callData", call);
                        player.SetData("callData", call);
                    }
                    else
                        player.SendChatMessage("!{#CECECE}That number is not available right now.");
                }
                else
                    player.SendChatMessage("You don't have a phone.");

            }
            else
                player.SendChatMessage("You don't have a phone.");
        }
       
        public static int GetPhoneNumber(Player player)
        {
            if (player.HasData("phoneNumber"))
                return player.GetData<int>("phoneNumber");

            return -1;
        }
        public static callData GetCallData(Player player)
        {
            return player.GetData<callData>("callData");
        }
        [Command("p", Alias = "phone")]
        public void Hangout(Player player)
        {
            if (player.HasData("callData"))
            {
                callData call = GetCallData(player);
                call.answared = true;

                player.SetData("callData",call);
                var caller = NAPI.Player.GetPlayerFromName(call.caller); caller.SetData("calldata", call);

                caller.SendChatMessage($"{player.Name} answered. Type in chat to speak with him.");
                player.SetData("inCall", true);
                caller.SetData("inCall", true);
                player.SendChatMessage("Call answered.");
            }
            else
            {
                player.SendChatMessage("No one called you.");
            }
        }
        public static async Task  onPlayerBuyPhone(Player player)
        {
            var number = GeneratePhoneNumber();
            player.SetData("phoneNumber", number);
            await databaseManager.updateQuery($"UPDATE accounts SET phoneNumber = '{number}' WHERE username = 'clau'").Execute();
        }
        [Command("h", Alias = "hangup")]
        public void Hangup(Player player)
        {
            if (player.HasData("callData") && player.HasData("inCall") && player.GetData<bool>("inCall"))
            {
                callData call = player.GetData<callData>("callData");
                NAPI.Player.GetPlayerFromName(call.answerer).ResetData("callData");
                NAPI.Player.GetPlayerFromName(call.caller).ResetData("callData");
                var calller = NAPI.Player.GetPlayerFromName(call.caller);
                var anwserer = NAPI.Player.GetPlayerFromName(call.answerer);
                calller.SendChatMessage("!{CECECE}Call got hung up.");
                anwserer.SendChatMessage("!{CECECE}Call got hung up.");
                calller.SetData("inCall", false);
                anwserer.SetData("inCall", false);

            }
            else
                player.SendChatMessage("You are not during a call.");
        }
        [ServerEvent(Event.ChatMessage)]
        public void EventChatMessage(Player client, string message)
        {
            if (client.HasData("callData") && client.HasData("inCall") && client.GetData<bool>("inCall"))
            {
                callData call = client.GetData < callData>("callData");
                if (call.answared)
                {
                    NAPI.Player.GetPlayerFromName(call.caller).SendChatMessage($"[In call] {client.Name}: {message}");
                    NAPI.Player.GetPlayerFromName(call.answerer).SendChatMessage($"[In call] {client.Name}: {message}");


                    Player[] clients = NAPI.Pools.GetAllPlayers().FindAll(x => x.Position.DistanceTo2D(client.Position) <= 35).ToArray();

                    for (int i = 0; i < clients.Length; i++)
                    {
                        if (!clients[i].Exists)
                            continue;
                        if (clients[i] == client || clients[i] == NAPI.Player.GetPlayerFromName(call.answerer))
                            return;

                            clients[i].SendChatMessage($"{client.Name} ({client.Value}) : [In call] {message}");

                    }
                }

            }

        }

    }
}
