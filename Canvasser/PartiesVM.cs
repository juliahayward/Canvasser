using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Canvasser
{
    public class PartiesVM : ObservableCollection<Party>
    {
        private CanvasserDataContext _context;
        private bool ignoreEvents;

        public PartiesVM(CanvasserDataContext context)
        {
            this.ignoreEvents = true;
            this._context = context;
            // Cannot add multiple items to ObservableCollection in single step :-(
            foreach (var item in _context.Parties.OrderBy(x => x.Name))
            {
                this.Add(item);
            }
            this.ignoreEvents = false;
        }
        
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!ignoreEvents)
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (var shipper in e.NewItems)
                        {
                            _context.Parties.InsertOnSubmit((Party)shipper);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        foreach (var shipper in e.OldItems)
                        {
                            _context.Parties.DeleteOnSubmit((Party)shipper);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        break;
                    default:
                        break;
                }
            }
            base.OnCollectionChanged(e);
        }

        public void Save()
        {
            if (this._context != null)
            {
                this._context.SubmitChanges();
            }
        }
    }
}
