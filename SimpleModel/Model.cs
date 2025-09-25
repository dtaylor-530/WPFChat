using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace ChatterServer.Services
{
    public class Model
    {
        private SynchronizationContext current;

        public ObservableCollection<IChange> Changes { get; } = new();

        public Model()
        {
            current = SynchronizationContext.Current;
        }

        public void AddChange<T>(T change) where T : IChange
        {
            current.Post(_ =>
            {
                if (Changes.OfType<T>().LastOrDefault()?.Equals(change) == true)
                {
                    return;
                }
                Changes.Add(change);
            }, null);
        }

        public string String<T>() where T : IStringChange => Changes.ToArray().OfType<T>().LastOrDefault().Value;
        public int Int<T>() where T : IIntChange => Changes.ToArray().OfType<T>().LastOrDefault().Value;
        public bool Bool<T>() where T : IBooleanChange => Changes.ToArray().OfType<T>().LastOrDefault().Value;

        public Dictionary<string, string> Dictionary<T>()
        {
            Dictionary<string, string> _dictionary = new Dictionary<string, string>();
            foreach (var change in Changes.OfType<T>())
            {
                if (change is IDictionaryAddChange unac)
                {
                    _dictionary[unac.Key] = unac.Value;
                }
                if (change is IDictionaryRemoveChange re)
                {
                    _dictionary.Remove(re.Key);
                }
            }
            return _dictionary;
        }

        public static Model Instance { get; } = new Model();
    }


}
