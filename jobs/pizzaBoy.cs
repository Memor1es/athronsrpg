using GTANetworkAPI;
using System;
using System.Collections.Generic;

namespace rpg {
    class pizza_boy : Script {
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart() {
            NAPI.Task.Run(() => {
                NAPI.TextLabel.CreateTextLabel($"~w~ID: ~r~{(int)playerManager.job.pizza}\n~w~Job: ~r~Pizza Boy\n~w~Use ~r~/getjob ~w~to get this job.", new Vector3(-1182.634, -883.8701, 13.800), 15, 1, 4, new Color(0, 162, 255, 255));
                jobs.jobList.Add(new jobs.jobModels(playerManager.job.pizza, "Pizza Boy", new Vector3(-1182.634, -883.8701, 15.76559)));

                NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("s_m_m_linecook"), new Vector3(-1182.634, -883.8701, 13.800), 310f, false, true, true);
            });

        }

        [Command("pizza")]
        public void onGetPizza(Player player) {
            if (jobs.doesPlayerHasJob(player) != playerManager.job.pizza) {
                player.SendChatMessage("!{#2CB6D0}You are not a Pizza Boy.");
                return;
            }

            if (player.Vehicle != null && (player.Vehicle.HasData("type") && player.Vehicle.GetData<vehicleManager.type>( "type" ) != vehicleManager.type.pizza ) || !player.Vehicle.HasData("type")) {
                player.SendChatMessage("!{#CECECE}You can only use this command while you are on a pizza scooter.");
                return;
            }
            if (player.HasSharedData("inJob"))
            {
                player.SendChatMessage( "You are already in pizza boy job." );
                return;
            }

            player.Vehicle.AddAttachment("pizzaBoxCar", false);

            player.SetSharedData( "inJob", true );
            NewPlayerPizzaLocation(player);
        }
        [RemoteEvent("TakePizzaFromVehicle")]
        public void TakePizzaFromVehicle(Player player)
        {
            var veh = vehicleManager.GetClosestVehicle(player);
            if (veh != null && veh.HasAttachment("pizzaBoxCar"))
            {
                player.AddAttachment("pizzaBox", false);
                player.SetSharedData("pizzaInHands", true);
                player.PlayAnimation("anim@heists@box_carry@", "idle", 49); // 49 = Loop + Upper Body Only + Allow Rotation
            }
        }

        [RemoteEvent("playerDeliveredPizza")]
        public void OnPlayerDeliveryPizza(Player player, int money)
        {
            player.SendChatMessage($"~g~You got {money} for the delivered pizza.");
            player.AddAttachment("pizzaBox", true);
            player.SetSharedData("pizzaInHands", false);
            player.StopAnimation();
            NewPlayerPizzaLocation(player);
        }

        public void NewPlayerPizzaLocation(Player player)
        {
            var location = GetRandomPizzaDeliveryLocation(player);
            int next_money = (int)player.Position.DistanceTo(location) * 23;
            player.TriggerEvent("NewPizzaLocation", location, next_money);
        }

        public Vector3 GetRandomPizzaDeliveryLocation(Player player)
        {


            int randomNumber = -1;


            Random rnd = new Random();

            while (true)
            {
                randomNumber = rnd.Next(house.houses.Count);
                if (player.HasData("lastHouse") && player.GetData<int>("lastHouse") == randomNumber)
                    continue;
                else
                    break;

            }

            house.model random_house = house.houses[randomNumber];

            if (random_house == null)
            {
                Console.WriteLine("Error occured. Check your code.");
                player.SendChatMessage("Error ocoured. Please contact the server admin.");
                return new Vector3();
            }

            player.SetData("lastHouse", randomNumber);
            return random_house.enter;
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatID) {
            if (vehicle.GetData<vehicleManager.type>("type") == vehicleManager.type.pizza)
            {
                if (jobs.doesPlayerHasJob(player) != playerManager.job.pizza ) {
                    player.SendChatMessage("!{#2CB6D0}You are not a Pizza Boy.");
                    player.WarpOutOfVehicle();
                    return;
                }
            }
        }

    }
    
}
