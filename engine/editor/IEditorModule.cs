using OpenTK.Windowing.Desktop;

namespace wraithspire.engine.editor
{
    internal interface IEditorModule
    {
        string Name { get; }
        string Description { get; }
        string Category { get; }
        string Version { get; }
        
        bool IsVisible { get; set; }
        bool IsEnabled { get; set; }
        
        void Initialize(EditorUI editor, SceneManager manager);
        void OnEnable();
        void OnDisable();
        
        void Update(float deltaTime);
        void Render(GameWindow window);
        void DrawSettings();
    }
}
