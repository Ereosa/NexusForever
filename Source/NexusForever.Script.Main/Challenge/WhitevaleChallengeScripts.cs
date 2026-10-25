using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Defusing the Situation: "Safely defuse mines without taking any damage from the blast", Sunderstone Proximity Mines (shared with quests 5450 and 5451).
    /// </summary>
    /// <remarks>
    /// The mine blast isn't told apart from other damage, taking any damage ends the run on the tier it reached.
    /// </remarks>
    [ScriptFilterOwnerId(236)]
    public class DefusingTheSituationChallengeScript : ChallengeScript
    {
        private const uint ProximityMineCreatureId = 23151;

        private uint health;

        public override void OnActivate()
        {
            health = Player.Health;
        }

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (entity.CreatureId == ProximityMineCreatureId)
                AddProgress();
        }

        public override void Update(double lastTick)
        {
            uint current = Player.Health;
            if (current < health)
            {
                if (Challenge.CurrentTier > 0u)
                    Complete();
                else
                    Fail();
                return;
            }

            health = current;
        }
    }

    /// <summary>
    /// Deadrock Statue: "Run to the statue in center Deadrock Prison", the statue stands at the centre of the prison (the challenge indicator).
    /// </summary>
    [ScriptFilterOwnerId(238)]
    public class DeadrockStatueChallengeScript : ChallengeScript
    {
        private const float StatueRange = 10f;

        public override void Update(double lastTick)
        {
            WorldLocation2Entry centre = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            if (centre != null && HorizontalDistance(Player.Position, new Vector3(centre.Position0, centre.Position1, centre.Position2)) <= StatueRange)
                Complete();
        }
    }

    /// <summary>
    /// Lightning Rod: "Don't let the lightning hit you", tiers in seconds, the strikes are marked with the Lightning Strike hazard target.
    /// </summary>
    [ScriptFilterOwnerId(344)]
    public class LightningRodChallengeScript : HazardDodgeChallengeScript
    {
        protected override uint MarkerCreatureId => 38674;

        public LightningRodChallengeScript(ICreatureInfoManager creatureInfoManager)
            : base(creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Speedy Delivery: "Make as many deliveries as you can in 3 minutes" around Protostar's Prosperity Junction.
    /// </summary>
    /// <remarks>
    /// Which Protostar worker wanted which delivery was server event data that isn't in the client tables, a delivery counts when the
    /// player reaches a Protostar Employee or Maintainer Bot other than the last one delivered to.
    /// </remarks>
    [ScriptFilterOwnerId(345)]
    public class SpeedyDeliveryChallengeScript : ChallengeScript
    {
        private const float DeliveryRange = 3f;
        private static readonly uint[] recipientCreatureIds = [26286, 40161];

        private uint lastRecipient;

        public override void OnActivate()
        {
            lastRecipient = 0u;
        }

        public override void Update(double lastTick)
        {
            foreach (uint creatureId in recipientCreatureIds)
            {
                foreach (IWorldEntity recipient in Player.GetVisibleCreature<IWorldEntity>(creatureId))
                {
                    if (recipient.Guid == lastRecipient || Vector3.Distance(recipient.Position, Player.Position) > DeliveryRange)
                        continue;

                    lastRecipient = recipient.Guid;
                    AddProgress();
                    return;
                }
            }
        }
    }
}
