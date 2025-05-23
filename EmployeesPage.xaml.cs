using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Azhin222
{
    /// <summary>
    /// Страница для работы с данными сотрудников
    /// </summary>
    public partial class СотрудникиPage : Page
    {
        private Entities1 _context;

        /// <summary>
        /// Коллекция сотрудников для отображения в DataGrid
        /// </summary>
        public ObservableCollection<Сотрудник> EmployeeList { get; set; }

        private Сотрудник _selectedEmployee;

        /// <summary>
        /// Инициализирует новый экземпляр страницы сотрудников
        /// </summary>
        public СотрудникиPage()
        {
            InitializeComponent();
            _context = new Entities1();
            LoadData();
            DataContext = this;
        }

        /// <summary>
        /// Загружает данные сотрудников из базы данных
        /// </summary>
        private void LoadData()
        {
            EmployeeList = new ObservableCollection<Сотрудник>(_context.Сотрудник.ToList());
            EmployeeGrid.ItemsSource = EmployeeList;
        }

        /// <summary>
        /// Обработчик изменения выбора в комбобоксе сортировки
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = SortComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                switch (selectedItem.Content.ToString())
                {
                    case "ID Сотрудника":
                        SortById();
                        break;
                    case "Фамилия":
                        SortByLastName();
                        break;
                    case "Без сортировки":
                        LoadData();
                        break;
                }
            }
        }

        /// <summary>
        /// Сортирует сотрудников по ID
        /// </summary>
        private void SortById()
        {
            var sorted = EmployeeList.OrderBy(emp => emp.ID_Сотрудника).ToList();
            EmployeeList.Clear();
            foreach (var emp in sorted)
                EmployeeList.Add(emp);
        }

        /// <summary>
        /// Сортирует сотрудников по фамилии
        /// </summary>
        private void SortByLastName()
        {
            var sorted = EmployeeList.OrderBy(emp => emp.Фамилия).ToList();
            EmployeeList.Clear();
            foreach (var emp in sorted)
                EmployeeList.Add(emp);
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => FilterSearch();

        /// <summary>
        /// Обработчик нажатия кнопки поиска
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void SearchButton_Click(object sender, RoutedEventArgs e) => FilterSearch();

        /// <summary>
        /// Фильтрует сотрудников по введенному тексту
        /// </summary>
        private void FilterSearch()
        {
            string text = SearchBox.Text.ToLower();
            var filtered = _context.Сотрудник
                .Where(emp =>
                    (emp.Имя ?? "").ToLower().Contains(text) ||
                    (emp.Фамилия ?? "").ToLower().Contains(text) ||
                    (emp.Должность ?? "").ToLower().Contains(text) ||
                    (emp.Контактные_данные ?? "").ToLower().Contains(text))
                .ToList();

            EmployeeList.Clear();
            foreach (var emp in filtered)
                EmployeeList.Add(emp);
        }

        /// <summary>
        /// Обработчик нажатия кнопки добавления сотрудника
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(IdBox.Text, out int newId) || newId < 1)
            {
                MessageBox.Show("Некорректный ID", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_context.Сотрудник.Any(emp => emp.ID_Сотрудника == newId))
            {
                MessageBox.Show("ID уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newEmp = new Сотрудник
            {
                ID_Сотрудника = newId,
                Имя = FirstNameBox.Text,
                Фамилия = LastNameBox.Text,
                Должность = PositionBox.Text,
                Контактные_данные = ContactBox.Text
            };

            _context.Сотрудник.Add(newEmp);
            _context.SaveChanges();
            EmployeeList.Add(newEmp);

            MessageBox.Show("Сотрудник успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработчик нажатия кнопки редактирования сотрудника
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null)
            {
                MessageBox.Show("Выберите сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _selectedEmployee.Имя = FirstNameBox.Text;
            _selectedEmployee.Фамилия = LastNameBox.Text;
            _selectedEmployee.Должность = PositionBox.Text;
            _selectedEmployee.Контактные_данные = ContactBox.Text;

            _context.SaveChanges();
            EmployeeGrid.Items.Refresh();

            MessageBox.Show("Запись успешно отредактирована!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработчик нажатия кнопки удаления сотрудника
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null)
            {
                MessageBox.Show("Выберите сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить данного сотрудника?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _context.Сотрудник.Remove(_selectedEmployee);
                _context.SaveChanges();
                EmployeeList.Remove(_selectedEmployee);

                MessageBox.Show("Сотрудник успешно удалён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки сохранения изменений
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Сохранено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик изменения выбранного сотрудника в DataGrid
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Данные события</param>
        private void EmployeeGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeeGrid.SelectedItem is Сотрудник selected)
            {
                _selectedEmployee = selected;
                IdBox.Text = selected.ID_Сотрудника.ToString();
                FirstNameBox.Text = selected.Имя;
                LastNameBox.Text = selected.Фамилия;
                PositionBox.Text = selected.Должность;
                ContactBox.Text = selected.Контактные_данные;
            }
        }
    }
}