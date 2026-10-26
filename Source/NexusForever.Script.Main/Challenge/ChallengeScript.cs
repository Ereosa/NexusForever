using System.Numerics;
using NexusForever.Game.Abstract.Challenges;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Base for challenge scripts, finishing challenges whose goal isn't in the client data.
    /// </summary>
    public abstract class ChallengeScript : IChallengeScript, IOwnedScript<IChallenge>
    {
        protected IChallenge Challenge { get; private set; }
        protected IPlayer Player => Challenge.Player;

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IChallenge owner)
        {
            Challenge = owner;
        }

        public virtual void Update(double lastTick)
        {
        }

        public virtual void OnActivate()
        {
        }

        public virtual void OnEntityActivated(IWorldEntity entity)
        {
        }

        public virtual void OnEntityKilled(IUnitEntity entity)
        {
        }

        public virtual void OnStop(bool completed)
        {
        }

        protected void AddProgress(uint count = 1u)
        {
            Player.ChallengeManager.UpdateProgress(Challenge.Entry.Id, count);
        }

        protected void Complete()
        {
            Player.ChallengeManager.Complete(Challenge.Entry.Id);
        }

        protected void Fail(uint localizedTextId = 0u)
        {
            Player.ChallengeManager.Fail(Challenge.Entry.Id, localizedTextId);
        }

        /// <summary>
        /// Returns if the player is inside the supplied world location, horizontally within its radius and at least as high as its centre less <paramref name="heightTolerance"/>.
        /// </summary>
        protected bool IsAtOrAbove(uint worldLocation2Id, float heightTolerance)
        {
            WorldLocation2Entry location = GameTableManager.Instance.WorldLocation2.GetEntry(worldLocation2Id);
            if (location == null || Player.Map?.Entry.Id != location.WorldId)
                return false;

            float dx = Player.Position.X - location.Position0;
            float dz = Player.Position.Z - location.Position2;
            return dx * dx + dz * dz <= location.Radius * location.Radius
                && Player.Position.Y >= location.Position1 - heightTolerance;
        }

        protected static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            return Vector2.Distance(new Vector2(a.X, a.Z), new Vector2(b.X, b.Z));
        }
    }
}
