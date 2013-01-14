// -----------------------------------------------------------------------
// <copyright file="AddWatermark.cs" company="Illallangi Enterprises">
// Copyright (C) 2013 Illallangi Enterprises
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Management.Automation;
using System.Drawing;
using System.Reflection;

namespace Illallangi.ImageManipulation.PowerShell
{
    [Cmdlet("Add", "Watermark")]
    public class AddWatermark : Cmdlet
    {
        #region Fields

        private Bitmap _bitmap;
        private Graphics _graphics;

        #endregion

        #region Properties

        private Bitmap Bitmap
        {
            get
            {
                return this._bitmap;
            }
            set 
            { 
                this._graphics = null;
                this._bitmap = value;
            }
        }

        private Graphics Graphics
        {
            get { return this._graphics ?? (this._graphics = Graphics.FromImage(this.Bitmap)); }
        }

        [Parameter(ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        public string Input { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ValueFromPipeline = true)]
        public string Watermark { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public int Opacity { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string Output { get; set; }

        #endregion

        #region Methods

        protected override void BeginProcessing()
        {
            try
            {
                this.Debug("Loading Bitmap {0}", this.Input);
                this.Bitmap = AddWatermark.GetBitmap(this.Input);
                this.Debug("Done");
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        #region Private Methods

        private void Debug(string format, params object[] args)
        {
            this.WriteDebug(string.Format(format, args));
        }

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
