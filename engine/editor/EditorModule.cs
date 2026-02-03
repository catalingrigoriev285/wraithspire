using OpenTK.Windowing.Desktop;

namespace wraithspire.engine.editor
{
    internal abstract class EditorModule : IEditorModule
    {
        public abstract string Name { get; }
        public virtual string Description => "No description provided.";
        public virtual string Category => "Uncategorized";
        public virtual string Version => "1.0.0";
        
        public virtual bool IsVisible { get; set; } = true;
        
        private bool _isEnabled = true;
        public bool IsEnabled 
        { 
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (_isEnabled) OnEnable();
                    else OnDisable();
                }
            }
        }
        
        protected EditorUI? _editor;
        protected SceneManager? _manager;

        public virtual void Initialize(EditorUI editor, SceneManager manager)
        {
            _editor = editor;
            _manager = manager;
            if (IsEnabled) OnEnable();
        }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        public virtual void Update(float deltaTime) { }
        public abstract void Render(GameWindow window);
        public virtual void DrawSettings() { }
    }
}
