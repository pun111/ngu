using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;


namespace Ngu
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propName = "") 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        //protected virtual void OnPropertyChanged(object currentValue, [CallerMemberName] string propName = "", string message = null)
        //                                                => PropertyChanged?.Invoke(this, new EnhancedPropertyChangedEventArgs(currentValue, propName, message: message));
        //protected virtual void OnPropertyChanged(object currentValue, object previousValue, [CallerMemberName] string propName = "", string message = null)
        //                                                => PropertyChanged?.Invoke(this, new EnhancedPropertyChangedEventArgs(currentValue: currentValue, previousValue: previousValue,
        //                                                    propertyName: propName, message: message));
        //protected virtual void                      OnPropertyChanged<T>(T currentValue, [CallerMemberName] string propName = "") 
        //                                                => PropertyChanged?.Invoke(this, new EnhancedPropertyChangedEventArgs<T>(currentValue, propName));
        //protected void                              OnPropertyChanged<T>(T currentValue, T previousValue, [CallerMemberName] string propName = "")
        //                                                => PropertyChanged?.Invoke(this, new EnhancedPropertyChangedEventArgs<T>(currentValue, previousValue, propName));
    }
}
