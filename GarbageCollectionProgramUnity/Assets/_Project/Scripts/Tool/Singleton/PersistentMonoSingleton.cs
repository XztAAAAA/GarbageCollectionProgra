using UnityEngine;

namespace Core
{

    /// <summary>
    /// This singleton is persistent across scenes by calling <see cref="UnityEngine.Object.DontDestroyOnLoad(Object)"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PersistentMonoSingleton<T> : MonoSingleton<T> where T : MonoSingleton<T>
    {

        #region Protected Methods

        protected override void OnCreateSingleton()
        {
            base.OnCreateSingleton();
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public virtual void Init()
        {
        }

        #endregion

    }

}