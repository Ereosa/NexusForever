using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Alizar.Celestion
{
    /// <summary>
    /// Firefighting (6673): "Steal a Dominion Fire Suppressor", drive it through Emberwood spraying the forest fires and repair it when it overheats.
    /// </summary>
    /// <remarks>
    /// The Fire Suppressor at Point Firestorm (30875) is a taxi unit with vehicle 397 and the generic quest activate spell, nothing in the client
    /// data boards it (report 108). Using it seats the player as the pilot of a Fire Suppressor of their own and shows its action bar (294):
    /// Water Spray Down (35409, ravel signal 11161) and Repairing Vehicle (35293, ravel signal 11014, removes Overheating 35315).
    /// A spray puts out the fires (31645, "Creature only fire") around the suppressor, each fire once per player. The suppressor overheats after
    /// a number of sprays and won't spray until repaired. Spray reach, the sprays before it overheats and the per player fires are guessed.
    /// </remarks>
    [ScriptFilterCreatureId(FireSuppressorCreatureId)]
    public class FireSuppressorEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint FireSuppressorCreatureId = 30875u;
        private const uint FireCreatureId           = 31645u;
        private const ushort Firefighting           = 6673;

        private const uint StealObjectiveId         = 10422u;
        private const uint ExtinguishObjectiveId    = 10462u;
        private const uint RepairObjectiveId        = 10426u;
        private const uint ExtinguishFiresObjectiveId = 11527u;

        private const ushort SuppressorActionBar    = 294;
        private const uint SpraySignal              = 11161u;
        private const uint RepairSignal             = 11014u;
        private const uint OverheatingSpell4Id      = 35315u;
        private const uint CameraShakeSpell4Id      = 35318u;

        private const float SprayRange              = 10f;
        private const uint SpraysBeforeOverheat     = 6u;

        private IWorldEntity owner;

        // the player's own suppressor
        private uint pilotGuid;
        private bool piloted;
        private uint sprays;
        private bool overheated;
        private double check;
        private double pendingTime;

        // fires put out, per character, shared by every suppressor
        private static readonly Dictionary<ulong, HashSet<uint>> doused = [];

        // players with a suppressor of their own, using the parked one both activates and boards it
        private static readonly HashSet<ulong> stealing = [];

        #region Dependency Injection

        private readonly IEntityFactory entityFactory;
        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly IFactory<ISpellParameters> spellParametersFactory;

        public FireSuppressorEntityScript(
            IEntityFactory entityFactory,
            ICreatureInfoManager creatureInfoManager,
            IFactory<ISpellParameters> spellParametersFactory)
        {
            this.entityFactory          = entityFactory;
            this.creatureInfoManager    = creatureInfoManager;
            this.spellParametersFactory = spellParametersFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        private bool IsPlayerSuppressor => owner.Type == EntityType.Vehicle;

        public void OnActivateSuccess(IPlayer activator)
        {
            Steal(activator);
        }

        public void OnBoard(IPlayer player)
        {
            Steal(player);
        }

        /// <summary>
        /// The parked suppressor seats the player in a suppressor of their own.
        /// </summary>
        private void Steal(IPlayer player)
        {
            if (IsPlayerSuppressor || player.PlatformGuid != null || !player.CanMount() || stealing.Contains(player.CharacterId))
                return;

            IQuest quest = player.QuestManager.GetActiveQuest(Firefighting);
            if (quest == null || quest.State != QuestState.Accepted)
                return;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(FireSuppressorCreatureId);
            if (creatureInfo == null)
                return;

            quest.ObjectiveUpdate(StealObjectiveId, 1u);
            stealing.Add(player.CharacterId);
            ulong characterId = player.CharacterId;

            var suppressor = entityFactory.CreateEntity<IVehicleEntity>();
            suppressor.Initialise(creatureInfo, 0u, 0u);
            suppressor.Rotation = owner.Rotation;
            suppressor.AddToMap(owner.Map, owner.Position, (map, guid, position) =>
            {
                if (map.GetEntity<IPlayer>(player.Guid) is not IPlayer pilot || map.GetEntity<IVehicleEntity>(guid) is not IVehicleEntity vehicle
                    || vehicle.VehicleEntry == null || vehicle.VehicleEntry.NumberPilots == 0u)
                {
                    stealing.Remove(characterId);
                    map.GetEntity<IVehicleEntity>(guid)?.RemoveFromMap();
                    return;
                }

                vehicle.InvokeScriptCollection<FireSuppressorEntityScript>(s => s.SetPilot(pilot));
                vehicle.EnqueuePassengerAdd(pilot, VehicleSeatType.Pilot, 0);
            });
        }

        private ulong pilotCharacterId;

        private void SetPilot(IPlayer player)
        {
            pilotGuid        = player.Guid;
            pilotCharacterId = player.CharacterId;
        }

        public void OnRavelSignal(IUnitEntity source, uint signal)
        {
            if (!IsPlayerSuppressor || !piloted || source.Guid != pilotGuid || source is not IPlayer player)
                return;

            switch (signal)
            {
                case SpraySignal:
                    Spray(player);
                    break;
                case RepairSignal:
                    Repair(player);
                    break;
            }
        }

        private void Spray(IPlayer player)
        {
            if (overheated)
                return;

            IQuest quest = player.QuestManager.GetActiveQuest(Firefighting);
            if (quest == null)
                return;

            if (!doused.TryGetValue(player.CharacterId, out HashSet<uint> fires))
                doused[player.CharacterId] = fires = [];

            foreach (IWorldEntity fire in owner.Map.Search(owner.Position, SprayRange, new SearchCheckCreature(FireCreatureId)))
            {
                if (!fires.Add(fire.Guid))
                    continue;

                quest.ObjectiveUpdate(ExtinguishObjectiveId, 1u);
                quest.ObjectiveUpdate(ExtinguishFiresObjectiveId, 1u);
            }

            if (++sprays < SpraysBeforeOverheat)
                return;

            overheated = true;
            CastOn(player, OverheatingSpell4Id);
            CastOn(player, CameraShakeSpell4Id);
        }

        private void Repair(IPlayer player)
        {
            if (!overheated)
                return;

            overheated = false;
            sprays     = 0u;
            player.QuestManager.GetActiveQuest(Firefighting)?.ObjectiveUpdate(RepairObjectiveId, 1u);
        }

        private void CastOn(IPlayer player, uint spell4Id)
        {
            ISpellParameters parameters = spellParametersFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            parameters.PrimaryTargetId        = player.Guid;
            player.CastSpell(spell4Id, parameters);
        }

        public void Update(double lastTick)
        {
            if (!IsPlayerSuppressor || pilotGuid == 0u)
                return;

            check -= lastTick;
            if (check > 0d)
                return;
            check = 0.5d;

            var vehicle = (IVehicleEntity)owner;
            IPlayer pilot = owner.Map?.GetEntity<IPlayer>(pilotGuid);
            bool seated = pilot != null && vehicle.GetPassenger(pilotGuid) != null;

            if (!piloted)
            {
                // boarding never happened (the seat was refused), the player can try again
                if (!seated)
                {
                    pendingTime += 0.5d;
                    if (pendingTime < 10d)
                        return;

                    stealing.Remove(pilotCharacterId);
                    pilotGuid = 0u;
                    owner.RemoveFromMap();
                    return;
                }

                piloted = true;
                pilot.ShowActionBar(ShortcutSet.VehicleBar, SuppressorActionBar, owner.Guid);
                return;
            }

            // the quest is done or given up, the player is put down where they are
            IQuest quest = pilot?.QuestManager.GetActiveQuest(Firefighting);
            if (seated && (quest == null || quest.State != QuestState.Accepted))
            {
                vehicle.PassengerRemove(pilot);
                seated = false;
            }

            if (seated)
                return;

            pilot?.HideActionBar(ShortcutSet.VehicleBar, SuppressorActionBar);
            pilotGuid = 0u;
            stealing.Remove(pilotCharacterId);
            if (owner.InWorld)
                owner.RemoveFromMap();
        }

        private class SearchCheckCreature : ISearchCheck<IWorldEntity>
        {
            private readonly uint creatureId;

            public SearchCheckCreature(uint creatureId)
            {
                this.creatureId = creatureId;
            }

            public bool CheckEntity(IWorldEntity entity)
            {
                return entity.CreatureId == creatureId;
            }
        }
    }
}
