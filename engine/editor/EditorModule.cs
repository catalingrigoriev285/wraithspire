using OpenTK.Windowing.Desktop;

namespace wraithspire.engine.editor
{
    internal abstract class EditorModule : IEditorModule
    {
        public abstract string Name { get; }
        public bool IsVisible { get; set; } = true;
        
        protected EditorUI? _editor;
        protected SceneManager? _manager;

        public virtual void Initialize(EditorUI editor, SceneManager manager)
        {
            _editor = editor;
            _manager = manager;
        }

        public virtual void Update(float deltaTime) { }
        public abstract void Render(GameWindow window);
    }
}
