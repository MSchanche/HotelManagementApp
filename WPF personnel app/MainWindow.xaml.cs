using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using HotelRegistrationWebsite.Models; // Use your actual models namespace

namespace WPF_personnel_app
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly string _connectionString =
            "Server=tcp:fenrirdev.database.windows.net,1433;Initial Catalog=HotelDatabase;Persist Security Info=False;User ID=FenrirDev;Password=aEF.f27UwyBP;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        private string _selectedRole;
        private ObservableCollection<Messages> _messages = new();
        private Messages _selectedMessage;
        private string _updateText;
        private int _currentEmployeeId = 1; // Default value, should be set based on login
        private ObservableCollection<MessageEntries> _messageEntries = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged(nameof(SelectedRole));
                LoadMessages();
            }
        }

        public ObservableCollection<Messages> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        public Messages SelectedMessage
        {
            get => _selectedMessage;
            set
            {
                _selectedMessage = value;
                OnPropertyChanged(nameof(SelectedMessage));
                LoadMessageEntries(); // Add this call to load entries when a message is selected
            }
        }

        public string UpdateText
        {
            get => _updateText;
            set
            {
                _updateText = value;
                OnPropertyChanged(nameof(UpdateText));
            }
        }

        public ObservableCollection<MessageEntries> MessageEntries
        {
            get => _messageEntries;
            set
            {
                _messageEntries = value;
                OnPropertyChanged(nameof(MessageEntries));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Test database connection at startup
            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Database connection error: {ex.Message}\n\nThe application will still open but functionality may be limited.",
                    "Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RoleSelection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem selectedItem)
            {
                SelectedRole = selectedItem.Content.ToString();
            }
        }

        private void LoadMessages()
        {
            if (string.IsNullOrEmpty(SelectedRole))
                return;

            Messages.Clear();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                // Map role selection to WorkTypeID
                int workTypeId = SelectedRole switch
                {
                    "Cleaning" => GetWorkTypeIdByName(connection, "cleaning"),
                    "Maintenance" => GetWorkTypeIdByName(connection, "maintenance"),
                    "Room Service" => GetWorkTypeIdByName(connection, "room service"),
                    _ => -1
                };
                if (workTypeId == -1)
                    return;

                // Get message status IDs for "New" and "Pending"
                int newStatusId = GetMessageStatusIdByName(connection, "new");
                int pendingStatusId = GetMessageStatusIdByName(connection, "pending");

                string query = @"
                    SELECT m.messageID, b.bookingID, m.roomID, m.accountID, m.workTypeID, 
                        m.messageStatusID, m.creationTime, r.roomName, ms.messageStatusName, 
                        wt.workTypeName
                    FROM Hotels.Messages m
                    JOIN Hotels.Room r ON m.roomID = r.roomID
                    JOIN Hotels.MessageStatusTypes ms ON m.messageStatusID = ms.messageStatusID
                    JOIN Hotels.WorkTypes wt ON m.workTypeID = wt.workTypeID
                    LEFT JOIN Hotels.Bookings b ON m.bookingID = b.bookingID
                    WHERE m.workTypeID = @WorkTypeID AND 
                        (m.messageStatusID = @NewStatusID OR 
                        (m.messageStatusID = @PendingStatusID AND m.accountID = @AccountID))
                    ORDER BY m.creationTime DESC";

                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@WorkTypeID", workTypeId);
                command.Parameters.AddWithValue("@NewStatusID", newStatusId);
                command.Parameters.AddWithValue("@PendingStatusID", pendingStatusId);
                command.Parameters.AddWithValue("@AccountID", _currentEmployeeId);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Messages.Add(new Messages
                    {
                        MessageID = reader.GetInt32(reader.GetOrdinal("messageID")),
                        BookingID = reader.IsDBNull(reader.GetOrdinal("bookingID"))
                            ? null
                            : (int?)reader.GetInt32(reader.GetOrdinal("bookingID")),
                        RoomID = reader.GetInt32(reader.GetOrdinal("roomID")),
                        AccountID = reader.IsDBNull(reader.GetOrdinal("accountID"))
                            ? null
                            : (int?)reader.GetInt32(reader.GetOrdinal("accountID")),
                        WorkTypeID = reader.GetInt32(reader.GetOrdinal("workTypeID")),
                        MessageStatusID = reader.GetInt32(reader.GetOrdinal("messageStatusID")),
                        CreationTime = reader.GetDateTime(reader.GetOrdinal("creationTime")),
                        // Set the navigation properties with partial data
                        Room = new Room { RoomName = reader.GetString(reader.GetOrdinal("roomName")) },
                        MessageStatusTypes = new MessageStatusTypes
                        { MessageStatusName = reader.GetString(reader.GetOrdinal("messageStatusName")) },
                        WorkTypes = new WorkTypes { WorkTypeName = reader.GetString(reader.GetOrdinal("workTypeName")) }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading messages: {ex.Message}", "Database Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private int GetWorkTypeIdByName(SqlConnection connection, string workTypeName)
        {
            string query = "SELECT workTypeID FROM Hotels.WorkTypes WHERE workTypeName = @WorkTypeName";
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@WorkTypeName", workTypeName);

            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        private int GetMessageStatusIdByName(SqlConnection connection, string statusName)
        {
            string query =
                "SELECT messageStatusID FROM Hotels.MessageStatusTypes WHERE messageStatusName = @StatusName";
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@StatusName", statusName);

            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        private void LoadMessageEntries()
        {
            if (SelectedMessage == null)
                return;

            MessageEntries.Clear();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    SELECT me.messageEntryID, me.messageID, me.accountID, me.creationTime, me.entryContent,
                       a.name AS AccountName
                    FROM Hotels.MessageEntries me
                    LEFT JOIN Hotels.Account a ON me.accountID = a.accountID
                    WHERE me.messageID = @MessageID
                    ORDER BY me.creationTime DESC";

                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@MessageID", SelectedMessage.MessageID);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    MessageEntries.Add(new MessageEntries
                    {
                        MessageEntryID = reader.GetInt32(reader.GetOrdinal("messageEntryID")),
                        MessageID = reader.GetInt32(reader.GetOrdinal("messageID")),
                        AccountID = reader.IsDBNull(reader.GetOrdinal("accountID"))
                            ? null
                            : (int?)reader.GetInt32(reader.GetOrdinal("accountID")),
                        CreationTime = reader.GetDateTime(reader.GetOrdinal("creationTime")),
                        EntryContent = reader.GetString(reader.GetOrdinal("entryContent")),
                        // Set the account with partial data
                        Account = new Account
                        {
                            Name = reader.IsDBNull(reader.GetOrdinal("AccountName"))
                                ? "Unknown"
                                : reader.GetString(reader.GetOrdinal("AccountName"))
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading message entries: {ex.Message}", "Database Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void StartTask_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMessage == null || SelectedMessage.MessageStatusTypes.MessageStatusName != "new")
                return;

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                // Get the status ID for "Pending"
                int pendingStatusId = GetMessageStatusIdByName(connection, "pending");

                string query = @"
                    UPDATE Hotels.Messages 
                    SET messageStatusID = @StatusID, accountID = @AccountID 
                    WHERE messageID = @MessageID";

                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@StatusID", pendingStatusId);
                command.Parameters.AddWithValue("@AccountID", _currentEmployeeId);
                command.Parameters.AddWithValue("@MessageID", SelectedMessage.MessageID);
                command.ExecuteNonQuery();

                // Add an entry showing the task was started
                string entryQuery = @"
                    INSERT INTO Hotels.MessageEntries (messageID, accountID, creationTime, entryContent)
                    VALUES (@MessageID, @AccountID, @CreationTime, @EntryContent)";

                using SqlCommand entryCommand = new(entryQuery, connection);
                entryCommand.Parameters.AddWithValue("@MessageID", SelectedMessage.MessageID);
                entryCommand.Parameters.AddWithValue("@AccountID", _currentEmployeeId);
                entryCommand.Parameters.AddWithValue("@CreationTime", DateTime.Now);
                entryCommand.Parameters.AddWithValue("@EntryContent", "Task started");
                entryCommand.ExecuteNonQuery();

                MessageBox.Show("Task started successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh the messages
                LoadMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting task: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMessage == null || SelectedMessage.MessageStatusTypes.MessageStatusName != "pending" || string.IsNullOrWhiteSpace(UpdateText))
                return;

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                // Get the status ID for "Resolved"
                int resolvedStatusId = GetMessageStatusIdByName(connection, "resolved");

                // Update the message status
                string updateMessageQuery = @"
                    UPDATE Hotels.Messages 
                    SET messageStatusID = @StatusID
                    WHERE messageID = @MessageID";

                using (SqlCommand command = new(updateMessageQuery, connection))
                {
                    command.Parameters.AddWithValue("@StatusID", resolvedStatusId);
                    command.Parameters.AddWithValue("@MessageID", SelectedMessage.MessageID);
                    command.ExecuteNonQuery();
                }

                // Add a message entry with the update text
                string addEntryQuery = @"
                    INSERT INTO Hotels.MessageEntries (messageID, accountID, creationTime, entryContent)
                    VALUES (@MessageID, @AccountID, @CreationTime, @EntryContent)";

                using (SqlCommand command = new(addEntryQuery, connection))
                {
                    command.Parameters.AddWithValue("@MessageID", SelectedMessage.MessageID);
                    command.Parameters.AddWithValue("@AccountID", _currentEmployeeId);
                    command.Parameters.AddWithValue("@CreationTime", DateTime.Now);
                    command.Parameters.AddWithValue("@EntryContent", UpdateText);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Task completed successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateText = "";
                LoadMessages();
                LoadMessageEntries();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error completing task: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMessage == null || string.IsNullOrWhiteSpace(UpdateText))
                return;

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                string query = @"
                    INSERT INTO Hotels.MessageEntries (messageID, accountID, creationTime, entryContent)
                    VALUES (@MessageID, @AccountID, @CreationTime, @EntryContent)";

                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@MessageID", SelectedMessage.MessageID);
                command.Parameters.AddWithValue("@AccountID", _currentEmployeeId);
                command.Parameters.AddWithValue("@CreationTime", DateTime.Now);
                command.Parameters.AddWithValue("@EntryContent", UpdateText);
                command.ExecuteNonQuery();

                MessageBox.Show("Update added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateText = "";

                // Refresh entries
                LoadMessageEntries();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding update: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
