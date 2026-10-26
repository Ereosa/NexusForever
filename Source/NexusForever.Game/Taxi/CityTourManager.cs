using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Taxi;
using NexusForever.Game.Chat;
using NexusForever.Game.Static.Chat;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Taxi
{
    /// <summary>
    /// Guided tours of Thayd and Illium (Thayd Tour 9898-9900, Illium Tour 9902-9904): the player boards the tour vehicle,
    /// rides its Spline2 route, stops at the sights while the tour guide talks, and is dropped at the end of the route.
    /// </summary>
    /// <remarks>
    /// The routes are the Spline2 splines that start on each tour vehicle, their nodes carry the stops (delay) and numbered cues (event id).
    /// The guide lines exist in the client text but nothing links them to the cues, they are assigned to the cues by the sights
    /// spawned around each stop (taxi kiosk, bank, auctioneer, ...), guessed. The flavour holograms of the tours (spotlights,
    /// celebrity holograms, icons) have no spawns and aren't shown. Ride speed is guessed, see <see cref="BaseSpeed"/>.
    /// </remarks>
    public class CityTourManager : ICityTourManager
    {
        #region Dependency Injection

        private readonly IEntityFactory entityFactory;
        private readonly ICreatureInfoManager creatureInfoManager;

        public CityTourManager(
            IEntityFactory entityFactory,
            ICreatureInfoManager creatureInfoManager)
        {
            this.entityFactory       = entityFactory;
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Speed of a tour vehicle, the node frame times of the routes only give relative speeds (0.5 near the stops, up to 1.5 between them).
        /// </summary>
        private const float BaseSpeed = 12f;
        private const float MinimumSpeed = 4f;

        /// <summary>
        /// Stops shorter than this are the vehicle settling at the start of the route.
        /// </summary>
        private const float MinimumStop = 2f;

        private record Tour(ushort QuestId, uint BoardObjectiveId, uint TourObjectiveId, ushort SplineId, uint GuideNameTextId,
            Dictionary<uint, uint[]> Lines);

        private static readonly Dictionary<uint, Tour> tours = new()
        {
            // Jakke: Spaceport Horizon, Pilgrim's Promenade, Academy Corner
            [69653] = new(9898, 18895, 18896, 22053, 648621, new()
            {
                [1] = [650145],         // city taxi
                [2] = [650928],         // mailbox
                [3] = [650151],         // shipping and transport sector
                [5] = [650152],         // XAS HQ
                [6] = [650153],         // guild registrar
                [7] = [650154],         // bank, auctioneer
                [8] = [650303]          // crafting
            }),
            // Beraxa: Traverse Tunnels, Court of Judgment, Arborian Gardens, Shindig Hollow, Fortune's Ground
            [69654] = new(9899, 18897, 18898, 22091, 648624, new()
            {
                [1] = [651189],         // dye vendor
                [2] = [651193],         // Traverse Tunnels
                [3] = [651194],         // Court of Judgment
                [4] = [651195],         // Arborian Gardens
                [5] = [651198],         // Shindig Hollow
                [6] = [651199],         // Fortune's Ground bazaar
                [7] = [651200, 651262], // auctioneer and commodities broker
                [8] = [651201]          // bank
            }),
            // Darak: Protostar Housing Initiative, Fortune's Ground, Lair of the Black Hoods, FCON Headquarters
            [69655] = new(9900, 18899, 18900, 22092, 648623, new()
            {
                [1] = [651190],         // housing
                [5] = [653984, 653985], // mount vendor
                [2] = [651205, 651557], // Elder Gem merchant
                [3] = [651206],         // Lair of the Black Hoods
                [4] = [651207, 651559], // transmat terminal
                [6] = [653986]          // FCON quartermasters
            }),
            // Aexa: Spaceport Alpha, Parade Grounds, Luminous Gardens
            [69791] = new(9902, 18912, 18913, 22113, 648622, new()
            {
                [1] = [653250],         // taxi
                [2] = [654798],         // ships
                [3] = [654806, 654808], // bank, auctioneer
                [4] = [654811],         // mailbox
                [5] = [654813, 654814], // Enigma Chamber
                [6] = [654817],         // Parade Grounds
                [9] = [654893],         // transmat terminal
                [7] = [654829, 654830], // mount vendor
                [8] = [654831]          // housing
            }),
            // Vector: Luminous Gardens, Legion's Way
            [69795] = new(9903, 18914, 18915, 22114, 648625, new()
            {
                [1] = [653249],         // crafting
                [2] = [654832],         // dye vendor
                [3] = [654833],         // Conqueror's Square
                [6] = [657164],         // training grounds
                [4] = [655360],         // quartermasters
                [5] = [655363, 655365], // auctioneer, bank
                [7] = [657167]          // Luminous Chapel
            }),
            // Crydax: Fate's Landing
            [69796] = new(9904, 18916, 18917, 22115, 648626, new()
            {
                [1] = [653248],         // guilds
                [2] = [655374],         // market district
                [3] = [655375, 655381], // Elder Gem merchant
                [4] = [655380],         // Cathedral of Vigilance
                [5] = [655382],         // bank
                [6] = [658599]          // fountains
            })
        };

        /// <summary>
        /// Start the tour of the supplied tour vehicle for <see cref="IPlayer"/>, returns false if the vehicle isn't a tour or the player can't take it.
        /// </summary>
        public bool TryStart(IPlayer player, IWorldEntity vehicle)
        {
            if (!tours.TryGetValue(vehicle.CreatureId, out Tour tour))
                return false;

            // the guides send players back to the previous tour, the vehicle only takes riders on the tour's quest
            IQuest quest = player.QuestManager.GetActiveQuest(tour.QuestId);
            IQuestObjective tourObjective = quest?.GetQuestObjective(tour.TourObjectiveId);
            if (tourObjective == null || tourObjective.IsComplete())
                return false;

            if (player.TaxiManager.FlightDestination != null || player.CinematicManager.IsPlaying)
                return false;

            List<TaxiLeg> legs = BuildLegs(tour);
            if (legs.Count == 0)
                return false;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(vehicle.CreatureId);
            if (creatureInfo == null)
                return false;

            if (player.PlatformGuid != null)
            {
                foreach (Abstract.Spell.ISpell spell in player.GetSpellsByEffect(SpellEffectType.SummonMount).ToList())
                    spell.Finish();
                player.Dismount();
            }

            IQuestObjective boardObjective = quest.GetQuestObjective(tour.BoardObjectiveId);
            if (boardObjective != null && !boardObjective.IsComplete())
                player.QuestManager.ObjectiveUpdate(tour.BoardObjectiveId, boardObjective.ObjectiveInfo.Entry.Count);

            Vector3 end = legs.Last(l => l.Points.Count > 0).Points[^1];

            // every rider gets a vehicle of their own, the one in the world stays for the next player
            var taxi = entityFactory.CreateEntity<ITaxiEntity>();
            taxi.Initialise(player, creatureInfo, legs, end, player.Rotation, p => FinishTour(p, tour));
            taxi.EnqueuePassengerAdd(player, VehicleSeatType.Passenger, 0);
            taxi.AddToMap(player.Map, legs[0].Points.Count > 0 ? legs[0].Points[0] : vehicle.Position);
            return true;
        }

        private static void FinishTour(IPlayer player, Tour tour)
        {
            IQuestObjective objective = player.QuestManager.GetActiveQuest(tour.QuestId)?.GetQuestObjective(tour.TourObjectiveId);
            if (objective != null && !objective.IsComplete())
                player.QuestManager.ObjectiveUpdate(tour.TourObjectiveId, objective.ObjectiveInfo.Entry.Count);
        }

        /// <summary>
        /// Split the tour route at its stops and cues, each leg ends on one.
        /// </summary>
        private static List<TaxiLeg> BuildLegs(Tour tour)
        {
            List<Spline2NodeEntry> nodes = GameTableManager.Instance.Spline2Node.Entries
                .Where(n => n.SplineId == tour.SplineId)
                .OrderBy(n => n.Ordinal)
                .ToList();
            SplineType type = TaxiRouteSplines.GetType(tour.SplineId);

            var legs = new List<TaxiLeg>();
            var points = new List<Vector3>();
            float length = 0f;
            float time = 0f;

            for (int i = 0; i < nodes.Count; i++)
            {
                Spline2NodeEntry node = nodes[i];
                var position = new Vector3(node.Position0, node.Position1, node.Position2);

                if (points.Count > 0 && Vector3.DistanceSquared(points[^1], position) > 0.01f)
                {
                    length += Vector3.Distance(points[^1], position);
                    // the frame time of a stop node includes its wait
                    time   += nodes[i].FrameTime - nodes[i - 1].FrameTime - (nodes[i - 1].Delay >= MinimumStop ? nodes[i - 1].Delay : 0f);
                    points.Add(position);
                }
                else if (points.Count == 0)
                    points.Add(position);

                bool stop  = node.Delay >= MinimumStop;
                bool cue   = node.EventId != 0u;
                bool last  = i == nodes.Count - 1;
                if (!stop && !cue && !last)
                    continue;

                float speed = time > 0f ? MathF.Max(MinimumSpeed, BaseSpeed * length / time) : BaseSpeed;
                tour.Lines.TryGetValue(node.EventId, out uint[] lines);
                lines ??= [];

                uint pause = stop ? (uint)(node.Delay * 1000f) : 0u;
                // several lines at one stop share its wait, each is said by its own leg without movement
                uint share = lines.Length > 1 ? pause / (uint)lines.Length : pause;

                legs.Add(new TaxiLeg(points.ToList(), type, speed, share, lines.Length > 0 ? p => Say(p, tour, lines[0]) : null));
                for (int l = 1; l < lines.Length; l++)
                {
                    uint textId = lines[l];
                    legs.Add(new TaxiLeg([], type, speed, l == lines.Length - 1 ? pause - share * (uint)l : share, p => Say(p, tour, textId)));
                }

                points = [position];
                length = 0f;
                time   = 0f;
            }

            return legs;
        }

        private static void Say(IPlayer player, Tour tour, uint textId)
        {
            string text = GameTableManager.Instance.TextEnglish.GetEntry(textId);
            if (string.IsNullOrEmpty(text) || player.PlatformGuid == null)
                return;

            // the guides talk to their rider, from the vehicle
            var builder = new ChatMessageBuilder
            {
                Type     = ChatChannelType.NPCSay,
                Text     = text,
                Guid     = player.PlatformGuid.Value,
                FromName = GameTableManager.Instance.TextEnglish.GetEntry(tour.GuideNameTextId)
            };
            player.Session.EnqueueMessageEncrypted(builder.Build());
        }
    }
}
