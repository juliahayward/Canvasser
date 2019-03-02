using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace Canvasser
{
    public class Intention
    {
        public string Code { get; set; }
    }

    public partial class Elector
    {
        public string FullPN
        {
            get
            {
                return (PNs > 0) ?
                    PN + "/" + PNs : PN + "";
            }
        }

        public bool VotedInLastElection
        {
            get { return Voted2018.HasValue && Voted2018.Value; }
            set { Voted2018 = value; }
        }

        public bool PostalInLastElection
        {
            get { return Postal2018.HasValue && Postal2018.Value; }
            set { Postal2018 = value; }
        }

        public bool PostalInNextElection
        {
            get { return Postal2019.HasValue && Postal2019.Value; }
            set { Postal2019 = value; }
        }

        public bool Selected { get; set; }

        public string Annotations
        {
            get
            {
                return "17: " + Intention2017
                    + ((Voted2017.HasValue && Voted2017.Value) ? " (v)" : "")
                    + ((Postal2017.HasValue && Postal2017.Value) ? " (post)" : "")
                    + Environment.NewLine 
                    + "18: " + Intention2018
                    + ((Voted2018.HasValue && Voted2018.Value) ? " (v)" : "")
                    + ((Postal2018.HasValue && Postal2018.Value) ? " (post)" : "");
            }
        }

        public string Intention
        {
            get
            {
                if (!string.IsNullOrEmpty(Intention2018)) return Intention2018;
                if (!string.IsNullOrEmpty(Intention2017)) return Intention2017;
                if (!string.IsNullOrEmpty(Intention2016)) return Intention2016;
                if (!string.IsNullOrEmpty(Intention2015)) return Intention2015;
                if (!string.IsNullOrEmpty(Intention2014)) return Intention2014;
                if (!string.IsNullOrEmpty(Intention2013)) return Intention2013;
                return Intention2012;
            }
        }

        public SolidColorBrush PartyColour
        {
            get
            {
                return PartyColours(Intention);
            }
        }

        public SolidColorBrush PartyColour2012
        {
            get
            {
                return PartyColours(Intention2012);
            }
        }

        public SolidColorBrush PartyColour2013
        {
            get
            {
                return PartyColours(Intention2013);
            }
        }

        public SolidColorBrush PartyColour2014
        {
            get
            {
                return PartyColours(Intention2014);
            }
        }

        public SolidColorBrush PartyColour2015
        {
            get
            {
                return PartyColours(Intention2015);
            }
        }

        public SolidColorBrush PartyColour2016
        {
            get
            {
                return PartyColours(Intention2016);
            }
        }

        public SolidColorBrush PartyColour2017
        {
            get
            {
                return PartyColours(Intention2017);
            }
        }

        public SolidColorBrush PartyColour2018
        {
            get
            {
                return PartyColours(Intention2018);
            }
        }


        private SolidColorBrush PartyColours(string intention)
        {
            switch (intention)
            {
                case "D":
                case "P":
                    return Brushes.Purple;
                case "CON":
                case "CON soft":
                    return Brushes.Blue;
                case "LAB":
                case "LAB soft":
                    return Brushes.Red;
                case "LIBDEM":
                case "LIBDEM soft":
                    return Brushes.Yellow;
                case "UKIP":
                    return Brushes.Violet;
                case "GREEN":
                    return Brushes.Green;
                default:
                    return Brushes.White;
            }
        }

        public SolidColorBrush PartyTextColour
        {
            get
            {
                switch (Intention)
                {
                    case "D":
                    case "P":
                    case "CON":
                    case "CON soft":
                    case "UKIP":
                    case "GREEN":
                        return Brushes.White;
                    case "LAB":
                    case "LAB soft":
                    case "LIBDEM":
                    case "LIBDEM soft":
                    default:
                        return Brushes.Black;
                }
            }
        }
    }

    public class ElectorsVM : ObservableCollection<Elector>
    {
        private CanvasserDataContext dataDC;
        private bool ignoreEvents;

        public ElectorsVM(CanvasserDataContext context)
        {
            this.ignoreEvents = true;
            this.dataDC = context;
            var electorsList = dataDC.Electors.Where(e => e.PD != null)
                               .OrderBy(e => e.PD).ThenBy(e => e.PN).ThenBy(e => e.PNs);

            // Cannot add multiple items to ObservableCollection in single step :-(
            foreach (var e in electorsList)
            {
                this.Add(e);
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
                            dataDC.Electors.InsertOnSubmit((Elector)shipper);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        foreach (var shipper in e.OldItems)
                        {
                            dataDC.Electors.DeleteOnSubmit((Elector)shipper);
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
            if (this.dataDC != null)
            {
                this.dataDC.SubmitChanges();
            }
        }

      
    }
}
