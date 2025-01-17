﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YeelightController.Helpers;
using YeelightController.MVVM.ViewModel;

namespace YeelightController.MVVM.View
{
    /// <summary>
    /// Interaction logic for DevicesView.xaml
    /// </summary>
    public partial class DevicesView : UserControl
    {
        public DevicesView()
        {
            InitializeComponent();         
        }

        private void StackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void stackPanelProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var panel = sender as StackPanel;
            if(panel.Visibility == Visibility.Visible)
            {
                btnRefresh.IsEnabled = false;
                btnTurnOff.IsEnabled = false;
                btnTurnOn.IsEnabled = false;
            }
            else
            {
                btnRefresh.IsEnabled = true;
                btnTurnOn.IsEnabled=true;
                btnTurnOff.IsEnabled=true;
            }
        }


        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Link.OpenInBrowser(e.Uri.ToString());
            e.Handled = true;
        }

    }
}
