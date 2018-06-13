using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CPAS.Models;
using GalaSoft.MvvmLight.Messaging;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using CPAS.Interface;

namespace CPAS.ViewModels
{
    public enum USER
    {
        OPERATOR,
        ENGINNER,
        MANAGER
    }
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        #region MyDateTime
        private string _strViewID = "Home";
        private DateTime _myDateTime;
        private string _strUserName="";
        private int _level;
        private ObservableCollection<MessageItem> _messageCollection=new ObservableCollection<MessageItem>();
        private Dictionary<string, string> LogInfoDic = new Dictionary<string, string>();
        

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
        public int Level {
            set { if (_level != value)
                {
                    _level = value;
                    RaisePropertyChanged();
                }
            }
            get { return _level; }
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
        public RelayCommand<Tuple<string,string>> LogInCommand { get { return new RelayCommand<Tuple<string,string>>(t=> {
            Tuple<string, string> tuple = t as Tuple<string, string>;
            string usr = tuple.Item1;
            string psd = tuple.Item2;
            if(LogInfoDic.Keys.Contains(usr))
            {
                if (psd == LogInfoDic[usr])
                {
                    StrUserName = usr;
                    switch (usr)
                    {
                        case "Operator":
                            Level = 0;
                            break;
                        case "Engineer":
                            Level = 1;
                            break;
                        case "Manager":
                            Level = 2;
                            break;
                        default:
                            break;
                    }
                }

            }
        }); } }
        public RelayCommand LogOutCommand { get { return new RelayCommand(() => { Level = 0; StrUserName = "Operator"; }); } }
        #endregion

        #region Ctor
        public MainWindowViewModel()
        {
            Level = 0;
            LogInfoDic.Add("Operator","111");
            LogInfoDic.Add("Engineer", "222");
            LogInfoDic.Add("Manager", "333");
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