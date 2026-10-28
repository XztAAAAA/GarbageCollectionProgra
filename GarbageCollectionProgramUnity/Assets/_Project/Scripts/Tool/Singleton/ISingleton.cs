namespace Core
{

    /// <summary>
    /// The singleton interface.
    /// </summary>
    public interface ISingleton
    {
        public void InitializeSingleton();

        public void ClearSingleton();

        public void DestroySingleton();

        public void Register();
    }

}