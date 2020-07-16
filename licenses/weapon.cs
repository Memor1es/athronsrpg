using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace rpg
{
    public class weaponLicense : Script
    {
       

        [RemoteEvent("StartPracticTest")]
        public void practic(Player player)
        {
            player.GiveWeapon(WeaponHash.Pistol, 50);
        }
        [RemoteEvent("onPlayerFinishLicense")]
        public void onPlayerFinishLicense(Player player, string license, int hours)
        {
            player.SendChatMessage("Congratulations, you got the " + license + " for " + 10 + " hours.");
            player.giveLicense(mainLicense.license.gun, 10);
        }
        
        [Command("buylicense")]
        public void buylicense(Player player)
        {
            player.TriggerEvent("StartWeaponLicense");
            player.SetSharedData("in_weapon_license", true);
        }
        [Command("licenses")]
        public void showLicenses(Player player)
        {
            player.SendChatMessage($"{player.Name}'s licenses:");
            foreach (var item in player.getLicenses())
                player.SendChatMessage($"{Enum.GetName(typeof(mainLicense.license), item.type)} for {item.hours}");
                
            
        }
        [Command("givelicense")]
        public void giveLicense(Player player,string username, mainLicense.license license, int hours)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            var target = playerManager.getPlayer(username);
            if (target.IsNull)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }
            target.giveLicense(license, hours, player);
        }
        [Command("removelicense")]
        public async void removeLicense(Player player, string username, mainLicense.license license)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }

            var target = playerManager.getPlayer(username);
            if (target.IsNull)
            {
                player.SendChatMessage("!{#CECECE}The specified player ID is either not connected or has not authenticated.");
                return;
            }
            await target.removeLicense(license, player);
        }

    }

}
