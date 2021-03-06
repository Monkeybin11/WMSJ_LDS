﻿using CPAS.Interface;
using CPAS.Models;
using CPAS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CPAS.Views
{
    /// <summary>
    /// UC_ParaSetting.xaml 的交互逻辑
    /// </summary>
    public partial class UC_ParaSetting : UserControl , Iauthor
    {
        public UC_ParaSetting()
        {
            InitializeComponent();
        }

       
        public int Level { get; set; }
        public static DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(int), typeof(UC_ParaSetting));
        public int GetLever()
        {
            throw new NotImplementedException();
        }
        public void SetLever(int nLever)
        {
            throw new NotImplementedException();
        }
        private void SelectThisItem_Click(object sender, RoutedEventArgs e)
        {
          
            if (PrescriptionListBox.SelectedItem != null)
                (DataContext as MainWindowViewModel).SetPrescriptionUsedCommand.Execute(PrescriptionListBox.SelectedItem);

        }
    }
}
