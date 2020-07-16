namespace rpg.Singleton {
    public abstract class singleton<T>
    where T : singleton<T>, new() {
        private static T _instance = new T( );
        public static T Instance( ) {
            return _instance;
        }
    }
}