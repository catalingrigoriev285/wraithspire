using OpenTK.Windowing.Desktop;

namespace wraithspire.engine.editor
{
    internal interface IEditorModule
    {
        string Name { get; }
        bool IsVisible { get; set; }
        void Initialize(EditorUI editor, SceneManager manager);
        void Update(float deltaTime);
        void Render(GameWindow window);
    }
}
