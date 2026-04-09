namespace TimeLoop.Models {
    public readonly struct PlayerActivitySummary {
        public PlayerActivitySummary(int connectedPlayers, int authorizedPlayers) {
            ConnectedPlayers = connectedPlayers;
            AuthorizedPlayers = authorizedPlayers;
        }

        public int ConnectedPlayers { get; }

        public int AuthorizedPlayers { get; }
    }
}
