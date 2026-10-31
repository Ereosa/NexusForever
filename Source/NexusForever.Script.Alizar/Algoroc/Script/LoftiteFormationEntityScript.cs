using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Alizar.Algoroc.Script
{
    /// <summary>
    /// Loftite Formations in First Strike Mine (Automated Demolition, "Use Demolitionbots to destroy Loftite Formations").
    /// </summary>
    /// <remarks>
    /// The Automated Demolitionbot item (12452) casts the bot's summon spell (26699), which only sends ravel signal 6322: the retail
    /// script that brought the bot out isn't in the client data (report 123). The formation nearest the player (within 25 m) answers it:
    /// a Demolitionbot (17573) rolls out from beside the player to the formation, casts Demolish (26701) and the formation breaks,
    /// counting for the player's objective (its reward pane target group lists the formations). The formation is back after a
    /// minute. Ranges, the bot's speed and the respawn time are guessed.
    /// </remarks>
    [ScriptFilterCreatureId(LoftiteFormation)]
    public class LoftiteFormationEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint LoftiteFormation   = 17581u;
        private const uint Demolitionbot      = 17573u;
        private const ushort AutomatedDemolition = 4771;
        private const uint SummonSignal       = 6322u;
        private const uint DemolishSpell4Id   = 26701u;
        private const uint FormationTargetGroup = 7564u;
        private const float SummonRange       = 25f;
        private const float BotSpeed          = 6f;

        private IWorldEntity owner;
        private uint? botGuid;
        private uint playerGuid;
        private double timer = -1d;
        private bool demolishing;

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly IFactory<ISpellParameters> spellParametersFactory;

        public LoftiteFormationEntityScript(
            ICreatureInfoManager creatureInfoManager,
            IFactory<ISpellParameters> spellParametersFactory)
        {
            this.creatureInfoManager    = creatureInfoManager;
            this.spellParametersFactory = spellParametersFactory;
        }

        #endregion

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        public void OnRavelSignal(IUnitEntity source, uint signal)
        {
            if (signal != SummonSignal || source is not IPlayer player || botGuid.HasValue || owner.Map == null)
                return;

            if (player.QuestManager.GetActiveQuest(AutomatedDemolition)?.State != QuestState.Accepted)
                return;

            // only the formation nearest the player sends a bot
            float distance = Vector3.Distance(player.Position, owner.Position);
            if (distance > SummonRange)
                return;
            if (owner.Map.Search(player.Position, distance - 0.01f, new SearchCheckFormation()).Any())
                return;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(Demolitionbot);
            if (creatureInfo == null)
                return;

            playerGuid = player.Guid;
            Vector3 start = player.Position + new Vector3(1.5f, 0f, 1.5f);
            owner.SummonFactory.Summon<INonPlayerEntity>(creatureInfo, start, player.Rotation, (map, guid, vector) =>
            {
                botGuid = guid;
                if (map.GetEntity<IWorldEntity>(guid) is not IWorldEntity bot)
                    return;

                Vector3 towards = owner.Position - start;
                Vector3 stop = owner.Position - (towards.LengthSquared() > 4f ? Vector3.Normalize(towards) * 2f : Vector3.Zero);
                bot.MovementManager.LaunchSpline([start, stop], SplineType.Linear, SplineMode.OneShot, BotSpeed);
                timer = Vector3.Distance(start, stop) / BotSpeed + 0.5d;
            });
        }

        public void Update(double lastTick)
        {
            if (timer < 0d)
                return;

            timer -= lastTick;
            if (timer > 0d)
                return;

            IUnitEntity bot = botGuid.HasValue ? owner.Map?.GetEntity<IUnitEntity>(botGuid.Value) : null;
            if (!demolishing)
            {
                demolishing = true;
                timer = 2d;
                if (bot != null)
                {
                    ISpellParameters parameters = spellParametersFactory.Resolve();
                    parameters.UserInitiatedSpellCast = false;
                    bot.CastSpell(DemolishSpell4Id, parameters);
                }
                return;
            }

            timer = -1d;
            demolishing = false;
            bot?.RemoveFromMap();
            botGuid = null;

            if (owner.Map?.GetEntity<IPlayer>(playerGuid) is IPlayer player)
                player.QuestManager.ObjectiveUpdateRewardPane(QuestObjectiveType.ActivateEntity, FormationTargetGroup, 1u);

            owner.DespawnWithRespawn(TimeSpan.FromMinutes(1d));
        }

        private class SearchCheckFormation : ISearchCheck<IWorldEntity>
        {
            public bool CheckEntity(IWorldEntity entity)
            {
                return entity.CreatureId == LoftiteFormation;
            }
        }
    }
}
