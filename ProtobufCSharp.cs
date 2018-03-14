using System;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell;
using VSLangProj80;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace ProtobufCSharp
{
    [ComVisible(true)]
    [Guid("2748C3FE-A955-4719-A840-61845C77269B")]
    [CodeGeneratorRegistration(
        typeof(ProtobufCSharp),
        "ProtobufCSharp", 
        vsContextGuids.vsContextGuidVCSProject, 
        GeneratesDesignTimeSource = true)]
    [CodeGeneratorRegistration(
        typeof(ProtobufCSharp),
        "ProtobufCSharp",
        "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}",
        GeneratesDesignTimeSource = true)]
    [CodeGeneratorRegistration(
        typeof(ProtobufCSharp),
        "ProtobufCSharp",
        "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}",
        GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(ProtobufCSharp))]
    public class ProtobufCSharp : BaseCodeGeneratorWithSite
    {
#pragma warning disable 0414
        //The name of this generator (use for 'Custom Tool' property of project item)
        internal static string name = "ProtobufCSharp";
#pragma warning restore 0414

        protected override byte[] GenerateCode(string inputFileContent)
        {
            try
            {
                var input = File.ReadAllText(base.InputFilePath);

                EnsuteTools();

                var toolsPath = GetToolsPath();
                var toolsCommandLine = @"-I=""{0}"" --csharp_out=""{1}"" ""{0}\{2}""";
                Func<string, string, string, string> cmd = (inputPath, outputPath, inputFile) =>
                {
                    return String.Format(toolsCommandLine, inputPath, outputPath, inputFile);
                };
                var outputDirectory = Path.Combine(toolsPath, Guid.NewGuid().ToString());
                var cmdLine = cmd(Path.GetDirectoryName(InputFilePath), outputDirectory
                    , Path.GetFileName(InputFilePath));

                Directory.CreateDirectory(outputDirectory);
                var outputFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(InputFilePath) + ".cs");
                var result = "";
                try
                {
                    var p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = Path.Combine(
                        // hardcoded platform specific tool directory
                        Path.Combine(toolsPath, "windows_x64"), 
                        "protoc.exe");
                    p.StartInfo.Arguments = cmdLine;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    p.WaitForExit();

                    result = File.ReadAllText(outputFilePath);
                }
                finally
                {
                    File.Delete(outputFilePath);
                    Directory.Delete(outputDirectory);
                }

                return Encoding.UTF8.GetBytes(result);
            }
            catch (Exception e)
            {
                GeneratorError(0, e.ToString(), (uint)0, (uint)0);
            }
            return null;
        }

        private void EnsuteTools()
        {
            var toolsPath = GetToolsPath();
            if (!Directory.Exists(toolsPath))
            {
                Directory.CreateDirectory(toolsPath);
                var assembly = typeof(ProtobufCSharp).Assembly;
                var streamName = String.Format("{0}.Protobuf.tools.zip", typeof(ProtobufCSharp).Namespace);
                if (assembly.GetManifestResourceNames().Any(c => c == streamName))
                {
                    var filename = Path.Combine(GetToolsPath(), "tools.zip");
                    using (var stream = assembly.GetManifestResourceStream(streamName))
                    {
                        using (var fs = File.OpenWrite(filename))
                        {
                            stream.CopyTo(fs);
                            stream.Flush();
                            fs.Flush();
                        }
                    }
                    ExtractZipFile(filename, null, GetToolsPath());
                }
            }
        }
        public void ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        private string GetToolsPath()
        {
            return Path.Combine(Path.GetTempPath(), "ProtobufCSharp");
        }
    }
}