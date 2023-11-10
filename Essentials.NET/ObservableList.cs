using System.Collections.ObjectModel;

namespace Essentials.NET;

public class ObservableList<T> : ObservableCollection<T>, IObservableList<T>, IObservableReadOnlyList<T>
{
    public ObservableList() { }

    public ObservableList(IEnumerable<T> enumerable) : base(enumerable) { }

    public ObservableList(List<T> list) : base(list) { }
}
