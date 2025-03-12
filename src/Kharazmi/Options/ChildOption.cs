using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Kharazmi.Options
{
    public interface IChildOption: INotifyPropertyChanged
    {
        bool Enable { get; set; }
        bool IsDirty { get; }
        string OptionKey { get; }
        void MakeDirty(bool value);
        bool IsValid();
        IReadOnlyCollection<ValidationResult> ValidationResults();
        void Validate();
    }

    public abstract class ChildOption : IChildOption
    {
        private static readonly List<ValidationResult> Validation = new();
        private bool _isDirty;

        protected ChildOption()
        {
            OptionKey = GetType().Name;
        }

        public bool Enable { get; set; }

        [System.Text.Json.Serialization.JsonIgnore, JsonIgnore]
        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (value == _isDirty) return;
                _isDirty = value;
                OnPropertyChanged();
            }
        }

        public string OptionKey { get; set; }
        public void MakeDirty(bool value) => IsDirty = value;

        public bool IsValid() => Validation.Count == 0;

        public IReadOnlyCollection<ValidationResult> ValidationResults() => Validation;

        protected void AddValidation(string errorMessage, params string[] memberName)
            => Validation.Add(new ValidationResult(errorMessage, memberName));

        protected void AddValidation(ValidationResult value)
            => Validation.Add(value);

        protected void AddValidations(IEnumerable<ValidationResult> values)
            => Validation.AddRange(values);

        public abstract void Validate();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}