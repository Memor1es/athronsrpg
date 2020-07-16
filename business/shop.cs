using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using rpg.Database;
namespace rpg
{
    
    public class shop : Script
    {
        #region
        public class buyItem
        {
            public int price { get; set; }
            public int totalPrice { get; set; }
            public int count { get; set; }
            public string type { get; set; }
        }
        public class external
        {
            public Vector3 checkin { get; set; }
            public Vector3 checkout { get; set; }
            public List<raft> rafts { get; set; }
            public Vector3 center { get; set; }

        }
        public class item
        {
            public int price { get; set; }
            public string type { get; set; }
            public int count { get; set; }
            public item() { }
            public  item(string name, int price)
            {
                this.price = price;
                this.count = 1;
                this.type = name;
             }
        }
        public class raft
        {
            public string name { get; set; }
            public Vector3 position { get; set; }
            public List<item> items { get; set; }
            public raft() { }
            public raft(Vector3 position, string name)
            {
                this.name = name;
                this.position = position;
                this.items = new List<item>();
            }
        }
        #endregion   //classes and stuff
        public static async Task readAllShops()
        {
          
            await databaseManager.selectQuery($"SELECT * FROM business WHERE type = '{(int)type.shop}'", (DbDataReader reader) =>
            {
                business.model shop = new business.model();
                shop.bizId = (int)reader["id"];
                shop.owner = (string)reader["owner"];
                shop.external = NAPI.Util.FromJson<external>((string)reader["external"]);
                shop.enterPos = NAPI.Util.FromJson<Vector3>((string)reader["enterPos"]);
                shop.type = type.shop;
                shop.balance = ( int ) reader[ "balance" ];
                shop.sale = ( int ) reader[ "sale" ];
                shop.name = (string)reader["name"];
                business.businessList.Add(shop);
            }).Execute();

          
        }

        [Command("gotoshop")]
        public void shopp(Player player, int id)
        {
            player.Position = business.getBusinessList( type.gunShop )[ id].enterPos;
        }
        [ServerEvent(Event.ResourceStart)]
        public async void Start()
        {
            await readAllShops( );
            NAPI.Task.Run(() =>
            {
                foreach (var shop in business.getBusinessList( type.shop ) )
                {

                    ColShape pnsshape = NAPI.ColShape.CreateSphereColShape(shop.external.center, 8, 0);
                    pnsshape.SetData("shop", 1);
                    pnsshape.SetData("bizid", shop.bizId);
                    business.createBusiness( shop );

                }
            });
        }
        [RemoteEvent( "playerTakeShopBag" )]
        public void playerTakeShopBag(Player player )
        {
            player.AddAttachment( "shopBag", false );
        }
        [Command("cos")]
        public void playecsd(Player player)
        {
            player.AddAttachment( "shopBag", false );
        }
        [Command("createshop")]
        public void AddShopCommand(Player player, string name)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            business.model shop = new business.model();
            shop.name = name;
            shop.owner = "AdmCmd"; shop.external = new external();

            shop.external.rafts = new List<raft>();
            
            shop.enterPos = player.Position;
            player.SetData("inShopCreation", shop);
            player.SendChatMessage($"Esti in creearea shopului. Te rog acum sa seetezi iesirea daca este nevoie. Scrie /setexit false/true (false => nu are iesire deci nu e in vW $$ true => are iesire si este in vw)");
        }
        [Command("editshop")]
        public void onPlayerEditShop(Player player, int id)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (player.HasData("inShopEditing"))
            {
                player.SendChatMessage("esti deja in editarea shopului.");
                return;
            }
            business.model shop =  business.getBusinessList(type.shop).Find(a => a.bizId == id);
            if (shop == null)
            {
                player.SendChatMessage("nu exista shopul ");
                return;
            }
            player.SetData("inShopCreation", shop);
            player.SendChatMessage("Esti in editatrea shopului acum,");
            player.SetData("inShopEditing", true);
        }
        [Command("viewrafts")]
        public void onPlayerViewRaft(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea/editarea unui shop.");
                return;
            }

            business.model shop = player.GetData<business.model>("inShopCreation");
            player.SendChatMessage("rafturile sunt: ");
            for (int i = 0; i < shop.external.rafts.Count; i++)
            {
                raft raft = shop.external.rafts[i];
                player.SendChatMessage($"ID: {i} Nume: {raft.name}, Items: {raft.items.Count}, Position: {NAPI.Util.ToJson(raft.name)}");
            }

        }
        [Command("viewitems")]
        public void viewItems(Player player, int id)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }

            business.model shop = player.GetData<business.model>("inShopCreation");

            if (shop.external.rafts[id] == null)
            {
                player.SendChatMessage($"Raft invalid. Foloseste comanda /viewrafts {shop.bizId} pentru a vedea rafturile.");
                return;
            }

            for (int i = 0; i < shop.external.rafts[id].items.Count; i++)
            {
                var item = shop.external.rafts[id].items[i];
                player.SendChatMessage($"ID: {i} Item: {item.type} Price: {item.price}");
            }
        }
        [Command("additem")]
        public void addItem(Player player, int indexRaft, string item, int price)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }
            var itemDB = inventory.items.Find(a => a.name.ToLower() == item.ToLower());
            if (itemDB == null)
            {
                player.SendChatMessage("item-ul nu exista.");
                return;
            } 

            business.model shop = player.GetData<business.model>("inShopCreation");

            if (shop.external.rafts[indexRaft] == null)
            {
                player.SendChatMessage($"Raft invalid. Foloseste comanda /viewrafts {shop.bizId} pentru a vedea rafturile.");
                return;
            }
            shop.external.rafts[indexRaft].items.Add(new item(itemDB.name, price));
            player.SendChatMessage("a fost sters itemu");

        }
        [Command("deleteitem")]
        public void deleteItem(Player player, int indexRaft, int index)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }

            business.model shop = player.GetData<business.model>("inShopCreation");

            if (shop.external.rafts[indexRaft] == null)
            {
                player.SendChatMessage($"Raft invalid. Foloseste comanda /viewrafts {shop.bizId} pentru a vedea rafturile.");
                return;
            }
            shop.external.rafts[indexRaft].items.RemoveAt(index);
            player.SendChatMessage("a fost sters itemu");

        }
        [Command("setexit")]
        public void setExit(Player player, bool value)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }
          
            business.model shop = player.GetData<business.model>("inShopCreation");
            if (value)
            {
                shop.exitPos = player.Position;
            }
            else
                shop.exitPos = new Vector3();
                player.SendChatMessage("A fost setata pozitia de iesire.");
        }
        [Command("setcenter")]
        public void setCenter(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }

            business.model shop = player.GetData<business.model>("inShopCreation");
            shop.external.center = player.Position;
        }
        [Command("setcheckin")]
        public void setCheckin(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }

            business.model shop = player.GetData<business.model>("inShopCreation");
            shop.external.checkin = player.Position;
            player.SendChatMessage("A fost setata pozitia de checkin(unde iei cosu).");
        }
        [Command("setcheckout")]
        public void setCheckout(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }

            business.model shop = player.GetData<business.model>("inShopCreation");
            shop.external.checkout = player.Position;
            player.SendChatMessage("A fost setata pozitia de checkout(unde platesti).");
        }
        [Command("addraft")]
        public void addraft(Player player, string name)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }

            business.model shop = player.GetData<business.model>("inShopCreation");
            shop.external.rafts.Add(new raft(player.Position, name));
            player.SendChatMessage("A fost creat raftul Foloseste comanda /viewrafts pentru a vedea rafturile.");
        }
        [Command("viewrafts")]
        public void addraft(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }

            business.model shop = player.GetData<business.model>("inShopCreation");
            
            player.SendChatMessage("Rafturile sunt:");
            for (int i = 0; i < shop.external.rafts.Count; i++)
            {
                player.SendChatMessage($"id: {i} nume: {shop.external.rafts[i]} positia: {NAPI.Util.ToJson(shop.external.rafts[i].position)}");
            }
            player.SendChatMessage("Foloseste comanda /removeraft id pentru a sterge un raft.");
        }
        [Command("removeraft")]
        public void addraft(Player player, int id)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }

            business.model shop = player.GetData<business.model>("inShopCreation");

            bool exist = shop.external.rafts[id] != null ? true : false;
            if (!exist)
            {
                player.SendChatMessage("Raftul nu exista. Foloseste comanda /viewrafts pentru a vedea rafturile.");
                  
                return;
            }
            player.SendChatMessage("A fost sters raftul.");
        }

        [Command("finishshop")]
        public async void finishShop(Player player)
        {
            if (playerManager.doesPlayerHasAdmin(player) <= 0)
            {
                player.SendChatMessage("!{#CEF0AC}You need to be an admin.");
                return;
            }
            if (!player.HasData("inShopCreation"))
            {
                player.SendChatMessage("Nu esti in creearea unui shop.");
                return;
            }
            business.model shop = player.GetData<business.model>("inShopCreation");
            if (player.HasData("inShopEditing"))
            {
              
                await databaseManager.updateQuery($"UPDATE business SET external = '{NAPI.Util.ToJson(shop.external)}' WHERE id = '{shop.bizId}'").Execute();
            }
            else
            {
            

                shop.bizId = await databaseManager.updateQuery($"INSERT INTO business (type, name, owner, enterPos, exitPos, external) VALUES ('{(int)type.shop}', '{shop.name}','{shop.owner}','{NAPI.Util.ToJson(shop.enterPos)}', '{NAPI.Util.ToJson(shop.exitPos)}', '{NAPI.Util.ToJson(shop.external)}')").Execute();

                player.SendChatMessage($"A fost creat shopul cu id-ul {shop.bizId}");
            }

        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public void OnPlayerEnterColshape(ColShape shape, Player player)
        {

            if (shape.HasData("shop") && shape.HasData("bizid"))
            {

                business.model shop = business.businessList.Find(x => x.bizId == shape.GetData<int>("bizid"));
                Console.WriteLine($"{NAPI.Util.ToJson(shop)}");
                player.TriggerEvent("onEnterShop", shop);
              

            }
        }
        [ServerEvent(Event.PlayerExitColshape)]
        public void onPlayerExitColshape(ColShape shape, Player player)
        {

            if (shape.HasData("shop") && shape.HasData("bizid"))
            {
                player.TriggerEvent("onExitShop");
              
                player.AddAttachment( "shopBag", true );
            }
        }

        [RemoteEvent("onPlayerBuy")]
        public async void EventBuy(Player player, string itemsBought, int price, int bizId)
        {
            var items = NAPI.Util.FromJson<List<buyItem>>(itemsBought);
            player.AddAttachment( "shopBag", true );
            items.ForEach(async item => {


                var result = inventory.addItemToPlayer(player, new inventory.item(item.type, item.count, 100));
                if (!result.Item1)
                {
                    player.SendChatMessage($"You exceded the stack limit for {item.type}.");
                    price -= result.Item2 * item.price;
                }
                else
                {
                    player.SendChatMessage("You succesfully bought " + item.type);
                    await onPlayerBuyItemTask(player, item.type);
                }
                });
             await player.takeMoneyAsync(price);
              await business.utilities.addBalance( bizId, price );
        }
       public static async Task onPlayerBuyItemTask(Player player, string item)
        {
            switch (item)
            {
                case inventory.itemList.phone:
                    await phone.onPlayerBuyPhone(player); break;
                default:
                    break;
            }
        }
    }
}


