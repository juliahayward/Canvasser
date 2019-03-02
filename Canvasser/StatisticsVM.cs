using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Canvasser
{
    public class Statistics
    {
        public string Name { get; set; }
        public int Total { get; set; }
        public int Derek { get; set; }
        public int Con { get; set; }
        public int Lab { get; set; }
        public int Ukip { get; set; }
        public int Libdem { get; set; }
        public int Green { get; set; }

        public string DerekS { get { return (Total == 0) ? "" : Derek + " (" + Math.Round(Derek * 100.0 / Total) + "%)"; } }
        public string ConS { get { return (Total == 0) ? "" : Con + " (" + Math.Round(Con * 100.0 / Total) + "%)"; } }
        public string LabS { get { return (Total == 0) ? "" : Lab + " (" + Math.Round(Lab * 100.0 / Total) + "%)"; } }
        public string UkipS { get { return (Total == 0) ? "" : Ukip + " (" + Math.Round(Ukip * 100.0 / Total) + "%)"; } }
        public string LibdemS { get { return (Total == 0) ? "" : Libdem + " (" + Math.Round(Libdem * 100.0 / Total) + "%)"; } }
        public string GreenS { get { return (Total == 0) ? "" : Green + " (" + Math.Round(Green * 100.0 / Total) + "%)"; } }
    }

    public class StatisticsVM : ObservableCollection<Statistics>
    {
        private CanvasserDataContext dataDC;
        private bool ignoreEvents;

        public StatisticsVM(CanvasserDataContext context)
        {
            this.ignoreEvents = true;
            this.dataDC = context;

            var pdCodes = context.PollingDistricts.OrderBy(x => x.DisplayOrder).Select(x => x.PD);

            // Cannot add multiple items to ObservableCollection in single step :-(
            foreach (var pd in pdCodes)
            {
                var pdElectors = dataDC.Electors.Where(x => x.PD == pd).ToList();
                var s = new Statistics()
                {
                    Name = pd,
                    Derek = pdElectors.Where(x => (x.Intention == "D" || x.Intention == "P")).Count(),
                    Con = pdElectors.Where(x => (x.Intention == "CON" || x.Intention == "CON soft")).Count(),
                    Lab = pdElectors.Where(x => (x.Intention == "LAB" || x.Intention == "LAB soft")).Count(),
                    Libdem = pdElectors.Where(x => (x.Intention == "LIBDEM" || x.Intention == "LIBDEM soft")).Count(),
                    Ukip = pdElectors.Where(x => (x.Intention == "UKIP")).Count(),
                    Green = pdElectors.Where(x => (x.Intention == "GREEN")).Count()
                };
                s.Total = s.Derek + s.Con + s.Lab + s.Libdem + s.Ukip + s.Green;

                this.Add(s);
            }
            var totalS = new Statistics()
            {
                Name = "Grand total",
                Derek = this.Sum(x => x.Derek),
                Con = this.Sum(x => x.Con),
                Lab = this.Sum(x => x.Lab),
                Libdem = this.Sum(x => x.Libdem),
                Ukip = this.Sum(x => x.Ukip),
                Green = this.Sum(x => x.Green),
                Total = this.Sum(x => x.Total)
            };
            this.Add(totalS);

            this.ignoreEvents = false;

            
        }

    }
}
