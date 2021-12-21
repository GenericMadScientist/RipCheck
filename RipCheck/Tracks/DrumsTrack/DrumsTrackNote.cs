namespace RipCheck
{
    enum DrumsTrackNote : byte
    {
        // Does not include flam markers
        // Inverse order for easier referencing of charts in a piano-roll view
        Roll2Lane = 127,
        Roll1Lane = 126,
        BRE1 = 124,
        BRE2 = 123,
        BRE3 = 122,
        BRE4 = 121,
        BRE5 = 120,
        Overdrive = 116,
        GreenTomMarker = 112,
        BlueTomMarker = 111,
        YellowTomMarker = 110,
        ScoreDuelPlayer2 = 106,
        ScoreDuelPlayer1 = 105,
        Solo = 103,

        Expert5LaneGreen = 101,
        Expert4Green5Orange = 100,
        ExpertBlue = 99,
        ExpertYellow = 98,
        ExpertRed = 97,
        ExpertKick = 96,
        ExpertPlusKick = 95,

        Hard5LaneGreen = 89,
        Hard4Green5Orange = 88,
        HardBlue = 87,
        HardYellow = 86,
        HardRed = 85,
        HardKick = 84,

        Medium5LaneGreen = 77,
        Medium4Green5Orange = 76,
        MediumBlue = 75,
        MediumYellow = 74,
        MediumRed = 73,
        MediumKick = 72,

        Easy5LaneGreen = 65,
        Easy4Green5Orange = 64,
        EasyBlue = 63,
        EasyYellow = 62,
        EasyRed = 61,
        EasyKick = 60,

        FloorTomRH = 51,
        FloorTomLH = 50,
        Tom2RH = 49,
        Tom2LH = 48,
        Tom1RH = 47,
        Tom1LH = 46,
        Crash2SoftLH = 45,
        Crash2HardLH = 44,
        RideLH = 43,
        RideRH = 42,
        Crash2Choke = 41,
        Crash1Choke = 40,
        Crash2SoftRH = 39,
        Crash2HardRH = 38,
        Crash1SoftRH = 37,
        Crash1HardRH = 36,
        Crash1SoftLH = 35,
        Crash1HardLH = 34,
        OtherPercussionRH = 32,
        HiHatRH = 31,
        HiHatLH = 30,
        SnareSoftRH = 29,
        SnareSoftLH = 28,
        SnareHardRH = 27,
        SnareHardLH = 26,
        HiHatOpen = 25,
        KickRF = 24
    }

    enum DrumsTrackVelocity
    {
        Accent = 127,
        Ghost = 1
    }
}