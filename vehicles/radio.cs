using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace rpg 
{
    public class radio : Script
    {
        [Command( "startradio" )]
        public void onPlayerStartRadio( Player player, string url )
        {
            if ( player.Vehicle == null )
            {
                player.SendChatMessage( "You need to be in a vehicle to use this command." );
                return;
            }

            player.Vehicle.SetSharedData( "radio", url );
            player.SendChatMessage( "Currently playing Formatia Marinica Namol - Sarba de la Macin ( Videoclip ) 2019." );
        }
    }
}
