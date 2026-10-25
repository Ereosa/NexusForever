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

        private readonly HashSet<uint> touched = [];

        public override void OnActivate()
        {
            touched.Clear();
        }

        public override void Update(double lastTick)
        {
            foreach (uint creatureId in CreatureIds)
            {
                foreach (IWorldEntity entity in Player.GetVisibleCreature<IWorldEntity>(creatureId))
                {
                    if (HorizontalDistance(entity.Position, Player.Position) > TouchRange
                        || Math.Abs(entity.Position.Y - Player.Position.Y) > TouchHeight
                        || !touched.Add(entity.Guid))
                        continue;

                    AddProgress();
                    if (!Challenge.Activated)
                        return;
                }
            }
        }
    }
}
