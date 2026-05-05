using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WellifyApp
{
    public class CalendarDay : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _hasRecord;
        private bool _isComplete;

        public DateTime Date { get; set; }
        public string DayNumber => IsEmpty ? "" : Date.Day.ToString();
        public bool IsToday { get; set; }
        public bool IsEmpty { get; set; }
        public bool IsNotShowing => !IsEmpty;

        // Has record
        public bool HasRecord
        {
            get => _hasRecord;
            set
            {
                if (_hasRecord != value)
                {
                    _hasRecord = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowMissingDot));
                    OnPropertyChanged(nameof(ShowPartialDot));
                }
            }
        }

        // check if record is complete
        public bool IsComplete
        {
            get => _isComplete;
            set
            {
                if (_isComplete != value)
                {
                    _isComplete = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowPartialDot));
                }
            }
        }

        // 🔴 No record
        public bool ShowMissingDot =>
            !HasRecord &&
            Date.Date < DateTime.UtcNow.AddHours(8).Date &&
            !IsEmpty;

        // 🟡 Partial record
        public bool ShowPartialDot =>
            HasRecord &&
            !IsComplete &&
            Date.Date < DateTime.UtcNow.AddHours(8).Date &&
            !IsEmpty;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}