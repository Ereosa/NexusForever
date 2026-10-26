namespace NexusForever.Game.Static.Cinematic
{
    /// <summary>
    /// Cinematic scenes, numbered after the "GCxxx" game cinematic numbers the client uses in creature, sound and model names.
    /// </summary>
    /// <remarks>
    /// The numbers up to 102 are also the ids of the Cinematic table (CinematicRace player models).
    /// Scenes without a game cinematic number in the client data are numbered from 1000.
    /// </remarks>
    public enum CinematicSceneId : ushort
    {
        // Crimson Isle
        CrimsonIsleShieldGenerator      = 7,
        CrimsonIsleDominionInvasion     = 8,
        CrimsonIsleEldanAnnihilator     = 9,
        CrimsonIsleDrakenArrival        = 10,
        CrimsonIsleChuaArrival          = 11,

        // Deradune
        DeraduneArrival                 = 12,
        DeraduneMetalMawIntro           = 16,
        DeraduneFalkrinDiplomacy        = 239,
        DeraduneZonagaIntro             = 1003,
        DeraduneBloodfeast              = 1005,

        // Algoroc
        AlgorocAugmentation             = 17,
        AlgorocWorldDestroyerDeath      = 18,
        AlgorocCannonSpawn              = 60,
        AlgorocSkullsEyeEscape          = 62,
        AlgorocJudgeKainLosesAnArm      = 267,
        AlgorocArrival                  = 141,
        AlgorocDeparture                = 197,
        AlgorocSkullsEyeArrival         = 1001,
        AlgorocSkullsEyeTakeoff         = 1002,

        // Levian Bay
        LevianBayCassianArrival         = 28,
        LevianBayMechariArrival         = 29,
        LevianBayStarCommStation        = 30,
        LevianBayElderCube              = 31,
        LevianBayEscape                 = 32,

        // Everstar Grove
        EverstarGroveMordeshArrival     = 34,
        EverstarGroveAurinArrival       = 35,
        EverstarGroveElderootAwakens    = 36,
        EverstarGroveElderootBurns      = 37,
        EverstarGroveThroughThePortal   = 175,

        // Ellevar
        EllevarHighShamanRitual         = 66,
        EllevarArrival                  = 142,
        EllevarIkthianAvengerDestroyed  = 67,
        EllevarCallidorsAugmentation    = 65,

        // Celestion
        CelestionForestFightsBack       = 68,
        CelestionCannonShootsDownShip   = 70,

        // Northern Wilds
        NorthernWildsHumanIntro         = 2,
        NorthernWildsSadieDeath         = 174,
        NorthernWildsDeparture          = 1004,

        // Transport ships
        IlliumDepartureTransport        = 145,
        IlliumDepartureAttackship       = 146,
        ThaydDepartureTransport         = 149,
        ThaydDepartureAttackship        = 150,
        FarsideArrivalExile             = 151,
        FarsideArrivalDominion          = 152,
        WilderrunArrivalExile           = 153,
        WilderrunArrivalDominion        = 154,
        WhitevaleArrivalExile           = 155,
        WhitevaleArrivalDominion        = 156,
        MalgraveArrivalExile            = 157,
        MalgraveArrivalDominion         = 158,
        GrimvaultArrivalExile           = 160,
        GrimvaultArrivalDominion        = 162,
        WhitevaleDepartureExile         = 198,
        WhitevaleDepartureDominion      = 199,
        FarsideDepartureExile           = 200,
        FarsideDepartureDominion        = 201,
        WilderrunDepartureExile         = 202,
        WilderrunDepartureDominion      = 203,
        MalgraveDepartureExile          = 204,
        MalgraveDepartureDominion       = 205,
        NorthernWildsArrival            = 210,
        IlliumArrival                   = 143,
        ThaydArrival                    = 147,
        GrimvaultCrashExile             = 159,
        GrimvaultCrashDominion          = 161,

        // New Player Experience
        NpeIntroDominion                = 289,
        NpeCombatDominion               = 290,
        NpeQuestingDominion             = 291,
        NpeHousingDominion              = 292,
        NpeDepartureBayDominion         = 293,
        NpeIntroExile                   = 294,
        NpeCombatExile                  = 295,
        NpeQuestingExile                = 296,
        NpeHousingExile                 = 297,
        NpeDepartureBayExile            = 298,
        NpeAnnihilator                  = 299
    }
}
