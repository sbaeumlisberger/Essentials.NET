using System.Collections.Specialized;
using System.ComponentModel;

namespace Essentials.NET;

public interface IObservableReadOnlyList<T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    bool Contains(T value);

    int IndexOf(T value);
}
