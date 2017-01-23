namespace ExBuddy.OrderBotTags.Behaviors.Objects
{
    public enum ShopItem
    {
#if RB_CN
        CrpDelineation = -10,

        BsmDelineation = -9,

        ArmDelineation = -8,

        GsmDelineation = -7,

        LtwDelineation = -6,

        WvrDelineation = -5,

        AlcDelineation = -4,

        CulDelineation = -3,

        CommercialEngineeringManual = -2,

        RedCrafterToken = 0,

        RedGatherToken = 1,

        CommercialSurvivalManual = 5,

        HiCordial = 6,

        BlueGatherToken = 7,

        BlueToken = BlueGatherToken,

        RedBalloon = 9,

        MagmaWorm = 10,

        FiendWorm = 11,

        BruteLeech = 12,

        CraneFly = 13,

        KukuruPowder = 30,

        BouillonCube = 31,

        BeanSauce = 32,

        BeanPaste = 33,

        GoldenApple = 34,

        SolsticeGarlic = 35,

        MatureOliveOil = 36,

        LoaghtanFilet = 37,

        PowderedMermanHorn = 38

#else
        #region BlueCrafter
        BlueCrafterToken = 0,

        CommercialEngineeringManual = 1,

        SweetCreamMilk = 2,

        StoneCheese = 3,

        HeavensEgg = 4,

        CarbonFiber = 5,

        LoaghtanFilet = 6,

        GoldenApple = 7,

        SolsticeGarlic = 8,

        MatureOliveOil = 9,

        PowderedMermanHorn = 10,

        BouillonCube = 11,

        BeanSauce = 12,

        BeanPaste = 13,

        KukuruPowder = 14,

        AdeptsHat = 15,

        AdeptsGown = 16,

        AdeptsGloves = 17,

        AdeptsHose = 18,

        AdeptsThighboots = 19,

        CrpDelineation = 20,

        BsmDelineation = 21,

        ArmDelineation = 22,

        GsmDelineation = 23,

        LtwDelineation = 24,

        WvrDelineation = 25,

        AlcDelineation = 26,

        CulDelineation = 27,
        #endregion

        #region RedCrafter
        RedCrafterToken = -5,

        GoblinCup = -4,

        CompetenceIV = -3,

        CunningIV = -2,

        CommandIV = -1,
        #endregion

        #region BlueGatherer
        BlueGatherToken = -40,

        BlueToken = BlueGatherToken,

        HiCordial = -39,

        CommercialSurvivalManual = -38,

        TrailblazersScarf = -37,

        TrailblazersVest = -36,

        TrailblazersWristguards = -35,

        TrailblazersSlops = -34,

        TrailblazersShoes = -33,

        BruteLeech = -32,

        CraneFly = -31,

        FiendWorm = -30,

        MagmaWorm = -29,

        RedBalloon = -28,

        CrownTrout = -27,

        CrownTroutHQ = -26,

        RetributionStaff = -25,

        RetributionStaffHQ = -24,

        ThiefBetta = -23,

        ThiefBettaHQ = -22,

        GoldsmithCrab = -21,

        GoldsmithCrabHQ = -20,

        Pterodactyl = -19,

        PterodactylHQ = -18,

        Eurhinosaur = -17,

        EurhinosaurHQ = -16,

        GemMarimo = -15,

        GemMarimoHQ = -14,

        Sphalerite = -13,

        SphaleriteHQ = -12,

        WindSilk = -11,

        CloudCottonBoll = -10,

        CloudCottonBollHQ = -9,

        DinosaurLeather = -8,

        RoyalMistletoe = -7,

        RoyalMistletoeHQ = -6,
        #endregion

        #region RedGatherer
        RedGatherToken = -45,

        GoblinDice = -44,

        GuerdonIV = -43,

        GuileIV = -42,

        GraspIV = -41
        #endregion
#endif
    }
}
