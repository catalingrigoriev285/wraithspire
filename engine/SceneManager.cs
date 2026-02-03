using System;
using System.Collections.Generic;
using System.Linq;

namespace wraithspire.engine
{
    internal class SceneManager : IDisposable
    {
        public Dictionary<string, Scene> Scenes { get; private set; } = new Dictionary<string, Scene>();
        public Scene? ActiveScene { get; private set; }

        public void Initialize()
        {
            // Create default scene
            CreateScene("Main");
        }

        public void CreateScene(string name)
        {
            if (Scenes.ContainsKey(name))
            {
                // Handle duplicate name or just return
                return; 
            }

            var newScene = new Scene(name);
            newScene.Initialize();
            Scenes.Add(name, newScene);

            if (ActiveScene == null)
            {
                ActiveScene = newScene;
            }
        }

        public void LoadScene(string name)
        {
            if (Scenes.ContainsKey(name))
            {
                ActiveScene = Scenes[name];
            }
        }

        public void DeleteScene(string name)
        {
            if (Scenes.ContainsKey(name))
            {
                var sceneToDelete = Scenes[name];
                
                // If deleting active scene, switch to another one if possible
                if (ActiveScene == sceneToDelete)
                {
                    if (Scenes.Count > 1)
                    {
                        var otherScene = Scenes.Keys.FirstOrDefault(k => k != name);
                        if (otherScene != null)
                            ActiveScene = Scenes[otherScene];
                    }
                    else
                    {
                        // Don't delete the last scene, or handle it appropriately
                        // For now we prevent deleting the last scene
                        return;
                    }
                }

                sceneToDelete.Dispose();
                Scenes.Remove(name);
            }
        }

        public void Dispose()
        {
            foreach (var scene in Scenes.Values)
            {
                scene.Dispose();
            }
            Scenes.Clear();
        }
    }
}
