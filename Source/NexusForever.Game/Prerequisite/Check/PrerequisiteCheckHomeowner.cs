using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Homeowner)]
    public class PrerequisiteCheckHomeowner : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckHomeowner> log;

        public PrerequisiteCheckHomeowner(
            ILogger<PrerequisiteCheckHomeowner> log)
        {
            this.log = log;
        }

        #endregion

        /// <remarks>
        /// Met when the character owns (Equal) or doesn't own (NotEqual) a house, eg: "Only a homeowner can claim this item".
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool? result = PrerequisiteCompare.Compare(comparison, player.ResidenceManager.Residence != null);
            if (result == null)
                log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Homeowner}!");

            return result ?? false;
        }
    }
}
