using System;
using NexusForever.Game.Abstract.Storefront;
using NexusForever.Game.Static.AccountInventory;
using NexusForever.Game.Storefront;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.AccountInventory;

namespace NexusForever.WorldServer.Network.Message.Handler.Account
{
    public class ClientStorefrontPurchaseCharacterHandler : IMessageHandler<IWorldSession, ClientStorefrontPurchaseCharacter>
    {
        public void HandleMessage(IWorldSession session, ClientStorefrontPurchaseCharacter purchase)
        {
            AccountOperationResult result = Purchase(session, purchase);
            session.EnqueueMessageEncrypted(new ServerAccountOperationResult
            {
                Operation = AccountOperation.MTXPurchase,
                Result    = result
            });
        }

        private static AccountOperationResult Purchase(IWorldSession session, ClientStorefrontPurchaseCharacter purchase)
        {
            IOfferItem offer = GlobalStorefrontManager.Instance.GetStoreOfferItem(purchase.OfferId);
            if (offer == null || !offer.Visible)
                return AccountOperationResult.InvalidOffer;

            IOfferItemPrice price = offer.GetPriceDataForCurrency(purchase.CurrencyId);
            if (price == null)
                return AccountOperationResult.InvalidPrice;

            ulong cost = (ulong)Math.Ceiling(price.Price);
            if (!session.Account.CurrencyManager.CanAfford(purchase.CurrencyId, cost))
                return AccountOperationResult.NotEnoughCurrency;

            session.Account.CurrencyManager.CurrencySubtractAmount(purchase.CurrencyId, cost);

            // purchases go to the account inventory, the player then takes them to a character
            foreach (IOfferItemData item in offer.Items)
                for (uint i = 0u; i < Math.Max(1u, item.Amount); i++)
                    session.Account.ItemManager.AddItem(item.ItemId);

            return AccountOperationResult.Ok;
        }
    }
}
