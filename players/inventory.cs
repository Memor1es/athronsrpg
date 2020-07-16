using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace rpg
{

    public class inventory : Script
    {
        public class itemList
        {
            public const string fishingRod = "fishingrod";
            public const string catFish = "catfish";
            public const string cod = "cod";
            public const string eel = "eel";
            public const string goldFish = "goldfish";
            public const string perch = "perch";
            public const string piranha = "piranha";
            public const string shrimp = "shrimp";
            public const string carp = "carp";
            public const string phone = "phone";
            public const string phonebook = "phonebook";

        }

        public enum itemPermision
        {
            usable,
            dropable,
            damageable,
            sell,
            trade
        }
        public enum itemCategory
        {
            fish,
            food,
            utils
        }
        [ServerEvent(Event.ResourceStart)]
        public void onInventoryStart()
        {
            items.Add(new itemLimit(itemList.fishingRod, "Fishing rod", 1, itemCategory.utils, new itemPermision[] { itemPermision.usable, itemPermision.damageable, itemPermision.dropable }, "http://citstw.go.ro/items/fishing-rod.svg"));
            items.Add(new itemLimit(itemList.catFish, "Catfish", 3, itemCategory.fish, new itemPermision[] { itemPermision.sell }, "http://citstw.go.ro/items/catfish.svg"));
            items.Add(new itemLimit(itemList.cod, "Cod", 3, itemCategory.fish, new itemPermision[] { itemPermision.sell }, "http://citstw.go.ro/items/cod.svg", 500));
            items.Add(new itemLimit(itemList.eel, "Ell", 3, itemCategory.fish, new itemPermision[] { itemPermision.sell }, "http://citstw.go.ro/items/eel.svg", 1500));
            items.Add(new itemLimit(itemList.goldFish, "Goldfish", 3, itemCategory.fish, new itemPermision[] { itemPermision.sell }, "http://citstw.go.ro/items/goldfish.svg"));
            items.Add(new itemLimit(itemList.perch, "Perch", 3, itemCategory.fish, new itemPermision[] { itemPermision.sell }, "http://citstw.go.ro/items/perch.svg"));
            items.Add(new itemLimit(itemList.piranha, "Piranha", 3, itemCategory.fish, new itemPermision[] { itemPermision.sell }, "http://citstw.go.ro/items/piranha.svg"));
            items.Add(new itemLimit(itemList.shrimp, "Shrimp", 3, itemCategory.fish, new itemPermision[] { itemPermision.sell }, "http://citstw.go.ro/items/shrimp.svg"));
            items.Add(new itemLimit(itemList.carp, "Carp", 3, itemCategory.fish, new itemPermision[] { itemPermision.sell }, "http://citstw.go.ro/items/carp.svg"));
            items.Add(new itemLimit(itemList.phone, "Phone", 1, itemCategory.utils, new itemPermision[] {  }, "http://citstw.go.ro/items/phone.png"));
            items.Add(new itemLimit(itemList.phonebook, "Phonebook", 1, itemCategory.utils, new itemPermision[] { }, "http://citstw.go.ro/items/phonebook.png"));
            Console.WriteLine(NAPI.Util.ToJson(items));
        }
       public static List<itemLimit> items = new List<itemLimit>();
        public class itemLimit
        {
            public string name { get; set; }
            public string displayName { get; set; }
            public int price { get; set; }
            public int maxCount { get; set; }
            public itemCategory category { get; set; }
            public itemPermision[] permissions { get; set; }
            public string imageURL { get; set; }
            public itemLimit() { }
            public itemLimit(string name, string displayName, int maxCount, itemCategory category, itemPermision[] permissions, string imageURL, int price = 0)
            {
                this.maxCount = maxCount;
                this.permissions = permissions;
                this.displayName = displayName;
                this.name = name;
                this.category = category;
                this.imageURL = imageURL;
                this.price = price;
            }
        }
        public class item
        {
            public string name { get; set; }
            public int count { get; set; }
            public int slot { get; set; }
            public dynamic external { get; set; }
            public itemCategory category { get; set; }
            public item() { }
            public item(string name, int count, dynamic external)
            {
                this.count = count;
                this.name = name;
                this.slot = -1;
                this.external = external;
            }
        }
      

        public static List<item> getPlayerInventory(Player player)
        {
            List<item> inventory = new List<item>();
            if (player.HasData("inventory"))
              inventory = player.GetData< List < item >> ("inventory");

            return inventory;
        }
        public static bool hasPlayerItem(Player player, string item)
        {
            return getPlayerInventory(player).Find(a => a.name.ToLower() == item.ToLower()) != null; 
        }
        public static void removeItem(Player player, string item, int count)
        {
            var itemsInv = getPlayerInventory(player); var itemInv = itemsInv.Find(a => a.name.ToLower() == item.ToLower());

            itemInv.count -= count;

            if (itemInv.count <= 0)
              itemsInv.Remove(itemInv);

            updateInventory(player, itemsInv);
        }
        public static Tuple<bool, int> addItemToPlayer(Player player, item item)
        {
            List<item> inventoryItems = getPlayerInventory(player);

            var itemLocal = inventoryItems.Find(s => s.name.ToLower() == item.name.ToLower());
            var maxCount = items.Find(a => a.name.ToLower() == item.name.ToLower()).maxCount;
            if ( itemLocal != null )
            {
         
                int countBought = 0;
                itemLocal.count += item.count;
                if ( itemLocal.count > maxCount )
                {
                    itemLocal.count = maxCount;
                    updateInventory( player, inventoryItems );
                    return new Tuple<bool, int>( false, item.count - maxCount );
                }

                updateInventory( player, inventoryItems );
                return new Tuple<bool, int>( true, 0 );
            }
            else
            {
                if ( item.count > maxCount )
                    item.count = maxCount;
                inventoryItems.Add( item );
                updateInventory( player, inventoryItems );
                Console.WriteLine( NAPI.Util.ToJson( inventoryItems ) );
                return new Tuple<bool, int>( true, item.count - maxCount );
            }
          
            return new Tuple<bool, int>(true, 0);
        }
        public static void updateInventory(Player player, List<item> items)
        {
           
            player.SetData("inventory", items);
            player.TriggerEvent("reciveInventoryUpdate", items);


            
        }
        [RemoteEvent("onPlayerSendInventory")]
        public void onPlayerSendInventory(Player player, string data)
        {
            Console.WriteLine("INVENTORY RECIVED " + data);
            player.SetData("inventory", NAPI.Util.FromJson< List<item>>(data));
        }
        [RemoteEvent("onPlayerSellItem")]
        public async void onPlayerSellItem(Player player, string item, int price)
        {
            removeItem(player, item, 3);
            await player.giveMoney(price);
        }

        [RemoteEvent("playerUseItem")]
        public void onPlayerUseItem(Player player, string item)
        {
            switch (item)
            {
                case itemList.fishingRod:
                    {
                        player.AddAttachment("fishingRod", false);

                    }
                    break;
                default:
                    break;
            }
        }
        [Command("giveitem")]
        public void getitem(Player player, string name, int count, int external)
        {
           var success =  addItemToPlayer(player, new item(name, count, external));
            if (!success.Item1)
                player.SendChatMessage("Item stack limit exceded for " + name);
            else
            player.SendChatMessage($"You gave {count} {name}.");
        }
        [Command("myinventory")]
        public void invent(Player player)
        {
            player.SendChatMessage($"Your inventory is:");

            Console.WriteLine(NAPI.Util.ToJson(getPlayerInventory(player)));
            foreach (var item in getPlayerInventory(player))
            {
                player.SendChatMessage($"{item.name} count: {item.count} slot: {item.slot}");
            }
        }
    }
}
