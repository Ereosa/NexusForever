namespace NexusForever.Game.Static.Cinematic
{
    /// <summary>
    /// Cinematic scenes, numbered after the "GCxxx" game cinematic numbers the client uses in creature, sound and model names.
    /// </summary>
    /// <remarks>
    /// The numbers up to 104 are also the ids of the Cinematic table (CinematicRace player models).
    /// </remarks>
    public enum CinematicSceneId : ushort
    {
        // Northern Wilds
        NorthernWildsHumanIntro               = 2,
        // Crimson Isle
        CrimsonIsleShieldGenerator            = 7,
        CrimsonIsleDominionInvasion           = 8,
        CrimsonIsleEldanAnnihilator           = 9,
        CrimsonIsleDrakenArrival              = 10,
        CrimsonIsleChuaArrival                = 11,
        // Deradune
        DeraduneArrival                       = 12,
        DeraduneMetalMawIntro                 = 16,
        // Algoroc
        AlgorocAugmentation                   = 17,
        AlgorocWorldDestroyerDeath            = 18,
        AlgorocCannonSpawn                    = 60,
        AlgorocSkullsEyeEscape                = 62,
        // Levian Bay
        LevianBayCassianArrival               = 28,
        LevianBayMechariArrival               = 29,
        LevianBayStarCommStation              = 30,
        LevianBayElderCube                    = 31,
        LevianBayEscape                       = 32,
        // Everstar Grove
        EverstarGroveMordeshArrival           = 34,
        EverstarGroveAurinArrival             = 35,
        EverstarGroveElderootAwakens          = 36,
        EverstarGroveElderootBurns            = 37,
        // Ellevar
        EllevarCallidorsAugmentation          = 65,
        EllevarHighShamanRitual               = 66,
        EllevarIkthianAvengerDestroyed        = 67,
        // Celestion
        CelestionForestFightsBack             = 68,
        CelestionCannonShootsDownShip         = 70,
        // Northern Wilds
        NorthernWildsSadieDeath               = 174,
        EverstarGroveThroughThePortal         = 175,
        // Deradune
        DeraduneFalkrinDiplomacy              = 239,
        // Algoroc
        AlgorocJudgeKainLosesAnArm            = 267,
        // New Player Experience
        NpeIntroDominion                      = 289,
        NpeCombatDominion                     = 290,
        NpeQuestingDominion                   = 291,
        NpeHousingDominion                    = 292,
        NpeDepartureBayDominion               = 293,
        NpeIntroExile                         = 294,
        NpeCombatExile                        = 295,
        NpeQuestingExile                      = 296,
        NpeHousingExile                       = 297,
        NpeDepartureBayExile                  = 298,
        NpeAnnihilator                        = 299
    }
}
