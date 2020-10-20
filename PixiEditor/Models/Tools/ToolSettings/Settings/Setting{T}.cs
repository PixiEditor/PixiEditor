namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public abstract class Setting<T> : Setting
    {
        private T value;

        protected Setting(string name)
            : base(name)
        {
        }

        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                RaisePropertyChanged("Value");
            }
        }
    }
}