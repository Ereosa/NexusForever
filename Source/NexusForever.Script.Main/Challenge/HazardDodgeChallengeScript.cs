using System.Numerics;
using NexusForever.Game.Abstract.Entity.Creature;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Challenges counting the seconds the player avoids hazards that keep striking where they stand (mortars, lightning).
    /// </summary>
    /// <remarks>
    /// Retail ran the hazards from server event scripts. Each strike marks the player's position with the hazard's target creature,
    /// anyone still within <see cref="StrikeRadius"/> of it when it lands is hit and the run ends on the tier it reached.
    /// </remarks>
    public abstract class HazardDodgeChallengeScript : ChallengeScript
    {
        protected abstract uint MarkerCreatureId { get; }
        protected virtual double StrikeInterval => 2.5d;
        protected virtual double StrikeDelay => 1.5d;
        protected virtual float StrikeRadius => 4f;

        private class Strike
        {
            public Vector3 Position { get; init; }
            public double LandTime { get; init; }
            public uint MarkerGuid { get; set; }
        }

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly List<Strike> strikes = [];
        private double time;
        private double nextStrike;
        private double nextSecond;

        protected HazardDodgeChallengeScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        public override void OnActivate()
        {
            strikes.Clear();
            time       = 0d;
            nextStrike = StrikeInterval;
            nextSecond = 1d;
        }

        public override void Update(double lastTick)
        {
            time += lastTick;

            foreach (Strike strike in strikes.Where(s => time >= s.LandTime).ToList())
            {
                strikes.Remove(strike);
                if (strike.MarkerGuid != 0u)
                    Player.SummonFactory.Unsummon(strike.MarkerGuid);

                if (Vector3.Distance(strike.Position, Player.Position) <= StrikeRadius)
                {
                    if (Challenge.CurrentTier > 0u)
                        Complete();
                    else
                        Fail();
                    return;
                }
            }

            if (time >= nextSecond)
            {
                nextSecond += 1d;
                AddProgress();
                if (!Challenge.Activated)
                    return;
            }

            if (time >= nextStrike)
            {
                nextStrike += StrikeInterval;
                LaunchStrike();
            }
        }

        public override void OnStop(bool completed)
        {
            foreach (Strike strike in strikes.Where(s => s.MarkerGuid != 0u))
                Player.SummonFactory.Unsummon(strike.MarkerGuid);
            strikes.Clear();
        }

        private void LaunchStrike()
        {
            var strike = new Strike
            {
                Position = Player.Position,
                LandTime = time + StrikeDelay
            };
            strikes.Add(strike);

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(MarkerCreatureId);
            if (creatureInfo != null)
                Player.SummonFactory.Summon(creatureInfo, strike.Position, Vector3.Zero, (map, guid, position) => strike.MarkerGuid = guid);
        }
    }
}
