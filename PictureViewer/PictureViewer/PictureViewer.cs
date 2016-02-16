//2016.01.30 Previous and After button.
//  Problem: How to remove zero lenght item of the file in Files List.
//           Response when the directory changed.
using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;

namespace PictureViewer
{
    public partial class PictureViewer : Form
    {
        Dictionary<string, Assembly> _libs = new Dictionary<string, Assembly>();
        // Get the length of sting. 
        /*[DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern Int32 StrFormatByteSize(
            long fileSize,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer,
            int bufferSize);
            */

        // PictureBoxes we use to display thumbnails.
        private List<PictureBox> PictureBoxes = new List<PictureBox>();
        // Thumbnail sizes.
        private const int ThumbWidth = 140;
        private const int ThumbHeight = 140;
        // The selected PictureBox.
        private PictureBox SelectedPictureBox = null;

        // Declare list string of filenames.
        private List<string> pbImageDirFilenames = new List<string>();
        // Declare iterate count of list pbImageDirFilenames.
        private int pbImageIter = 0;
        private string selectedFloderPath = "";

        /// <summary>
        /// Constructor
        /// </summary>
        public PictureViewer()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.AssemblyResolve += FindDLL;
        }

        private void PictureViewer_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// PictureViewer key down
        /// Ctrl+O, Ctrl+s, Ctrl+Shift+s response from keyboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureViewer_KeyDown(object sender, KeyEventArgs e)
        {

            // Open file, Ctrl+O and Ctrl+Shift+O
            if (e.Control && e.KeyCode == Keys.O)
            {
                // Ctrl+Shift+O
                if (e.Shift) { openFolderToolStripMenuItem_Click(sender, e); }
                // Ctrl+O
                else { openFileToolStripMenuItem_Click(sender, e); }
            }

            // Save file, Ctrl+s
            if (e.Control && e.KeyCode == Keys.S) { saveFileToolStripMenuItem_Click(sender, e); }

            // Save as, Ctrl+Shift+s
            if(e.Control && e.Shift && e.KeyCode == Keys.S) { saveAsToolStripMenuItem_Click(sender, e); }
        }
        /// <summary>
        /// Open the choosed file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = "Open File...";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                //test file existing or not
                if (File.Exists(openFile.FileName))
                {
                    //check file fromat though method
                    if (checkImageFileFormat(openFile.FileName)) { getDirFiles(openFile.FileName); }
                    else { MessageBox.Show(string.Format("{0} file format is invaild.", openFile.FileName)); }
                }
                else { MessageBox.Show(string.Format("{0} file is not exist.", openFile.FileName)); }
            }
        }
        /// <summary>
        /// Open files from choose folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFolder = new FolderBrowserDialog();
            if (openFolder.ShowDialog() == DialogResult.OK)
            {
                if (Directory.Exists(openFolder.SelectedPath))
                {
                    selectedFloderPath = openFolder.SelectedPath;
                    imageThumbnailArea(selectedFloderPath);
                }
                else { MessageBox.Show(string.Format("{0} directory is not exist.", openFolder.SelectedPath)); }
            }
        }
        /// <summary>
        /// Display image file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displayImageFile(string openFile)
        {
            // Remove flpThumbnails
            try {
                flpThumbnails.Visible = false;
                pbImage.Visible = true;
                label1.Visible = false;
            } finally { }
            

            //reset picturebox properties
            //correct format,and out of memorry,show message memory out via MessageBox 
            try
            {
                //Image newImage = new Bitmap(openFile);
                Image newImage = null;
                using (FileStream stream = new FileStream(openFile, FileMode.Open))
                {
                    if (stream.Length != 0)
                    {
                        newImage = Image.FromStream(stream);
                    }
                    else {
                        this.ClientSize = new System.Drawing.Size(600, 629);
                        pbImage.Visible = false;
                        label1.Visible = true;
                    }
                }

                if(newImage != null)
                {
                    this.pbImage.Location = new System.Drawing.Point(0, 29);
                    this.pbImage.Size = new System.Drawing.Size(284, 229);
                    this.pbImage.TabIndex = 1;
                    this.pbImage.TabStop = false;
                    this.pbImage.Image = newImage;
                    this.pbImage.Height = newImage.Height;
                    this.pbImage.Width = newImage.Width;
                    this.pbImage.Name = openFile;
                    this.ClientSize = new System.Drawing.Size(newImage.Width, newImage.Height + 29);
                    this.Text = openFile + "   " + "Picture Viewer";
                }
            }
            catch (OutOfMemoryException e) { MessageBox.Show(string.Format("Out of Memory: {0}", e.Message)); }

        }
        /// <summary>
        /// Display thumbnail for image;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imageThumbnailArea(string folderPath)
        {
            // Try remove pbImage adn reset CientSize
            try {
                pbImage.Visible = false;
                flpThumbnails.Visible = true;
                label1.Visible = false;
            } finally { }

            this.ClientSize = new System.Drawing.Size(600, 629);

            #region Add flpThumbnail and set the attribute
            // 
            // flpThumbnails
            //
            this.flpThumbnails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top
                | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.flpThumbnails.AutoScroll = true;
            //this.flpThumbnails.BackColor = System.Drawing.Color.LightGreen;
            this.flpThumbnails.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.flpThumbnails.Location = new System.Drawing.Point(0, 29);
            this.flpThumbnails.Name = folderPath;
            this.flpThumbnails.Size = new System.Drawing.Size(600, 600);
            this.flpThumbnails.TabIndex = 3;
            #endregion

            // Delete the old PictureBoxes.
            foreach (PictureBox pic in PictureBoxes)
            {
                pic.Click -= PictureBox_Click;
                pic.Dispose();
            }
            flpThumbnails.Controls.Clear();
            PictureBoxes = new List<PictureBox>();
            SelectedPictureBox = null;

            // List PictrueBox to show images thumbnail
            var images = getImagesInFloder(folderPath);

            foreach (var image in images)
            {
                if (File.Exists(image))
                {
                    Boolean fileHidden = fileAttrHidden(image);
                    
                    if (!fileHidden){ displayImageThumbnail(image);}
                    else { continue; }
                    
                }
            }
        }
        /// <summary>
        /// Get the images in the foler, just three images fromat: .gif .png, .jpg, tif.
        /// </summary>
        /// <param name="folderPath"></param>
        private List<string> getImagesInFloder(string folderPath)
        {
            string[] patterns = { ".png", ".gif", ".jpg", ".jpeg", ".bmp", ".tif" };
            string[] fileInfos = Directory.GetFiles(folderPath);
            NumericComparer ns = new NumericComparer();
            Array.Sort(fileInfos, ns);

            List<string> images = new List<string>();
            foreach(string file in fileInfos)
            {
                FileAttributes attrs = File.GetAttributes(file);
                if ((attrs & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    if (patterns.Contains(Path.GetExtension(file).ToLower())) images.Add(file);
                }
            }

            return images;
        }
        /// <summary>
        /// Chcek the name suffix of the file name. 
        /// Return bool type, .JPG, .JPEG, .PNG, .TIF is ture, else is false.
        /// </summary>
        /// <param name="imageFilePath"></param>
        /// <returns></returns>
        private Boolean checkImageFileFormat(string imagePath)
        {
            if (imagePath.ToUpper().EndsWith(".JPG") ||
                imagePath.ToUpper().EndsWith(".JPEG") ||
                imagePath.ToUpper().EndsWith(".PNG") ||
                imagePath.ToUpper().EndsWith(".TIF"))
            {
                return true;
            }
            return false;
        }
        // Select the clicked PictureBox.
        private void PictureBox_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            if (SelectedPictureBox == pic) return;

            // Deselect the previous PictureBox.
            if (SelectedPictureBox != null) SelectedPictureBox.BorderStyle = BorderStyle.None;

            // Select the clicked PictureBox.
            SelectedPictureBox = pic;
            SelectedPictureBox.BorderStyle = BorderStyle.FixedSingle;
        }
        // Get dir files.
        private void getDirFiles(string file)
        {
            pbImageDirFilenames = getImagesInFloder(Path.GetDirectoryName(file));
            pbImageIter = pbImageDirFilenames.FindIndex(f => f == file);
            displayImageFile(file);
        }
        // Double clicked PictureBox
        private void PictureBox_DoubleClick(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            string picpath = SelectedPictureBox.Name;
            getDirFiles(pic.Name);
        }
        
        /// <summary>
        /// Check file hidden attribute.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private Boolean fileAttrHidden(string filePath)
        {
            FileAttributes attrs = File.GetAttributes(filePath);
            if ( ( attrs & FileAttributes.Hidden ) == FileAttributes.Hidden) { return true; } else { return false; }
        }
        /// <summary>
        /// Dispaly thumbmail for image.
        /// </summary>
        /// <param name="filePath"></param>
        private void displayImageThumbnail(string filePath)
        {
            // Load the picture into a PictureBox.
            PictureBoxCtrl pic = new PictureBoxCtrl();

            pic.ClientSize = new Size(ThumbWidth, ThumbHeight);

            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                if (stream.Length != 0) { pic.Image = Image.FromStream(stream); }
                else { return; }
            }

            pic.Name = filePath;

            // If the image is too big, zoom.
            if ((pic.Image.Width > ThumbWidth) ||
                (pic.Image.Height > ThumbHeight))
            {
                pic.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                pic.SizeMode = PictureBoxSizeMode.CenterImage;
            }

            // Add the Click event handler.
            pic.DoubleClick += PictureBox_DoubleClick;
            pic.Click += PictureBox_Click;

            // Add a tooltip.
            FileInfo file_info = new FileInfo(filePath);
            
            pic.Tag = file_info;

            // Add the PictureBox to the FlowLayoutPanel.
            pic.Parent = flpThumbnails;
        }
        /// <summary>
        /// Save file to choose path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.pbImage.Name != "")
            {
                ImageFormat extension = getFileFromat(this.pbImage.Name);
                try
                {
                    ImageConverter imageConverter = new ImageConverter();
                    byte[]  imageBytes = (byte[]) imageConverter.ConvertTo( new Bitmap(this.pbImage.Image), typeof( byte[] ) );
                    File.WriteAllBytes(this.pbImage.Name, imageBytes);
                }
                catch (SecurityException se)
                { MessageBox.Show(string.Format("Security Exception:\n\n{}", se.Message)); }
            }
        }
        /// <summary>
        /// Action of save as.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = @"All files (*.*)|*.*|Bitmap Image (.bmp)|*.bmp|Gif Image (.gif)|*.gif|JPEG Image (.jpeg)|*.jpeg
|Png Image (.png)|*.png|Tiff Image (.tiff)|*.tiff";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;
                dialog.Title = "Save As...";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    //MessageBox.Show(string.Format("{0}",dialog.FileName));
                    this.pbImage.Name = dialog.FileName;
                    saveFileToolStripMenuItem_Click(sender, e);
                    this.Text = dialog.FileName + "   " + "Picture Viewer";

                }
            }
        }
        /// <summary>
        /// Get image file format.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private ImageFormat getFileFromat(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            switch(extension)
            {
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".png":
                    return ImageFormat.Png;
                case ".tif":
                    return ImageFormat.Tiff;
                case ".gif":
                    return ImageFormat.Gif;
                default:
                    return ImageFormat.Jpeg;
            }
        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        // Display before file
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (flpThumbnails.Visible != true)
            {
                if (pbImageIter != 0) pbImageIter--;
                else pbImageIter = pbImageDirFilenames.Count() - 1;
                displayImageFile(pbImageDirFilenames[pbImageIter]);
            }
        }
        // Display post file
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (flpThumbnails.Visible != true)
            {
                if (pbImageIter == pbImageDirFilenames.Count() - 1) pbImageIter = 0;
                else pbImageIter++;
                displayImageFile(pbImageDirFilenames[pbImageIter]);
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (this.pbImage.Name != "pbImage")
            {
                if (this.flpThumbnails.Name != "")
                {
                    this.flpThumbnails.Visible = false;
                    this.pbImage.Visible = true;
                    this.ClientSize = new System.Drawing.Size(pbImage.Width, pbImage.Height + 29);
                }
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if(this.flpThumbnails.Name != "flpThumbnails")
            {
                if(this.pbImage.Name != "")
                {
                    this.pbImage.Visible = false;
                    this.flpThumbnails.Visible = true;
                    this.ClientSize = new System.Drawing.Size(600, 629);
                }
            }
        }

        private Assembly FindDLL(object sender, ResolveEventArgs args)
        {
            string keyName = new AssemblyName(args.Name).Name;

            // If DLL is loaded then don't load it again just return
            if (_libs.ContainsKey(keyName)) return _libs[keyName];

            // <-- To find out the Namespace name go to Your Project >> Properties >> Application >> Default namespace
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PictureViewer.Shlwapi.dll"))
            {
                byte[] buffer = new BinaryReader(stream).ReadBytes((int)stream.Length);
                Assembly assembly = Assembly.Load(buffer);
                _libs[keyName] = assembly;
                return assembly;
            }
        }
    }
}