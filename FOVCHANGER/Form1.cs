using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FOVCHANGER
{
    public partial class Form1 : Form
    {
        //private string pakFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak";
        //private string tempExtractedFilePath = Path.Combine(Path.GetTempPath(), "temp.sso"); // Temporary extracted file path

        private string basePath;
        private string pakFilePath;
        private string tempExtractedFilePath;
        private string cacheFilePath;
        private string backupFilePath;
        private string originalFilePath;



        // Define file paths and default values for each .sso file
        private Dictionary<string, List<string>> presets = new Dictionary<string, List<string>>
        {
            { @"ssl/camera/cam_modes/cam_base.sso", new List<string>                            { "73.7", "93.7", "103.7" } }, // Default, 93.7, 103.7

            { @"ssl/camera/cam_modes/mgs/cam_jetpack.sso", new List<string>                     { "90.0", "110.0", "120.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_jetpack_pve.sso", new List<string>                 { "90.0", "110.0", "120.0" } }, // Default, Preset 1, Preset 2

            { @"ssl/camera/cam_modes/mgs/cam_jetpack_hover.sso", new List<string>               { "85.0", "105.0", "115.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_sprint.sso", new List<string>                      { "85.0", "105.0", "115.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_grappling_hook.sso", new List<string>              { "85.0", "105.0", "115.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_falling.sso", new List<string>                     { "85.0", "105.0", "115.0" } }, // Default, Preset 1, Preset 2

            { @"ssl/camera/cam_modes/mgs/cam_unarmed_mode.sso", new List<string>                { "80.0", "100.0", "110.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_target_lock.sso", new List<string>                 { "80.0", "100.0", "110.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_sorcerer_grab.sso", new List<string>               { "80.0", "100.0", "110.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_orbital_drop.sso", new List<string>                { "80.0", "100.0", "110.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_incap_falling.sso", new List<string>               { "80.0", "100.0", "110.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_falling_attack.sso", new List<string>              { "80.0", "100.0", "110.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_aim_minizoom_heavy.sso", new List<string>          { "80.0", "100.0", "110.0" } }, // Default, Preset 1, Preset 2

            { @"ssl/camera/cam_modes/mgs/cam_unarmed_minizoom.sso", new List<string>            { "70.0", "90.0", "100.0" } }, // Default, Preset 1, Preset 2

            { @"ssl/camera/cam_modes/mgs/cam_aim_minizoom.sso", new List<string>                { "65.0", "85.0", "95.0" } }, // Default, Preset 1, Preset 2

            { @"ssl/camera/cam_modes/mgs/cam_attention_focus.sso", new List<string>             { "60.0", "80.0", "90.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_death.sso", new List<string>                       { "60.0", "80.0", "90.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_last_stand_aim_minizoom.sso", new List<string>     { "60.0", "80.0", "90.0" } }, // Default, Preset 1, Preset 2

            { @"ssl/camera/cam_modes/mgs/cam_unarmed_augmented_vision.sso", new List<string>    { "45.0", "65.0", "75.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_aim_augmented_vision_heavy.sso", new List<string>  { "45.0", "65.0", "75.0" } }, // Default, Preset 1, Preset 2
            { @"ssl/camera/cam_modes/mgs/cam_aim_augmented_vision.sso", new List<string>        { "45.0", "65.0", "75.0" } },  // Default, Preset 1, Preset 2

            { @"ssl/camera/death_camera.sso", new List<string>                                  { "73.7", "93.7", "103.7" } }, // Default, 93.7, 103.7
            { @"ssl/camera/fly_camera_project.sso", new List<string>                            { "73.7", "93.7", "103.7" } }, // Default, 93.7, 103.7
            { @"ssl/camera/fly_camera.sso", new List<string>                                    { "60.0", "80.0", "90.0" } }  // Default, 93.7, 103.7


        };

        private TextBox[] fovTextBoxes; // Array to hold textboxes for each .sso file

        public Form1()
        {
            InitializeComponent();
            // Initialize the array with textboxes for each .sso file
            fovTextBoxes = new TextBox[] { textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7, textBox8, textBox9, textBox10, textBox11, textBox12, textBox13, textBox14, textBox15, textBox16, textBox17, textBox18, textBox19, textBox20, textBox21, textBox22, textBox23, textBox24, textBox25 }; // Ensure these match the order of ssoFilePaths

            // Set up event handler for checkbox
            checkBoxDefaults.CheckedChanged += checkBox_CheckedChanged;
            checkBoxPreset1.CheckedChanged += checkBox_CheckedChanged;
            checkBoxPreset2.CheckedChanged += checkBox_CheckedChanged;

            
            
            // Initialize base path
            InitializeBasePath();
            // Initialize paths
            InitializePaths();
        }

        private void InitializePaths()
        {

            // Default executable path
            //string defaultExecutablePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\Warhammer 40000 Space Marine 2.exe";

            basePath = textBox26.Text.Trim(); // Get the base path from the TextBox

            if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
            {
                /*MessageBox.Show("Invalid installation path. Please specify a valid path.");*/
                return;
            }

            pakFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak");
            tempExtractedFilePath = Path.Combine(Path.GetTempPath(), "temp.sso"); // Temporary extracted file path
            cacheFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak.cache");
            backupFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak.BACKUP");
            originalFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak");

            UpdateBackupButtonState();
            UpdateRestoreButtonState();
            UpdateCacheButtonState();
        }

        private void InitializeBasePath()
        {
            // Default executable path
            string defaultExecutablePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\Warhammer 40000 Space Marine 2.exe";
            string defaultBasePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2";

            // Check if the executable exists
            if (File.Exists(defaultExecutablePath))
            {
                // Set the base path to the directory of the executable
                string basePath = Path.GetDirectoryName(defaultExecutablePath);
                textBox26.Text = basePath;
            }
            else
            {
                // Try to find the base path by searching all partitions
                string foundPath = SearchForInstallationFolder("Space Marine 2");

                if (!string.IsNullOrEmpty(foundPath))
                {
                    textBox26.Text = foundPath;
                }
                else
                {
                    // Prompt the user to specify the base path
                    using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.Description = "Select the installation folder for Space Marine 2";

                        DialogResult result = folderDialog.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            string userSpecifiedPath = folderDialog.SelectedPath;
                            if (Directory.Exists(userSpecifiedPath))
                            {
                                textBox26.Text = userSpecifiedPath;
                            }
                            else
                            {
                                MessageBox.Show("The specified path does not exist.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("No installation folder selected.");
                            textBox26.Text = defaultBasePath;
                            // Optionally exit the application if no path is selected
                            // Application.Exit();
                        }
                    }
                }
            }

            // Initialize paths and button states after setting the base path
            InitializePaths();
            UpdateBackupButtonState();
            UpdateRestoreButtonState();
            UpdateCacheButtonState();
        }

        // Method to search for the installation folder on all partitions
        private string SearchForInstallationFolder(string relativePath)
        {
            foreach (string drive in Environment.GetLogicalDrives())
            {
                try
                {
                    string searchPath = Path.Combine(drive, relativePath);
                    if (Directory.Exists(searchPath))
                    {
                        return Path.GetDirectoryName(searchPath);
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions (e.g., access denied) and continue searching
                    Console.WriteLine($"Error accessing drive {drive}: {ex.Message}");
                }
            }
            return null;
        }

        private void textBox26_TextChanged(object sender, EventArgs e)
        {
            InitializePaths();
        }

        private void UpdateCacheButtonState()
        {
            // Construct the cache file paths using the base path
            string cacheFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak.cache");
            string disabledCacheFilePath = cacheFilePath + ".DISABLED";

            // Check the existence of the files
            bool cacheFileExists = File.Exists(cacheFilePath);
            bool disabledCacheFileExists = File.Exists(disabledCacheFilePath);

            // Update the button state based on the file existence
            if (disabledCacheFileExists)
            {
                // Cache is disabled
                button5.Text = "Enable Cache";
                button5.ForeColor = System.Drawing.Color.Green;
            }
            else if (cacheFileExists)
            {
                // Cache is enabled
                button5.Text = "Disable Cache";
                button5.ForeColor = System.Drawing.Color.DarkRed;
            }
            else
            {
                // Cache file is missing
                button5.Text = "Cache File Missing";
                button5.ForeColor = System.Drawing.Color.Gray;
            }

            // Optionally disable the button if neither file is present
            button5.Enabled = cacheFileExists || disabledCacheFileExists;
        }

        private void UpdateBackupButtonState()
        {
            // Construct the backup file path using the base path
            string backupFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak.BACKUP");
            button3.Enabled = !File.Exists(backupFilePath); // Enable if backup file does not exist, disable otherwise
        }

        private void UpdateRestoreButtonState()
        {
            // Construct the backup file path using the base path
            string backupFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak.BACKUP");
            button4.Enabled = File.Exists(backupFilePath); // Enable if backup file exists, disable otherwise
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkedBox = sender as CheckBox;

            if (checkedBox != null)
            {
                // Uncheck other checkboxes
                UncheckOtherCheckboxes(checkedBox);

                // Update textboxes based on the checked checkbox
                if (checkBoxDefaults.Checked)
                {
                    UpdateTextBoxes(0); // Default values
                }
                else if (checkBoxPreset1.Checked)
                {
                    UpdateTextBoxes(1); // Preset 1 values
                }
                else if (checkBoxPreset2.Checked)
                {
                    UpdateTextBoxes(2); // Preset 2 values
                }
            }
        }

        private void UncheckOtherCheckboxes(CheckBox checkedBox)
        {
            foreach (Control control in this.Controls)
            {
                if (control is CheckBox checkbox && checkbox != checkedBox)
                {
                    checkbox.Checked = false;
                }
            }
        }


        private void UpdateTextBoxes(int presetIndex)
        {
            int index = 0;
            foreach (var kvp in presets)
            {
                if (index < fovTextBoxes.Length)
                {
                    var values = kvp.Value;
                    if (presetIndex < values.Count)
                    {
                        fovTextBoxes[index].Text = values[presetIndex];
                    }
                    index++;
                }
            }
        }
        // Function to extract a file from the .pak (ZIP) archive
        private void ExtractFileFromPak(string pakFilePath, string filePathInPak, string outputFilePath)
        {
            using (FileStream fs = new FileStream(pakFilePath, FileMode.Open, FileAccess.Read))
            using (ZipArchive zipFile = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                ZipArchiveEntry entry = zipFile.GetEntry(filePathInPak);
                if (entry != null)
                {
                    using (Stream zipStream = entry.Open())
                    using (FileStream outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                    {
                        zipStream.CopyTo(outputStream);
                    }
                }
                else
                {
                    throw new FileNotFoundException("File not found inside the .pak archive.");
                }
            }
        }

        // Function to extract the fov value from a line
        private float ExtractFovValue(string line)
        {
            // Regex pattern to match "fov" followed by any number of spaces/tabs and then the value
            string pattern = @"fov\s*=\s*([\d.]+)";
            Match match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string valueString = match.Groups[1].Value.Trim();
                if (float.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedFov))
                {
                    return parsedFov;
                }
            }
            throw new FormatException("FOV value not found or is in an invalid format.");
        }

        // Function to update the fov value in the line
        private string UpdateFovLine(string line, float newFovValue)
        {
            string pattern = @"fov\s*=\s*[\d.]+";
            string replacement = $"fov = {newFovValue.ToString("0.0", CultureInfo.InvariantCulture)}";
            return Regex.Replace(line, pattern, replacement, RegexOptions.IgnoreCase);
        }

        // Function to replace a file within the .pak archive
        private void ReplaceFileInPak(string pakFilePath, string filePathInPak, string modifiedFilePath)
        {
            string tempFilePath = Path.GetTempFileName(); // Temporary file path for updated .pak

            try
            {
                using (FileStream fs = new FileStream(pakFilePath, FileMode.Open, FileAccess.Read))
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Copy the .pak file content to the MemoryStream
                    fs.CopyTo(memoryStream);

                    // Create a new MemoryStream for the updated .pak file
                    using (MemoryStream updatedMemoryStream = new MemoryStream())
                    {
                        // Create a ZipArchive from the MemoryStream
                        using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Update, leaveOpen: true))
                        {
                            // Remove the old file if it exists
                            var oldEntry = archive.GetEntry(filePathInPak);
                            if (oldEntry != null)
                            {
                                oldEntry.Delete();
                            }

                            // Add the new file
                            using (FileStream newFileStream = new FileStream(modifiedFilePath, FileMode.Open, FileAccess.Read))
                            {
                                ZipArchiveEntry newEntry = archive.CreateEntry(filePathInPak, CompressionLevel.Optimal);
                                using (Stream entryStream = newEntry.Open())
                                {
                                    newFileStream.CopyTo(entryStream);
                                }
                            }

                            // Save the updated archive to the new MemoryStream
                            archive.Dispose();
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            memoryStream.CopyTo(updatedMemoryStream);
                        }

                        // Save the updated .pak file
                        using (FileStream tempFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                        {
                            updatedMemoryStream.Seek(0, SeekOrigin.Begin);
                            updatedMemoryStream.CopyTo(tempFileStream);
                        }
                    }
                }

                // Replace the old .pak file with the updated one
                File.Delete(pakFilePath);
                File.Move(tempFilePath, pakFilePath);
            }
            finally
            {
                // Clean up temporary files if they still exist
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                progressBar.Value = 0;
                progressBar.Maximum = presets.Count; // Updated to use presets
                int index = 0;

                foreach (var kvp in presets)
                {
                    if (index < fovTextBoxes.Length)
                    {
                        // Extract each .sso file
                        ExtractFileFromPak(pakFilePath, kvp.Key, tempExtractedFilePath);

                        // Read all lines from the extracted .sso file
                        string[] lines = File.ReadAllLines(tempExtractedFilePath);

                        // Extract the fov value from the file
                        foreach (var line in lines)
                        {
                            try
                            {
                                float parsedFov = ExtractFovValue(line);
                                // Display the fov value in the corresponding TextBox with 1 decimal place
                                fovTextBoxes[index].Text = parsedFov.ToString("0.0", CultureInfo.InvariantCulture);
                                break; // Assuming the value appears only once, break after the first match
                            }
                            catch (FormatException)
                            {
                                // Continue searching if the line does not contain a valid FOV value
                            }
                        }

                        index++;
                        progressBar.Value = index; // Update progress bar
                        Application.DoEvents(); // Process all Windows messages to update UI
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string backupFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak.BACKUP";

            // Check if the backup file exists
            if (!File.Exists(backupFilePath))
            {
                DialogResult result = MessageBox.Show("Backup not found. Do you want to continue patching?", "Backup Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    return; // Exit the method if the user chooses not to continue
                }
            }

            try
            {
                progressBar.Value = 0;
                progressBar.Maximum = presets.Count; // Updated to use presets
                int index = 0;

                foreach (var kvp in presets)
                {
                    if (index < fovTextBoxes.Length)
                    {
                        string newValueStr = fovTextBoxes[index].Text;

                        if (float.TryParse(newValueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out float newValue))
                        {
                            // Extract the .sso file
                            ExtractFileFromPak(pakFilePath, kvp.Key, tempExtractedFilePath);

                            // Read all lines from the extracted .sso file
                            string[] lines = File.ReadAllLines(tempExtractedFilePath);

                            // Modify the extracted .sso file if necessary
                            var updatedLines = new List<string>();
                            bool valueChanged = false;
                            foreach (var line in lines)
                            {
                                try
                                {
                                    float currentFov = ExtractFovValue(line);
                                    if (Math.Abs(currentFov - newValue) > float.Epsilon)
                                    {
                                        valueChanged = true;
                                    }
                                    updatedLines.Add(UpdateFovLine(line, newValue));
                                }
                                catch (FormatException)
                                {
                                    // Preserve lines that do not contain a valid FOV value
                                    updatedLines.Add(line);
                                }
                            }

                            if (valueChanged)
                            {
                                // Save the modified .sso file
                                File.WriteAllLines(tempExtractedFilePath, updatedLines);

                                // Replace the modified .sso file in the .pak archive
                                ReplaceFileInPak(pakFilePath, kvp.Key, tempExtractedFilePath);
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Invalid FOV value entered for {kvp.Key}.");
                        }

                        index++;
                        progressBar.Value = index; // Update progress bar
                        Application.DoEvents(); // Process all Windows messages to update UI
                    }
                }

                MessageBox.Show("FOV values updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving files: {ex.Message}");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Get the base path from the TextBox
            string basePath = textBox26.Text.Trim();

            // Check if the base path is valid
            if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
            {
                MessageBox.Show("Invalid installation path. Please specify a valid path.");
                return;
            }

            // Construct the source and backup file paths using the base path
            string sourceFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak");
            string backupFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak.BACKUP");

            try
            {
                // Check if the backup already exists
                if (File.Exists(backupFilePath))
                {
                    MessageBox.Show("Backup file already exists.");
                    return; // Exit if backup already exists
                }

                // Perform the backup
                File.Copy(sourceFilePath, backupFilePath);
                MessageBox.Show("Backup created successfully.");

                // Disable the backup button as the backup now exists
                UpdateBackupButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating backup: {ex.Message}");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Get the base path from the TextBox
            string basePath = textBox26.Text.Trim();

            // Check if the base path is valid
            if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
            {
                MessageBox.Show("Invalid installation path. Please specify a valid path.");
                return;
            }

            // Construct the file paths using the base path
            string backupFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak.BACKUP");
            string originalFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak");

            try
            {
                if (File.Exists(originalFilePath))
                {
                    DialogResult result = MessageBox.Show(
                        "The original file already exists. Do you want to overwrite it?",
                        "Confirm Overwrite",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        return;
                    }
                }

                File.Delete(originalFilePath);
                File.Move(backupFilePath, originalFilePath);

                MessageBox.Show("File restored successfully.");

                UpdateBackupButtonState();
                UpdateRestoreButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring file: {ex.Message}");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Get the base path from the TextBox
            string basePath = textBox26.Text.Trim();

            // Check if the base path is valid
            if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
            {
                MessageBox.Show("Invalid installation path. Please specify a valid path.");
                return;
            }

            // Construct the cache file paths using the base path
            string cacheFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak.cache");
            string disabledCacheFilePath = cacheFilePath + ".DISABLED";

            try
            {
                // Check if the cache file is disabled
                if (File.Exists(disabledCacheFilePath))
                {
                    // Enable cache by renaming the .DISABLED file back to the original name
                    File.Move(disabledCacheFilePath, cacheFilePath);
                    MessageBox.Show("Cache file enabled successfully.");
                    button5.Text = "Disable Cache";
                    button5.ForeColor = System.Drawing.Color.DarkRed;
                }
                else
                {
                    // Disable cache by renaming the cache file to .DISABLED
                    if (File.Exists(cacheFilePath))
                    {
                        File.Move(cacheFilePath, disabledCacheFilePath);
                        MessageBox.Show("Cache file disabled successfully.");
                        button5.Text = "Enable Cache";
                        button5.ForeColor = System.Drawing.Color.Green;
                    }
                    else
                    {
                        MessageBox.Show("Cache file does not exist.");
                    }
                }

                // Update the button state after changing the cache file status
                UpdateCacheButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling cache file status: {ex.Message}");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string newBasePath = textBox26.Text.Trim();

            if (Directory.Exists(newBasePath))
            {
                basePath = newBasePath;  // Make sure this assignment is happening
                UpdatePaths(basePath);  // Ensure paths are updated
                UpdateCacheButtonState();  // Update the button state
                UpdateBackupButtonState();  // Update backup button state if needed
                UpdateRestoreButtonState();  // Update restore button state if needed

                MessageBox.Show($"Path updated successfully. New base path: {basePath}");
            }
            else
            {
                MessageBox.Show("The specified path does not exist. Please check and try again.");
            }
        }



        private void UpdatePaths(string basePath)
        {
            // Update the base path for all file paths
            pakFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak");
            tempExtractedFilePath = Path.Combine(Path.GetTempPath(), "temp.sso");

            backupFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak.BACKUP");
            originalFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak");
            cacheFilePath = Path.Combine(basePath, @"client_pc\root\paks\client\default\default_other.pak.cache");
            string disabledCacheFilePath = cacheFilePath + ".DISABLED";

            // Debugging information to verify paths
            Console.WriteLine($"Backup File Path: {backupFilePath}");
            Console.WriteLine($"Original File Path: {originalFilePath}");
            Console.WriteLine($"Cache File Path: {cacheFilePath}");
            Console.WriteLine($"Disabled Cache File Path: {disabledCacheFilePath}");

            // Update the paths for buttons
            UpdateBackupButtonState();
            UpdateRestoreButtonState();
            UpdateCacheButtonState();
        }
    }
}
