using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
namespace rpg
{
    public class dmv : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void onServerStart()
        {
                uint hash = NAPI.Util.GetHashKey("Dilettante");
                NAPI.TextLabel.CreateTextLabel("[Driving School]", new Vector3(-722.1975, -99.04565, 38.73443), 20.0f, 0.75f, 4, new Color(140, 4, 36), false);
                NAPI.TextLabel.CreateTextLabel("Type /exam to start the driving school exam.", new Vector3(-722.1975, -99.04565, 38.12257), 20.0f, 0.75f, 4, new Color(206, 206, 206, 255), false);

            NAPI.Marker.CreateMarker(1, new Vector3(-722.1975, -99.04565, 36.12257), new Vector3(), new Vector3(), 5, new Color(255, 235, 110, 100));
        }

        [Command("dmv")]
        public void dmvtp(Player client)
        {
            Vector3 spawnPos = new Vector3(-727.5327, -68.84687, 41.22844);
            client.Position = spawnPos;
        }
        [Command("vehhealth")]
        public void s(Player client)
        {
            if (client.Vehicle != null)
                client.SendChatMessage("Current vehicle health: " + client.Vehicle.Health);
        }

        [RemoteEvent("finishDMV")]
        public void onPlayerFinishDMV(Player client, int hours)
        {
            client.ResetData("inDMV");
           
            client.giveLicense(mainLicense.license.vehicle, hours);
            client.SendChatMessage($"Congratulations, you received the driving license for {hours} hours!");
        }
        [RemoteEvent("playerFailDMV")]
        public void onPlayerFailDMV(Player client)
        {
            client.ResetData("inDMV");
            if (client.Vehicle != null)
                client.Vehicle.Delete();

        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seat)
        {
            if (!vehicle.HasData("type") || vehicle.HasData("type") && vehicle.GetData<vehicleManager.type>("type") != vehicleManager.type.dmv)
                return;

            if (!player.HasData("inDMV"))
            {
                player.WarpOutOfVehicle();
                player.SendChatMessage("!{#CECECE}This car can be only driven by driving school users.");
                return;
            }

             if (seat != 0)
             {
                player.SendChatMessage("!{#CECECE}Please enter the driver's seat.");
                return;
             }

            player.TriggerEvent("startDMV");

        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicle(Player player, Vehicle vehicle)
        {
           if (player.HasData("inDMV") && vehicle.HasData("type") && vehicle.GetData<vehicleManager.type>("type")  == vehicleManager.type.dmv)
            {
                 player.WarpOutOfVehicle();
                player.Delete();
                player.TriggerEvent("onPlayerFailDMV");

            }
  
        }
        [Command("exam")]
        public void StartExam(Player client)
        {
            if (client.Position.DistanceTo(new Vector3(-722.1975, -99.04565, 36.12257)) > 10)
            {
                client.SendChatMessage("!{#CECECE}You need to be at the driving school.");
                return;
            }
            if (client.GetData<bool>("inDmv"))
            {
                client.SendChatMessage("!{#CECECE}You are already in the driving school exam.");
                return;
            }

            if (client.hasLicense(mainLicense.license.vehicle))
            {
                client.SendChatMessage("!{#CECECE}You already have a driver's license.");
                return;
            }


            Vector3 pos = new Vector3(-727.5327, -68.84687, 41.22844);

           
            client.SetData("inDMV", true);
            client.SendChatMessage("!{#67AAB1}Go in the parking lot and get in an exam car.");
            
        }
    }
}
