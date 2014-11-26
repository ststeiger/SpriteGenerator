
using System.Linq;
using System.Windows.Forms;


// https://spritegenerator.codeplex.com/
// https://github.com/ststeiger/SpriteGenerator/tree/master/SpriteGenerator
// http://stackoverflow.com/questions/448981/what-characters-are-valid-in-css-class-selectors
// http://www.fclose.com/39627/how-to-compressuncompress-files-in-linux-using-gzip-bzip2-7z-rar-and-zip/
namespace SpriteGenerator
{


    public partial class SpritesForm : System.Windows.Forms.Form
    {


        private bool[] buttonGenerateEnabled = new bool[3];
        private LayoutProperties layoutProp = new LayoutProperties();
        public bool done = false;


        public SpritesForm()
        {
            InitializeComponent();
            layoutProp.layout = radioButtonAutomaticLayout.Text;
        }


        //Generate button click event. Start generating output image and CSS file.
        private void buttonGenerate_Click(object sender, System.EventArgs e)
        {
            layoutProp.outputSpriteFilePath = textBoxOutputImageFilePath.Text;
            layoutProp.outputCssFilePath = textBoxOutputCSSFilePath.Text;
            layoutProp.distanceBetweenImages = (int)numericUpDownDistanceBetweenImages.Value;
            layoutProp.marginWidth = (int)numericUpDownMarginWidth.Value;
            Sprite sprite = new Sprite(layoutProp);
            sprite.Create();
            //Sprite sprite = new Sprite(inputFilePaths, textBoxOutputImageFilePath.Text, textBoxOutputCSSFilePath.Text, layout,
            //    (int)numericUpDownDistanceBetweenImages.Value, (int)numericUpDownMarginWidth.Value, imagesInRow, imagesInColumn);
            this.Close();
        }








        //Browse input images folder.
		private void buttonBrowseFolder_Click(object sender, System.EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBoxInputDirectoryPath.Text = folderBrowserDialog.SelectedPath;
                ImageFolderSet();
            }

        }


        public void ImageFolderSet()
        {
            string[] filters = { ".png", ".jpg", ".jpeg", ".gif" };
            layoutProp.inputFilePaths = (from filter in filters
                                         from file in System.IO.Directory.GetFiles(this.textBoxInputDirectoryPath.Text)
                                         where file.EndsWith(filter)
                                         select file).ToArray();
            //If there is no file with the enabled formats in the choosen directory.
            if (layoutProp.inputFilePaths.Length == 0)
                System.Windows.Forms.MessageBox.Show("This directory does not contain image files.");

            //If there are files with the enabled formats in the choosen directory.
            else
            {
                buttonGenerateEnabled[0] = true;
                buttonGenerate.Enabled = buttonGenerateEnabled.All(element => element == true);

                radioButtonAutomaticLayout.Checked = true;
                int width = System.Drawing.Image.FromFile(layoutProp.inputFilePaths[0]).Width;
                int height = System.Drawing.Image.FromFile(layoutProp.inputFilePaths[0]).Height;

                //Horizontal layout radiobutton is enabled only when all image heights are the same.
                radioButtonHorizontalLayout.Enabled = layoutProp.inputFilePaths.All(
                    file => System.Drawing.Image.FromFile(file).Height == height);

                //Vertical layout radiobutton is enabled only when all image widths are the same.
                radioButtonVerticalLayout.Enabled = layoutProp.inputFilePaths.All(
                    file => System.Drawing.Image.FromFile(file).Width == width);

                //Rectangular layout radiobutton is enabled only when all image heights and all image widths are the same.
                radioButtonRectangularLayout.Enabled =
                       radioButtonHorizontalLayout.Enabled
                    && radioButtonVerticalLayout.Enabled;

                //Setting rectangular layout dimensions.
                if (radioButtonRectangularLayout.Enabled)
                {
                    numericUpDownImagesInRow.Minimum = 1;
                    numericUpDownImagesInRow.Maximum = layoutProp.inputFilePaths.Length;
                    layoutProp.imagesInRow = (int)numericUpDownImagesInRow.Value;
                    layoutProp.imagesInColumn = (int)numericUpDownImagesInColumn.Value;
                }
                else
                {
                    numericUpDownImagesInRow.Minimum = 0;
                    numericUpDownImagesInColumn.Minimum = 0;
                    numericUpDownImagesInRow.Value = 0;
                    numericUpDownImagesInColumn.Value = 0;
                }
            }
        }


        //Select output image file path.
		private void buttonSelectOutputImageFilePath_Click(object sender, System.EventArgs e)
        {
            saveFileDialogOutputImage.ShowDialog();
            if (saveFileDialogOutputImage.FileName != "")
            {
                if (buttonGenerateEnabled[2] && textBoxOutputCSSFilePath.Text[0] != saveFileDialogOutputImage.FileName[0])
					System.Windows.Forms.MessageBox.Show("Output image and CSS file must be on the same drive.");
                else
                {
                    this.textBoxOutputImageFilePath.Text = saveFileDialogOutputImage.FileName;
                    buttonGenerateEnabled[1] = true;
                    buttonGenerate.Enabled = buttonGenerateEnabled.All(element => element == true);
                }
            }
        }


        //Select output CSS file path.
		private void buttonSelectOutputCssFilePath_Click(object sender, System.EventArgs e)
        {
            saveFileDialogOutputCss.ShowDialog();
            if (saveFileDialogOutputCss.FileName != "")
            {
                if (buttonGenerateEnabled[1] && 
                    textBoxOutputImageFilePath.Text[0] != saveFileDialogOutputCss.FileName[0])
					System.Windows.Forms.MessageBox.Show("Output image and CSS file must be on the same drive.");
                else
                {
                    this.textBoxOutputCSSFilePath.Text = saveFileDialogOutputCss.FileName;
                    buttonGenerateEnabled[2] = true;
                    buttonGenerate.Enabled = buttonGenerateEnabled.All(element => element == true);
                }
            }
        }


        //Rectangular layout radiobutton checked change.
		private void radioButtonRectangularLayout_CheckedChanged(object sender, System.EventArgs e)
        {
            radioButtonLayout_CheckedChanged(sender, e);
            //Enabling numericupdowns to select layout dimension.
            if (radioButtonRectangularLayout.Checked)
            {
                numericUpDownImagesInRow.Enabled = true;
                numericUpDownImagesInColumn.Enabled = true;
                labelX.Enabled = true;
                labelSprites.Enabled = true;
                numericUpDownImagesInRow.Maximum = layoutProp.inputFilePaths.Length;
            }
            //Disabling numericupdowns
            else
            {
                numericUpDownImagesInRow.Enabled = false;
                numericUpDownImagesInColumn.Enabled = false;
                labelX.Enabled = false;
                labelSprites.Enabled = false;
            }
        }


        //Checked change event for all layout radiobutton.
		private void radioButtonLayout_CheckedChanged(object sender, System.EventArgs e)
        {
            //Setting layout field value.
			if (((System.Windows.Forms.RadioButton)sender).Checked)
				layoutProp.layout = ((System.Windows.Forms.RadioButton)sender).Text;
        }


        //Sprites in row numericupdown value changed event
		private void numericUpDownImagesInRow_ValueChanged(object sender, System.EventArgs e)
        {
            int numberOfFiles = layoutProp.inputFilePaths.Length;
            //Setting sprites in column numericupdown value
            numericUpDownImagesInColumn.Minimum = numberOfFiles / (int)numericUpDownImagesInRow.Value;
            numericUpDownImagesInColumn.Minimum += (numberOfFiles % (int)numericUpDownImagesInRow.Value) > 0 ? 1 : 0;
            numericUpDownImagesInColumn.Maximum = numericUpDownImagesInColumn.Minimum;
            
            layoutProp.imagesInRow = (int)numericUpDownImagesInRow.Value;
            layoutProp.imagesInColumn = (int)numericUpDownImagesInColumn.Value;
        }


        private void textBoxOutputImageFilePath_TextChanged(object sender, System.EventArgs e)
        {
            if (this.CanGenerateImage)
                this.buttonGenerate.Enabled = true;
        }


        private void textBoxOutputCSSFilePath_TextChanged(object sender, System.EventArgs e)
        {
            if (this.CanGenerateImage)
                this.buttonGenerate.Enabled = true;
        }


        private void textBoxInputDirectoryPath_TextChanged(object sender, System.EventArgs e)
        {
            if (System.IO.Directory.Exists(textBoxInputDirectoryPath.Text))
                ImageFolderSet();

            if (this.CanGenerateImage)
                this.buttonGenerate.Enabled = true;
        }


        public bool CanGenerateImage
        {
            get{
                return (   !string.IsNullOrEmpty(textBoxOutputImageFilePath.Text) 
                    && !string.IsNullOrEmpty(textBoxOutputCSSFilePath.Text) 
                    && !string.IsNullOrEmpty(textBoxInputDirectoryPath.Text)
                    && System.IO.Directory.Exists(textBoxInputDirectoryPath.Text)
                    );
            }
        }


        private void SpritesForm_Load(object sender, System.EventArgs e)
        {
            string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            this.textBoxOutputImageFilePath.Text = System.IO.Path.Combine(dir, "sprite.png");
            this.textBoxOutputCSSFilePath.Text = System.IO.Path.Combine(dir, "sprite.css");
            this.textBoxInputDirectoryPath.Text = System.IO.Path.GetFullPath(System.IO.Path.Combine(dir, "../../TestImages"));
        }
        

	} // End Class SpritesForm : Form


} // End Namespace SpriteGenerator
