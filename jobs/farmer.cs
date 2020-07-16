using GTANetworkAPI;

namespace rpg {
    class farmer : Script {
        [ServerEvent( Event.ResourceStart )]
        public void onResourceStart( ) {
            NAPI.Task.Run( ( ) => {
                NAPI.TextLabel.CreateTextLabel($"~w~ID: ~r~{(int)playerManager.job.farmer}\n~w~Job: ~r~Farmer\n~w~Use ~r~/getjob ~w~to get this job.", new Vector3(1219.42, 1847.864, 78.99397), 15, 1, 4, new Color(0, 162, 255, 255));           
                jobs.jobList.Add( new jobs.jobModels(playerManager.job.farmer, "Farmer", new Vector3( 1219.42, 1847.864, 78.99397 ) ) );

                NAPI.Ped.CreatePed( NAPI.Util.GetHashKey( "a_m_m_farmer_01" ), new Vector3( 1219.42, 1847.864, 79 ), 180f, false, true, true );
            } );
        }

        [ServerEvent( Event.PlayerEnterVehicle )]
        public void onPlayerEnterVehicle( Player player, Vehicle vehicle, int seat ) {

            if ( vehicle.HasData( "type" ) && vehicle.GetData<vehicleManager.type>( "type" ) == vehicleManager.type.farmer) {
                if ( jobs.doesPlayerHasJob( player ) != playerManager.job.farmer) {
                    player.SendChatMessage( "!{#2CB6D0}You are not a Farmer." );
                    player.WarpOutOfVehicle( );
                    return;
                }

                vehicle.SetData( "in_use", true );
                vehicle.SetData( "need_respawn", false );
                player.TriggerEvent("playerStartedFarmer");
                player.SetSharedData( "inFarmerWork", true );
                player.SetSharedData( "inJob", true );
            }
        }

        [RemoteEvent( "farmerFailed" )]
        public void OnPlayerFarmerFailed( Player player, string reason ) {
            player.SendChatMessage( $"!{"{#2CB6D0}"}You job failed because {reason}" );

            if ( player.Vehicle != null )
                player.Vehicle.Delete( );

            player.SetSharedData( "inJob", false ); //might crash
        }

        [RemoteEvent( "playerFinishedFarmer" )]
        public void OnPlayerFinishedFarmer( Player player ) {
            player.SetSharedData( "inJob", false );

            player.SendChatMessage( $"You sold a flour bag and.... to be continued. " );
            player.WarpOutOfVehicle( );

            if ( player.Vehicle != null )
                player.Vehicle.Delete( );
        }
    }
}
