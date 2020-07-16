using GTANetworkAPI;
using rpg.Database;
using System.Threading.Tasks;

namespace rpg {
    class arms_dealer : Script {
        [ServerEvent( Event.ResourceStart )]
        public void onResourceStart( ) {
            NAPI.Task.Run( ( ) => {
                NAPI.TextLabel.CreateTextLabel( $"~w~ID: ~r~{(int)playerManager.job.armsdealer}\n~w~Job: ~r~Arms Dealer\n~w~Use ~r~/getjob ~w~to get this job.", new Vector3( 244.1408, -42.49718, 69.89651 ), 15, 1, 4, new Color( 0, 162, 255, 255 ) );
                NAPI.TextLabel.CreateTextLabel( "Materials Pickup!\n\nType /getmats as an Arms Dealer\nto collect materials.", new Vector3( 188.6136, 348.1722, 107.6055 ), 15, 1, 4, new Color( 0, 162, 255, 255 ) );
                jobs.jobList.Add( new jobs.jobModels(playerManager.job.armsdealer, "Arms Dealer", new Vector3( 244.1408, -42.49718, 69.89651 ) ) );

                NAPI.Ped.CreatePed( NAPI.Util.GetHashKey( "s_m_y_dealer_01" ), new Vector3( 244.1408, -42.49718, 69.89651 ), 110f, false, true, true );
            } );
        }

        [Command( "getmats" )]
        public void onGetMats( Player player ) {
            if ( jobs.doesPlayerHasJob( player ) != playerManager.job.armsdealer) {
                player.SendChatMessage( "!{#2CB6D0}You are not an Arms Dealer." );
                return;
            }

            if ( player.HasSharedData( "inArmsDealerWork" ) ) {
                player.TriggerEvent( "destroyAllCheckpoints" );
            }

            if ( player.Position.DistanceTo( new Vector3( 188.6136, 348.1722, 107.6055 ) ) > 10 ) {
                player.TriggerEvent( "addCheckpointToMats", true );
                player.SetSharedData( "inArmsDealerWork", true );
            } else {
                player.TriggerEvent( "delieverMatsToCheckpoint", true );
                player.TriggerEvent( "inArmsDealerWork", true );
            }
        }

        [RemoteEvent( "finishedDelieveringMats" )]
        public async Task onFinishedDelieveringMatsAsync( Player player ) {
            player.SetSharedData( "inArmsDealerWork", false );
            player.SetSharedData( "materials", player.GetSharedData<int>( "materials" ) + 1000 );

            await databaseManager.updateQuery( $"UPDATE accounts SET materials = '{player.GetSharedData<int>( "materials" )}' WHERE username = '{player.Name}'" ).Execute( );
        }
    }
}
