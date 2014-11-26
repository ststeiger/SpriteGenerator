
using System.Linq;
//using SpriteGenerator.Utility;


namespace SpriteGenerator
{
    class Sprite
    {
		private System.Collections.Generic.Dictionary<int, System.Drawing.Image> images;
		private System.Collections.Generic.Dictionary<int, string> cssClassNames;
        private LayoutProperties layoutProp;

        public Sprite(LayoutProperties _layoutProp)
        {
			images = new System.Collections.Generic.Dictionary<int, System.Drawing.Image>();
			cssClassNames = new System.Collections.Generic.Dictionary<int, string>();
            layoutProp = _layoutProp;
        }

        public void Create()
        {
            GetData(out images, out cssClassNames);

			System.IO.StreamWriter cssFile = System.IO.File.CreateText(layoutProp.outputCssFilePath);
			System.Drawing.Image resultSprite = null;


            string SpriteImage = relativeSpriteImagePath(layoutProp.outputSpriteFilePath, layoutProp.outputCssFilePath);
            SpriteImage = System.IO.Path.GetFileName(SpriteImage);


            cssFile.WriteLine(".sprite { background-image: url('" + SpriteImage + "'); background-color: transparent; background-repeat: no-repeat; }");

            switch (layoutProp.layout)
            {
                case "Automatic":
                    resultSprite = generateAutomaticLayout(cssFile);
                    break;
                case "Horizontal":
                    resultSprite = generateHorizontalLayout(cssFile);
                    break;
                case "Vertical":
                    resultSprite = generateVerticalLayout(cssFile);
                    break;
                case "Rectangular":
                    resultSprite = generateRectangularLayout(cssFile);
                    break;
                default:
                    break;
            }

            cssFile.Close();
			System.IO.FileStream outputSpriteFile = new System.IO.FileStream(layoutProp.outputSpriteFilePath
			                                                                 , System.IO.FileMode.Create);
			resultSprite.Save(outputSpriteFile, System.Drawing.Imaging.ImageFormat.Png);
            outputSpriteFile.Close();
        }

        /// <summary>
        /// Creates dictionary of images from the given paths and dictionary of CSS classnames from the image filenames.
        /// </summary> 
        /// <param name="inputFilePaths">Array of input file paths.</param>
        /// <param name="images">Dictionary of images to be inserted into the output sprite.</param>
        /// <param name="cssClassNames">Dictionary of CSS classnames.</param>
		private void GetData(out System.Collections.Generic.Dictionary<int, System.Drawing.Image> images
		                     , out System.Collections.Generic.Dictionary<int, string> cssClassNames)
        {
			images = new System.Collections.Generic.Dictionary<int, System.Drawing.Image>();
			cssClassNames = new System.Collections.Generic.Dictionary<int, string>();

            for (int i = 0; i < layoutProp.inputFilePaths.Length; i++)
            {
				System.Drawing.Image img = System.Drawing.Image.FromFile(layoutProp.inputFilePaths[i]);
                images.Add(i, img);
                string[] splittedFilePath = layoutProp.inputFilePaths[i].Split('\\');
                cssClassNames.Add(i, splittedFilePath[splittedFilePath.Length - 1].Split('.')[0]);
            }
        }

		private System.Collections.Generic.List<SpriteGenerator.Utility.Module> CreateModules()
        {
			System.Collections.Generic.List<SpriteGenerator.Utility.Module> modules = 
				new System.Collections.Generic.List<SpriteGenerator.Utility.Module>();
            foreach (int i in images.Keys)
				modules.Add(new SpriteGenerator.Utility.Module(i, images[i], layoutProp.distanceBetweenImages));
            return modules;
        }

        //CSS line
		private string CssLine(string cssClassName, System.Drawing.Rectangle rectangle)
        {
			cssClassName = System.IO.Path.GetFileNameWithoutExtension (cssClassName);
            cssClassName = cssClassName.Replace(" ", "_");

            string strInvalid = "~!@$%^&*()+=,./';:?><[]{}|`#\"\\";
            foreach(char chr in strInvalid)
            {
                cssClassName = cssClassName.Replace(chr.ToString(),"");
            }


            string line = "." + cssClassName + " { width: " + rectangle.Width.ToString() + "px; height: " + rectangle.Height.ToString() + 
                "px; background-position: " + (-1 * rectangle.X).ToString() + "px " + (-1 * rectangle.Y).ToString() + "px; }";
            return line;
        }

        //Relative sprite image file path
        private string relativeSpriteImagePath(string outputSpriteFilePath, string outputCssFilePath)
        {
            string[] splittedOutputCssFilePath = outputCssFilePath.Split('\\');
            string[] splittedOutputSpriteFilePath = outputSpriteFilePath.Split('\\');

            int breakAt = 0;
            for (int i = 0; i < splittedOutputCssFilePath.Length; i++)
                if (i < splittedOutputSpriteFilePath.Length && splittedOutputCssFilePath[i] != splittedOutputSpriteFilePath[i])
                {
                    breakAt = i;
                    break;
                }

            string relativePath = "";
            for (int i = 0; i < splittedOutputCssFilePath.Length - breakAt - 1; i++)
                relativePath += "../";
            relativePath += string.Join("/", splittedOutputSpriteFilePath, breakAt, splittedOutputSpriteFilePath.Length - breakAt);

            return relativePath;
        }

        //Automatic layout
		private System.Drawing.Image generateAutomaticLayout(System.IO.StreamWriter cssFile)
        {
            var sortedByArea = from m in CreateModules()
                               orderby m.Width * m.Height descending
                               select m;
			System.Collections.Generic.List<SpriteGenerator.Utility.Module> moduleList = 
				sortedByArea.ToList<SpriteGenerator.Utility.Module>();
			Placement placement = SpriteGenerator.Utility.Algorithm.Greedy(moduleList);

            //Creating an empty result image.
			System.Drawing.Image resultSprite = 
				new System.Drawing.Bitmap(placement.Width - layoutProp.distanceBetweenImages 
				                          + 2 * layoutProp.marginWidth,
                placement.Height - layoutProp.distanceBetweenImages + 2 * layoutProp.marginWidth);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(resultSprite);
            
            //Drawing images into the result image in the original order and writing CSS lines.
			foreach (SpriteGenerator.Utility.Module m in placement.Modules)
            {
                m.Draw(graphics, layoutProp.marginWidth);
				System.Drawing.Rectangle rectangle = 
					new System.Drawing.Rectangle(m.X + layoutProp.marginWidth, m.Y + layoutProp.marginWidth,
                    m.Width - layoutProp.distanceBetweenImages, m.Height - layoutProp.distanceBetweenImages);
                cssFile.WriteLine(CssLine(cssClassNames[m.Name], rectangle));
            }

            return resultSprite;
        }

        //Horizontal layout
		private System.Drawing.Image generateHorizontalLayout(System.IO.StreamWriter cssFile)
        {
            //Calculating result image dimension.
            int width = 0;
			foreach (System.Drawing.Image image in images.Values)
                width += image.Width + layoutProp.distanceBetweenImages;
            width = width - layoutProp.distanceBetweenImages + 2 * layoutProp.marginWidth;
            int height = images[0].Height + 2 * layoutProp.marginWidth;

            //Creating an empty result image.
			System.Drawing.Image resultSprite = new System.Drawing.Bitmap(width, height);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(resultSprite);
            
            //Initial coordinates.
            int actualXCoordinate = layoutProp.marginWidth;
            int yCoordinate = layoutProp.marginWidth;

            //Drawing images into the result image, writing CSS lines and increasing X coordinate.
            foreach(int i in images.Keys)
            {
				System.Drawing.Rectangle rectangle = 
					new System.Drawing.Rectangle(actualXCoordinate, yCoordinate, images[i].Width, images[i].Height);
                graphics.DrawImage(images[i], rectangle);
                cssFile.WriteLine(CssLine(cssClassNames[i], rectangle));
                actualXCoordinate += images[i].Width + layoutProp.distanceBetweenImages;
            }

            return resultSprite;
        }

        //Vertical layout
		private System.Drawing.Image generateVerticalLayout(System.IO.StreamWriter cssFile)
        {
            //Calculating result image dimension.
            int height = 0;
			foreach (System.Drawing.Image image in images.Values)
                height += image.Height + layoutProp.distanceBetweenImages;
            height = height - layoutProp.distanceBetweenImages + 2 * layoutProp.marginWidth;
            int width = images[0].Width + 2 * layoutProp.marginWidth;

            //Creating an empty result image.
			System.Drawing.Image resultSprite = new System.Drawing.Bitmap(width, height);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(resultSprite);
            
            //Initial coordinates.
            int actualYCoordinate = layoutProp.marginWidth;
            int xCoordinate = layoutProp.marginWidth;

            //Drawing images into the result image, writing CSS lines and increasing Y coordinate.
            foreach (int i in images.Keys)
            {
				System.Drawing.Rectangle rectangle = 
					new System.Drawing.Rectangle(xCoordinate, actualYCoordinate, images[i].Width, images[i].Height);
                graphics.DrawImage(images[i], rectangle);
                cssFile.WriteLine(CssLine(cssClassNames[i], rectangle));
                actualYCoordinate += images[i].Height + layoutProp.distanceBetweenImages;
            }

            return resultSprite;
        }

		private System.Drawing.Image generateRectangularLayout(System.IO.StreamWriter CSSFile)
        {
            //Calculating result image dimension.
            int imageWidth = images[0].Width;
            int imageHeight = images[0].Height;
            int width = layoutProp.imagesInRow * (imageWidth + layoutProp.distanceBetweenImages) -
                layoutProp.distanceBetweenImages + 2 * layoutProp.marginWidth;
            int height = layoutProp.imagesInColumn * (imageHeight + layoutProp.distanceBetweenImages) -
                layoutProp.distanceBetweenImages + 2 * layoutProp.marginWidth;

            //Creating an empty result image.
			System.Drawing.Image resultSprite = new System.Drawing.Bitmap(width, height);
			System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(resultSprite);

            //Initial coordinates.
            int actualYCoordinate = layoutProp.marginWidth;
            int actualXCoordinate = layoutProp.marginWidth;

            //Drawing images into the result image, writing CSS lines and increasing coordinates.
            for (int i = 0; i < layoutProp.imagesInColumn; i++)
            {
                for (int j = 0; (i * layoutProp.imagesInRow) + j < images.Count && j < layoutProp.imagesInRow; j++)
                {
					System.Drawing.Rectangle rectangle = 
						new System.Drawing.Rectangle(actualXCoordinate, actualYCoordinate, imageWidth, imageHeight);
                    graphics.DrawImage(images[i * layoutProp.imagesInRow + j], rectangle);
                    CSSFile.WriteLine(CssLine(cssClassNames[i * layoutProp.imagesInRow + j], rectangle));
                    actualXCoordinate += imageWidth + layoutProp.distanceBetweenImages;
                }
                actualYCoordinate += imageHeight + layoutProp.distanceBetweenImages;
                actualXCoordinate = layoutProp.marginWidth;
            }

            return resultSprite;
        }
    }
}
