using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Games_Library
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // We can access ListViewGame here because that's the Name of our list
            // using the x:Name property in the designer.
            ListViewGame.ItemsSource = ReadCSV(@"C:\Users\Rebin\source\repos\Games-Library\Games-Library/games_list");

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource);
            view.Filter = UserFilter;
        }

        public IEnumerable<Game> ReadCSV(string fileName)
        {
            // We change file extension here to make sure it's a .csv file.
            // TODO: Error checking.
            string[] lines = File.ReadAllLines(Path.ChangeExtension(fileName, ".csv"));

            // lines.Select allows me to project each line as a Game. 
            // This will give me an IEnumerable<Game> back.
            return lines.Select(line =>
            {
                string[] data = line.Split(';');
                // We return a Game with the data in order.
                return new Game(data[0].Substring(1), data[1], Convert.ToInt32(data[3]), data[5], data[6].Remove(data[6].Length - 1, 1));
            });
        }

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        private void lvUsersColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                ListViewGame.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            ListViewGame.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
 
        }

        private void TheList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Game SelectedItem = (Game)ListViewGame.SelectedItem;

            if (SelectedItem != null)
            {
                MessageBox.Show(SelectedItem.Name);
            }
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
                return ((item as Game).Name.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ListViewGame.ItemsSource).Refresh();
        }
    }
}
