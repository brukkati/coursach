using AngleSharp.Html.Dom;
using course;
using MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace coursach
{
    public class AppVM:INotifyPropertyChanged
    {
        public BindingList<Group> Groups { get; set; }
        public BindingList<Item> Items { get; set; }
        public BindingList<Item> SavedItemsList { get; set; }
        GroupDetails gd { get; set; }
        SavedItems si { get; set; }

        public int ItemPage { get; set; }

        public AppVM()
        {
            Groups = new BindingList<Group>();
            Items = new BindingList<Item>();
            SavedItemsList = new BindingList<Item>();
            //gd = new GroupDetails();
            //si = new SavedItems();
            ItemPage = 1;
        }


        private Group selectedGroup;
        public Group SelectedGroup
        {
            get { return selectedGroup; }
            set
            {
                selectedGroup = value;
                OnPropertyChanged("SelectedGroup");
            }
        }

        private Item selectedItem;
        public Item SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }


        private RelayCommand parseCommand;
        public RelayCommand ParseCommand
        {
            get
            {
                return parseCommand ??
                    (parseCommand = new RelayCommand(obj =>
                    {
                        if (SelectedGroup == null)
                        {
                            MessageBox.Show("Сначала выберите группу!");
                            return;
                        }
                        gd = new GroupDetails();
                        gd.Show();
                        selectedItem = null;
                        gd.DataContext = this;
                        GetItems();
                    }));
            }
        }

        private RelayCommand plusPageCommand;
        public RelayCommand PlusPageCommand
        {
            get
            {
                return plusPageCommand ??
                    (plusPageCommand = new RelayCommand(obj =>
                    {
                        ItemPage++;
                        Items.Clear();
                        SelectedItem = null;
                        OnPropertyChanged("ItemPage");
                        OnPropertyChanged("Items");
                        gd.PlusButton.IsEnabled = false;
                        gd.MinusButton.IsEnabled = false;
                        GetItems();
                    }
                    ));
            }
        }

        private RelayCommand minusPageCommand;
        public RelayCommand MinusPageCommand
        {
            get
            {
                return minusPageCommand ??
                    (minusPageCommand = new RelayCommand(obj =>
                    {
                        if (ItemPage == 1)
                            return;
                        ItemPage--;
                        Items.Clear();
                        SelectedItem= null;
                        OnPropertyChanged("ItemPage");
                        OnPropertyChanged("Items");
                        gd.PlusButton.IsEnabled = false;
                        gd.MinusButton.IsEnabled = false;
                        GetItems();
                    }
                    ));
            }
        }

        private RelayCommand showItemDescription;
        public RelayCommand ShowItemDescription
        {
            get
            {
                return showItemDescription ??
                    (showItemDescription = new RelayCommand(obj =>
                    {
                        if (SelectedItem != null)
                            MessageBox.Show(SelectedItem.Description);
                        else
                            MessageBox.Show("Выберите товар");
                    }));
            }
        }


        private RelayCommand saveItemToDb;
        public RelayCommand SaveItemToDb
        {
            get
            {
                return saveItemToDb ??
                    (saveItemToDb = new RelayCommand(obj =>
                    {
                        if (SelectedItem != null)
                            SaveToDb(SelectedItem);
                        else
                            MessageBox.Show("Выберите товар!");
                    }));
            }
        }

        private RelayCommand showSavedItems;
        public RelayCommand ShowSavedItems
        {
            get
            {
                return showSavedItems ??
                    (showSavedItems = new RelayCommand(obj =>
                    {
                        si = new SavedItems();
                        Items.Clear();
                        SelectedItem = null;
                        si.Show();
                        si.DataContext = this;
                        SavedItemsList = GetItemsFromDb();
                        OnPropertyChanged("SavedItemsList");
                        
                    }));
            }
        }

        private RelayCommand deleteItemFromSaved;
        public RelayCommand DeleteItemFromSaved
        {
            get
            {
                return deleteItemFromSaved ?? (
                    deleteItemFromSaved = new RelayCommand(obj =>
                    {
                        if (SelectedItem != null)
                            DeleteItemFromDb(SelectedItem);
                        else
                            MessageBox.Show("Выберите товар!");
                    }));
            }
        }

        private RelayCommand clearSavedList;
        public RelayCommand ClearSavedList
        {
            get
            {
                return clearSavedList ??
                    (clearSavedList = new RelayCommand(obj =>
                    {
                        ClearItemsDb();
                    }));
            }
        }

        private RelayCommand generateReportCommand;
        public RelayCommand GenerateReportCommand
        {
            get
            {
                return generateReportCommand ??
                    (generateReportCommand = new RelayCommand(obj =>
                    {
                        if (SelectedItem != null)
                        {
                            GenerateReport();
                            MessageBox.Show("Отчет был сформирован");
                        }
                        else
                            MessageBox.Show("Сначала выберите деталь!");
                    }
                    ));
            }
        }

        private RelayCommand generateChartCommand;
        public RelayCommand GenerateChartCommand
        {
            get
            {
                return generateChartCommand ??
                    (generateChartCommand = new RelayCommand(obj =>
                    {
                        GenerateChart();
                        MessageBox.Show("График был сформирован");
                    }));
            }
        }

        public void DeleteItemFromDb(Item i)
        {
            using(StarsstoreContext db = new StarsstoreContext())
            {
                var itemToDelete = db.Items.FirstOrDefault(o=> o.Name== i.Name && o.Price == i.Price && o.Description == i.Description);
                if (itemToDelete != null)
                    db.Items.Remove(itemToDelete);
                db.SaveChanges();
                SavedItemsList = GetItemsFromDb();
                OnPropertyChanged("SavedItemsList");
            }
        }

        public void ClearItemsDb()
        {
            using(StarsstoreContext db = new StarsstoreContext())
            {
                db.Database.ExecuteSqlCommand("TRUNCATE TABLE Items");
            }
            SavedItemsList.Clear();
            OnPropertyChanged("SavedItemsList");
        }

        public BindingList<Item> GetItemsFromDb()
        {
            var outputList = new BindingList<Item>();
            using (StarsstoreContext db = new StarsstoreContext())
            {
                foreach (Item i in db.Items)
                {
                    outputList.Add(i);
                }
            }
            return outputList;
        }

        public void SaveToDb(Item itemToSave)
        {
            gd.SaveButton.IsEnabled= false;
            using (StarsstoreContext db = new StarsstoreContext())
            {
                if (db.Items.FirstOrDefault(o => o.Name == itemToSave.Name && o.Price == itemToSave.Price && o.Description == itemToSave.Description) == null)
                {
                    db.Items.Add(itemToSave);
                    db.SaveChanges();
                    MessageBox.Show("Товар успешно сохранен");
                }
                else
                    MessageBox.Show("Этот товар уже был сохранен");
            }
            gd.SaveButton.IsEnabled = true;
        }
        public async void GetItems()
        {
            Items.Clear();
            Parser p = new Parser();
            Items = await p.ParseItems(SelectedGroup.Url, ItemPage);
            OnPropertyChanged("Items");
            gd.MinusButton.IsEnabled = true;
            gd.PlusButton.IsEnabled = true;
        }
        public async void GetGroups()
        {
            Parser p = new Parser();
            Groups = await p.ParseGroups("https://www.starsstore.ru/");
            OnPropertyChanged("Groups");
        }

        private void GenerateChart()
        {
            var excel = new ExcelGenerator();
            excel.GenerateChart(SavedItemsList  );
            excel.AddChartToReport();
        }

        private void GenerateReport()
        {
            var word = new WordGenerator(@"D:\учеба 5 семестр\ais\coursach\coursach\template.doc");

            var items = new Dictionary<string, string>
            {
                {"<ITEM>", SelectedItem.Name },
                {"<ITEMNAME>", SelectedItem.Name },
                {"<PRICE>", SelectedItem.Price },
            };
            word.Process(items);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}


