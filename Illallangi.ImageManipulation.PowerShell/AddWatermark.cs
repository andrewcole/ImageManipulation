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

        #endregion

        #region Constructors

        public AddWatermark()
        {
            this.Opacity = 100;
            this.Output = "{0}-{1}.{2}";
            this.OutputFormat = ImageFormat.Jpeg;
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
        public int Opacity { get; set; }

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
            this.InputGraphics.DrawImage(this.WatermarkBitmap, 0, 0);
            this.InputBitmap.Clone(
                new Rectangle(0,0,this.InputBitmap.Width, this.InputBitmap.Height), 
                PixelFormat.Undefined)
                    .Save(Path.Combine(this.SessionState.Path.CurrentFileSystemLocation.Path,
                                string.Format(this.Output, Path.GetFileNameWithoutExtension(this.Input), 
                                    Path.GetFileNameWithoutExtension(this.Watermark), 
                                    this.OutputFormat.ToString().ToLower())), 
                                this.OutputFormat);
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
