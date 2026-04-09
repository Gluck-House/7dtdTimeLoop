namespace TimeLoop.Models {
    public readonly struct BloodMoonStatus {
        public BloodMoonStatus(int currentDay, int dayTime, int bloodMoonStartTime, bool isScheduledBloodMoonDay) {
            CurrentDay = currentDay;
            DayTime = dayTime;
            BloodMoonStartTime = bloodMoonStartTime;
            IsScheduledBloodMoonDay = isScheduledBloodMoonDay;
        }

        public int CurrentDay { get; }

        public int DayTime { get; }

        public int BloodMoonStartTime { get; }

        public bool IsScheduledBloodMoonDay { get; }

        public bool IsBeforeBloodMoonStart => DayTime < BloodMoonStartTime;
    }
}
