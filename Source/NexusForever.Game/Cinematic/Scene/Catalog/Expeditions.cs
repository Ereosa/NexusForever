using NexusForever.Game.Static.Cinematic;

namespace NexusForever.Game.Cinematic.Scene
{
    public static partial class CinematicSceneCatalog
    {
        /// <summary>
        /// Expedition (shiphand) arrivals, departures and boss intros: Outpost M-13, Infestation, Rage Logic, Space Madness and the Gauntlet.
        /// </summary>
        /// <remarks>
        /// Scene lengths and camera cuts from the camera models' sequences, sounds from the GCxxx full mix sound events.
        /// Origins: WorldLocation2 entries with no facing from the 2015 build the scenes come from; where a world camera spline
        /// exists its first node confirms the origin (M-13 GC176: spline 22290, Infestation GC180: 22296, Rage Logic GC184: 22297/22298,
        /// Gauntlet show intro: 22289). Departure origins are the arrival's twin world location (a duplicate entry next to the arrival
        /// origin, like the captured Fragment Zero and Evil from the Ether scenes), the Gauntlet departure and fight intro origins are guesses.
        /// </remarks>
        private static void AddExpeditions()
        {
            // GC176: the Steady Traveler arrives at Outpost M-13, origin WorldLocation2 49828
            Add(CinematicSceneId.OutpostM13Arrival)
                .WithDuration(34000)
                .AtOrigin(-153.70251f, -509.4488f, -125.59024f)
                .WithCamera(70287)
                .CutTo(3900, 8).CutTo(7567, 9).CutTo(12234, 10)
                .WithActors(70288, 70289, 70290, 70291, 70292)
                .WithPlayerVisual(48260); // Play_GC176_M13_Arrival_FullMix

            // GC179: Captain Milo's shuttle leaves M-13, origin WorldLocation2 49672 next to the shuttle
            Add(CinematicSceneId.OutpostM13Departure)
                .WithDuration(28667)
                .AtOrigin(-414.23517f, -487.49002f, 109.5437f)
                .WithCamera(70337)
                .CutTo(16500, 8)
                .WithActors(70338, 70339, 70340)
                .WithPlayerVisual(48261); // Play_GC179_M13_Departure_FullMix

            // GC180: Captain Tolben's ship docks with the derelict, origin WorldLocation2 49654 (camera spline 22296 starts on it)
            Add(CinematicSceneId.InfestationArrival)
                .WithDuration(26667)
                .AtOrigin(0.029314011f, -499.91666f, 11.480595f)
                .WithCamera(70347)
                .CutTo(8033, 8).CutTo(11633, 9).CutTo(19500, 10)
                .WithActors(70348, 70349, 70350, 70351, 70352)
                .WithPlayerVisual(48269); // Play_GC180_Infestation_Arrival_FullMix

            // GC182: leaving the derelict, origin WorldLocation2 49655 (the arrival's twin)
            Add(CinematicSceneId.InfestationDeparture)
                .WithDuration(20000)
                .AtOrigin(0.031110525f, -499.91666f, -7.792266f)
                .WithCamera(70353)
                .CutTo(6300, 8).CutTo(13034, 9)
                .WithActors(70354, 70355, 70356, 70357, 70358)
                .WithPlayerVisual(48270); // Play_GC182_Infestation_Departure_FullMix

            // GC183: arrival at the Positron, origin WorldLocation2 49650
            Add(CinematicSceneId.RageLogicArrival)
                .WithDuration(27667)
                .AtOrigin(-161.97589f, -837.27057f, -63.936806f)
                .WithCamera(70039)
                .CutTo(7467, 8).CutTo(12334, 9)
                .WithActors(70040, 70041)
                .WithPlayerVisual(48271); // Play_GC183_RageLogic_Arrival_FullMix

            // GC184: Axiom's boss intro, origin WorldLocation2 49649 (camera splines 22297 and 22298 start on it)
            Add(CinematicSceneId.RageLogicBossIntro)
                .WithDuration(15667)
                .AtOrigin(-287.99722f, -709.83466f, -32.005035f)
                .WithCamera(70515)
                .CutTo(7833, 8).CutTo(12400, 9)
                .WithActors(70516, 70517)
                .WithPlayerVisual(48371); // Play_GC184_RageLogic_BossIntro_FullMix

            // GC185: leaving the Positron, origin WorldLocation2 49651 (the arrival's twin)
            Add(CinematicSceneId.RageLogicDeparture)
                .WithDuration(22000)
                .AtOrigin(-161.97589f, -837.27057f, -63.936806f)
                .WithCamera(70063)
                .CutTo(10400, 8)
                .WithActors(70064, 70065, 70066, 70067)
                .WithPlayerVisual(48272); // Play_GC185_RageLogic_Departure_FullMix

            // GC186: arrival at Space Station Venture, origin WorldLocation2 49680
            Add(CinematicSceneId.SpaceMadnessArrival)
                .WithDuration(27667)
                .AtOrigin(-12.190992f, 5.0815725f, 337.75552f)
                .WithCamera(70207)
                .CutTo(8567, 8).CutTo(12633, 9).CutTo(20500, 10)
                .WithActors(70208, 70209, 70210, 70211, 70212)
                .WithPlayerVisual(48273); // Play_GC186_SpaceMadness_Arrival_FullMix

            // GC189: leaving Space Station Venture, origin WorldLocation2 49681 (the arrival's twin)
            Add(CinematicSceneId.SpaceMadnessDeparture)
                .WithDuration(20000)
                .AtOrigin(-10.003754f, 5.0815725f, 335.42825f)
                .WithCamera(70343)
                .CutTo(6767, 8).CutTo(13200, 9)
                .WithActors(70344, 70345, 70346)
                .WithPlayerVisual(48274); // Play_GC189_SpaceMadness_Departure_FullMix

            // the Gauntlet show intro (actors named GC176, sound GC192), origin WorldLocation2 49796 (camera spline 22289 starts on it)
            Add(CinematicSceneId.GauntletShowIntro)
                .WithDuration(32000)
                .AtOrigin(-1006.8102f, 2.6173704f, 1184.2644f)
                .WithCamera(70296)
                .CutTo(16700, 8).CutTo(23333, 9)
                .WithActors(70297, 70298, 70299, 70300, 70380)
                .WithPlayerVisual(48425); // Play_GC192_Gauntlet_Arrival_FullMix

            // the Gauntlet departure (actors named GC176, no sound mix in the data), origin guessed: WorldLocation2 49797
            Add(CinematicSceneId.GauntletDeparture)
                .WithDuration(20000)
                .AtOrigin(-1007.1242f, 1.0357027f, 854.16925f)
                .WithCamera(70318)
                .CutTo(5033, 8).CutTo(8867, 9).CutTo(13667, 10)
                .WithActors(70319, 70320, 70321);

            // GC193: the Gauntlet fight intro with Brick, origin guessed: WorldLocation2 49805
            Add(CinematicSceneId.GauntletFightIntro)
                .WithDuration(27667)
                .AtOrigin(-1007.36505f, -0.98851585f, 881.0639f)
                .WithCamera(70545)
                .CutTo(3033, 8).CutTo(4900, 9).CutTo(8800, 10).CutTo(10667, 11).CutTo(11967, 12).CutTo(13667, 13).CutTo(14933, 14).CutTo(19467, 15).CutTo(24467, 16)
                .WithActors(70546, 70907)
                .WithPlayerVisual(48426); // Play_GC193_Gauntlet_BossIntro_FullMix
        }
    }
}
