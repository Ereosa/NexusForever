using System.Collections.Immutable;
using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.Game.Entity
{
    public class VendorInfo : IVendorInfo
    {
        public uint Id { get; }
        public float SellPriceMultiplier { get; }
        public float BuyPriceMultiplier { get; }
        public ImmutableList<EntityVendorCategoryModel> Categories { get; }
        public ImmutableList<EntityVendorItemModel> Items { get; }

        public VendorInfo(EntityModel model)
        {
            Id                  = model.EntityVendor.Id;
            SellPriceMultiplier = model.EntityVendor.SellPriceMultiplier;
            BuyPriceMultiplier  = model.EntityVendor.BuyPriceMultiplier;
            Categories          = model.EntityVendorCategory.ToImmutableList();
            Items               = model.EntityVendorItem.ToImmutableList();
        }

        public VendorInfo(CreatureVendorModel vendor, IEnumerable<CreatureVendorCategoryModel> categories, IEnumerable<CreatureVendorItemModel> items)
        {
            Id                  = vendor.CreatureId;
            SellPriceMultiplier = vendor.SellPriceMultiplier;
            BuyPriceMultiplier  = vendor.BuyPriceMultiplier;
            Categories          = categories
                .Select(c => new EntityVendorCategoryModel
                {
                    Id              = vendor.CreatureId,
                    Index           = c.Index,
                    LocalisedTextId = c.LocalisedTextId
                })
                .OrderBy(c => c.Index)
                .ToImmutableList();
            Items               = items
                .Select(i => new EntityVendorItemModel
                {
                    Id            = vendor.CreatureId,
                    Index         = i.Index,
                    CategoryIndex = i.CategoryIndex,
                    ItemId        = i.ItemId
                })
                .OrderBy(i => i.Index)
                .ToImmutableList();
        }

        /// <summary>
        /// Return <see cref="EntityVendorItemModel"/> at the supplied index;
        /// </summary>
        public EntityVendorItemModel GetItemAtIndex(uint index)
        {
            return Items.SingleOrDefault(i => i.Index == index);
        }

        public ServerVendorItemsUpdated Build()
        {
            var serverVendor = new ServerVendorItemsUpdated
            {
                SellPriceMultiplier = SellPriceMultiplier,
                BuyPriceMultiplier  = BuyPriceMultiplier,
                Unknown2            = true,
                Unknown3            = true,
                Unknown4            = false
            };

            foreach (EntityVendorCategoryModel category in Categories)
            {
                serverVendor.Categories.Add(new ServerVendorItemsUpdated.Category
                {
                    Index           = category.Index,
                    LocalisedTextId = category.LocalisedTextId
                });
            }
            foreach (EntityVendorItemModel item in Items)
            {
                serverVendor.Items.Add(new ServerVendorItemsUpdated.Item
                {
                    Index         = item.Index,
                    ItemId        = item.ItemId,
                    CategoryIndex = item.CategoryIndex,
                    Unknown6      = 0,
                    ExtraCost1 = new ServerVendorItemsUpdated.Item.ItemExtraCost()
                    {
                        ExtraCostType    = item.ExtraCost1Type,
                        Quantity         = item.ExtraCost1Quantity,
                        ItemOrCurrencyId = item.ExtraCost1ItemOrCurrencyId
                    },
                    ExtraCost2 = new ServerVendorItemsUpdated.Item.ItemExtraCost()
                    {
                        ExtraCostType    = item.ExtraCost2Type,
                        Quantity         = item.ExtraCost2Quantity,
                        ItemOrCurrencyId = item.ExtraCost2ItemOrCurrencyId
                    }
                });
            }

            return serverVendor;
        }
    }
}
