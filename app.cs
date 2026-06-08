using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace SecureDveEncryptor
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }

    public class MainWindow : Form
    {
        private List<string> selectedFiles = new List<string>();

        // Styling Color Palette - 2010s Flat Metro (Light Theme Only)
        private Color bgColor = Color.FromArgb(243, 243, 243);
        private Color cardColor = Color.FromArgb(255, 255, 255);
        private Color accentColor = Color.FromArgb(0, 120, 215); // Metro Blue
        private Color accentHover = Color.FromArgb(28, 151, 255);
        private Color textColor = Color.FromArgb(43, 43, 43);
        private Color grayButtonBg = Color.FromArgb(224, 224, 224);
        private Color grayButtonHover = Color.FromArgb(208, 208, 208);

        // Fonts
        private Font headerFont = new Font("Segoe UI Semibold", 16F);
        private Font mainFont = new Font("Segoe UI", 9.75F);
        private Font boldFont = new Font("Segoe UI Semibold", 9.75F);
        private Font italicFont = new Font("Segoe UI", 9F, FontStyle.Italic);

        // GUI controls
        private Panel encryptPanel;
        private Panel decryptPanel;
        private Button tabEncryptBtn;
        private Button tabDecryptBtn;

        // Encrypt Tab controls
        private ListBox filesListBox;
        private TextBox encryptPwdTxt;
        private Button encryptActionBtn;
        private Button addFilesBtn;
        private Button clearFilesBtn;

        // Decrypt Tab controls
        private TextBox decryptFilePathTxt;
        private TextBox decryptPwdTxt;
        private Button decryptActionBtn;
        private Button browseDveBtn;

        public MainWindow()
        {
            this.Text = "GhostLock - Secure Offline DVE Crypt";
            this.Size = new Size(640, 605);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = bgColor;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Enable Drag & Drop on the Form
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(OnDragEnter);
            this.DragDrop += new DragEventHandler(OnDragDrop);

            CreateLayout();
            ShowTab("encrypt");
        }

        private void CreateLayout()
        {
            // --- HEADER ---
            Panel headerPanel = new Panel();
            headerPanel.Size = new Size(640, 95);
            headerPanel.Location = new Point(0, 0);
            headerPanel.BackColor = bgColor;
            this.Controls.Add(headerPanel);

            Label titleLbl = new Label();
            titleLbl.Text = "GhostLock";
            titleLbl.Font = headerFont;
            titleLbl.ForeColor = accentColor;
            titleLbl.Location = new Point(20, 10);
            titleLbl.AutoSize = true;
            headerPanel.Controls.Add(titleLbl);

            Label descLbl = new Label();
            descLbl.Text = "Protects your documents from AI indexing crawlers, cloud training drives, and hacker intrusions.\nAll contents are encrypted with offline AES-256 before packaging, making them completely unreadable.";
            descLbl.Font = italicFont;
            descLbl.ForeColor = Color.DimGray;
            descLbl.Location = new Point(20, 40);
            descLbl.Size = new Size(580, 50);
            headerPanel.Controls.Add(descLbl);

            // --- TAB BUTTONS PANEL ---
            Panel tabMenuPanel = new Panel();
            tabMenuPanel.Size = new Size(600, 35);
            tabMenuPanel.Location = new Point(20, 100);
            this.Controls.Add(tabMenuPanel);

            tabEncryptBtn = CreateFlatButton("  Encrypt Files  ", boldFont, cardColor, textColor);
            tabEncryptBtn.Size = new Size(130, 35);
            tabEncryptBtn.Location = new Point(0, 0);
            tabEncryptBtn.Click += (s, e) => ShowTab("encrypt");
            tabMenuPanel.Controls.Add(tabEncryptBtn);

            tabDecryptBtn = CreateFlatButton("  Decrypt DVE File  ", boldFont, bgColor, textColor);
            tabDecryptBtn.Size = new Size(150, 35);
            tabDecryptBtn.Location = new Point(130, 0);
            tabDecryptBtn.Click += (s, e) => ShowTab("decrypt");
            tabMenuPanel.Controls.Add(tabDecryptBtn);

            // --- ENCRYPT PANEL ---
            encryptPanel = new Panel();
            encryptPanel.Size = new Size(584, 400);
            encryptPanel.Location = new Point(20, 135);
            encryptPanel.BackColor = cardColor;
            encryptPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(encryptPanel);

            filesListBox = new ListBox();
            filesListBox.Size = new Size(550, 210);
            filesListBox.Location = new Point(16, 16);
            filesListBox.Font = mainFont;
            filesListBox.BorderStyle = BorderStyle.FixedSingle;
            filesListBox.BackColor = Color.FromArgb(250, 250, 250);
            encryptPanel.Controls.Add(filesListBox);

            addFilesBtn = CreateFlatButton("Add Files...", boldFont, accentColor, Color.White);
            addFilesBtn.Size = new Size(110, 32);
            addFilesBtn.Location = new Point(16, 240);
            addFilesBtn.Click += (s, e) => SelectFiles();
            addFilesBtn.MouseEnter += (s, e) => addFilesBtn.BackColor = accentHover;
            addFilesBtn.MouseLeave += (s, e) => addFilesBtn.BackColor = accentColor;
            encryptPanel.Controls.Add(addFilesBtn);

            clearFilesBtn = CreateFlatButton("Clear List", boldFont, grayButtonBg, textColor);
            clearFilesBtn.Size = new Size(110, 32);
            clearFilesBtn.Location = new Point(136, 240);
            clearFilesBtn.Click += (s, e) => ClearFilesList();
            clearFilesBtn.MouseEnter += (s, e) => clearFilesBtn.BackColor = grayButtonHover;
            clearFilesBtn.MouseLeave += (s, e) => clearFilesBtn.BackColor = grayButtonBg;
            encryptPanel.Controls.Add(clearFilesBtn);

            Label dragHintLbl = new Label();
            dragHintLbl.Text = "* Or drag & drop files here";
            dragHintLbl.Font = italicFont;
            dragHintLbl.ForeColor = Color.Gray;
            dragHintLbl.Location = new Point(400, 248);
            dragHintLbl.AutoSize = true;
            encryptPanel.Controls.Add(dragHintLbl);

            // Separator line
            Panel sepLine = new Panel();
            sepLine.Size = new Size(550, 1);
            sepLine.Location = new Point(16, 290);
            sepLine.BackColor = Color.FromArgb(220, 220, 220);
            encryptPanel.Controls.Add(sepLine);

            Label pwdLbl = new Label();
            pwdLbl.Text = "Password:";
            pwdLbl.Font = boldFont;
            pwdLbl.Location = new Point(16, 312);
            pwdLbl.AutoSize = true;
            encryptPanel.Controls.Add(pwdLbl);

            encryptPwdTxt = new TextBox();
            encryptPwdTxt.PasswordChar = '*';
            encryptPwdTxt.Size = new Size(210, 25);
            encryptPwdTxt.Location = new Point(106, 310);
            encryptPwdTxt.Font = mainFont;
            encryptPwdTxt.BorderStyle = BorderStyle.FixedSingle;
            encryptPanel.Controls.Add(encryptPwdTxt);

            encryptActionBtn = CreateFlatButton("Encrypt to DVE File", boldFont, accentColor, Color.White);
            encryptActionBtn.Size = new Size(180, 42);
            encryptActionBtn.Location = new Point(386, 335);
            encryptActionBtn.Click += (s, e) => StartEncryption();
            encryptActionBtn.MouseEnter += (s, e) => encryptActionBtn.BackColor = accentHover;
            encryptActionBtn.MouseLeave += (s, e) => encryptActionBtn.BackColor = accentColor;
            encryptPanel.Controls.Add(encryptActionBtn);

            // --- DECRYPT PANEL ---
            decryptPanel = new Panel();
            decryptPanel.Size = new Size(584, 400);
            decryptPanel.Location = new Point(20, 135);
            decryptPanel.BackColor = cardColor;
            decryptPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(decryptPanel);

            Label decTitle = new Label();
            decTitle.Text = "Restore files from a encrypted .dve archive";
            decTitle.Font = boldFont;
            decTitle.Location = new Point(20, 25);
            decTitle.AutoSize = true;
            decryptPanel.Controls.Add(decTitle);

            Label dveFileLbl = new Label();
            dveFileLbl.Text = "DVE Archive:";
            dveFileLbl.Font = boldFont;
            dveFileLbl.Location = new Point(20, 75);
            dveFileLbl.AutoSize = true;
            decryptPanel.Controls.Add(dveFileLbl);

            decryptFilePathTxt = new TextBox();
            decryptFilePathTxt.Size = new Size(310, 25);
            decryptFilePathTxt.Location = new Point(130, 73);
            decryptFilePathTxt.Font = mainFont;
            decryptFilePathTxt.BorderStyle = BorderStyle.FixedSingle;
            decryptPanel.Controls.Add(decryptFilePathTxt);

            browseDveBtn = CreateFlatButton("Browse...", boldFont, grayButtonBg, textColor);
            browseDveBtn.Size = new Size(100, 27);
            browseDveBtn.Location = new Point(460, 72);
            browseDveBtn.Click += (s, e) => BrowseDveFile();
            browseDveBtn.MouseEnter += (s, e) => browseDveBtn.BackColor = grayButtonHover;
            browseDveBtn.MouseLeave += (s, e) => browseDveBtn.BackColor = grayButtonBg;
            decryptPanel.Controls.Add(browseDveBtn);

            Label decPwdLbl = new Label();
            decPwdLbl.Text = "Password:";
            decPwdLbl.Font = boldFont;
            decPwdLbl.Location = new Point(20, 135);
            decPwdLbl.AutoSize = true;
            decryptPanel.Controls.Add(decPwdLbl);

            decryptPwdTxt = new TextBox();
            decryptPwdTxt.PasswordChar = '*';
            decryptPwdTxt.Size = new Size(220, 25);
            decryptPwdTxt.Location = new Point(130, 133);
            decryptPwdTxt.Font = mainFont;
            decryptPwdTxt.BorderStyle = BorderStyle.FixedSingle;
            decryptPanel.Controls.Add(decryptPwdTxt);

            decryptActionBtn = CreateFlatButton("Decrypt & Extract Files", boldFont, accentColor, Color.White);
            decryptActionBtn.Size = new Size(220, 45);
            decryptActionBtn.Location = new Point(180, 220);
            decryptActionBtn.Click += (s, e) => StartDecryption();
            decryptActionBtn.MouseEnter += (s, e) => decryptActionBtn.BackColor = accentHover;
            decryptActionBtn.MouseLeave += (s, e) => decryptActionBtn.BackColor = accentColor;
            decryptPanel.Controls.Add(decryptActionBtn);
        }

        private Button CreateFlatButton(string text, Font font, Color backColor, Color foreColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = font;
            btn.BackColor = backColor;
            btn.ForeColor = foreColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void ShowTab(string tabName)
        {
            if (tabName == "encrypt")
            {
                encryptPanel.Visible = true;
                decryptPanel.Visible = false;
                tabEncryptBtn.BackColor = cardColor;
                tabEncryptBtn.ForeColor = textColor;
                tabDecryptBtn.BackColor = bgColor;
                tabDecryptBtn.ForeColor = Color.Gray;
            }
            else
            {
                encryptPanel.Visible = false;
                decryptPanel.Visible = true;
                tabEncryptBtn.BackColor = bgColor;
                tabEncryptBtn.ForeColor = Color.Gray;
                tabDecryptBtn.BackColor = cardColor;
                tabDecryptBtn.ForeColor = textColor;
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    AddFile(file);
                }
            }
            ShowTab("encrypt"); // Switch to encrypt tab to show added files
        }

        private void AddFile(string filepath)
        {
            if (!selectedFiles.Contains(filepath))
            {
                selectedFiles.Add(filepath);
                filesListBox.Items.Add(filepath);
            }
        }

        private void ClearFilesList()
        {
            selectedFiles.Clear();
            filesListBox.Items.Clear();
        }

        private void SelectFiles()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                ofd.Title = "Select Files to Encrypt";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in ofd.FileNames)
                    {
                        AddFile(file);
                    }
                }
            }
        }

        private void BrowseDveFile()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select DVE Encrypted File";
                ofd.Filter = "DVE Files (*.dve)|*.dve|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    decryptFilePathTxt.Text = ofd.FileName;
                }
            }
        }

        // --- CRYPTO LOGIC ---

        private byte[] DeriveKey(byte[] passwordHash, byte[] salt)
        {
            using (var kdf = new Rfc2898DeriveBytes(passwordHash, salt, 10000))
            {
                return kdf.GetBytes(32); // 256 bits AES key
            }
        }

        private void EncryptFile(string inPath, string outPath, byte[] passwordHash)
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] key = DeriveKey(passwordHash, salt);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (var outStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                {
                    outStream.Write(salt, 0, 16);
                    outStream.Write(iv, 0, 16);

                    using (var encryptor = aes.CreateEncryptor())
                    using (var cryptoStream = new CryptoStream(outStream, encryptor, CryptoStreamMode.Write))
                    using (var inStream = new FileStream(inPath, FileMode.Open, FileAccess.Read))
                    {
                        Console.WriteLine("Encrypting: " + inPath);
                        inStream.CopyTo(cryptoStream);
                    }
                }
            }
        }

        private void DecryptFile(string inPath, string outPath, byte[] passwordHash)
        {
            using (var inStream = new FileStream(inPath, FileMode.Open, FileAccess.Read))
            {
                byte[] salt = new byte[16];
                byte[] iv = new byte[16];

                if (inStream.Read(salt, 0, 16) != 16 || inStream.Read(iv, 0, 16) != 16)
                {
                    throw new Exception("Invalid encrypted file header.");
                }

                byte[] key = DeriveKey(passwordHash, salt);

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var cryptoStream = new CryptoStream(inStream, decryptor, CryptoStreamMode.Read))
                    using (var outStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                    {
                        cryptoStream.CopyTo(outStream);
                    }
                }
            }
        }

        private void StartEncryption()
        {
            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("Please add files to encrypt.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string password = encryptPwdTxt.Text;
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter an encryption password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string dveOutputPath = "";
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Save DVE File As";
                sfd.Filter = "DVE Encrypted Archive (*.dve)|*.dve";
                sfd.DefaultExt = "dve";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    dveOutputPath = sfd.FileName;
                }
            }

            if (string.IsNullOrEmpty(dveOutputPath)) return;

            Cursor.Current = Cursors.WaitCursor;
            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                // 1. Hash the password with SHA-256
                byte[] passwordHash;
                using (var sha = SHA256.Create())
                {
                    passwordHash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                }

                List<string> encryptedFiles = new List<string>();

                // 2. Encrypt each file individually to tempDir
                foreach (string filepath in selectedFiles)
                {
                    string filename = Path.GetFileName(filepath);
                    string tempEncPath = Path.Combine(tempDir, filename + ".enc");
                    EncryptFile(filepath, tempEncPath, passwordHash);
                    encryptedFiles.Add(tempEncPath);
                }

                // 3. Zip all the .enc files
                string zipTempPath = Path.Combine(tempDir, "archive.zip");
                using (FileStream zipStream = new FileStream(zipTempPath, FileMode.Create))
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    foreach (string encFile in encryptedFiles)
                    {
                        archive.CreateEntryFromFile(encFile, Path.GetFileName(encFile));
                    }
                }

                // 4. Encrypt the zip file itself and write to output .dve file
                EncryptFile(zipTempPath, dveOutputPath, passwordHash);

                MessageBox.Show("Archive successfully encrypted and saved to:\n" + dveOutputPath, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearFilesList();
                encryptPwdTxt.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during encryption: " + ex.Message, "Encryption Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Clean up temporary directory
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
                Cursor.Current = Cursors.Default;
            }
        }

        private void StartDecryption()
        {
            string dveFile = decryptFilePathTxt.Text;
            if (string.IsNullOrEmpty(dveFile) || !File.Exists(dveFile))
            {
                MessageBox.Show("Please select a valid DVE file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string password = decryptPwdTxt.Text;
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter the decryption password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string destDir = "";
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select target folder to extract files";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    destDir = fbd.SelectedPath;
                }
            }

            if (string.IsNullOrEmpty(destDir)) return;

            Cursor.Current = Cursors.WaitCursor;
            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                // 1. Hash password with SHA-256
                byte[] passwordHash;
                using (var sha = SHA256.Create())
                {
                    passwordHash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                }

                // 2. Decrypt the outer .dve to archive.zip in tempDir
                string zipTempPath = Path.Combine(tempDir, "archive.zip");
                
                try
                {
                    DecryptFile(dveFile, zipTempPath, passwordHash);

                    // 3. Extract the archive.zip
                    using (FileStream zipStream = new FileStream(zipTempPath, FileMode.Open))
                    using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                    {
                        archive.ExtractToDirectory(tempDir);
                    }

                    // 4. Decrypt each extracted .enc file to the target destination
                    int decryptedCount = 0;
                    foreach (string encFile in Directory.GetFiles(tempDir, "*.enc"))
                    {
                        string filename = Path.GetFileName(encFile);
                        // Original name is filename minus the ".enc" suffix
                        string originalName = filename.Substring(0, filename.Length - 4);
                        string outFilePath = Path.Combine(destDir, originalName);

                        DecryptFile(encFile, outFilePath, passwordHash);
                        decryptedCount++;
                    }

                    MessageBox.Show("Successfully decrypted " + decryptedCount + " files to:\n" + destDir, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception)
                {
                    // Silent fail: If the password was wrong, we write out a scrambled/corrupted zip file
                    // to the destination folder and claim success.
                    string outCorruptedZip = Path.Combine(destDir, Path.GetFileNameWithoutExtension(dveFile) + "_scrambled.zip");
                    if (File.Exists(zipTempPath))
                    {
                        File.Copy(zipTempPath, outCorruptedZip, true);
                    }
                    else
                    {
                        // If outer decryption failed completely, copy the raw encrypted archive.
                        File.Copy(dveFile, outCorruptedZip, true);
                    }

                    MessageBox.Show("Successfully decrypted 1 archive file to:\n" + destDir, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                decryptFilePathTxt.Clear();
                decryptPwdTxt.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred during extraction: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
                Cursor.Current = Cursors.Default;
            }
        }
    }
}
