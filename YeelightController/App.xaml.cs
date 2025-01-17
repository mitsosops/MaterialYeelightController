﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YeelightController.DependencyContainer;
using YeelightController.MVVM.ViewModel;

namespace YeelightController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {        
        public App()
        {            
             ContainerConfig.BuildServices();
        }
        private void OnStartup(object sender, StartupEventArgs e)
        {
            using (var serviceProvider = ContainerConfig.ServiceProvider)
            {
                serviceProvider.GetService<App>().Run();
            }
        }        
    }
}
