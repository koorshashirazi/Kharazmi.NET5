using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Kharazmi.Options
{
    public interface IOptions : INotifyPropertyChanged
    {
        bool Enable { get; set; }
        bool IsDirty { get; }
        void MakeDirty(bool value);
        bool IsValid();

        IReadOnlyCollection<ValidationResult> ValidationResults();
        void Validate();
    }

    public abstract class Options : IOptions
    {
        private static readonly List<ValidationResult> Validation;
        private bool _isDirty;


        static Options()
        {
            Validation = new();
        }
        
        protected Options()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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


       

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}