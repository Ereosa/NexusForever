using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Map.Lock;
using NexusForever.GameTable.Model;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.FragmentZero
{
    [ScriptFilterOwnerId(3180)]
    public class FragmentZeroMapScript : EventBaseContentMapScript
    {
        public override uint PublicEventId => 680u;

        // Fragment Zero Low Gravity, Fragment Zero Flash Light and Fragment Zero Space Helmet
        private static readonly uint[] ZoneSpells = [77905, 77906, 78215];

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;
        private readonly IGameTableManager gameTableManager;

        public FragmentZeroMapScript(
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager)
        {
            this.spellParametersFactory = spellParametersFactory;
            this.gameTableManager       = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IContentMapInstance"/> is loaded.
        /// </summary>
        public override void OnLoad(IContentMapInstance owner)
        {
            // creatures killed and objects used in the expedition stay gone
            owner.RespawnsDisabled = true;
            base.OnLoad(owner);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to map.
        /// </summary>
        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            if (entity is not IPlayer player)
                return;

            foreach (uint spellId in ZoneSpells)
            {
                ISpellParameters parameters = spellParametersFactory.Resolve();
                parameters.PrimaryTargetId = player.Guid;
                player.CastSpell(spellId, parameters);
            }

            // players who didn't come through a transport (eg: logged in here after a restart) leave by their faction's transport
            player.InstancePortalReturn ??= player.Faction1 == Faction.Dominion
                ? CreateReturn(22, new Vector3(-5664.35f, -977.071f, -851.552f))  // Transport to Fragment Zero, Deradune
                : CreateReturn(51, new Vector3(3690.11f, -1024.26f, -4619.81f)); // Transport to Fragment Zero, Algoroc
        }

        private IMapPosition CreateReturn(uint worldId, Vector3 position)
        {
            return new ReturnPosition
            {
                Info     = new ReturnInfo { Entry = gameTableManager.World.GetEntry(worldId) },
                Position = position
            };
        }

        private class ReturnInfo : IMapInfo
        {
            public WorldEntry Entry { get; init; }
            public IMapLock MapLock { get; init; }
        }

        private class ReturnPosition : IMapPosition
        {
            public IMapInfo Info { get; init; }
            public Vector3 Position { get; set; }
        }
    }
}
