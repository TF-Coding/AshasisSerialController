using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SerialNet
{
   public  class SimulatorData: INotifyPropertyChanged, IList<SimData>
    {
        private List<SimData> _col;
        public SimulatorData() { this._col = new List<SimData>(); }
        public SimulatorData(int capacity) {this._col = new List<SimData>(capacity); }
        public SimulatorData(IEnumerable<SimData> collection) {this._col = new List<SimData>(collection); }

        public event PropertyChangedEventHandler PropertyChanged;
        public void ForEach(Action<SimData> action)
        {
            this._col.ForEach(action);
        }
        public int IndexOf(SimData item)
        {
            return this._col.IndexOf(item);
        }

        public void Insert(int index, SimData item)
        {
            this._col.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this._col.RemoveAt(index);
        }

        public SimData this[int index]
        {
            get
            {
                return this._col[index];
            }
            set
            {
                this._col[index] = value;
            }
        }

        public void Add(SimData item)
        {
            this._col.Add(item);
        }

        public void Clear()
        {
            this._col.Clear();
        }

        public bool Contains(SimData item)
        {
            return this._col.Contains(item);
        }

        public void CopyTo(SimData[] array, int arrayIndex)
        {
            this._col.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this._col.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(SimData item)
        {
            return this._col.Remove(item);
        }

        public IEnumerator<SimData> GetEnumerator()
        {
            return this._col.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._col.GetEnumerator();
        }
    }
}
