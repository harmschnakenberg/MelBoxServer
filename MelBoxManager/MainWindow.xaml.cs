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

namespace MelBoxManager
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}

//TODO: 
//1) Named-Pipe-Kommunikation mit MelBoxSerer
//2) Direkte Abfragen in SQL-Datenbank
//3) Darstellung von SQL-Tabellen
//4) Formulare für Stammdatenerfassung
//5) Bereitschaftsdienste einstellen.