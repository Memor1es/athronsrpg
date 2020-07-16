using System.Text;
using GTANetworkAPI;

namespace rpg {
    public class main : Script {
        [ServerEvent( Event.ResourceStart )]
        public void ResourceStart( ) {
            Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );

            NAPI.Server.SetCommandErrorMessage( "!{#A9C4E4}SERVER: ~w~Unknown command." );
            NAPI.Server.SetDefaultSpawnLocation(new Vector3(-763.4022, 7.327758, 40.59016));
        }

        [ServerEvent(Event.PlayerConnected)]
        public void onPlayerConnected(Player player) {
            player.TriggerEvent( "connectFreeze" );
        }
    }
}