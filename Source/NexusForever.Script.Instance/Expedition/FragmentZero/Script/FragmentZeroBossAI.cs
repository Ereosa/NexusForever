using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.FragmentZero.Script
{
    /// <summary>
    /// Prototype Alpha, Beta and Delta, the xenobite prototypes of the Incubation Complex.
    /// </summary>
    [ScriptFilterCreatureId(67526, 67527, 67528, 69672, 69673, 69674)]
    public class PrototypeEntityScript : FragmentZeroCreatureAI
    {
        protected override uint[] AutoAttacks => [4495, 4496];

        public PrototypeEntityScript(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(56490, 9d, 3d);                      // Evil Eye
            AddAbility(56492, 12d, 7d);                     // Event Horizon
            AddAbility(56493, 15d, 10d, veteranOnly: true); // Spike Damage
        }
    }

    /// <summary>
    /// Project "Matron", the Skeech queen experiment in the Biomatics Chamber.
    /// </summary>
    [ScriptFilterCreatureId(69088, 69664)]
    public class MatronEntityScript : FragmentZeroCreatureAI
    {
        protected override uint[] AutoAttacks => [34062];

        public MatronEntityScript(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54621, 12d, 4d);  // Sudden Magics
            AddAbility(54622, 10d, 8d);  // Body Slam
            AddAbility(54626, 20d, 14d); // Creeping Power
            AddAbility(54628, 9d, 6d);   // Trick Spell
        }
    }

    /// <summary>
    /// The Life-Overseer, the Pell scientist Ohmna left in charge of Fragment Zero.
    /// </summary>
    /// <remarks>
    /// He watches the players (friendly to all) until they reach his creche, he then fights and calls his creations at 70% and 35% health.
    /// </remarks>
    [ScriptFilterCreatureId(67522, 69635)]
    public class LifeOverseerEntityScript : FragmentZeroCreatureAI
    {
        private int summons;

        protected override uint[] AutoAttacks => [34062];

        public LifeOverseerEntityScript(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(56073, 9d, 3d);                      // Organic Assault
            AddAbility(56075, 15d, 8d);                     // Angry Swarm
            AddAbility(56078, 12d, 6d);                     // Branching Growth
            AddAbility(56081, 18d, 12d, veteranOnly: true); // Overgrowth
        }

        /// <summary>
        /// The players found the creche: the Life-Overseer becomes hostile (Skeech faction).
        /// </summary>
        public void StartEncounter()
        {
            entity.SetFaction((Faction)248);
            entity.NpcYell(GetText(640531)); // Come to me, outsiders, and I shall use your flesh to feed the Great Work!
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (!entity.IsAlive || !entity.InCombat || summons >= 2)
                return;

            float threshold = summons == 0 ? 0.7f : 0.35f;
            if (entity.Health > entity.MaxHealth * threshold)
                return;

            summons++;
            entity.NpcYell(GetText(summons == 1 ? 640525u : 644470u)); // Hear me, my creations... / Where one of my creations falls...

            IPublicEvent publicEvent = entity.Map.PublicEventManager.GetEvent(680);
            publicEvent?.InvokeScriptCollection<FragmentZeroEventScript>(s => s.OnLifeOverseerSummon(entity));
        }

        private string GetText(uint id)
        {
            return gameTableManager.GetTextTable(Language.English)?.GetEntry(id) ?? string.Empty;
        }
    }
}
