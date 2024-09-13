using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Generic;

namespace FOVCHANGER
{
    public partial class Form1 : Form
    {
        private string pakFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak";
        private string tempExtractedFilePath = Path.Combine(Path.GetTempPath(), "temp.sso"); // Temporary extracted file path

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
            { @"ssl/camera/fly_camera.sso", new List<string>                                    { "60.0", "80.0", "90.7" } }  // Default, 93.7, 103.7


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

            // Check if the backup file exists and set the button state
            UpdateBackupButtonState();
            UpdateRestoreButtonState();
            UpdateCacheButtonState();
        }
        private void UpdateCacheButtonState()
        {
            string cacheFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak.cache";
            button5.Enabled = File.Exists(cacheFilePath); // Enable if cache file exists, disable otherwise
        }

        private void UpdateBackupButtonState()
        {
            string backupFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak.BACKUP";
            button3.Enabled = !File.Exists(backupFilePath); // Enable if backup file does not exist, disable otherwise
        }

        private void UpdateRestoreButtonState()
        {
            string backupFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak.BACKUP";
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
            {
                string sourceFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak";
                string backupFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak.BACKUP";

                try
                {
                    // Check if backup already exists
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
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string backupFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak.BACKUP";
            string originalFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak";

            try
            {
                // Check if the original file exists
                if (File.Exists(originalFilePath))
                {
                    // Ask user for confirmation to overwrite
                    DialogResult result = MessageBox.Show(
                        "The original file already exists. Do you want to overwrite it?",
                        "Confirm Overwrite",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        return; // Exit if user does not want to overwrite
                    }
                }

                // Perform the restore by renaming the backup file to the original file
                File.Delete(originalFilePath); // Delete the original file if it exists
                File.Move(backupFilePath, originalFilePath); // Rename backup file to original file

                MessageBox.Show("File restored successfully.");

                // Update button states
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
            string cacheFilePath = @"C:\Program Files (x86)\Steam\steamapps\common\Space Marine 2\client_pc\root\paks\client\default\default_other.pak.cache";
            string disabledCacheFilePath = cacheFilePath + ".DISABLED";

            try
            {
                if (File.Exists(cacheFilePath))
                {
                    // Rename the cache file to .cache.DISABLED
                    File.Move(cacheFilePath, disabledCacheFilePath);
                    MessageBox.Show("Cache file disabled successfully.");

                    // Update button state
                    UpdateCacheButtonState();
                }
                else
                {
                    MessageBox.Show("Cache file does not exist.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error disabling cache file: {ex.Message}");
            }
        }
    }
}
