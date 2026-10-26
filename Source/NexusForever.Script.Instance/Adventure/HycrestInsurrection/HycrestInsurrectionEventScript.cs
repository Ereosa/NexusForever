using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.HycrestInsurrection
{
    /// <summary>
    /// The Hycrest Insurrection (public event 419, world 1149): the Exile agents stir the farmers of Hycrest into rebellion against the Dominion.
    /// </summary>
    /// <remarks>
    /// Storylines from the creature descriptions ("PE421 Tactical 1"): Merciful, Tactical and Militant, five stages each. Stages one and two
    /// are voted (votes 45 and 46, options Merciful, Tactical, Militant), stage four too (vote 47 with all three, 48 without Militant, 49
    /// without Merciful), stage three continues the latest choice and the finale is the storyline followed most.
    /// Guessed: Breach of Protocol (426) and Midnight Counterstrike (428) as the Merciful and Militant third stages (only Thresher Initiative is
    /// tagged, Tactical 3), which stage four vote follows which third stage.
    /// </remarks>
    [ScriptFilterOwnerId(419)]
    public class HycrestInsurrectionEventScript : AdventureEventScript
    {
        private const int Merciful = 0;
        private const int Tactical = 1;
        private const int Militant = 2;

        protected override uint WorldId => 1149;
        protected override uint IntroEvent => 418;

        protected override uint[][] Storylines =>
        [
            [420, 423, 426, 429, 432], // Merciful: The Farmer's Daughter, The Great Escape, Breach of Protocol, Clearance, All Aboard
            [421, 424, 427, 430, 433], // Tactical: The Science of Revenge, The Keymaster, The Thresher Initiative, Raid Warning, Priority Target
            [422, 425, 428, 431, 434]  // Militant: Leveling the Field, Jailbreak, Midnight Counterstrike, The Underground, Coup de Grace
        ];

        protected override uint[] FinalObjectives => [1708]; // Lead the Hycrest Rebels to victory

        public HycrestInsurrectionEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(globalQuestManager, creatureInfoManager, entityFactory, spellParametersFactory, gameTableManager, cinematicFactory)
        {
        }

        protected override (uint VoteId, int[] Options)? GetVote(int stage, IReadOnlyList<int> chosen)
        {
            return stage switch
            {
                1 => (45u, [Merciful, Tactical, Militant]),
                2 => (46u, [Merciful, Tactical, Militant]),
                4 => chosen[^1] switch
                {
                    Militant => (48u, [Merciful, Tactical]),  // Clearance, Raid Warning
                    Merciful => (49u, [Militant, Tactical]),  // The Underground, Raid Warning
                    _        => (47u, [Merciful, Tactical, Militant])
                },
                _ => null
            };
        }
    }
}
