using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Challenges collected by running or jumping through creatures, each creature counts once a run.
    /// </summary>
    public abstract class TouchChallengeScript : ChallengeScript
    {
        protected abstract uint[] CreatureIds { get; }
        protected virtual float TouchRange => 3f;
        protected virtual float TouchHeight => 4f;

        /// <summary>
        /// Seconds before a creature counts again, null if it only counts once a run.
        /// </summary>
        /// <remarks>
        /// Challenges asking for "as many as you can" let the player go through the same whimfiber or updraft again after a while.
        /// </remarks>
        protected virtual double? RecountDelay => null;

        private readonly Dictionary<uint, double> touched = [];
        private double time;

        public override void OnActivate()
        {
            touched.Clear();
            time = 0d;
        }

        public override void Update(double lastTick)
        {
            time += lastTick;
            foreach (uint creatureId in CreatureIds)
            {
                foreach (IWorldEntity entity in Player.GetVisibleCreature<IWorldEntity>(creatureId))
                {
                    if (HorizontalDistance(entity.Position, Player.Position) > TouchRange
                        || Math.Abs(entity.Position.Y - Player.Position.Y) > TouchHeight)
                        continue;

                    if (touched.TryGetValue(entity.Guid, out double touchedAt) && (RecountDelay == null || time - touchedAt < RecountDelay.Value))
                        continue;

                    touched[entity.Guid] = time;
                    AddProgress();
                    if (!Challenge.Activated)
                        return;
                }
            }
        }
    }
}
