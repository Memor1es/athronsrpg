using GTANetworkAPI;
using rpg.Database;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace rpg
{
    public class gunshop : Script
    {
        public class external
        {
           public Vector3 buyPosition { get; set; }
            public Vector3 gunPosition { get; set; }
            public float gunRotationZ { get; set; }
            public Vector3 pedPosition { get; set; }
            public float pedHeading { get; set; }
        }
     


        [ServerEvent(Event.ResourceStart)]
        public async void Start()
        {
            Console.WriteLine("GunShop started.");
       
             await db.loadAllGunshops();
            NAPI.Task.Run(() =>
            {
               
                foreach (var shop in business.getBusinessList(type.gunShop))
                {
                    NAPI.Task.Run( ( ) => {
                        business.createBusiness( shop );


                        NAPI.Ped.CreatePed( NAPI.Util.GetHashKey( "u_m_m_aldinapoli" ), shop.external.pedPosition, shop.external.pedHeading, false, true, true );
                        ColShape pnsshape = NAPI.ColShape.CreateSphereColShape( shop.external.buyPosition, 5 );
                        pnsshape.SetData( "gunshop", 1 );
                        pnsshape.SetData( "bizid", shop.bizId );
                        Console.WriteLine( $"shop {shop.bizId}" );
                       
                    } );
                }
            });
        }
        [RemoteEvent("onPlayerBuyWeapon")]
        public async void onPlayerBuyWeapon(Player player, string weapon, int price, int bizId)
        {
            if ( await player.takeMoneyAsync( price ) )
            {
                player.GiveWeapon( ( WeaponHash ) NAPI.Util.GetHashKey( weapon ), 120 );
                await business.utilities.addBalance( bizId, price );
            }

        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public void onPlayerEntercolShape(ColShape shape, Player player)
        {

            if (shape.HasData("gunshop") && shape.HasData("bizid"))
            {

                business.model gs = business.businessList.Find(shop => shop.bizId == shape.GetData<int>("bizid"));

                player.TriggerEvent("EnterInGunShop", gs);
            }
        }
        [ServerEvent(Event.PlayerExitColshape)]
        public void onPlayerExitcolShape(ColShape shape, Player player)
        {

            if (shape.HasData("gunshop") && shape.HasData("bizid"))
            {

                player.TriggerEvent("ExitFromGunShop");
            }
        }
        [Command("addgunshop")]
        public void addGunShop(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            business.model gs = new business.model();

            gs.owner = "AdmBot";

            gs.sale = 0;
            gs.enterPos = player.Position;
            gs.external = new gunshop.external();
            player.SetData("gunShopCreator", gs);
            player.SendChatMessage($"Bizul de tip GunShop a fost adaugat cu id-ul {gs.bizId}. acesta find al lui {gs.owner} fiind spre vnzare cu pretu  de {gs.sale}.");
        }
        [Command("finishgunshop")]
        public  async void finishgs(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("gunShopCreator"))
            {
                player.SendChatMessage("nu esti in creearea unui gunshop");
                return;
            }
            var gunShop = player.GetData<business.model>("gunShopCreator");
            if (player.HasData("gsEditing") && player.GetData<bool>("gsEditing"))
            {
                player.SendChatMessage("a fost updatat gunshopu");
                await db.updateGunShop(gunShop, gunShop.bizId);
                player.ResetData("gsEditing");
            }
            else
            {
                await db.createGunShop(gunShop);
                player.SendChatMessage($"A fost creat bizu.");
            }
        }
        [Command("editgunshop")]
        public void editGunShop(Player player, int id)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            var gunShop = business.businessList.Find(shop => shop.bizId == id);
            if (gunShop == null)
            {
                player.SendChatMessage("nu exista.");
                return;
            }

            player.SetData("gunShopCreator", gunShop); player.SetData("gsEditing", true);
        }
     
        [Command("gssetpedposition")]
        public static void setPedPosition(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("gunShopCreator"))
            {
                player.SendChatMessage("nu esti in creearea unui gunshop");
                return;
            }
            var gunShop = player.GetData<business.model>("gunShopCreator");
            gunShop.external.pedPosition = player.Position;
            gunShop.external.pedHeading = player.Heading;
            player.SendChatMessage($"A fost setata pozitia pedului pentru biz");
        }
        [Command("gssetgunposition")]
        public  void setGunPosition(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("gunShopCreator"))
            {
                player.SendChatMessage("nu esti in creearea unui gunshop");
                return;
            }
            var gunShop = player.GetData<business.model>("gunShopCreator");
            gunShop.external.gunPosition = player.Position;
            gunShop.external.gunRotationZ = player.Heading;
            player.SendChatMessage($"A fost setata pozitia gunului pentru biz");
        }
        [Command("gssetgunrotation")]
        public void setGunRotation(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("gunShopCreator"))
            {
                player.SendChatMessage("nu esti in creearea unui gunshop");
                return;
            }
            var gunShop = player.GetData<business.model>("gunShopCreator");
            gunShop.external.gunRotationZ = player.Rotation.Z;
            player.SendChatMessage($"A fost setata rotatia gunului pentru biz");
        }
        
        [Command("gssetbuypos")]
        public  void setCheckIn(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("gunShopCreator"))
            {
                player.SendChatMessage("nu esti in creearea unui gunshop");
                return;
            }
            var gunShop = player.GetData<business.model>("gunShopCreator");
            gunShop.external.buyPosition = player.Position;
            player.SendChatMessage($"A fost setata pozitia buy pentru biz");
        }

      
        public class db
        {

            public static async Task<int> createGunShop(business.model gs)
            {
                return await databaseManager.updateQuery($"INSERT INTO business (owner, name, type, enterPos, external) VALUES ('{gs.owner}', '{gs.name}', '{(int)type.gunShop}', '{NAPI.Util.ToJson(gs.enterPos)}', '{NAPI.Util.ToJson(gs.external)}')").Execute();
            }
            public static async Task<int> updateGunShop(business.model gs, int id)
            {
                return await databaseManager.updateQuery($"UPDATE business SET external = '{NAPI.Util.ToJson(gs.external)}' WHERE id = '{id}'").Execute();
              
            }
            public static async Task loadAllGunshops()
            {
             
               
                await databaseManager.selectQuery($"SELECT * FROM business WHERE type = '{(int)type.gunShop}'", (DbDataReader reader) =>
                {
                    business.model shop = new business.model();
                    shop.bizId = (int)reader["id"];
                    shop.owner = (string)reader["owner"];
                    shop.name = (string)reader["name"];
                    shop.sale = (int)reader["sale"];
                  
                    shop.external = NAPI.Util.FromJson<external>((string)reader["external"]);

                    shop.balance = ( int ) reader[ "balance" ];
                    shop.type = type.gunShop;
                    shop.enterPos = NAPI.Util.FromJson<Vector3>((string)reader["enterPos"]);
                    shop.exitPos = NAPI.Util.FromJson<Vector3>((string)reader["exitPos"]);

                    business.businessList.Add(shop);
                }).Execute();

               
            }
   
        }
    }
}
