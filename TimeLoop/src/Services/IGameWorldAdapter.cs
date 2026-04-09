namespace TimeLoop.Services {
    public interface IGameWorldAdapter {
        ulong GetWorldTime();
        void SetWorldTime(ulong worldTime);
        void AdvanceWorldTime(ulong delta);
        int GetCurrentDay();
        int GetDayTime();
        double GetUnscaledTime();
    }
}
