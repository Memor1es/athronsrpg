using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace rpg
{
    public class chat : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void onStart()
        {
            NAPI.Server.SetGlobalServerChat(false);
        }

        [ServerEvent(Event.ChatMessage)]
        public void EventChatMessage(Player client, string message)
        {

            Player[] clients = NAPI.Pools.GetAllPlayers().FindAll(x => x.Position.DistanceTo2D(client.Position) <= 35).ToArray();

            for (int i = 0; i < clients.Length; i++)
            {
                if (!clients[i].Exists)
                    continue;

                if (!client.GetData<bool>("inCall"))
                    clients[i].SendChatMessage($"{client.Name} ({client.Value}) : {message}");

            }
            client.SetSharedData("message", message);
        }
    }
}