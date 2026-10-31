using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Combat;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database;
using NexusForever.Database.Auth;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Database.World;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Account;
using NexusForever.Game.Abstract.Achievement;
using NexusForever.Game.Abstract.Challenges;
using NexusForever.Game.Abstract.Crafting;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Taxi;
using NexusForever.Game.Account.Entitlement;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Abstract.Guild;
using NexusForever.Game.Abstract.Housing;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Map.Lock;
using NexusForever.Game.Abstract.Matching.Match;
using NexusForever.Game.Abstract.Matching.Queue;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Reputation;
using NexusForever.Game.Abstract.Reward;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Achievement;
using NexusForever.Game.Challenges;
using NexusForever.Game.Character;
using NexusForever.Game.Chat;
using NexusForever.Game.Crafting;
using NexusForever.Game.Configuration.Model;
using NexusForever.Game.Guild;
using NexusForever.Game.Housing;
using NexusForever.Game.Map;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Reputation;
using NexusForever.Game.Reward;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Chat;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Guild;
using NexusForever.Game.Static.Option;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.RBAC;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.PVP;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.RBAC;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.Setting;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Taxi;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.Internal;
using NexusForever.Network.Internal.Message.Player;
using NexusForever.Network.Session;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.Abilities;
using NexusForever.Network.World.Message.Model.Chat;
using NexusForever.Network.World.Message.Model.Info;
using NexusForever.Network.World.Message.Model.Pregame;
using NexusForever.Network.World.Message.Model.Housing;
using NexusForever.Network.World.Message.Model.Pvp;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Network.World.Message.Static;
using NexusForever.Script;
using NexusForever.Script.Template;
using NexusForever.Shared;
using NexusForever.Shared.Configuration;
using NexusForever.Shared.Game;
using NexusForever.Shared.Game.Events;
using NLog;
using Path = NexusForever.Game.Static.PlayerPath.Path;

namespace NexusForever.Game.Entity
{
    public class Player : UnitEntity, IPlayer
    {
        /// <summary>
        /// Determines which fields need saving for <see cref="IPlayer"/> when being saved to the database.
        /// </summary>
        [Flags]
        public enum PlayerSaveMask
        {
            None        = 0x0000,
            Location    = 0x0001,
            Path        = 0x0002,
            Costume     = 0x0004,
            InputKeySet = 0x0008,
            Flags       = 0x0020,
            Innate      = 0x0080,
            Sex         = 0x0100,
            Race        = 0x0200,
            BindPoint   = 0x0400
        }

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        // TODO: move this to the config file
        private const double SaveDuration = 60d;

        public override EntityType Type => EntityType.Player;

        public IAccount Account { get; private set; }

        public Abstract.Identity Identity { get; private set; }

        public ulong CharacterId { get => Identity.Id; }

        public string Name { get; private set; }

        public Sex Sex
        {
            get => sex;
            set
            {
                sex = value;
                saveMask |= PlayerSaveMask.Sex;

                SetVisualEmit(true);
            }
        }

        private Sex sex;

        public Race Race
        {
            get => race;
            set
            {
                race = value;
                saveMask |= PlayerSaveMask.Race;

                SetVisualEmit(true);
            }
        }

        private Race race;

        public Class Class { get; private set; }

        public CharacterFlag Flags
        {
            get => flags;
            set
            {
                flags = value;
                saveMask |= PlayerSaveMask.Flags;
            }
        }
        private CharacterFlag flags;

        public Path Path
        {
            get => path;
            set
            {
                path = value;
                PathActivatedTime = DateTime.UtcNow;
                saveMask |= PlayerSaveMask.Path;
            }
        }
        private Path path;

        public DateTime PathActivatedTime { get; private set; }

        public InputSets InputKeySet
        {
            get => inputKeySet;
            set
            {
                inputKeySet = value;
                saveMask |= PlayerSaveMask.InputKeySet;
            }
        }
        private InputSets inputKeySet;

        /// <summary>
        /// <see cref="BindPointEntry"/> id of the Transmat Terminal the player last bound to, 0 if they haven't.
        /// </summary>
        public ushort BindPoint { get; private set; }

        public override uint Level
        {
            get => base.Level;
            set
            {
                base.Level = value;

                CalculateDefaultProperties();
                SetBaseCharacterProperties();
            }
        }

        public DateTime CreateTime { get; private set; }
        public double TimePlayedTotal { get; private set; }
        public double TimePlayedLevel { get; private set; }
        public double TimePlayedSession { get; private set; }

        /// <summary>
        /// Guid of the <see cref="IWorldEntity"/> that currently being controlled by the <see cref="IPlayer"/>.
        /// </summary>
        public uint? ControlGuid { get; private set; }

        /// <summary>
        /// Guid of the <see cref="IPetEntity"/> currently summoned by the <see cref="IPlayer"/>.
        /// </summary>
        public uint? VanityPetGuid { get; set; }

        /// <summary>
        /// Id of the primary group that <see cref="IPlayer"/> is associated with.
        /// </summary>
        public ulong GroupAssociation { get; set; }

        public bool IsSitting => currentChairGuid != null;
        private uint? currentChairGuid;

        /// <summary>
        /// Returns if <see cref="IPlayer"/> has premium signature subscription.
        /// </summary>
        public bool SignatureEnabled => Account.RbacManager.HasPermission(Permission.Signature);

        public PvPFlag PvPFlags
        {
            get => pvpFlags;
            set
            {
                pvpFlags = value;

                EnqueueToVisible(new ServerUnitPvpStateChange
                {
                    UnitId = Guid,
                    State  = pvpFlags
                });
            }
        }

        private PvPFlag pvpFlags;

        public IGameSession Session { get; private set; }

        public bool IsOnline { get; private set; }

        public AccountPresenceState Presence { get; set; }
        public string AwayAutoResponse { get; set; }
        public string BusyAutoResponse { get; set; }

        /// <summary>
        /// Returns if <see cref="IPlayer"/>'s client is currently in a loading screen.
        /// </summary>
        public bool IsLoading { get; set; } = true;

        public IInventory Inventory { get; private set; }
        public ICurrencyManager CurrencyManager { get; }
        public IPathManager PathManager { get; private set; }
        public IPathMissionManager PathMissionManager { get; private set; }
        public IPathScientistManager PathScientistManager { get; private set; }
        public IPathSoldierManager PathSoldierManager { get; private set; }
        public IPathSettlerManager PathSettlerManager { get; private set; }
        public IPathExplorerManager PathExplorerManager { get; private set; }
        public ITitleManager TitleManager { get; private set; }
        public ISpellManager SpellManager { get; private set; }
        public ICostumeManager CostumeManager { get; private set; }
        public IPetCustomisationManager PetCustomisationManager { get; private set; }
        public ICharacterKeybindingManager KeybindingManager { get; private set; }
        public IDatacubeManager DatacubeManager { get; private set; }
        public IMailManager MailManager { get; private set; }
        public IZoneMapManager ZoneMapManager { get; private set; }
        public IPrimeLevelManager PrimeLevelManager { get; private set; }

        /// <summary>
        /// Where the player entered an instance through an instance portal, the instance's exit portal returns them there.
        /// </summary>
        public IMapPosition InstancePortalReturn { get; set; }
        public IQuestManager QuestManager { get; private set; }
        public ICharacterAchievementManager AchievementManager { get; private set; }
        public ISupplySatchelManager SupplySatchelManager { get; private set; }
        public ITradeskillManager TradeskillManager { get; private set; }
        public ICraftingManager CraftingManager { get; private set; }
        public IChallengeManager ChallengeManager { get; private set; }
        public ITaxiManager TaxiManager { get; private set; }
        public IRewardTrackManager RewardTrackManager { get; private set; }
        public IXpManager XpManager { get; private set; }
        public IReputationManager ReputationManager { get; private set; }
        public IGuildManager GuildManager { get; }
        public IResidenceManager ResidenceManager { get; }
        public ICinematicManager CinematicManager { get; private set; }
        public ICharacterEntitlementManager EntitlementManager { get; private set; }
        public ILogoutManager LogoutManager { get; private set; }
        public IAppearanceManager AppearanceManager { get; private set; }
        public IResurrectionManager ResurrectionManager { get; private set; }
        public ICombatPetManager CombatPetManager { get; private set; }

        public IVendorInfo SelectedVendorInfo { get; set; } // TODO unset this when too far away from vendor

        private bool forceSave;
        private UpdateTimer saveTimer = new(SaveDuration);
        private PlayerSaveMask saveMask;

        private Dictionary<Property, Dictionary<ItemSlot, /*value*/float>> itemProperties = new();

        private UpdateTimer relocationTimer = new(TimeSpan.FromSeconds(1));

        #region Dependency Injection

        private readonly IInternalMessagePublisher messagePublisher;
        private readonly IEntityFactory entityFactory;
        private readonly IMatchingManager matchingManager;
        private readonly IMatchManager matchManager;
        private readonly ICreatureInfoManager creatureInfoManager;

        private readonly IStatUpdateManager<IPlayer> statUpdateManager;

        public Player(
            IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory,
            IStatUpdateManager<IPlayer> statUpdateManager,
            ISpellFactory spellFactory,
            IInternalMessagePublisher messagePublisher,
            IEntityFactory entityFactory,
            ICreatureInfoManager creatureInfoManager,
            IMatchingManager matchingManager,
            IMatchManager matchManager,
            ICurrencyManager currencyManager,
            IResurrectionManager resurrectionManager,
            IQuestManager questManager,
            IGuildManager guildManager,
            IResidenceManager residenceManager)
            : base(movementManager, entitySummonFactory, statUpdateManager, spellFactory)
        {
            this.messagePublisher    = messagePublisher;

            // TODO: can this be replaced by IEntitySummonFactory?
            this.entityFactory       = entityFactory;
            this.creatureInfoManager = creatureInfoManager;
            this.matchingManager     = matchingManager;
            this.matchManager        = matchManager;

            this.statUpdateManager   = statUpdateManager;

            // managers
            CurrencyManager     = currencyManager;
            GuildManager        = guildManager;
            ResidenceManager    = residenceManager;
            ResurrectionManager = resurrectionManager;
            QuestManager        = questManager;
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="IPlayer"/> from supplied <see cref="IGameSession"/> and <see cref="CharacterModel"/>.
        /// </summary>
        public void Initialise(IGameSession session, IAccount account, CharacterModel model)
        {
            ActivationRange   = BaseMap.DefaultVisionRange;

            Session           = session;

            Account           = account;
            Identity          = new Abstract.Identity { Id = model.Id, RealmId = RealmContext.Instance.RealmId };
            Name              = model.Name;
            sex               = (Sex)model.Sex;
            race              = (Race)model.Race;
            Class             = (Class)model.Class;
            path              = (Path)model.ActivePath;
            PathActivatedTime = model.PathActivatedTimestamp;
            InputKeySet       = (InputSets)model.InputKeySet;
            BindPoint         = model.BindPoint;
            Faction1          = (Faction)model.FactionId;
            Faction2          = (Faction)model.FactionId;
            flags             = (CharacterFlag)model.Flags;

            CreateTime        = model.CreateTime;
            TimePlayedTotal   = model.TimePlayedTotal;
            TimePlayedLevel   = model.TimePlayedLevel;

            statUpdateManager.Initialise(this);

            foreach (CharacterStatModel statModel in model.Stat)
                stats.Add((Static.Entity.Stat)statModel.Stat, new StatValue(statModel));

            SetStat(Static.Entity.Stat.Sheathed, 1u);

            // temp
            SetStat(Static.Entity.Stat.Dash, 200F);
            // sprint
            SetStat(Static.Entity.Stat.Resource0, 500f);

            CalculateDefaultProperties();
            SetBaseCharacterProperties();

            MaxInterruptArmour = 0;

            scriptCollection = ScriptManager.Instance.InitialiseOwnedCollection<IPlayer>(this);
            ScriptManager.Instance.InitialiseEntityScripts<IPlayer>(scriptCollection, this, null);

            // managers
            EntitlementManager      = new CharacterEntitlementManager(this, model);
            Account.RewardPropertyManager.Initialise(this);

            CostumeManager          = new CostumeManager(this, model);
            Inventory               = new Inventory(this, model);
            CostumeManager.UpdateVisuals();
            CurrencyManager.Initialise(this, model);
            PathManager             = new PathManager(this, model);
            TitleManager            = new TitleManager(this, model);
            SpellManager            = new SpellManager(this, model);
            PetCustomisationManager = new PetCustomisationManager(this, model);
            CombatPetManager        = new CombatPetManager(this, entityFactory, creatureInfoManager,
                LegacyServiceProvider.Provider.GetService<Abstract.Spell.Info.ISpellInfoManager>(), GameTableManager.Instance);
            KeybindingManager       = new CharacterKeybindingManager(this, model);
            DatacubeManager         = new DatacubeManager(this, model);
            MailManager             = new MailManager(this, model);
            ZoneMapManager          = new ZoneMapManager(this, model);
            PrimeLevelManager       = new PrimeLevelManager(this, model);
            PathMissionManager      = new PathMissionManager(this, model);
            PathScientistManager    = new PathScientistManager(this);
            PathSoldierManager      = new PathSoldierManager(this);
            PathSettlerManager      = new PathSettlerManager(this);
            PathExplorerManager     = new PathExplorerManager(this);
            QuestManager.Initialise(this, model);
            AchievementManager      = new CharacterAchievementManager(this, model);
            SupplySatchelManager    = new SupplySatchelManager(this, model);
            TradeskillManager       = new TradeskillManager(this, model);
            CraftingManager         = new CraftingManager(this);
            ChallengeManager        = new ChallengeManager(this, model);
            TaxiManager             = new TaxiManager(this, model, entityFactory, creatureInfoManager);
            RewardTrackManager      = new RewardTrackManager(this, model);
            XpManager               = new XpManager(this, model);
            ReputationManager       = new ReputationManager(this, model);
            GuildManager.Initialise(this, model);
            ResidenceManager.Initialise(this);
            CinematicManager        = new CinematicManager(this);

            LogoutManager           = new LogoutManager(this);
            LogoutManager.OnTimerFinished += Logout;

            AppearanceManager       = new AppearanceManager(this, model);
            ResurrectionManager.Initalise(this);

            // do dependant stat balance after all stats and properties have been set
            SetDependantStatBalance(true);
            foreach (IPropertyValue property in GetProperties())
                DependantStatBalance(property);

            PlayerManager.Instance.AddPlayer(this);
        }

        private void SetBaseCharacterProperties()
        {
            var baseProperties  = CharacterManager.Instance.GetCharacterBaseProperties();
            var classProperties = CharacterManager.Instance.GetCharacterClassBaseProperties(Class);

            foreach (IPropertyModifier propertyValue in baseProperties.Concat(classProperties))
                SetBaseProperty(propertyValue.Property, propertyValue.GetValue(Level));

            // property_base has no Focus pool and zeroes the Focus recovery rates, leaving Focus classes (Esper, Medic, Spellslinger) at 0 focus forever
            // fall back to the client defaults for recovery and a flat pool
            foreach (Property recovery in new[] { Property.BaseFocusRecoveryInCombat, Property.BaseFocusRecoveryOutofCombat })
            {
                if (GetProperty(recovery).BaseValue > 0f)
                    continue;

                UnitProperty2Entry entry = GameTableManager.Instance.UnitProperty2.GetEntry((uint)recovery);
                if (entry != null && entry.DefaultValue > 0f)
                    SetBaseProperty(recovery, entry.DefaultValue);
            }

            if (Class is Class.Esper or Class.Medic or Class.Spellslinger && GetProperty(Property.BaseFocusPool).BaseValue <= 0f)
                SetBaseProperty(Property.BaseFocusPool, DefaultFocusPool);
        }

        private const float DefaultFocusPool = 1000f;

        public override void Update(double lastTick)
        {
            LogoutManager.Update(lastTick);

            // don't process world updates while logout is finalising
            if (LogoutManager.State is LogoutState.Logout or LogoutState.Finished)
                return;

            base.Update(lastTick);

            UpdateAutoSprint(lastTick);
            TitleManager.Update(lastTick);
            SpellManager.Update(lastTick);
            CostumeManager.Update(lastTick);
            QuestManager.Update(lastTick);
            UpdateInstanceReturn(lastTick);
            UpdateQuestMicroInstances(lastTick);
            PathScientistManager.Update(lastTick);
            PathSoldierManager.Update(lastTick);
            PathExplorerManager.Update(lastTick);
            ChallengeManager.Update(lastTick);
            ResurrectionManager.Update(lastTick);
            CinematicManager.Update(lastTick);

            relocationTimer.Update(lastTick);
            if (relocationTimer.HasElapsed)
            {
                messagePublisher.PublishAsync(new PlayerPositionUpdatedMessage
                {
                    Identity = Identity.ToInternalIdentity(),
                    Position = new Network.Internal.Message.Shared.Position
                    {
                        X = Position.X,
                        Y = Position.Y,
                        Z = Position.Z
                    }
                }).FireAndForgetAsync();

                relocationTimer.Reset(false);
            }

            saveTimer.Update(lastTick);
            if (saveTimer.HasElapsed || forceSave)
            {
                forceSave = false;

                double timeSinceLastSave = GetTimeSinceLastSave();
                TimePlayedSession += timeSinceLastSave;
                TimePlayedLevel += timeSinceLastSave;
                TimePlayedTotal += timeSinceLastSave;

                Save();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            QuestManager.Dispose();
        }

        /// <summary>
        /// Save <see cref="IPlayer"/> to database, invoke supplied <see cref="Action"/> once save is complete.
        /// </summary>
        /// <remarks>
        /// This is a delayed save, <see cref="AuthContext"/> changes are saved first followed by <see cref="CharacterContext"/> changes.
        /// Packets for session will not be handled until save is complete.
        /// </remarks>
        public void Save(Action callback = null)
        {
            Session.Events.EnqueueEvent(new TaskEvent(DatabaseManager.Instance.GetDatabase<AuthDatabase>().Save(Save),
            () =>
            {
                Session.Events.EnqueueEvent(new TaskEvent(DatabaseManager.Instance.GetDatabase<CharacterDatabase>().Save(Save),
                () =>
                {
                    callback?.Invoke();
                    Session.CanProcessIncomingPackets = true;
                    saveTimer.Resume();
                }));
            }));

            saveTimer.Reset(false);

            // prevent packets from being processed until asynchronous player save task is complete
            Session.CanProcessIncomingPackets = false;
        }

        /// <summary>
        /// Save <see cref="IPlayer"/> to database.
        /// </summary>
        /// <remarks>
        /// This is an instant save, <see cref="AuthContext"/> changes are saved first followed by <see cref="CharacterContext"/> changes.
        /// This will block the calling thread until the database save is complete.
        /// </remarks>
        public async void SaveDirect()
        {
            await DatabaseManager.Instance.GetDatabase<AuthDatabase>().Save(Save);
            await DatabaseManager.Instance.GetDatabase<CharacterDatabase>().Save(Save);
        }

        /// <summary>
        /// Save database changes for <see cref="Player"/> to <see cref="AuthContext"/>.
        /// </summary>
        public void Save(AuthContext context)
        {
            Account.Save(context);
        }

        /// <summary>
        /// Save database changes for <see cref="Player"/> to <see cref="CharacterContext"/>.
        /// </summary>
        public void Save(CharacterContext context)
        {
            var model = new CharacterModel
            {
                Id = CharacterId
            };

            EntityEntry<CharacterModel> entity = context.Attach(model);

            if (saveMask != PlayerSaveMask.None)
            {
                if ((saveMask & PlayerSaveMask.Location) != 0)
                {
                    // logging out mid-flight puts the player at the taxi's destination instead of in the air
                    Vector3 position = Position;
                    Vector3 rotation = Rotation;
                    if (TaxiManager.FlightDestination is { } flight)
                    {
                        position = flight.Position;
                        rotation = flight.Rotation;
                    }

                    model.LocationX = position.X;
                    entity.Property(p => p.LocationX).IsModified = true;

                    model.LocationY = position.Y;
                    entity.Property(p => p.LocationY).IsModified = true;

                    model.LocationZ = position.Z;
                    entity.Property(p => p.LocationZ).IsModified = true;

                    model.RotationX = rotation.X;
                    entity.Property(p => p.RotationX).IsModified = true;

                    model.RotationY = rotation.Y;
                    entity.Property(p => p.RotationY).IsModified = true;

                    model.RotationZ = rotation.Z;
                    entity.Property(p => p.RotationZ).IsModified = true;

                    model.WorldId = (ushort)Map.Entry.Id;
                    entity.Property(p => p.WorldId).IsModified = true;

                    model.WorldZoneId = (ushort)(Zone?.Id ?? 0);
                    entity.Property(p => p.WorldZoneId).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Path) != 0)
                {
                    model.ActivePath = (uint)Path;
                    entity.Property(p => p.ActivePath).IsModified = true;
                    model.PathActivatedTimestamp = PathActivatedTime;
                    entity.Property(p => p.PathActivatedTimestamp).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.InputKeySet) != 0)
                {
                    model.InputKeySet = (sbyte)InputKeySet;
                    entity.Property(p => p.InputKeySet).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.BindPoint) != 0)
                {
                    model.BindPoint = BindPoint;
                    entity.Property(p => p.BindPoint).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Flags) != 0)
                {
                    model.Flags = (uint)Flags;
                    entity.Property(p => p.Flags).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Sex) != 0)
                {
                    model.Sex = (byte)Sex;
                    entity.Property(p => p.Sex).IsModified = true;                    
                }

                if ((saveMask & PlayerSaveMask.Race) != 0)
                {
                    model.Race = (byte)Race;
                    entity.Property(p => p.Race).IsModified = true;
                }

                saveMask = PlayerSaveMask.None;
            }

            model.TimePlayedLevel = (uint)TimePlayedLevel;
            entity.Property(p => p.TimePlayedLevel).IsModified = true;
            model.TimePlayedTotal = (uint)TimePlayedTotal;
            entity.Property(p => p.TimePlayedTotal).IsModified = true;

            model.IsOnline = IsOnline;
            entity.Property(p => p.IsOnline).IsModified = true;

            if (!IsOnline)
            {
                model.LastOnline = DateTime.UtcNow;
                entity.Property(p => p.LastOnline).IsModified = true;
            }

            foreach (IStatValue stat in stats.Values)
                stat.SaveCharacter(CharacterId, context);

            Inventory.Save(context);
            CurrencyManager.Save(context);
            PathManager.Save(context);
            TitleManager.Save(context);
            CostumeManager.Save(context);
            PetCustomisationManager.Save(context);
            KeybindingManager.Save(context);
            SpellManager.Save(context);
            DatacubeManager.Save(context);
            MailManager.Save(context);
            ZoneMapManager.Save(context);
            PrimeLevelManager.Save(context);
            PathMissionManager.Save(context);
            QuestManager.Save(context);
            AchievementManager.Save(context);
            SupplySatchelManager.Save(context);
            TradeskillManager.Save(context);
            ChallengeManager.Save(context);
            TaxiManager.Save(context);
            RewardTrackManager.Save(context);
            XpManager.Save(context);
            ReputationManager.Save(context);
            GuildManager.Save(context);
            EntitlementManager.Save(context);
            AppearanceManager.Save(context);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new PlayerEntityModel
            {
                Id        = CharacterId,
                RealmId   = RealmContext.Instance.RealmId,
                Name      = Name,
                Race      = Race,
                Class     = Class,
                Sex       = Sex,
                Bones     = AppearanceManager.GetBones()
                    .Select(b => b.BoneValue)
                    .ToList(),
                Title     = TitleManager.ActiveTitleId,
                GuildIds  = GuildManager
                    .Select(g => g.Id)
                    .ToList(),
                GuildName = GuildManager.GuildAffiliation?.Name,
                GuildType = GuildManager.GuildAffiliation?.Type ?? GuildType.None,
                PvPFlag   = PvPFlags,

                // We use Group 1 as the "dominant group"
                GroupId   = GroupAssociation
            };
        }

        /// <summary>
        /// Position the player arrived at on the current map, used to recall players that are stuck.
        /// </summary>
        public Vector3 MapEntryPosition { get; private set; }

        /// <summary>
        /// Client player.moveSpeed option: 0 = walk, 1 = run with shift to sprint, 2 = sprint whenever out of combat.
        /// </summary>
        /// <remarks>
        /// The client defaults to 2 and only sends shift sprint (<see cref="Network.World.Message.Model.Spell.ClientSetSprintState"/>) in combat or with option 1,
        /// out of combat sprint for option 2 is applied by the server.
        /// </remarks>
        public uint MoveSpeedOption { get; set; } = 2u;

        /// <summary>
        /// Sprint - Tier 2, the out of combat sprint (game formula 1227).
        /// </summary>
        private const uint OutOfCombatSprintSpell4Id = 80529;

        private readonly UpdateTimer autoSprintTimer = new(TimeSpan.FromSeconds(0.5d));

        /// <summary>
        /// Keep the out of combat sprint active while the player.moveSpeed option asks for it.
        /// </summary>
        private void UpdateAutoSprint(double lastTick)
        {
            autoSprintTimer.Update(lastTick);
            if (!autoSprintTimer.HasElapsed)
                return;

            autoSprintTimer.Reset();

            bool wanted = MoveSpeedOption == 2u && IsAlive && !InCombat && PlatformGuid == null && CanTeleport();
            bool active = HasSpell(OutOfCombatSprintSpell4Id, out ISpell spell);
            if (wanted && !active)
            {
                ISpellParameters parameters = LegacyServiceProvider.Provider.GetService<IFactory<ISpellParameters>>().Resolve();
                parameters.UserInitiatedSpellCast = false;
                CastSpell(OutOfCombatSprintSpell4Id, parameters);
            }
            else if (!wanted && active && MoveSpeedOption == 2u)
                spell.Finish();
        }

        public override void OnAddToMap(IBaseMap map, uint guid, Vector3 vector)
        {
            // a teleport that was still loading when the character logged out finished after cleanup,
            // take the character straight back off the map instead of logging a ghost back in
            if (cleanedUp)
            {
                base.OnAddToMap(map, guid, vector);
                RemoveFromMap();

                log.Trace($"Removed character {Name}({CharacterId}) from map {map.Entry.Id}, it was added after logout cleanup.");
                return;
            }

            MapEntryPosition = vector;

            // this must come before OnAddToMap
            // the client UI initialises the Holomark checkboxes during OnDocumentReady
            SendCharacterFlagsUpdated();

            // the area grid gives the zone until the client reports the zone it is in on the new map
            clientZoneReported = false;

            base.OnAddToMap(map, guid, vector);

            // resummon vanity pet if it existed before teleport
            if (pendingTeleport?.VanityPetId != null)
            {
                ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(pendingTeleport.VanityPetId.Value);
                if (creatureInfo != null)
                {
                    // TODO: this should really be replaced with EntitySummonFactory
                    var pet = entityFactory.CreateEntity<IPetEntity>();
                    pet.Initialise(this, creatureInfo);
                    pet.AddToMap(map, Position);
                }
            }

            SendPacketsAfterAddToMap();

            // bots stay summoned through teleports
            CombatPetManager.OnAddToMap();

            if (!IsAlive)
            {
                if (pendingTeleport.Resurrect)
                    Map.Resurrect(ResurrectionType.WakeHere, this);
                else
                    OnDeath(null);
            }

            if (PreviousMap == null)
                OnLogin();

            RestoreInstanceReturn();

            messagePublisher.PublishAsync(new PlayerWorldUpdatedMessage
            {
                Identity    = Identity.ToInternalIdentity(),
                WorldId     = Map.Entry.Id
            }).FireAndForgetAsync();
        }

        /// <summary>
        /// Distance from an instance portal (eg: the Combat Training Room teleporters) that steps into it.
        /// </summary>
        private const float InstancePortalRange = 3.5f;

        /// <summary>
        /// Delay before a player is sent back from a quest instance once its quest is achieved.
        /// </summary>
        private const double InstanceReturnDelay = 5d;

        private double instancePortalCooldown;
        private (ushort WorldId, Vector3 Position, ushort QuestId)? instanceReturn;
        private double instanceReturnTimer = -1d;

        private static readonly Dictionary<uint, WorldLocation2Entry> zoneEntryLocations = [];

        /// <remarks>
        /// Quest instance portals carry no destination (InstancePortal only has a name and level range), retail scripts sent the player
        /// to the zone the quest's enter zone objective names when they stepped into the portal at that objective's marker.
        /// </remarks>
        private void CheckInstancePortals(Vector3 position)
        {
            if (instancePortalCooldown > 0d || IsLoading || !CanTeleport())
                return;

            IInstancePortalEntity portal = visibleEntities.Values
                .OfType<IInstancePortalEntity>()
                .FirstOrDefault(e => Vector3.Distance(e.Position, position) <= InstancePortalRange);
            if (portal == null)
            {
                CheckLevelTeleporters(position);
                return;
            }

            instancePortalCooldown = 5d;

            // the quest is still in progress, so its instance takes the player back in after the enter objective is done (eg: leaving
            // the Combat Training Room before Support Training: Healing's patients are healed, report 77)
            foreach (IQuest quest in QuestManager.GetActiveQuests().Where(q => q.State == QuestState.Accepted))
            {
                foreach (IQuestObjective objective in quest.Where(o => o.ObjectiveInfo.Type == QuestObjectiveType.EnterZone).OrderBy(o => o.IsComplete()))
                {
                    if (!objective.ObjectiveInfo.GetAreaLocations().Any(l => l.WorldId == Map.Entry.Id
                        && Vector3.Distance(new Vector3(l.Position0, l.Position1, l.Position2), portal.Position) <= 15f))
                        continue;

                    WorldLocation2Entry destination = GetZoneEntryLocation(objective.ObjectiveInfo.Entry.Data);
                    if (destination == null)
                        continue;

                    instanceReturn = ((ushort)Map.Entry.Id, position, quest.Id);
                    instanceReturnTimer = -1d;
                    TeleportTo((ushort)destination.WorldId, destination.Position0, destination.Position1, destination.Position2);
                    return;
                }
            }
        }

        /// <summary>
        /// Distance from a level teleporter (eg: the Eldan Teleporters of the Star-Comm station) that steps onto it.
        /// </summary>
        private const float LevelTeleporterRange = 2.5f;

        /// <summary>
        /// How far above a level teleporter's origin its pad can be (the Eldan Teleporter pads are 2.4 to 4.6 m above).
        /// </summary>
        private const float LevelTeleporterHeight = 6f;

        private static readonly Dictionary<ushort, List<(uint CreatureId, string Name, Vector3 Position)>> levelTeleporters = [];

        /// <remarks>
        /// Teleporter creatures without an activate spell (Eldan Teleporter on the Levian Bay surface and in the station below) were moved
        /// between by retail scripts, they come in pairs on different levels: stepping onto one moves the player to the nearest teleporter
        /// with the same name at least 50 m above or below.
        /// </remarks>
        private void CheckLevelTeleporters(Vector3 position)
        {
            if (CheckPairedTeleporters(position))
                return;

            List<(uint CreatureId, string Name, Vector3 Position)> teleporters = GetLevelTeleporters((ushort)Map.Entry.Id);
            if (teleporters.Count == 0)
                return;

            // the creature's origin sits below the pad the player stands on (4.6 m on the Levian Bay surface), so only the horizontal distance counts
            (uint CreatureId, string Name, Vector3 Position) source = teleporters
                .FirstOrDefault(t => Vector2.Distance(new Vector2(t.Position.X, t.Position.Z), new Vector2(position.X, position.Z)) <= LevelTeleporterRange
                    && position.Y - t.Position.Y is > -2f and < LevelTeleporterHeight);
            if (source.Name == null)
                return;

            (uint CreatureId, string Name, Vector3 Position) destination = teleporters
                .Where(t => t.Name == source.Name && MathF.Abs(t.Position.Y - source.Position.Y) >= 50f
                    && Vector3.Distance(t.Position, source.Position) <= 600f)
                .OrderBy(t => Vector3.Distance(t.Position, source.Position))
                .FirstOrDefault();
            if (destination.Name == null)
                return;

            instancePortalCooldown = 5d;

            // arrive beside the destination teleporter so it doesn't send the player straight back
            Vector3 arrival = destination.Position + new Vector3(LevelTeleporterRange + 1.5f, 0f, 0f);
            // indoors there is no terrain, arrive above the pad and drop onto it rather than inside the floor below
            arrival.Y = Map.GetTerrainHeight(arrival.X, arrival.Z) is float height && MathF.Abs(height - destination.Position.Y) < LevelTeleporterHeight
                ? height
                : destination.Position.Y + LevelTeleporterHeight - 1f;
            TeleportToLocal(arrival);
        }

        /// <summary>
        /// Teleporter pads whose destination the quests name, as pairs of <see cref="WorldLocation2Entry"/> pad locations.
        /// </summary>
        /// <remarks>
        /// The Elder Vault under Star-Comm Station has four Eldan Teleporters, each leading to a different part of the station, most of them
        /// too far away for the nearest teleporter rule, which sent every one of them back up to the station (report 104). The pairs come from
        /// the Levian Bay quest objectives: Seizing Control (5859) "access Security Hub Sigma" and "return to the Elder Vault", The Key to
        /// Power (5959) "access Engineering Sublevel Theta" and "return to the Elder Vault", Lost in the Fog (5865) "access the cave to the
        /// surface" (Surfskitter Cave), Enter the Station (5868) the way in from Star-Comm Station.
        /// </remarks>
        private static readonly (uint From, uint To)[] pairedTeleporters =
        [
            (19319u, 19318u), // Elder Vault north - Star-Comm Station
            (19321u, 19323u), // Elder Vault east - Security Hub Sigma
            (19932u, 19934u), // Elder Vault west - Engineering Sublevel Theta
            (20585u, 19335u)  // Elder Vault south - Surfskitter Cave
        ];

        private bool CheckPairedTeleporters(Vector3 position)
        {
            foreach ((uint from, uint to) in pairedTeleporters)
            {
                foreach ((uint source, uint target) in new[] { (from, to), (to, from) })
                {
                    WorldLocation2Entry sourceEntry = GameTableManager.Instance.WorldLocation2.GetEntry(source);
                    if (sourceEntry == null || sourceEntry.WorldId != Map.Entry.Id)
                        continue;

                    var pad = new Vector3(sourceEntry.Position0, sourceEntry.Position1, sourceEntry.Position2);
                    if (Vector2.Distance(new Vector2(pad.X, pad.Z), new Vector2(position.X, position.Z)) > LevelTeleporterRange
                        || position.Y - pad.Y is <= -3f or >= LevelTeleporterHeight)
                        continue;

                    WorldLocation2Entry targetEntry = GameTableManager.Instance.WorldLocation2.GetEntry(target);
                    if (targetEntry == null || targetEntry.WorldId != Map.Entry.Id)
                        return false;

                    instancePortalCooldown = 5d;

                    // arrive beside the destination pad so it doesn't send the player straight back
                    var destination = new Vector3(targetEntry.Position0, targetEntry.Position1, targetEntry.Position2);
                    Vector3 arrival = destination + new Vector3(LevelTeleporterRange + 1.5f, 0f, 0f);
                    arrival.Y = Map.GetTerrainHeight(arrival.X, arrival.Z) is float height && MathF.Abs(height - destination.Y) < LevelTeleporterHeight
                        ? height
                        : destination.Y + 1f;
                    TeleportToLocal(arrival);
                    return true;
                }
            }

            return false;
        }

        private static List<(uint CreatureId, string Name, Vector3 Position)> GetLevelTeleporters(ushort worldId)
        {
            lock (levelTeleporters)
            {
                if (levelTeleporters.TryGetValue(worldId, out var teleporters))
                    return teleporters;

                TextTable text = GameTableManager.Instance.GetTextTable(Language.English);
                teleporters = DatabaseManager.Instance.GetDatabase<WorldDatabase>().GetEntities(worldId)
                    .Where(e => e.Type is EntityType.NonPlayer or EntityType.Simple)
                    .Select(e => (Model: e, Creature: GameTableManager.Instance.Creature2.GetEntry(e.Creature)))
                    .Where(e => e.Creature != null && e.Creature.Spell4IdActivate.All(s => s == 0u))
                    .Select(e => (e.Model.Creature, Name: text?.GetEntry(e.Creature.LocalizedTextIdName) ?? "", Position: new Vector3(e.Model.X, e.Model.Y, e.Model.Z)))
                    .Where(e => e.Name.EndsWith("Teleporter", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                levelTeleporters.Add(worldId, teleporters);
                return teleporters;
            }
        }

        /// <summary>
        /// A player logging in (or arriving some other way) inside a quest instance has no remembered way back,
        /// the way back is the marker of the enter zone objective that leads into this world (the portal outside).
        /// </summary>
        /// <remarks>
        /// The player is sent back once the quest is no longer in progress: achieved, abandoned or never taken.
        /// </remarks>
        private void RestoreInstanceReturn()
        {
            if (instanceReturn != null || Map == null)
                return;

            // open world continents (Alizar, Olyssia, Isigrol, ...) are never quest instances, some enter zone objectives of
            // quests the player never took (eg: Journey to Malgrave) point at zones on a continent and sent players logging in
            // there to another continent
            if (IsOpenWorld((ushort)Map.Entry.Id))
                return;

            foreach (Quest2Entry questEntry in GameTableManager.Instance.Quest2.Entries.Where(q => q != null))
            {
                foreach (uint objectiveId in questEntry.Objectives.Where(o => o != 0u))
                {
                    QuestObjectiveEntry objective = GameTableManager.Instance.QuestObjective.GetEntry(objectiveId);
                    if (objective == null || (QuestObjectiveType)objective.Type != QuestObjectiveType.EnterZone)
                        continue;

                    WorldLocation2Entry destination = GetZoneEntryLocation(objective.Data);
                    if (destination == null || destination.WorldId != Map.Entry.Id)
                        continue;

                    // the player has to be inside the instance zone the objective leads to (same top level zone), not elsewhere on the world
                    if (Zone == null || GetRootZoneId(Zone.Id) != GetRootZoneId(objective.Data))
                        continue;

                    WorldLocation2Entry portal = GameTableManager.Instance.WorldLocation2.GetEntry(objective.WorldLocationsIdIndicator00);
                    if (portal == null || portal.WorldId == Map.Entry.Id)
                        continue;

                    // only quest instances, entered through an instance portal at the objective's marker (not zones reached by a portal spell or taxi)
                    var portalPosition = new Vector3(portal.Position0, portal.Position1, portal.Position2);
                    if (!GetInstancePortalPositions((ushort)portal.WorldId).Any(p => Vector3.Distance(p, portalPosition) <= 15f))
                        continue;

                    // prefer the quest the player has
                    if (QuestManager.GetQuestState((ushort)questEntry.Id) == null && instanceReturnCandidate != null)
                        continue;

                    instanceReturnCandidate = ((ushort)portal.WorldId, new Vector3(portal.Position0, portal.Position1, portal.Position2), (ushort)questEntry.Id);
                    if (QuestManager.GetQuestState((ushort)questEntry.Id) != null)
                        break;
                }
            }

            instanceReturn = instanceReturnCandidate;
            instanceReturnCandidate = null;
        }

        private (ushort WorldId, Vector3 Position, ushort QuestId)? instanceReturnCandidate;

        private static uint GetRootZoneId(uint zoneId)
        {
            WorldZoneEntry zone = GameTableManager.Instance.WorldZone.GetEntry(zoneId);
            for (int i = 0; zone != null && zone.ParentZoneId != 0u && i < 16; i++)
                zone = GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) ?? zone;
            return zone?.Id ?? zoneId;
        }

        private static readonly Lazy<HashSet<uint>> openWorlds = new(() => GameTableManager.Instance.TaxiNode.Entries
            .Where(n => n != null && n.TaxiNodeTypeEnum is 0u or 2u)
            .Select(n => GameTableManager.Instance.WorldLocation2.GetEntry(n.WorldLocation2Id))
            .Where(l => l != null)
            .Select(l => l.WorldId)
            .ToHashSet());

        /// <summary>
        /// Returns if the world is an open world continent, worlds with taxi kiosks or rapid transport destinations.
        /// </summary>
        private static bool IsOpenWorld(ushort worldId)
        {
            return GameTableManager.Instance.World.GetEntry(worldId)?.Type == 0 && openWorlds.Value.Contains(worldId);
        }

        private static readonly Dictionary<ushort, List<Vector3>> instancePortalPositions = [];

        private static List<Vector3> GetInstancePortalPositions(ushort worldId)
        {
            lock (instancePortalPositions)
            {
                if (!instancePortalPositions.TryGetValue(worldId, out List<Vector3> positions))
                {
                    positions = DatabaseManager.Instance.GetDatabase<WorldDatabase>().GetEntities(worldId)
                        .Where(e => e.Type == EntityType.InstancePortal)
                        .Select(e => new Vector3(e.X, e.Y, e.Z))
                        .ToList();
                    instancePortalPositions.Add(worldId, positions);
                }

                return positions;
            }
        }

        /// <summary>
        /// Return the location a player arrives at in the supplied zone, the zone's spawn point (radius 1 location) when it has one.
        /// </summary>
        private static WorldLocation2Entry GetZoneEntryLocation(uint zoneId)
        {
            lock (zoneEntryLocations)
            {
                if (zoneEntryLocations.TryGetValue(zoneId, out WorldLocation2Entry cached))
                    return cached;

                WorldLocation2Entry location = GameTableManager.Instance.WorldLocation2.Entries
                    .Where(l => l != null && l.WorldZoneId == zoneId)
                    .OrderBy(l => l.Radius)
                    .FirstOrDefault();
                zoneEntryLocations[zoneId] = location;
                return location;
            }
        }

        /// <summary>
        /// Seconds a quest micro instance waits before taking the player in, a scene of the step before (eg: the Skull's Eye taking off) plays first.
        /// </summary>
        private const double QuestMicroInstanceDelay = 8d;

        /// <summary>
        /// Distance to the marker of the step before a quest micro instance (or of the enter zone objective leading into it) that takes the player in.
        /// </summary>
        private const float QuestMicroInstanceRange = 40f;

        private double questMicroInstanceCheck;
        private (ushort QuestId, uint WorldId, double Remaining)? questMicroInstancePending;

        /// <summary>
        /// Take the player into the small instance map a quest step happens on (the Skull's Eye in Finish Him!, Boss Xagg's Dust Stalker)
        /// once the step before it is done at its marker.
        /// </summary>
        /// <remarks>
        /// Retail moved the player with server scripts (a ship taking off, an access panel), the client data only has the steps' markers on
        /// the instance map. The player arrives at the instance map's first world location and returns where they left once the quest is
        /// achieved (<see cref="UpdateInstanceReturn"/>).
        /// </remarks>
        private void UpdateQuestMicroInstances(double lastTick)
        {
            if (Map == null || IsLoading || instanceReturn != null)
                return;

            if (questMicroInstancePending is { } pending)
            {
                double remaining = pending.Remaining - lastTick;
                if (remaining > 0d)
                {
                    questMicroInstancePending = (pending.QuestId, pending.WorldId, remaining);
                    return;
                }

                questMicroInstancePending = null;
                if (QuestManager.GetQuestState(pending.QuestId) != QuestState.Accepted || !CanTeleport())
                    return;

                WorldLocation2Entry start = GetMicroInstanceStart(pending.WorldId);
                if (start == null)
                    return;

                instanceReturn      = ((ushort)Map.Entry.Id, Position, pending.QuestId);
                instanceReturnTimer = -1d;
                TeleportTo((ushort)start.WorldId, start.Position0, start.Position1, start.Position2);
                return;
            }

            questMicroInstanceCheck -= lastTick;
            if (questMicroInstanceCheck > 0d)
                return;
            questMicroInstanceCheck = 1d;

            foreach (IQuest quest in QuestManager.GetActiveQuests().Where(q => q.State == QuestState.Accepted))
            {
                IQuestObjective current = quest
                    .Where(o => !o.IsComplete() && !o.ObjectiveInfo.IsOptional())
                    .OrderBy(o => o.Index)
                    .FirstOrDefault();
                if (current == null)
                    continue;

                List<WorldLocation2Entry> markers = current.ObjectiveInfo.GetAreaLocations().ToList();
                if (markers.Count == 0 || markers.Any(l => l.WorldId == Map.Entry.Id))
                    continue;

                uint worldId = markers[0].WorldId;
                if (markers.Any(l => l.WorldId != worldId) || !IsQuestMicroInstance(worldId))
                    continue;

                // the step before was done here, at its marker
                IQuestObjective previous = quest
                    .Where(o => o.Index < current.Index && !o.ObjectiveInfo.IsOptional())
                    .OrderByDescending(o => o.Index)
                    .FirstOrDefault();
                if (previous == null || !previous.ObjectiveInfo.GetAreaLocations().Any(l => l.WorldId == Map.Entry.Id
                    && Vector3.Distance(Position, new Vector3(l.Position0, l.Position1, l.Position2)) <= Math.Max(QuestMicroInstanceRange, l.Radius)))
                    continue;

                questMicroInstancePending = (quest.Id, worldId, QuestMicroInstanceDelay);
                return;
            }

            // an enter zone step into a micro instance, at its marker (eg: Enter the Dust Stalker at its access panel)
            foreach (IQuest quest in QuestManager.GetActiveQuests().Where(q => q.State == QuestState.Accepted))
            {
                foreach (IQuestObjective objective in quest.Where(o => o.ObjectiveInfo.Type == QuestObjectiveType.EnterZone && !o.IsComplete()))
                {
                    WorldLocation2Entry destination = GetZoneEntryLocation(objective.ObjectiveInfo.Entry.Data);
                    if (destination == null || destination.WorldId == Map.Entry.Id || !IsQuestMicroInstance(destination.WorldId))
                        continue;

                    if (!objective.ObjectiveInfo.GetAreaLocations().Any(l => l.WorldId == Map.Entry.Id
                        && Vector3.Distance(Position, new Vector3(l.Position0, l.Position1, l.Position2)) <= Math.Max(10f, l.Radius)))
                        continue;

                    questMicroInstancePending = (quest.Id, destination.WorldId, 1d);
                    return;
                }
            }
        }

        /// <summary>
        /// Small instance maps of quest steps: mini dungeon and quest micro maps, never continents, housing or battlegrounds.
        /// </summary>
        private static bool IsQuestMicroInstance(uint worldId)
        {
            WorldEntry world = GameTableManager.Instance.World.GetEntry(worldId);
            return world != null && (uint)world.Type is 1u or 10u;
        }

        private static WorldLocation2Entry GetMicroInstanceStart(uint worldId)
        {
            return GameTableManager.Instance.WorldLocation2.Entries
                .Where(l => l != null && l.WorldId == worldId)
                .OrderBy(l => l.Id)
                .FirstOrDefault();
        }

        /// <summary>
        /// Send the player back to where they entered a quest instance once that quest is achieved.
        /// </summary>
        private void UpdateInstanceReturn(double lastTick)
        {
            if (instancePortalCooldown > 0d)
                instancePortalCooldown -= lastTick;

            if (instanceReturn is not { } entry || Map == null || IsLoading)
                return;

            if (Map.Entry.Id == entry.WorldId)
            {
                // back on the original world (by other means), nothing to return from
                if (instanceReturnTimer < 0d && !(pendingTeleport != null))
                    instanceReturn = null;
                return;
            }

            if (instanceReturnTimer < 0d)
            {
                QuestState? state = QuestManager.GetQuestState(entry.QuestId);
                if (state is QuestState.Accepted)
                    return;

                // the quest is handed in inside the instance (Knock Knock: Corrigan Doon waits in the facility)
                if (state is QuestState.Achieved && Quest.GlobalQuestManager.Instance.GetQuestReceivers(entry.QuestId)
                    .Any(c => GetVisibleCreature<IWorldEntity>(c).Any()))
                    return;

                instanceReturnTimer = InstanceReturnDelay;
                return;
            }

            instanceReturnTimer -= lastTick;
            if (instanceReturnTimer > 0d || !CanTeleport())
                return;

            instanceReturn = null;
            instanceReturnTimer = -1d;
            TeleportTo(entry.WorldId, entry.Position.X, entry.Position.Y, entry.Position.Z);
        }

        private bool clientZoneReported;

        /// <remarks>
        /// The map area grid is 2D and can't tell sub zones stacked on top of each other apart (eg: an Exo-Lab underneath a village),
        /// the client resolves its zone from the full area data, so once it reports a zone that is used instead.
        /// </remarks>
        protected override bool HasExternalZone => clientZoneReported;

        /// <summary>
        /// Invoked when the client reports it has moved into a different zone.
        /// </summary>
        public void OnClientZoneChange(ushort zoneId)
        {
            WorldZoneEntry zone = GameTableManager.Instance.WorldZone.GetEntry(zoneId);
            if (zone == null)
                return;

            clientZoneReported = true;
            SetZone(zone);

            // instance maps without area data only know their zone once the client reports it
            RestoreInstanceReturn();
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer"/> is relocated.
        /// </summary>
        protected override void OnRelocate(Vector3 vector)
        {
            base.OnRelocate(vector);
            saveMask |= PlayerSaveMask.Location;

            ZoneMapManager.OnRelocate(vector);
            PathMissionManager.OnRelocate(vector);
            CheckInstancePortals(vector);
            QuestManager.OnRelocate(vector);

            if (!relocationTimer.IsTicking)
                relocationTimer.Resume();
        }

        protected override void OnZoneUpdate()
        {
            if (Zone != null)
            {
                TextTable tt = GameTableManager.Instance.GetTextTable(Language.English);
                if (tt != null)
                {
                    GlobalChatManager.Instance.SendMessage(Session, $"New Zone: ({Zone.Id}){tt.GetEntry(Zone.LocalizedTextIdName)}");
                }

                uint tutorialId = AssetManager.Instance.GetTutorialIdForZone(Zone.Id);
                if (tutorialId > 0)
                {
                    Session.EnqueueMessageEncrypted(new ServerTutorial
                    {
                        TutorialId = tutorialId
                    });
                }

                // entering a sub zone also enters every parent zone (e.g. a room inside a lab)
                for (WorldZoneEntry zone = Zone; zone != null; zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
                    QuestManager.ObjectiveUpdate(QuestObjectiveType.EnterZone, zone.Id, 1);
            }

            ZoneMapManager.OnZoneUpdate();
            QuestManager.CheckCommunicatorMessages();
            PathMissionManager.OnZoneUpdate();
            PathSettlerManager.OnZoneUpdate();
            ChallengeManager.OnZoneUpdate();
            RewardTrackManager.OnZoneUpdate();

            if (Zone != null)
                Map?.OnPlayerZoneChange(this, Zone.Id);

            messagePublisher.PublishAsync(new PlayerWorldZoneUpdatedMessage
            {
                Identity    = Identity.ToInternalIdentity(),
                WorldZoneId = (ushort)Zone?.Id,
            }).FireAndForgetAsync();
        }

        private void SendPacketsAfterAddToMap()
        {
            DateTime start = DateTime.UtcNow;

            SendInGameTime();
            PathManager.SendInitialPackets();
            BuybackManager.Instance.SendBuybackItems(this);

            ResidenceManager.SendHousingBasics();
            Session.EnqueueMessageEncrypted(new ServerHousingNeighbours());
            Session.EnqueueMessageEncrypted(new ServerInstanceSettings() { ClientEntitySendUpdateInterval = 125 });

            SetControl(this);

            CostumeManager.SendInitialPackets();
            Account.CostumeManager.SendInitialPackets();

            if (PreviousMap == null)
            {
                var playerCreate = new ServerPlayerCreate
                {
                    ItemProficiencies = GetItemProficiencies(),
                    FactionData       = new ServerPlayerCreate.Faction
                    {
                        FactionId          = Faction1, // This does not do anything for the player's "main" faction. Exiles/Dominion
                        FactionReputations = ReputationManager
                            .Select(r => new ServerPlayerCreate.Faction.FactionReputation
                            {
                                FactionId = r.Id,
                                Value     = r.Amount
                            })
                            .ToList()
                    },
                    ActiveCostumeIndex    = CostumeManager.CostumeIndex ?? -1,
                    InputKeySet           = (uint)InputKeySet,
                    BindPoint             = BindPoint,
                    CharacterEntitlements = EntitlementManager
                        .Select(e => new ServerPlayerCreate.CharacterEntitlement
                        {
                            Entitlement = e.Type,
                            Count       = e.Amount
                        })
                        .ToList(),
                    TradeskillMaterials   = SupplySatchelManager.BuildNetworkPacket(),
                    Xp                    = XpManager.TotalXp,
                    RestBonusXp           = XpManager.RestBonusXp
                };

                foreach (ICurrency currency in CurrencyManager)
                    playerCreate.Money[(byte)currency.Id - 1] = currency.Amount;

                foreach (IItem item in Inventory
                    .Where(b => b.Location != InventoryLocation.Ability)
                    .SelectMany(i => i))
                {
                    playerCreate.Inventory.Add(new InventoryItem
                    {
                        Item   = item.Build(),
                        Reason = ItemUpdateReason.NoReason
                    });
                }

                playerCreate.SpecIndex = SpellManager.ActiveActionSet;
                CombatPetManager.AddToPlayerCreate(playerCreate);
                Session.EnqueueMessageEncrypted(playerCreate);
            }

            TitleManager.SendTitles();
            SpellManager.SendInitialPackets();
            PetCustomisationManager.SendInitialPackets();
            DatacubeManager.SendInitialPackets();
            MailManager.SendInitialPackets();
            ZoneMapManager.SendInitialPackets();
            PathMissionManager.SendInitialPackets();
            Account.CurrencyManager.SendInitialPackets();
            Account.ItemManager.SendInitialPackets();
            EntitlementEffects.ApplyAll(this);
            Account.DailyLoginManager.OnLogin();
            QuestManager.SendInitialPackets();
            AchievementManager.SendInitialPackets(null);
            TradeskillManager.SendInitialPackets();
            Account.RewardPropertyManager.SendInitialPackets();
            RewardTrackManager.SendInitialPackets();
            ChallengeManager.SendInitialPackets();
            TaxiManager.SendInitialPackets();
            ResurrectionManager.SendInitialPackets();

            Session.EnqueueMessageEncrypted(new ServerPhaseVisibilityWorldLocation
            {
                PhasesIPerceive = 1,
                PhasesThatPerceiveMe = 1
            });

            log.Trace($"Player {Name} took {(DateTime.UtcNow - start).TotalMilliseconds}ms to send packets after add to map.");
        }

        public ItemProficiency GetItemProficiencies()
        {
            //TODO: Store proficiencies in DB table and load from there. Do they change ever after creation? Perhaps something for use on custom servers?
            ClassEntry classEntry = GameTableManager.Instance.Class.GetEntry((ulong)Class);
            return (ItemProficiency)classEntry.StartingItemProficiencies;
        }

        protected override void OnRemoveFromMap()
        {
            DestroyDependents();
            base.OnRemoveFromMap();
        }

        /// <summary>
        /// Returns if <see cref="IGridEntity"/> can see supplied <see cref="IGridEntity"/>.
        /// </summary>
        private static readonly PrerequisiteParameters visibilityParameters = new() { UnhandledMeets = true };

        protected override bool CanSeeEntity(IGridEntity entity)
        {
            bool? canSeeMe = entity.InvokeScriptCollection<bool, ICanSeeMeScript>(c => c.CanSeeMe(this));
            if (canSeeMe != null && !canSeeMe.Value)
                return false;

            // creatures with several story versions (Queen Myala before and after a quest) only show the one matching the player's progress
            if (entity is IWorldEntity worldEntity && worldEntity.CreatureInfo?.PrerequisiteVisibilityEntry != null
                && !PrerequisiteManager.Instance.Meets(this, worldEntity.CreatureInfo.PrerequisiteVisibilityEntry.Id, visibilityParameters))
                return false;

            return base.CanSeeEntity(entity);
        }

        /// <summary>
        /// Add tracked <see cref="IGridEntity"/> that is in vision range.
        /// </summary>
        protected override void AddVisible(IGridEntity entity)
        {
            base.AddVisible(entity);

            if (entity is IWorldEntity worldEntity)
                Session.EnqueueMessageEncrypted(worldEntity.BuildCreatePacket(IsLoading));

            if (entity is IPlayer playerEntity)
                Session.EnqueueMessageEncrypted(new ServerSetUnitPathType
                {
                    UnitId = playerEntity.Guid,
                    Path = playerEntity.Path
                });

            if (entity == this)
            {
                Session.EnqueueMessageEncrypted(new ServerPlayerChanged
                {
                    Guid = entity.Guid,
                    Unknown1 = 1
                });
            }

            if (entity is IWorldEntity scannableEntity)
                PathScientistManager.OnAddVisible(scannableEntity);

            if (entity is IUnitEntity unitEntity && unitEntity.InCombat)
            {
                Session.EnqueueMessageEncrypted(new ServerUnitEnteredCombat
                {
                    UnitId = unitEntity.Guid,
                    InCombat = unitEntity.InCombat
                });
            }
        }

        /// <summary>
        /// Remove tracked <see cref="IGridEntity"/> that is no longer in vision range.
        /// </summary>
        protected override void RemoveVisible(IGridEntity entity)
        {
            base.RemoveVisible(entity);

            if (entity is IWorldEntity && entity != this)
            {
                Session.EnqueueMessageEncrypted(new ServerEntityDestroy
                {
                    Guid     = entity.Guid,
                    Unknown0 = true
                });
            }
        }

        protected override void AddVisible(uint gridX, uint gridZ)
        {
            base.AddVisible(gridX, gridZ);
            Map.GridAddVisiblePlayer(gridX, gridZ);
        }

        protected override void RemoveVisible(uint gridX, uint gridZ)
        {
            base.RemoveVisible(gridX, gridZ);
            Map.GridRemoveVisiblePlayer(gridX, gridZ);
        }

        /// <summary>
        /// Set the <see cref="IWorldEntity"/> that currently being controlled by the <see cref="IPlayer"/>.
        /// </summary>
        public void SetControl(IWorldEntity entity)
        {
            if (ControlGuid != null)
            {
                IWorldEntity control = Map.GetEntity<IWorldEntity>(ControlGuid.Value);
                if (control != null)
                    control.ControllerGuid = null;
            }

            bool hadControl = ControlGuid != null;
            ControlGuid = entity?.Guid;

            if (ControlGuid != null)
            {
                entity.ControllerGuid = Guid;

                Session.EnqueueMessageEncrypted(new ServerMovementControl
                {
                    Ticket    = 1,
                    Immediate = true,
                    UnitId    = ControlGuid.Value
                });
            }
            else if (hadControl)
                Session.EnqueueMessageEncrypted(new ServerMovementControlRemove());
        }

        private void Logout()
        {
            OnLogout();
            Cleanup();
        }

        private void Cleanup()
        {
            log.Trace($"Cleanup for character {Name}({CharacterId})...");

            PlayerManager.Instance.RemovePlayer(this);
            CleanupManager.Instance.AddPlayer(this);

            log.Trace($"Waiting to cleanup character {Name}({CharacterId})...");

            Session.Events.EnqueueEvent(new TimeoutPredicateEvent(TimeSpan.FromSeconds(15), CanCleanup,
                () =>
            {
                try
                {
                    log.Trace($"Cleanup for character {Name}({CharacterId}) has started...");

                    cleanedUp = true;

                    Save(() =>
                    {
                        if (Map != null)
                            RemoveFromMap();

                        messagePublisher.PublishAsync(new PlayerLoggedOutMessage
                        {
                            Identity = Identity.ToInternalIdentity()
                        }).FireAndForgetAsync();

                        Dispose();
                    });
                }
                finally
                {
                    CleanupManager.Instance.RemovePlayer(this);
                    log.Trace($"Cleanup for character {Name}({CharacterId}) has completed.");

                    LogoutManager.State = LogoutState.Finished;
                }
            }));
        }

        private bool cleanedUp;

        private bool CanCleanup()
        {
            return pendingTeleport == null;
        }

        private void OnLogin()
        {
            scriptCollection.Invoke<IPlayerScript>(s => s.OnLogin());

            string motd = RealmContext.Instance.Motd;
            if (motd?.Length > 0)
                GlobalChatManager.Instance.SendMessage(Session, motd, "MOTD", ChatChannelType.Realm);

            GuildManager.OnLogin();

            ShutdownManager.Instance.OnLogin(this);

            matchingManager.OnLogin(this);
            matchManager.OnLogin(this);

            IsOnline = true;
            forceSave = true;

            messagePublisher.PublishAsync(new PlayerLoggedInMessage
            {
                Identity  = Identity.ToInternalIdentity(),
                AccountId = Account.Id
            }).FireAndForgetAsync();
        }

        private void OnLogout()
        {
            GuildManager.OnLogout();
            ChallengeManager.OnLogout();

            matchingManager.OnLogout(this);
            matchManager.OnLogout(this);

            IsOnline = false;

            scriptCollection.Invoke<IPlayerScript>(s => s.OnLogout());
        }

        /// <summary>
        /// Set the Transmat Terminal <see cref="IPlayer"/> recalls to.
        /// </summary>
        public void SetBindPoint(ushort bindPointId)
        {
            if (bindPointId == BindPoint)
                return;

            if (GameTableManager.Instance.BindPoint.GetEntry(bindPointId) == null)
                throw new ArgumentException($"{bindPointId} is not a valid bind point id!");

            BindPoint = bindPointId;
            saveMask |= PlayerSaveMask.BindPoint;

            Session.EnqueueMessageEncrypted(new ServerBindPointUpdate
            {
                BindPointId = bindPointId
            });

            // the first bind teaches "Recall - Transmat"
            SpellManager.GrantSpells();
        }

        /// <summary>
        /// Returns if <see cref="IPlayer"/> can teleport.
        /// </summary>
        public bool CanTeleport() => pendingTeleport == null && !pendingLocalTeleport;

        private PendingTeleport pendingTeleport;
        private bool pendingLocalTeleport;

        /// <summary>
        /// Teleport <see cref="IPlayer"/> to supplied location.
        /// </summary>
        public void TeleportTo(ushort worldId, float x, float y, float z, IMapLock mapLock = null, TeleportReason reason = TeleportReason.Relocate)
        {
            WorldEntry entry = GameTableManager.Instance.World.GetEntry(worldId);
            if (entry == null)
                throw new ArgumentException($"{worldId} is not a valid world id!");

            TeleportTo(entry, new Vector3(x, y, z), mapLock, reason);
        }

        /// <summary>
        /// Teleport <see cref="IPlayer"/> to supplied location.
        /// </summary>
        public void TeleportTo(WorldEntry entry, Vector3 position, IMapLock mapLock = null, TeleportReason reason = TeleportReason.Relocate)
        {
            TeleportTo(new MapPosition
            {
                Info = new MapInfo
                {
                    Entry   = entry,
                    MapLock = mapLock
                },
                Position = position
            }, reason);
        }

        /// <summary>
        /// Teleport <see cref="IPlayer"/> to supplied location.
        /// </summary>
        public void TeleportTo(IMapPosition mapPosition, TeleportReason reason = TeleportReason.Relocate)
        {
            if (!CanTeleport())
            {
                SendGenericError(GenericError.InstanceTransferPending);
                return;
            }

            if (DisableManager.Instance.IsDisabled(DisableType.World, mapPosition.Info.Entry.Id))
            {
                SendSystemMessage($"Unable to teleport to world {mapPosition.Info.Entry.Id} because it is disabled.");
                return;
            }

            // store vanity pet summoned before teleport so it can be summoned again after being added to the new map
            uint? vanityPetId = null;
            if (VanityPetGuid != null)
            {
                IPetEntity pet = GetVisible<IPetEntity>(VanityPetGuid.Value);
                vanityPetId = pet?.CreatureId;
            }

            pendingTeleport = new PendingTeleport
            {
                Reason      = reason,
                MapPosition = mapPosition,
                VanityPetId = vanityPetId,
                Resurrect   = reason == TeleportReason.EndMatch && !IsAlive
            };

            SetControl(null);

            IMapPosition source = null;
            if (Map != null)
            {
                source = new MapPosition
                {
                    Info = new MapInfo
                    {
                        Entry   = Map.Entry,
                        MapLock = (Map as IMapInstance)?.MapLock
                    },
                    Position = Position
                };
            }

            MapManager.Instance.AddToMap(this, source, mapPosition, OnAddToMap, OnTeleportToFailed, OnTeleportToFailed);

            log.Trace($"Teleporting {Name}({CharacterId}) to map: {mapPosition.Info.Entry.Id}, instance: {mapPosition.Info.MapLock?.InstanceId ?? null}.");
        }

        /// <summary>
        /// Show loading screen for supplied <see cref="IMapPosition"/>.
        /// </summary>
        public void ShowLoadingScreen(IMapPosition position)
        {
            IsLoading = true;

            Session.EnqueueMessageEncrypted(new ServerChangeWorld
            {
                WorldId  = (ushort)position.Info.Entry.Id,
                Position = new Position(position.Position)
            });
        }

        /// <summary>
        /// Teleport <see cref="IPlayer"/> to supplied location.
        /// </summary>
        public void TeleportToLocal(Vector3 position, bool showLoadingScreen = true, Action<Vector3> callback = null)
        {
            if (!CanTeleport())
            {
                SendGenericError(GenericError.InstanceTransferPending);
                return;
            }

            pendingLocalTeleport = true;

            if (showLoadingScreen)
                Session.EnqueueMessageEncrypted(new ServerLoadingScreen());

            if (callback != null)
            {
                RelocateOnMap(position, (vector) =>
                {
                    OnTeleportToLocal(vector);
                    callback.Invoke(vector);
                });
            }
            else
                RelocateOnMap(position, OnTeleportToLocal);

            log.Trace($"Teleporting {Name}({CharacterId}) to location {position.X}, {position.Y}, {position.Z}.");
        }

        private void OnTeleportToLocal(Vector3 position)
        {
            SetControl(null);

            MovementManager.SetPosition(position, false);
            MovementManager.BroadcastNetworkEntityCommands();

            SetControl(this);

            if (VanityPetGuid != null)
            {
                IPetEntity pet = Map.GetEntity<IPetEntity>(VanityPetGuid.Value);
                pet?.RelocateOnMap(position);
            }

            CombatPetManager.OnRelocate(position);

            pendingLocalTeleport = false;
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer"/> teleport fails.
        /// </summary>
        private void OnTeleportToFailed(GenericError error)
        {
            if (Map != null)
            {
                SendGenericError(error);
                pendingTeleport = null;

                SetControl(this);

                log.Trace($"Error {error} occured during teleport for {Name}({CharacterId})!");
            }
            else
            {
                // player failed prerequisites to enter map on login
                // can not proceed, disconnect the client with a message
                Session.EnqueueMessageEncrypted(new ServerForceKick
                {
                    Reason = ForceKickReason.WorldDisconnect
                });

                log.Trace($"Error {error} occured during teleport for {Name}({CharacterId}), client will be disconnected!");
            }
        }

        private void OnTeleportToFailed(Exception ex)
        {
            Session.EnqueueMessageEncrypted(new ServerForceKick
            {
                Reason = ForceKickReason.WorldDisconnect
            });

            log.Trace(ex, $"Exception occured during teleport for {Name}({CharacterId}), client will be disconnected!");
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer""/> has finished loading and is ready to enter world.
        /// </summary>
        public void OnEnteredWorld()
        {
            // right before the loading screen is removed we can now send the actual network entity commands
            // this ensures there is no desync between the client and server caused by loading
            foreach (IGridEntity item in visibleEntities.Values)
                if (item is IWorldEntity we && we.MovementManager.RequiresSynchronisation)
                    we.MovementManager.SendNetworkEntityCommands(Session);

            Session.EnqueueMessageEncrypted(new ServerPlayerEnteredWorld());

            pendingTeleport = null;
            IsLoading = false;

            // communicator messages for the zone entered, skipped while loading
            QuestManager.CheckCommunicatorMessages();

            CinematicManager.OnEnteredWorld();
        }

        /// <summary>
        /// Used to send the current in game time to this player
        /// </summary>
        private void SendInGameTime()
        {
            uint lengthOfInGameDayInSeconds = SharedConfiguration.Instance.Get<RealmConfig>().LengthOfInGameDay;
            if (lengthOfInGameDayInSeconds == 0u)
                lengthOfInGameDayInSeconds = (uint)TimeSpan.FromHours(3.5d).TotalSeconds; // Live servers were 3.5h per in game day

            double timeOfDay = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds / lengthOfInGameDayInSeconds % 1;

            Session.EnqueueMessageEncrypted(new ServerTimeOfDay
            {
                TimeOfDay   = (uint)(timeOfDay * TimeSpan.FromDays(1).TotalSeconds),
                LengthOfDay = lengthOfInGameDayInSeconds
            });
        }

        /// <summary>
        /// Make <see cref="IPlayer"/> sit on provided <see cref="IWorldEntity"/>.
        /// </summary>
        public void Sit(IWorldEntity chair)
        {
            if (IsSitting)
                Unsit();

            currentChairGuid = chair.Guid;

            // TODO: Emit interactive state from the entity instance itself
            chair.EnqueueToVisible(new ServerUnitInUse
            {
                UnitId = chair.Guid,
                InUse  = true
            }, true);
            EnqueueToVisible(new ServerUnitSetChair
            {
                UnitId      = Guid,
                UnitIdChair = chair.Guid,
                WaitForUnit = false
            }, true);
        }

        /// <summary>
        /// Remove <see cref="IPlayer"/> from the <see cref="IWorldEntity"/> it is sitting on.
        /// </summary>
        public void Unsit()
        {
            if (!IsSitting)
                return;

            IWorldEntity currentChair = GetVisible<IWorldEntity>(currentChairGuid.Value);
            if (currentChair == null)
                throw new InvalidOperationException();

            // TODO: Emit interactive state from the entity instance itself
            currentChair.EnqueueToVisible(new ServerUnitInUse
            {
                UnitId = currentChair.Guid,
                InUse  = false
            }, true);
            EnqueueToVisible(new ServerUnitSetChair
            {
                UnitId      = Guid,
                UnitIdChair = 0,
                WaitForUnit = false
            }, true);

            currentChairGuid = null;
        }

        /// <summary>
        /// Send <see cref="GenericError"/> to <see cref="IPlayer"/>.
        /// </summary>
        public void SendGenericError(GenericError error)
        {
            Session.EnqueueMessageEncrypted(new ServerGenericError
            {
                Error = error
            });
        }

        private readonly Dictionary<ShortcutSet, ushort> shownActionBars = [];

        /// <summary>
        /// Show the supplied action bar to the player, the buttons on it cast their spells through <see cref="CastActionBarShortcut"/>.
        /// </summary>
        public void ShowActionBar(ShortcutSet shortcutSet, ushort actionBarShortcutSetId, uint associatedUnitId)
        {
            shownActionBars[shortcutSet] = actionBarShortcutSetId;
            Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet            = shortcutSet,
                ActionBarShortcutSetId = actionBarShortcutSetId,
                AssociatedUnitId       = associatedUnitId
            });
        }

        /// <summary>
        /// Hide the supplied action bar if it is still the one shown on <paramref name="shortcutSet"/>.
        /// </summary>
        public void HideActionBar(ShortcutSet shortcutSet, ushort actionBarShortcutSetId)
        {
            if (!shownActionBars.TryGetValue(shortcutSet, out ushort shownId) || shownId != actionBarShortcutSetId)
                return;

            shownActionBars.Remove(shortcutSet);
            Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet            = shortcutSet,
                ActionBarShortcutSetId = 0
            });

            // the engineer pet bar shares its bar with bars shown by spell effects
            if (shortcutSet == ShortcutSet.PrimaryPetBar)
                CombatPetManager.UpdateActionBar();
        }

        /// <summary>
        /// Return the action bar shown on <paramref name="shortcutSet"/> with <see cref="ShowActionBar"/>.
        /// </summary>
        public ushort? GetShownActionBar(ShortcutSet shortcutSet)
        {
            return shownActionBars.TryGetValue(shortcutSet, out ushort shownId) ? shownId : null;
        }

        /// <summary>
        /// Cast the spell of a button on an action bar shown with <see cref="ShowActionBar"/>, returns false if no such bar is shown.
        /// </summary>
        public bool CastActionBarShortcut(ShortcutSet shortcutSet, uint slot, uint targetUnitId, Vector3 position)
        {
            if (!shownActionBars.TryGetValue(shortcutSet, out ushort shownId))
                return false;

            ActionBarShortcutSetEntry entry = GameTableManager.Instance.ActionBarShortcutSet.GetEntry(shownId);
            if (entry == null)
                return false;

            uint[] types   = [entry.ShortcutType00, entry.ShortcutType01, entry.ShortcutType02, entry.ShortcutType03, entry.ShortcutType04, entry.ShortcutType05,
                              entry.ShortcutType06, entry.ShortcutType07, entry.ShortcutType08, entry.ShortcutType09, entry.ShortcutType10, entry.ShortcutType11];
            uint[] objects = [entry.ObjectId00, entry.ObjectId01, entry.ObjectId02, entry.ObjectId03, entry.ObjectId04, entry.ObjectId05,
                              entry.ObjectId06, entry.ObjectId07, entry.ObjectId08, entry.ObjectId09, entry.ObjectId10, entry.ObjectId11];
            if (slot >= types.Length || objects[slot] == 0u)
                return false;

            // ShortcutType 6 and 7 are spells, see CombatPetManager
            if (types[slot] is not (6u or 7u) || GameTableManager.Instance.Spell4.GetEntry(objects[slot]) == null)
                return true;

            ISpellParameters parameters = LegacyServiceProvider.Provider.GetService<IFactory<ISpellParameters>>().Resolve();
            parameters.PrimaryTargetId        = targetUnitId;
            parameters.TargetPosition         = new Network.World.Entity.Position(position);
            parameters.UserInitiatedSpellCast = true;
            CastSpell(objects[slot], parameters);
            return true;
        }

        /// <summary>
        /// Send message to <see cref="IPlayer"/> using the <see cref="ChatChannel.System"/> channel.
        /// </summary>
        /// <param name="text"></param>
        public void SendSystemMessage(string text)
        {
            Session.EnqueueMessageEncrypted(new ServerChat
            {
                Channel = new Channel
                {
                    ChatChannelId = ChatChannelType.System
                },
                From = new Network.World.Message.Model.Shared.Identity
                {
                    Id = 0,
                    RealmId = 0,
                },
                Text = text
            });
        }

        /// <summary>
        /// Returns whether this <see cref="IPlayer"/> is allowed to summon or be added to a mount.
        /// </summary>
        public bool CanMount()
        {
            return PlatformGuid == null && pendingTeleport == null;
        }

        /// <summary>
        /// Dismounts this <see cref="IPlayer"/> from a vehicle that it's attached to
        /// </summary>
        public void Dismount()
        {
            if (PlatformGuid == null)
                return;

            // the platform can also be a moving platform (elevators, lifts) rather than a vehicle
            if ((GetVisible<IGridEntity>(PlatformGuid.Value) ?? Map?.GetEntity<IGridEntity>(PlatformGuid.Value)) is IVehicleEntity vehicle)
                vehicle.PassengerRemove(this);
            else
                SetPlatform(null);
        }

        /// <summary>
        /// Remove all entities associated with the <see cref="IPlayer"/>
        /// </summary>
        private void DestroyDependents()
        {
            // vehicle will be removed if player is the last passenger
            Dismount();

            if (VanityPetGuid != null)
            {
                IPetEntity pet = GetVisible<IPetEntity>(VanityPetGuid.Value);
                pet?.RemoveFromMap();
                VanityPetGuid = null;
            }

            RemoveControlUnit();

            PathScientistManager.DismissScanbot();
            PathSoldierManager.Cancel();
            PathExplorerManager.Cancel();

            // combat pets are summoned again if the player is added to another map
            CombatPetManager.OnRemoveFromMap();
        }

        private void RemoveControlUnit()
        {
            if (ControlGuid == null || ControlGuid == Guid)
                return;

            IWorldEntity controlled = Map.GetEntity<IWorldEntity>(ControlGuid.Value);
            controlled?.RemoveFromMap();
            SetControl(this);
        }

        /// <summary>
        /// Returns the time in seconds that has past since the last <see cref="IPlayer"/> save.
        /// </summary>
        public double GetTimeSinceLastSave()
        {
            return SaveDuration - saveTimer.Time;
        }

        /// <summary>
        /// Return <see cref="Disposition"/> between <see cref="IPlayer"/> and <see cref="Faction"/>.
        /// </summary>
        public override Disposition GetDispositionTo(Faction factionId, bool primary = true)
        {
            IFactionNode targetFaction = FactionManager.Instance.GetFaction(factionId);
            if (targetFaction == null)
                throw new ArgumentException($"Invalid faction {factionId}!");

            // find disposition based on reputation level
            Disposition? dispositionFromReputation = GetDispositionFromReputation(targetFaction);
            if (dispositionFromReputation.HasValue)
                return dispositionFromReputation.Value;

            return base.GetDispositionTo(factionId, primary);
        }

        private Disposition? GetDispositionFromReputation(IFactionNode node)
        {
            if (node == null)
                return null;

            // check if current node has required reputation
            IReputation reputation = ReputationManager.GetReputation(node.FactionId);
            if (reputation != null)
                return FactionNode.GetDisposition(FactionNode.GetFactionLevel(reputation.Amount));

            // check if parent node has required reputation
            return GetDispositionFromReputation(node.Parent);
        }

        /// <summary>
        /// Add a new <see cref="CharacterFlag"/>.
        /// </summary>
        public void SetFlag(CharacterFlag flag)
        {
            Flags |= flag;
            SendCharacterFlagsUpdated();
        }

        /// <summary>
        /// Remove an existing <see cref="CharacterFlag"/>.
        /// </summary>
        public void RemoveFlag(CharacterFlag flag)
        {
            Flags &= ~flag;
            SendCharacterFlagsUpdated();
        }

        /// <summary>
        /// Returns if supplied <see cref="CharacterFlag"/> exists.
        /// </summary>
        public bool HasFlag(CharacterFlag flag)
        {
            return (Flags & flag) != 0;
        }

        /// <summary>
        /// Send <see cref="ServerCharacterFlagsUpdated"/> to client.
        /// </summary>
        public void SendCharacterFlagsUpdated()
        {
            Session.EnqueueMessageEncrypted(new ServerCharacterFlagsUpdated
            {
                Flags = flags
            });
        }

        /// <summary>
        /// Add or update <see cref="IItemVisual"/>.
        /// </summary>
        /// <remarks>
        /// Checks for <see cref="IItemVisual"/> from <see cref="ICostume"/> if equipped for the same <see cref="ItemSlot"/> and replace if necessary.
        /// </remarks>
        public override void AddVisual(IItemVisual visual)
        {
            if (CostumeManager.CostumeIndex.HasValue)
            {
                IItemVisual costumeVisual = CostumeManager.GetItemVisual(CostumeManager.CostumeIndex.Value, visual.Slot);
                if (costumeVisual != null && costumeVisual.DisplayId.HasValue)
                    visual = costumeVisual;
            }

            base.AddVisual(visual);
        }

        protected override ServerEntityVisualUpdate BuildVisualUpdate()
        {
            // TODO: note sure if this should be handled better? Should Race and Sex be on the world entity?
            // when emitting a visual update, include player specific properties
            ServerEntityVisualUpdate update = base.BuildVisualUpdate();
            update.Race = (byte)Race;
            update.Sex  = (byte)Sex;
            return update;
        }

        /// <summary>
        /// Add a <see cref="Property"/> modifier given a <see cref="ItemSlot"/> and value.
        /// </summary>
        public void AddItemProperty(Property property, ItemSlot itemSlot, float value)
        {
            if (itemProperties.TryGetValue(property, out Dictionary<ItemSlot, float> itemDict))
            {
                if (itemDict.ContainsKey(itemSlot))
                    itemDict[itemSlot] = value;
                else
                    itemDict.Add(itemSlot, value);
            }
            else
            {
                itemProperties.Add(property, new Dictionary<ItemSlot, float>
                {
                    { itemSlot, value }
                });
            }

            CalculateProperty(property);
        }

        /// <summary>
        /// Remove a <see cref="Property"/> modifier by a item that is currently affecting this <see cref="IPlayer"/>.
        /// </summary>
        public void RemoveItemProperty(Property property, ItemSlot itemSlot)
        {
            if (itemProperties.TryGetValue(property, out Dictionary<ItemSlot, float> itemDict))
                itemDict.Remove(itemSlot);

            CalculateProperty(property);
        }

        /// <summary>
        /// Calculate the primary value for <see cref="Property"/>.
        /// </summary>
        protected override void CalculatePropertyValue(IPropertyValue propertyValue)
        {
            base.CalculatePropertyValue(propertyValue);

            if (itemProperties.TryGetValue(propertyValue.Property, out Dictionary<ItemSlot, float> properties))
                foreach (float values in properties.Values)
                    propertyValue.Value += values;
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer"/> has a <see cref="Property"/> updated.
        /// </summary>
        protected override void OnPropertyUpdate(IPropertyValue propertyValue)
        {
            messagePublisher.PublishAsync(new PlayerPropertyUpdatedMessage
            {
                Identity = Identity.ToInternalIdentity(),
                Property = propertyValue.Property,
                Value    = propertyValue.Value
            }).FireAndForgetAsync();
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> has a <see cref="Stat"/> updated.
        /// </summary>
        protected override void OnStatUpdate(IStatValue statValue, float previousValue)
        {
            messagePublisher.PublishAsync(new PlayerStatUpdatedMessage
            {
                Identity = Identity.ToInternalIdentity(),
                Stat     = statValue.Stat,
                Value    = statValue.Value
            }).FireAndForgetAsync();
        }

        /// <summary>
        /// Determine if this <see cref="IPlayer"/> can attack supplied <see cref="IUnitEntity"/>.
        /// </summary>
        public override bool CanAttack(IUnitEntity target)
        {
            if (target is IPlayer player)
            {
                if (player.PvPFlags == PvPFlag.Disabled)
                    return false;
            }

            return base.CanAttack(target);
        }

        /// <summary>
        /// Modify the health of this <see cref="IPlayer"/> by the supplied amount.
        /// </summary>
        /// <remarks>
        /// If the <see cref="DamageType"/> is <see cref="DamageType.Heal"/> amount is added to current health otherwise subtracted.
        /// </remarks>
        public override void ModifyHealth(uint amount, DamageType? type, IUnitEntity source)
        {
            base.ModifyHealth(amount, type, source);

            if (type == DamageType.Heal)
                Map.PublicEventManager.UpdateStat(this, PublicEventStat.HealingReceived, amount);
            else if (type != null)
                Map.PublicEventManager.UpdateStat(this, PublicEventStat.DamageReceived, amount);

            Session.EnqueueMessageEncrypted(new ServerPlayerHealthUpdate
            {
                UnitId = Guid,
                Health = Health,
                Mask   = type switch
                {
                    DamageType.Fall      => UpdateHealthMask.FallDamage,
                    DamageType.Suffocate => UpdateHealthMask.SuffocateDamage,
                    _                    => UpdateHealthMask.None
                }
            });

            if (Health > 0 && DeathState != null)
                OnResurrection(source);
        }

        protected override void OnDeath(IUnitEntity killer)
        {
            PathSoldierManager.OnDeath();
            base.OnDeath(killer);

            Dismount();
            RemoveControlUnit();
            CombatPetManager.OnDeath();

            UpdateRangeChecks();

            Map.PublicEventManager.UpdateStat(this, PublicEventStat.Deaths, 1);
            Map.OnDeath(this);
        }

        /// <summary>
        /// Invoked when a new <see cref="IHostileEntity"/> is added to the threat list.
        /// </summary>
        public override void OnThreatAddTarget(IHostileEntity hostile)
        {
            base.OnThreatAddTarget(hostile);

            IUnitEntity unit = GetVisible<IUnitEntity>(hostile.HatedUnitId);
            if (unit != null)
                CombatPetManager.OnOwnerThreatAdd(unit);
        }

        protected override void RewardKiller(IPlayer player)
        {
            // TODO: handle PvP rewards
        }

        protected void OnResurrection(IUnitEntity resurrector)
        {
            DeathState = null;
            RemoveControlUnit();

            UpdateRangeChecks();
        }
    }
}
