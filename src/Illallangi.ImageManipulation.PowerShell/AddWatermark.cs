// -----------------------------------------------------------------------
// <copyright file="AddWatermark.cs" company="Illallangi Enterprises">
// Copyright (C) 2013 Illallangi Enterprises
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Management.Automation;
using System.Drawing;
using System.Reflection;

namespace Illallangi.ImageManipulation.PowerShell
{
    [Cmdlet("Add", "Watermark")]
    public class AddWatermark : PSCmdlet
    {
        #region Fields

        /// <summary>
        /// Holds the current value of the Input property.
        /// </summary>
        private string currentInput;

        /// <summary>
        /// Holds the current value of the InputBitmap property.
        /// </summary>
        private Bitmap currentInputBitmap;

        /// <summary>
        /// Holds the current value of the InputGraphics property.
        /// </summary>
        private Graphics currentInputGraphics;

        /// <summary>
        /// Holds the current value of the Watermark property.
        /// </summary>
        private string currentWatermark;

        /// <summary>
        /// Holds the current value of the WatermarkBitmap property.
        /// </summary>
        private Bitmap currentWatermarkBitmap;

        /// <summary>
        /// Holds the current value of the WatermarkGraphics property.
        /// </summary>
        private Graphics currentWatermarkGraphics;

        /// <summary>
        /// Holds the current value of the WatermarkWidth property.
        /// </summary>
        private float? currentWatermarkWidth;

        /// <summary>
        /// Holds the current value of the WatermarkHeight property.
        /// </summary>
        private float? currentWatermarkHeight;

        #endregion

        #region Constructors

        public AddWatermark()
        {
            this.Opacity = 100;
            this.Output = "{0}-{1}.{2}";
            this.OutputFormat = ImageFormat.Jpeg;
            this.WatermarkX = 0;
            this.WatermarkY = 0;
        }

        #endregion

        #region Properties

        #region Private Properties
        
        private Bitmap InputBitmap
        {
            get
            {
                return string.IsNullOrEmpty(this.Input)
                           ? null
                           : (this.currentInputBitmap ??
                                (this.currentInputBitmap = AddWatermark.GetBitmap(Path.GetFullPath(Path.Combine(this.SessionState.Path.CurrentFileSystemLocation.Path, this.Input)))));
            }
        }

        private Graphics InputGraphics
        {
            get
            {
                return string.IsNullOrEmpty(this.Input)
                           ? null
                           : (this.currentInputGraphics ?? 
                                (this.currentInputGraphics = Graphics.FromImage(this.InputBitmap)));
            }
        }

        private float InputWidth
        {
            get { return this.InputBitmap.Width; }
        }

        private float InputHeight
        {
            get { return this.InputBitmap.Height; }
        }
        
        private Bitmap WatermarkBitmap
        {
            get
            {
                return string.IsNullOrEmpty(this.Watermark)
                           ? null
                           : (this.currentWatermarkBitmap ??
                                (this.currentWatermarkBitmap = AddWatermark.GetBitmap(Path.GetFullPath(Path.Combine(this.SessionState.Path.CurrentFileSystemLocation.Path, this.Watermark)))));
            }
        }

        private Graphics WatermarkGraphics
        {
            get
            {
                return string.IsNullOrEmpty(this.Watermark)
                           ? null
                           : (this.currentWatermarkGraphics ?? 
                                (this.currentWatermarkGraphics = Graphics.FromImage(this.WatermarkBitmap)));
            }
        }
        
        #endregion

        #region Public Properties

        [Parameter(ValueFromPipeline = false, ValueFromPipelineByPropertyName = false, Mandatory = true)]
        public string Input
        {
            get { return this.currentInput; }
            set
            {
                if (this.currentInput == value) return;
                this.currentInput = value;
                this.currentInputBitmap = null;
                this.currentInputGraphics = null;
            }
        }
        
        [Parameter(ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public int Opacity 
        { 
            get
            {
                return 100;
            }
            set
            {
                throw new NotImplementedException();
            } 
        }

        [Parameter(ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public string Output { get; set; }

        [Parameter(ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public ImageFormat OutputFormat { get; set; }

        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string Watermark
        {
            get { return this.currentWatermark; }
            set
            { 
                if (this.currentWatermark == value) return;
                this.currentWatermark = value;
                this.currentWatermarkBitmap = null;
                this.currentWatermarkGraphics = null;
            }
        }

        [Parameter(ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public float WatermarkWidth
        {
            get
            {
                if (this.currentWatermarkWidth.HasValue)
                {
                    return this.currentWatermarkWidth.Value;
                }
                if (this.currentWatermarkHeight.HasValue)
                {
                    return (this.currentWatermarkHeight.Value/this.WatermarkBitmap.Height)*this.WatermarkBitmap.Width;
                }
                return this.WatermarkBitmap.Width;
            }
            set { this.currentWatermarkWidth = value; }
        }

        [Parameter(ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public float WatermarkHeight
        {
            get
            {
                if (this.currentWatermarkHeight.HasValue)
                {
                    return this.currentWatermarkHeight.Value;
                }
                if (this.currentWatermarkWidth.HasValue)
                {
                    return (this.currentWatermarkWidth.Value / this.WatermarkBitmap.Width) * this.WatermarkBitmap.Height;
                }
                return this.WatermarkBitmap.Height;
            }
            set { this.currentWatermarkHeight = value; }
        }

        [Parameter(ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public int WatermarkX { get; set; }

        [Parameter(ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        public int WatermarkY { get; set; }

        #endregion

        #endregion

        #region Methods

        #region Protected Methods

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            using (var bitmap = new Bitmap(this.InputBitmap))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var x = ((this.InputWidth - this.WatermarkWidth) * this.WatermarkX) / 100;
                var y = ((this.InputHeight - this.WatermarkHeight) * this.WatermarkY) / 100;
                graphics.DrawImage(this.WatermarkBitmap,
                                   x,
                                   y,
                                   this.WatermarkWidth,
                                   this.WatermarkHeight);
                bitmap.Save(Path.Combine(this.SessionState.Path.CurrentFileSystemLocation.Path,
                                         string.Format(this.Output, Path.GetFileNameWithoutExtension(this.Input),
                                                       Path.GetFileNameWithoutExtension(this.Watermark),
                                                       this.OutputFormat.ToString().ToLower())),
                            this.OutputFormat);
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

        #endregion

        #region Private Methods

        private static Bitmap GetBitmap(string file)
        {
            Bitmap bitmap;
            var stream = AddWatermark.GetStream(file);
            try
            {
                bitmap = new Bitmap(stream);
            }
            finally
            {
                bool flag = stream == null;
                if (!flag)
                {
                    stream.Dispose();
                }
            }
            return bitmap;
        }

        private static Stream GetStream(string file)
        {
            var length = !File.Exists(file);
            if (length)
            {
                Assembly ass = Assembly.GetAssembly(typeof(AddWatermark));
                string[] manifestResourceNames = ass.GetManifestResourceNames();
                int num = 0;
                while (true)
                {
                    length = num < (int)manifestResourceNames.Length;
                    if (!length)
                    {
                        break;
                    }
                    string resourceName = manifestResourceNames[num];
                    length = !resourceName.EndsWith(file);
                    if (!length)
                    {
                        return ass.GetManifestResourceStream(resourceName);
                    }
                    num++;
                }
                throw new FileNotFoundException(string.Format("File not found: {0}", file), file);
            }
            return new FileStream(file, FileMode.Open);
        }

        #endregion

        #endregion
    }
}
