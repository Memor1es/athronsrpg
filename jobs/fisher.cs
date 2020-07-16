using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
namespace rpg
{
    public class fisher : Script
    {
        static ColShape fishShape = null; static ColShape secondShape = null;
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {


            NAPI.TextLabel.CreateTextLabel($"~w~ID: ~r~{(int)playerManager.job.fisher}\n~w~Job: ~r~Fisherman\n~w~Use ~r~/getjob ~w~to get this job.", new Vector3(-1836.1743, -1206.7675, 14.304758), 15, 1, 4, new Color(0, 162, 255, 255));
            jobs.jobList.Add(new jobs.jobModels(playerManager.job.fisher, "Fisherman", new Vector3(-1836.1743, -1206.7675, 14.304758)));

            var ped = NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("a_m_m_indian_01"), new Vector3(-1836.1743, -1206.7675, 14.304758), 310f, false, true, true);
       

            Console.WriteLine(NAPI.Util.ToJson(fishShape));

        }
        [RemoteEvent("onPlayerStartFish")]
        public void onPlayerStartFish(Player player)
        {
            player.StopAnimation();
            player.PlayAnimation("amb@world_human_stand_fishing@idle_a", "idle_c", 49);
            player.TriggerEvent("onPlayerStartFish");

        }
      /*  [Command("try")]
        public void tra(Player player)
        {
            player.TriggerEvent("onPlayerTryFish");
        } */
        [Command("fish")]
        public void onPlayerTryFish(Player player)
        {
            if (jobs.doesPlayerHasJob(player) != playerManager.job.fisher)
            {
                player.SendChatMessage("!{#CECECE}You aren't a fisherman.");
                return;
            }
               
           
          if (!player.HasAttachment("fishingRod"))
            {
                player.SendChatMessage($"{"!{#CECECE}"}Equip a fishing rod to fish.");
                return;
            }
            player.TriggerEvent("onPlayerTryFish");
           
        }
        [Command("gotofish")]
        public void gotoFish(Player player)
        {
            player.Position = new Vector3(-1842.8291, -1249.904, 8.61578);
        }
        [RemoteEvent("onPlayerFailFish")]
        public void onPlayerFailFish(Player player)
        {
            player.StopAnimation();
            player.AddAttachment("fishingRod", true);
        }
        [RemoteEvent("onPlayerFinishFish")]
        public void onPlayerFinishFish(Player player, string fish)
        {
            player.StopAnimation();
            player.AddAttachment("fishingRod", true);

            foreach (Player players in NAPI.Player.GetPlayersInRadiusOfPlayer(30, player))
                players.SendChatMessage($"~y~{player.Name} ({player.Value}) caught a {fish}.");

            player.SendChatMessage("!{#F0ACAC}You caught a " + fish + " that can be sold for about $15.568.");
            player.SendChatMessage("!{#F0ACAC}Go to a 24/7 shop if you want to sell it.");
            inventory.addItemToPlayer(player, new inventory.item(fish, 1, 0));
            var inventoryItems = inventory.getPlayerInventory(player);
            var itm = inventoryItems.Find(a => a.name == inventory.itemList.fishingRod);
            if (itm == null)
                return;
            itm.external -= 10;

            if (itm.external > 0)

                inventory.updateInventory(player, inventoryItems);
          else
            {
                inventory.removeItem(player, itm.name, 1);
                player.SendChatMessage("Your fishing rod broke.");
            }



            //player.SendChatMessage($"Mai ai durabilitate {  itm.external} / 100");
        }
    }
}

