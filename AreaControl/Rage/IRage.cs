namespace AreaControl.Rage
{
    public interface IRage
    {
        void DisplayNotification(string message);

        void LogTrivial(string message);

        void LogTrivialDebug(string message);

        void FiberYield();
    }
}