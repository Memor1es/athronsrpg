using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace rpg
{
    public class vehicleIndicators : Script
    {
        [RemoteEvent("toggleIndicator")]
        public void onToggleIndicator(Player client, int id)
        {
            if (!client.Vehicle.IsNull)
            {
                switch (id)
                {
                    case 0:
                        {
                            bool boolean = client.Vehicle.HasSharedData("indicatorLeft") ? client.Vehicle.GetSharedData<bool>("indicatorLeft") : false;
                            client.Vehicle.SetSharedData("indicatorLeft", !boolean);
                            client.Vehicle.SetSharedData("indicatorRight", false);
                        }
                        break;
                    case 1:
                        {
                            bool boolean = client.Vehicle.HasSharedData("indicatorRight") ? client.Vehicle.GetSharedData<bool>("indicatorRight") : false;
                            client.Vehicle.SetSharedData("indicatorRight", !boolean);
                            client.Vehicle.SetSharedData("indicatorLeft", false);
                        }
                        break;
                }
              
            }
        }
        [Command("lights")]
        public void onToggleLights(Player client)
        {
            if (client.Vehicle != null)
            {
                client.Vehicle.SetSharedData("lights", true);

            }
        }
    }
}
