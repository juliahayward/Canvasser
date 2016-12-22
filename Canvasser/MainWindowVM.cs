using Canvasser.Extensions;
using JuliaHayward.Common.Environment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Canvasser
{
    public class MainWindowVM : VmBase
    {
        private readonly CanvasserDataContext _context;
        private readonly ObservableRangeCollection<TargetVoter> _targets;
        private readonly ObservableCollection<PollingDistrict> _districts;

        public MainWindowVM(CanvasserDataContext context)
        {
            _context = context;

            try
            {
                _targets = new ObservableRangeCollection<TargetVoter>(
                            _context.TargetVoters.Where(x => !x.Voted).OrderBy(x => x.PD)
                            .ThenBy(x => x.PN).ThenBy(x => x.PNs));

                _districts = new ObservableRangeCollection<PollingDistrict>(
                            _context.PollingDistricts.OrderBy(x => x.PD));
            }
            catch (Exception)
            {
                // Old schemas 
            }
        }

        public ObservableCollection<PollingDistrict> Districts
        {
            get
            {
                return _districts;
            }
        }

        public void RefreshTargetVoters()
        {
            _targets.Clear();
            _targets.AddRange(_context.TargetVoters.Where(x => !x.Voted).OrderBy(x => x.PD)
                            .ThenBy(x => x.PN).ThenBy(x => x.PNs));
        }

        public Visibility IsDebug
        {
            get
            {
                return JuliaEnvironment.CurrentEnvironment.IsDebug() ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private int _targetCount;
        public int TargetCount 
        { 
            get { return _targetCount; }
            set
            { 
                if (value != _targetCount) 
                { 
                    _targetCount = value; 
                    OnPropertyChanged("TargetCount");
                    OnPropertyChanged("TargetVotedPerc");
                }
            }
        }

        private int _targetVoted;
        public int TargetVoted
        {
            get { return _targetVoted; }
            set
            {
                if (value != _targetVoted)
                {
                    _targetVoted = value;  
                    OnPropertyChanged("TargetVoted");
                    OnPropertyChanged("TargetVotedPerc");
                }
            }
        }

        private string _targetVotedPerc;
        public string TargetVotedPerc
        {
            get
            {
                if (TargetCount > 0)
                    return "" + (TargetVoted * 100.0 / TargetCount).ToString("0.0") + "%";
                else
                    return "0%";
            }
        }

        private string _targetPD;
        public string TargetPD
        {
            get { return _targetPD; }
            set
            {
                if (value != _targetPD) { _targetPD = value; OnPropertyChanged("TargetPD"); }
            }
        }

        public ObservableCollection<TargetVoter> FilteredTargetVoters
        {
            get
            {
                return _targets;
            }
        }

        private string _nextNumber = "";
        public string NextNumber
        {
            get { return _nextNumber; }
            set
            {
                    if (value != _nextNumber) { _nextNumber = value; OnPropertyChanged("NextNumber"); }
            }
        }

        private ICommand _command;
        public ICommand TellerNumberCommand {
            get
            {
                if (_command == null) _command = new ActionCommand(() =>
                {
                    int pn, pns;
                    var parts = NextNumber.Split('/');
                    try
                    {
                        pn = int.Parse(parts[0]);
                        pns = (parts.Length == 1) ? 0 : int.Parse(parts[1]);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Invalid card number");
                        return;
                    }

                    var hit = _targets.FirstOrDefault(x => x.PD == TargetPD && x.PN == pn && x.PNs == pns);
                    if (hit != null)
                    {
                        _targets.Remove(hit);
                        TargetVoted++;
                    }
                    var dataHit = _context.TargetVoters.FirstOrDefault(x => x.PD == TargetPD && x.PN == pn && x.PNs == pns);
                    if (dataHit != null)
                    {
                        dataHit.Voted = true;
                        _context.SubmitChanges();
                    }
                    NextNumber = "";
                });
                return _command;
            }
        }


        private ICommand _printKOListCommand;
        public ICommand PrintKnockUpListCommand
        {
            get
            {
                if (_printKOListCommand == null) _printKOListCommand = new ActionCommand(() =>
               {
                   var printer = new KnockUpListPrinter(_context, _targets);
                   printer.Print(TargetPD);
               });
                return _printKOListCommand;
            }
        }
    }


    public class VmBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

        }
    }

        public class ActionCommand : ICommand
        {
            private readonly Action _action;

            public ActionCommand(Action action)
            {
                _action = action;
            }

            public void Execute(object parameter)
            {
                _action();
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
        }   
}
