using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MilkManufacture
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Entities.Manufacture_bdEntities1 Context
        { get; } = new Entities.Manufacture_bdEntities1();

        public static Entities.User CurrentUser = null;
    }
}
