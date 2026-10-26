using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Challenges collected by running through objects that only exist while the challenge runs, each object touched counts and is
    /// replaced by a new one somewhere else.
    /// </summary>
    /// <remarks>
    /// Used where retail spawned the objects for the challenge and no record of where they were exists, see the derived scripts for
    /// what their positions are based on.
    /// </remarks>
    public abstract class SummonedTouchChallengeScript : ChallengeScript
    {
        protected abstract uint CreatureId { get; }

        /// <summary>
        /// Number of objects out at once.
        /// </summary>
        protected virtual int Count => 6;

        protected virtual float TouchRange => 3f;
        protected virtual float TouchHeight => 4f;

        /// <summary>
        /// Return where the objects can be, chosen from at random.
        /// </summary>
        protected abstract List<Vector3> GetPositions();

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly List<uint> objects = [];
        private List<Vector3> positions = [];

        protected SummonedTouchChallengeScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        public override void OnActivate()
        {
            positions = GetPositions();
            for (int i = 0; i < Math.Min(Count, positions.Count); i++)
                SummonObject();
        }

        private void SummonObject()
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(CreatureId);
            if (creatureInfo == null || positions.Count == 0)
                return;

            Vector3 position = positions[Random.Shared.Next(positions.Count)];
            Player.SummonFactory.Summon(creatureInfo, position, Vector3.Zero, (map, guid, p) => objects.Add(guid));
        }

        public override void Update(double lastTick)
        {
            foreach (uint guid in objects.ToList())
            {
                IWorldEntity entity = Player.GetVisible<IWorldEntity>(guid);
                if (entity == null
                    || HorizontalDistance(entity.Position, Player.Position) > TouchRange
                    || MathF.Abs(entity.Position.Y - Player.Position.Y) > TouchHeight)
                    continue;

                objects.Remove(guid);
                Player.SummonFactory.Unsummon(guid);
                OnTouched();

                AddProgress();
                if (!Challenge.Activated)
                    return;

                SummonObject();
            }
        }

        /// <summary>
        /// Invoked when the player touches one of the objects.
        /// </summary>
        protected virtual void OnTouched()
        {
        }

        public override void OnStop(bool completed)
        {
            foreach (uint guid in objects)
                Player.SummonFactory.Unsummon(guid);
            objects.Clear();
        }
    }
}
