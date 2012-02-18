using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ApplicationCore;
using EQEmu.Spawns;

namespace SpawnsPlugin
{
    public class PackDialogViewModel : ApplicationCore.ViewModels.ViewModelBase
    {
        private int _start = 0;
        private int _end = 0;
        private DelegateCommand _packCommand;

        public int Start
        {
            get { return _start; }
            set
            {
                _start = value;
                NotifyPropertyChanged("Start");
                _packCommand.RaiseCanExecuteChanged();
            }
        }

        public int End
        {
            get { return _end; }
            set
            {
                _end = value;
                NotifyPropertyChanged("End");
                _packCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand PackCommand
        {
            get { return _packCommand; }
            set
            {
                _packCommand = value;
                NotifyPropertyChanged("PackCommand");
            }
        }

        public PackDialogViewModel(ZoneSpawns zonespawns)
        {
            if (zonespawns == null)
            {
                throw new NullReferenceException();
            }

            PackCommand = new DelegateCommand(x =>
                {
                    zonespawns.PackTable(_start, _end);

                    System.Windows.MessageBox.Show("Pack complete", "Completed");
                },
                y =>
                {
                    return _start < _end && _end - _start >= zonespawns.Spawns.Count;
                });
        }
    }
}
