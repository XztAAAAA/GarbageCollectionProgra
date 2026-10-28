using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Core
{
    public class SingletonManager : PersistentMonoSingleton<SingletonManager>
    {
        private readonly Stack<Type> stack = new();
        private readonly Dictionary<Type, ISingleton> singletons = new();

        public override void ClearSingleton()
        {
            base.ClearSingleton();
            lock (this)
            {
                while (stack.Count > 0)
                {
                    Type type = this.stack.Pop();
                    if (this.singletons.Remove(type, out ISingleton singleton))
                    {
                        singleton.DestroySingleton();
                    }
                }
            }
        }

        public T AddSingleton<T>() where T : ISingleton, new()
        {
            lock (this)
            {
                Type type = typeof(T);
                if (singletons.TryGetValue(type, out ISingleton existing))
                {
                    return (T)existing;
                }

                T singleton = new();
                singleton.Register();
                singletons.Add(type, singleton);
                stack.Push(type);
                singleton.InitializeSingleton();
                return singleton;
            }
        }

    }
}