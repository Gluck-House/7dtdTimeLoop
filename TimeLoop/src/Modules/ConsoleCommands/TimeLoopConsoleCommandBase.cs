namespace TimeLoop.Modules.ConsoleCommands {
    public abstract class TimeLoopConsoleCommandBase : ConsoleCmdAbstract {
        protected abstract string GetHelpText();

        public sealed override string GetHelp() {
            return GetHelpText();
        }

        public sealed override string getHelp() {
            return GetHelpText();
        }
    }
}
