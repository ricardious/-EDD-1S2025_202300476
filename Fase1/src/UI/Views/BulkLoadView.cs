using System;
using System.IO;
using Gtk;
using Newtonsoft.Json.Linq;
using AutoGestPro.src.UI.Common;
using AutoGestPro.src.Services;


namespace AutoGestPro.src.UI.Views
{
    public class BulkLoadView : Box
    {
        // Private fields for services
        private readonly UserService userService;
        private readonly VehicleService vehicleService;
        private readonly SparePartsService sparePartsService;

        // UI components
        private ComboBoxText cmbEntityType;
        private Button btnSelectFile;
        private Button btnLoadData;
        private Label lblLoadStatus;
        private ScrolledWindow swPreview;
        private TextView txtPreview;

        // State
        private string selectedFilePath;

        // Event handlers
        public event EventHandler<MessageEventArgs> ShowMessage;

        public BulkLoadView(UserService userService, VehicleService vehicleService, SparePartsService sparePartsService)
            : base(Orientation.Vertical, 15)
        {
            // Store references to data structures
            this.userService = userService;
            this.vehicleService = vehicleService;
            this.sparePartsService = sparePartsService;

            // Configure view
            Margin = 20;
            BuildInterface();
        }

        private void BuildInterface()
        {
            // Title
            Label lblTitle = new()
            {
                Markup = "<span font='18'>Bulk Data Load</span>",
                Halign = Align.Start
            };
            PackStart(lblTitle, false, false, 0);

            // Entity type selection
            Box selectionBox = new(Orientation.Horizontal, 10);
            PackStart(selectionBox, false, false, 10);

            Label lblType = new("Entity type:");
            selectionBox.PackStart(lblType, false, false, 0);

            cmbEntityType = new ComboBoxText();
            cmbEntityType.AppendText("Users");
            cmbEntityType.AppendText("Vehicles");
            cmbEntityType.AppendText("Spare Parts");
            cmbEntityType.Active = 0;
            selectionBox.PackStart(cmbEntityType, false, false, 0);
            cmbEntityType.TooltipText = "Choose the type of data you want to import.";

            btnSelectFile = new Button("Select JSON file");
            btnSelectFile.Clicked += SelectFile_Clicked;
            btnSelectFile.TooltipText = "Select a .json file with entity data to preview and load.";
            selectionBox.PackStart(btnSelectFile, false, false, 10);

            lblLoadStatus = new Label("No file selected");
            selectionBox.PackStart(lblLoadStatus, false, false, 0);

            // File preview
            Frame framePreview = new("JSON File Preview");
            PackStart(framePreview, true, true, 0);

            swPreview = new ScrolledWindow
            {
                ShadowType = ShadowType.EtchedIn
            };

            txtPreview = new TextView
            {
                Editable = false,
                WrapMode = WrapMode.Word
            };
            swPreview.Add(txtPreview);

            framePreview.Add(swPreview);

            // Load button
            Box actionsBox = new(Orientation.Horizontal, 0)
            {
                Halign = Align.End
            };
            PackStart(actionsBox, false, false, 10);

            btnLoadData = new Button("Load Data");
            btnLoadData.StyleContext.AddClass("suggested-action");
            btnLoadData.Sensitive = false;
            btnLoadData.Clicked += LoadData_Clicked;
            btnLoadData.TooltipText = "Load the selected data into the system.";
            actionsBox.PackStart(btnLoadData, false, false, 0);
        }

        private void SelectFile_Clicked(object sender, EventArgs e)
        {
            Window parentWindow = Toplevel as Window;
            if (parentWindow == null)
            {
                OnShowMessage("Error: No se puede obtener la ventana principal", MessageType.Error);
                return;
            }

            FileChooserDialog dialog = null;
            try
            {
                dialog = new FileChooserDialog(
                    "Select JSON File",
                    parentWindow,
                    FileChooserAction.Open,
                    "Cancel", ResponseType.Cancel,
                    "Select", ResponseType.Accept
                );

                dialog.Filter = new FileFilter { Name = "JSON files" };
                dialog.Filter.AddPattern("*.json");
        
                if (dialog.Run() == (int)ResponseType.Accept)
                {
                    selectedFilePath = dialog.Filename;
                    lblLoadStatus.Text = $"Selected File: {System.IO.Path.GetFileName(selectedFilePath)}";
                    txtPreview.Buffer.Text = File.ReadAllText(selectedFilePath);
                    btnLoadData.Sensitive = true;
                }
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error opening dialog: {ex.Message}", MessageType.Error);
            }
            finally
            {
                dialog?.Destroy();
            }
        }

        private void LoadData_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
                {
                    OnShowMessage("No valid file selected.", MessageType.Error);
                    return;
                }

                string entityType = cmbEntityType.ActiveText;

                if (string.IsNullOrEmpty(entityType))
                {
                    OnShowMessage("Please select an entity type.", MessageType.Warning);
                    return;
                }

                // Show loading indicator
                lblLoadStatus.Text = "Processing...";

                // Check file size before loading
                FileInfo fileInfo = new FileInfo(selectedFilePath);
                long fileSizeInMB = fileInfo.Length / (1024 * 1024);

                if (fileSizeInMB > 10) // Warn for files larger than 10MB
                {
                    using (MessageDialog messageDialog = new MessageDialog(
                        this.Toplevel as Window,
                        DialogFlags.Modal,
                        MessageType.Warning,
                        ButtonsType.YesNo,
                        $"The selected file is {fileSizeInMB}MB which may use significant memory. Continue?"))
                    {
                        ResponseType response = (ResponseType)messageDialog.Run();
                        messageDialog.Destroy();

                        if (response != ResponseType.Yes)
                            return;
                    }
                }

                // Validate JSON structure based on selected entity type
                if (!ValidateJsonStructure(selectedFilePath, entityType))
                {
                    OnShowMessage($"The file does not have the correct structure for {entityType}.", MessageType.Error);
                    return;
                }

                // Process documents based on entity type
                switch (entityType)
                {
                    case "Users":
                        LoadUsers(selectedFilePath);
                        break;

                    case "Vehicles":
                        LoadVehicles(selectedFilePath);
                        break;

                    case "Spare Parts":
                        LoadSpareParts(selectedFilePath);
                        break;
                }
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error loading data: {ex.Message}", MessageType.Error);
            }
        }

        private bool ValidateJsonStructure(string jsonFilePath, string entityType)
        {
            try
            {
                using (var streamReader = new StreamReader(jsonFilePath))
                using (var reader = new Newtonsoft.Json.JsonTextReader(streamReader))
                {
                    // Check if file starts with an array
                    if (!reader.Read() || reader.TokenType != Newtonsoft.Json.JsonToken.StartArray)
                    {
                        OnShowMessage("Invalid JSON format: File must start with an array '['", MessageType.Error);
                        return false;
                    }

                    // Try to read the first object
                    if (!reader.Read() || reader.TokenType != Newtonsoft.Json.JsonToken.StartObject)
                    {
                        OnShowMessage("Invalid JSON format: No objects found in array", MessageType.Error);
                        return false;
                    }

                    // Load the first object to check its structure
                    var firstItem = JObject.Load(reader);

                    // Check required fields based on entity type
                    switch (entityType)
                    {
                        case "Users":
                            return ValidateUserFields(firstItem);
                        case "Vehicles":
                            return ValidateVehicleFields(firstItem);
                        case "Spare Parts":
                            return ValidateSparePartFields(firstItem);
                        default:
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error validating JSON structure: {ex.Message}", MessageType.Error);
                return false;
            }
        }

        private bool ValidateUserFields(JObject item)
        {
            string[] requiredFields = { "ID", "Nombres", "Apellidos", "Correo", "Contrasenia" };
            return ValidateRequiredFields(item, requiredFields);
        }

        private bool ValidateVehicleFields(JObject item)
        {
            string[] requiredFields = { "ID", "ID_Usuario", "Marca", "Modelo", "Placa" };
            return ValidateRequiredFields(item, requiredFields);
        }

        private bool ValidateSparePartFields(JObject item)
        {
            string[] requiredFields = { "ID", "Repuesto", "Detalles", "Costo" };
            return ValidateRequiredFields(item, requiredFields);
        }

        private bool ValidateRequiredFields(JObject item, string[] requiredFields)
        {
            foreach (string field in requiredFields)
            {
                if (!item.ContainsKey(field))
                {
                    OnShowMessage($"Missing required field: '{field}'", MessageType.Error);
                    return false;
                }
            }
            return true;
        }
        private unsafe void LoadUsers(string jsonFilePath)
        {
            try
            {
                var users = JArray.Parse(File.ReadAllText(jsonFilePath));
                int processedCount = 0;
                int failedCount = 0;

                foreach (var userObj in users)
                {
                    try
                    {
                        int id = (int)userObj["ID"];
                        string firstName = (string)userObj["Nombres"];
                        string lastName = (string)userObj["Apellidos"];
                        string email = (string)userObj["Correo"];
                        string password = (string)userObj["Contrasenia"];

                        // Add additional validation
                        if (string.IsNullOrWhiteSpace(firstName) ||
                            string.IsNullOrWhiteSpace(lastName) ||
                            string.IsNullOrWhiteSpace(email))
                        {
                            throw new ArgumentException("Invalid user data");
                        }

                        if (userService.GetUserById(id) == null)
                        {
                            userService.CreateUser(id, firstName, lastName, email, password);
                            processedCount++;
                        }
                        else
                        {
                            failedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        Console.WriteLine($"[Warning] Skipping invalid user: {ex.Message}");
                    }

                    if (processedCount % 100 == 0)
                    {
                        UpdateLoadStatus($"Processed {processedCount} users...");
                    }
                }

                OnShowMessage($"Users loaded: {processedCount} successful, {failedCount} skipped.", MessageType.Info);
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error loading users: {ex.Message}", MessageType.Error);
            }
        }
        
        private void UpdateLoadStatus(string message)
        {
            Application.Invoke((sender, e) =>
            {
                lblLoadStatus.Text = message;
            });
        }
        
        private unsafe void LoadVehicles(string jsonFilePath)
        {
            try
            {
                var vehicles = JArray.Parse(File.ReadAllText(jsonFilePath));
                int processedCount = 0;
                int failedCount = 0;

                foreach (var vehicleObj in vehicles)
                {
                    try
                    {
                        int id = (int)vehicleObj["ID"];
                        int userId = (int)vehicleObj["ID_Usuario"];
                        string brand = (string)vehicleObj["Marca"];
                        string model = (string)vehicleObj["Modelo"];
                        string plate = (string)vehicleObj["Placa"];

                        if (vehicleService.GetVehicleById(id) == null)
                        {
                            vehicleService.CreateVehicle(id, userId, brand, model, plate);
                        }
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        Console.WriteLine($"[Warning] Skipping invalid vehicle: {ex.Message}");
                    }
                }

                OnShowMessage($"Vehicles loaded successfully: {processedCount} items. Skipped: {failedCount} errors.", MessageType.Info);
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error loading vehicles: {ex.Message}", MessageType.Error);
            }
        }
        
        private unsafe void LoadSpareParts(string jsonFilePath)
        {
            try
            {
                var spareParts = JArray.Parse(File.ReadAllText(jsonFilePath));
                int processedCount = 0;
                int failedCount = 0;

                foreach (var partObj in spareParts)
                {
                    try
                    {
                        int id = (int)partObj["ID"];
                        string name = (string)partObj["Repuesto"];
                        string category = (string)partObj["Detalles"];
                        double price = (double)partObj["Costo"];

                        if (sparePartsService.Search(id) == null)
                        {
                            sparePartsService.CreateSparePart(id, name, category, price);
                        }
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        Console.WriteLine($"[Warning] Skipping invalid spare part: {ex.Message}");
                    }
                }

                OnShowMessage($"Spare parts loaded successfully: {processedCount} items. Skipped: {failedCount} errors.", MessageType.Info);
            }
            catch (Exception ex)
            {
                OnShowMessage($"Error loading spare parts: {ex.Message}", MessageType.Error);
            }
        }

        protected virtual void OnShowMessage(string message, MessageType messageType)
        {
            ShowMessage?.Invoke(this, new MessageEventArgs(message, messageType));
        }
    }

}