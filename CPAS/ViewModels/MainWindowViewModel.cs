using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CPAS.Models;
using GalaSoft.MvvmLight.Messaging;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;

namespace CPAS.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        #region MyDateTime
        private string _strViewID = "Home";
        private DateTime _myDateTime;
        private string _strUserName="";
        private ObservableCollection<MessageItem> _messageCollection=new ObservableCollection<MessageItem>();
        public DateTime MyDateTime
        {
            get { return _myDateTime; }
            set
            {
                if (_myDateTime != value)
                {
                    _myDateTime = value;
                    RaisePropertyChanged(() => MyDateTime);
                }
            }
        }
        public string StrCurViewID 
        {
            set {
                if (_strViewID != null && _strViewID != value)
                {
                    _strViewID = value;
                    RaisePropertyChanged(() => StrCurViewID);
                }
            } 
            get { return _strViewID; } 
        }
        public string StrUserName {
            set
            {
                if (_strUserName.Replace("user: ","") != value)
                {
                    _strUserName = "user: "+value;
                    RaisePropertyChanged();
                }     
            }
            get { return _strUserName; }
        }
       
        #endregion

        #region MessageCollection
        
    
        public ObservableCollection<MessageItem> MessageCollection
        {
            get { return _messageCollection; }
            set {
                if (_messageCollection != value)
                {
                    _messageCollection = value;
                    RaisePropertyChanged(() => MessageCollection);
                }
            }
        }
  
        public void ShowMessage(MessageItem msgItem)
        {
            if (MessageCollection.Count > 100)
                MessageCollection.RemoveAt(0);
            if (msgItem != null)
                MessageCollection.Add(msgItem);
        }
        #endregion

        #endregion

        #region Commands

        public RelayCommand<string> RibonCommand { get { return new RelayCommand<string>(str => StrCurViewID = str); } }
        public RelayCommand<string> ShowInfoCommand
        {
            get
            {
                return new RelayCommand<string>(str => {
                    switch (str)
                    { 
                        case "Clear":
                            MessageCollection.Clear();
                            break;
                    }
                });
            } 
        
        }
        public RelayCommand StartCommand { get {
                return new RelayCommand(() => {
                   Messenger.Default.Send<string>("Start from UI", "ShowError");
                });
            } }
        public RelayCommand StopCommand
        {
            get
            {
                return new RelayCommand(() => {
                    Messenger.Default.Send<string>("Stop from UI", "ShowInfo");
                });
            }
        }
        #endregion

        #region Ctor
        public MainWindowViewModel()
        {
            MessageCollection.Add(new MessageItem(){ MsgType=MSGTYPE.ERROR, StrMsg="ErrorInfo"});
            MessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.WARNING, StrMsg = "WarningInfo" });
            MessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.INFO, StrMsg = "Info" });
            Messenger.Default.Register<string>(this,"ShowError", str => {
                Application.Current.Dispatcher.Invoke(()=>MessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.ERROR, StrMsg = str}));
            });
            Messenger.Default.Register<string>(this,"ShowInfo", str => {
                Application.Current.Dispatcher.Invoke(() => MessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.INFO, StrMsg = str }));
            });
            Messenger.Default.Register<string>(this,"ShowWarning", str => {
                Application.Current.Dispatcher.Invoke(() => MessageCollection.Add(new MessageItem() { MsgType = MSGTYPE.WARNING, StrMsg = str }));
            });
            StrUserName = "Operator";
        }
        ~MainWindowViewModel()
        {
            Messenger.Default.Unregister<string>("ShowError");
            Messenger.Default.Unregister<string>("ShowInfo");
            Messenger.Default.Unregister<string>("ShowWarning");
        }
        #endregion

        #region Command Handlers

        private void OnRefreshDate()
        {
            MyDateTime = DateTime.Now;
        }

        private void OnRefreshPersons()
        {
            
        }

        private void OnDoNothing()
        {

        }

        private bool CanExecuteDoNothing()
        {
            return false;
        }

        #endregion

      
    }
}