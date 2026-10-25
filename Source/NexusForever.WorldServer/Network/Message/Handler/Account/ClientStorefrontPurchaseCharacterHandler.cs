using System;
using NexusForever.Game.Abstract.Account.Inventory;
using NexusForever.Game.Abstract.Storefront;
using NexusForever.Game.Static.AccountInventory;
using NexusForever.Game.Storefront;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.AccountInventory;
using NexusForever.WorldServer.Network.Message.Handler.Character;

namespace NexusForever.WorldServer.Network.Message.Handler.Account
{
    public class ClientStorefrontPurchaseCharacterHandler : IMessageHandler<IWorldSession, ClientStorefrontPurchaseCharacter>
    {
        #region Dependency Injection

        private readonly ICharacterListManager characterListManager;

        public ClientStorefrontPurchaseCharacterHandler(
            ICharacterListManager characterListManager)
        {
            this.characterListManager = characterListManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientStorefrontPurchaseCharacter purchase)
        {
            AccountOperationResult result = Purchase(session, purchase, out bool appliedAtCharacterSelect);
            session.EnqueueMessageEncrypted(new ServerAccountOperationResult
            {
                Operation = AccountOperation.MTXPurchase,
                Result    = result
            });

            // client waits for this and an account currency change before closing the purchase spinner
            session.EnqueueMessageEncrypted(new ServerStorePurchaseResult
            {
                Success = result == AccountOperationResult.Ok
            });

            // character slots bought at character select show once the character list is sent again
            if (appliedAtCharacterSelect)
                characterListManager.SendCharacterListPackets(session);
        }

        private static AccountOperationResult Purchase(IWorldSession session, ClientStorefrontPurchaseCharacter purchase, out bool appliedAtCharacterSelect)
        {
            appliedAtCharacterSelect = false;

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
            // account wide unlocks bought at character select (eg: character slots) are applied straight away
            foreach (IOfferItemData item in offer.Items)
            {
                for (uint i = 0u; i < Math.Max(1u, item.Amount); i++)
                {
                    IAccountItem accountItem = session.Account.ItemManager.AddItem(item.ItemId);
                    if (session.Player == null
                        && session.Account.ItemManager.IsAccountOnly(accountItem)
                        && session.Account.ItemManager.TakeItem(null, accountItem.Id) == AccountOperationResult.Ok)
                        appliedAtCharacterSelect = true;
                }
            }

            return AccountOperationResult.Ok;
        }
    }
}
