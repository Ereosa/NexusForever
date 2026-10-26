using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Quest
{
    /// <summary>
    /// Into the Undertow (Whitevale): "Summon Researcher Cowan's Extractorbot to dig up Primal Water samples at Silverscale Station".
    /// </summary>
    /// <remarks>
    /// Retail summoned the Extractorbot (26428) with "Summoning Primal Water Extractorbot" (31895), a server-side spell that no item or
    /// ability in the client data grants. As a stand-in the bot is summoned next to the player in the station's sample fields (the
    /// objective's markers) once every 30 seconds out of combat, digs for 8 seconds and leaves. Summon credit on arrival, dig credit when
    /// it is done, both counts are 4. All of this is a guess.
    /// </remarks>
    public abstract class IntoTheUndertowQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint Extractorbot     = 26428u;
        private const double SummonCooldown = 30d;
        private const double DigTime        = 8d;

        // Silverscale Station sample fields, WorldLocation2 19453-19455
        private static readonly (Vector3 Center, float Radius)[] fields =
        [
            (new Vector3(3898.6f, -927.9f, 906.1f), 150f),
            (new Vector3(3810.5f, -928.9f, 1270.6f), 150f),
            (new Vector3(4023.8f, -927.5f, 926.3f), 125f)
        ];

        protected abstract uint SummonObjectiveId { get; }
        protected abstract uint DigObjectiveId { get; }

        private IQuest owner;
        private double cooldown;
        private readonly List<(double Timer, uint Guid)> digging = [];

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        protected IntoTheUndertowQuestScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void Update(double lastTick)
        {
            IPlayer player = owner.GetOwner();
            cooldown -= lastTick;

            for (int i = digging.Count - 1; i >= 0; i--)
            {
                (double timer, uint guid) = digging[i];
                timer -= lastTick;
                if (timer > 0d)
                {
                    digging[i] = (timer, guid);
                    continue;
                }

                digging.RemoveAt(i);
                player?.Map?.GetEntity<IWorldEntity>(guid)?.RemoveFromMap();
                if (owner.State == QuestState.Accepted)
                    owner.ObjectiveUpdate(DigObjectiveId, 1u);
            }

            IQuestObjective summon = owner.GetQuestObjective(SummonObjectiveId);
            if (cooldown > 0d || player?.Map == null || player.IsLoading || player.InCombat || owner.State != QuestState.Accepted
                || summon == null || summon.IsComplete())
                return;
            if (!fields.Any(f => Vector3.Distance(player.Position, f.Center) <= f.Radius))
                return;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(Extractorbot);
            if (creatureInfo == null)
                return;

            cooldown = SummonCooldown;
            player.SummonFactory.Summon<INonPlayerEntity>(creatureInfo, player.Position + new Vector3(2f, 0f, 2f), player.Rotation,
                (map, guid, vector) => digging.Add((DigTime, guid)));
            owner.ObjectiveUpdate(SummonObjectiveId, 1u);
        }
    }

    [ScriptFilterOwnerId(5887)]
    public class IntoTheUndertowExileQuestScript : IntoTheUndertowQuestScript
    {
        protected override uint SummonObjectiveId => 9215u;
        protected override uint DigObjectiveId => 9218u;

        public IntoTheUndertowExileQuestScript(ICreatureInfoManager creatureInfoManager)
            : base(creatureInfoManager)
        {
        }
    }

    [ScriptFilterOwnerId(5888)]
    public class IntoTheUndertowDominionQuestScript : IntoTheUndertowQuestScript
    {
        protected override uint SummonObjectiveId => 9219u;
        protected override uint DigObjectiveId => 9220u;

        public IntoTheUndertowDominionQuestScript(ICreatureInfoManager creatureInfoManager)
            : base(creatureInfoManager)
        {
        }
    }
}
