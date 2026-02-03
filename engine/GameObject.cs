using System;
using System.Collections.Generic;
using wraithspire.engine.components;

namespace wraithspire.engine
{
    public class GameObject : IDisposable
    {
        public string Name { get; set; }

        public bool IsActive = true;

        public Transform Transform { get; private set; }

        private List<Component> _components = new List<Component>();

        public GameObject(string name)
        {
            Name = name;
            Transform = AddComponent<Transform>();
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T component = new T();
            component.GameObject = this;
            _components.Add(component);
            component.Awake();
            return component;
        }

        public T? GetComponent<T>() where T : Component
        {
            foreach (var component in _components)
            {
                if (component is T t)
                {
                    return t;
                }
            }
            return null;
        }

        public void Start()
        {
            foreach (var component in _components)
            {
                component.Start();
            }
        }

        public void Update()
        {
            if (!IsActive) return;

            foreach (var component in _components)
            {
                component.Update();
            }
        }

        public void Dispose()
        {
            // Dispose logic if components need it
        }
    }
}
