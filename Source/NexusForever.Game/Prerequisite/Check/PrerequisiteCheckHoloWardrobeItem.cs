using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.HoloWardrobeItem)]
    public class PrerequisiteCheckHoloWardrobeItem : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckHoloWardrobeItem> log;

        public PrerequisiteCheckHoloWardrobeItem(
            ILogger<PrerequisiteCheckHoloWardrobeItem> log)
        {
            this.log = log;
        }

        #endregion

        /// <remarks>
        /// Object is an item, met when it is (Equal) or isn't (NotEqual) unlocked in the account Holo-Wardrobe.
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool? result = PrerequisiteCompare.Compare(comparison, player.Account.CostumeManager.HasItemUnlock(objectId));
            if (result == null)
                log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.HoloWardrobeItem}!");

            return result ?? false;
        }
    }
}
