using System.ComponentModel;

namespace Jasily.ViewModel
{
    public class ModelProperty<T> : NotifyPropertyChangedObject
    {
        private T _value;

        public ModelProperty(T value = default)
        {
            this._value = value;
        }

        /// <summary>
        /// Gets or set <see cref="Value"/> without riase <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        public T Value
        {
            get => this._value;
            set => this._value = value;
        }

        /// <summary>
        /// Set value and raise <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        /// <param name="value"></param>
        public void ChangeValue(T value) => this.ChangeModelProperty(ref this._value, value, nameof(this.Value));

        public static implicit operator T(ModelProperty<T> property) => property._value;
    }
}
