using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using ImageProcessing.Utils;

namespace ImageProcessing.SharpShader
{
    public class SharpShader : TemplateImageProcessor
    {
        private delegate bool DProcessCommand(SharpShader ss, string s, Expression e, Dictionary<string, string> aliases);
        private static readonly Dictionary<string, DProcessCommand> CommandProcessors = new Dictionary<string, DProcessCommand>
        {
            {"#alias", Handle_Alias},
            {"loadfromfile", HandleLoadFromFile},
            {"setread", HandleSetRead},
            {"setwrite", HandleSetWrite},
            {"setreadwrite", HandleSetReadWrite},
            {"setnoaccess", HandleSetNoAccess},
            {"savetofile", HandleSaveToFile},
            {"createtarget", HandleCreateTarget},
            {"processfull", HandleProcess}
        };
        public SharpShader(params string[] args)
        {
            Tasks = new List<ImageProcessingTask>();
            LastLoadMessages = "";
            SharedShaderArguments = new Dictionary<string, string>();

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                SharedShaderArguments["%" + i] = arg;
            }
        }
        public Dictionary<string, string> SharedShaderArguments { get; private set; }
        public List<ImageProcessingTask> Tasks { get; private set; }
        public string LastLoadMessages { get; private set; }
        public bool LoadFrom(StreamReader stream)
        {
            var aliases = new Dictionary<string, string>(SharedShaderArguments);
            while (!stream.EndOfStream)
            {
                var s = stream.ReadLine();
                if (s == null)
                    break;
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                var e = new Expression(s);
                var cmd = e.Command.ToLowerInvariant().Replace("_", "");
                DProcessCommand cmdproc;
                if (!CommandProcessors.TryGetValue(cmd, out cmdproc))
                {
                    LastLoadMessages =
                        string.Format("SS Error in line:{0} {1}{0} Unknown instruction - '{2}'.",
                            Environment.NewLine, s, e.Command);
                    return false;
                }
                if (!cmdproc(this, s, e, aliases))
                    return false;
            }
            return true;
        }
        public bool LoadFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                LastLoadMessages = string.Format("File not found - {0}", fileName);
                return false;
            }
            using (var f = new StreamReader(fileName))
            {
                return LoadFrom(f);
            }
        }

        protected override IEnumerable<ImageProcessingTask> GetTasks()
        {
            return Tasks;
        }

        private static bool CheckArgumentCount(SharpShader ss, string s, Expression e, int expected = 0)
        {
            if (e.ArgsCount == expected) return true;
            ss.LastLoadMessages = FormErrorMessage(s, "{0} arguments expected, got {1}.", expected, e.ArgsCount);
            return false;
        }
        private static bool CheckArgumentCount(SharpShader ss, string s, Expression e, int expectedAtLeast, int expectedAtMost)
        {
            if ((e.ArgsCount >= expectedAtLeast) && (e.ArgsCount <= expectedAtMost)) 
                return true;
            ss.LastLoadMessages = FormErrorMessage(s, "{0}..{1} arguments expected, got {2}.", expectedAtLeast,expectedAtMost, e.ArgsCount);
            return false;
        }
        private static bool Handle_Alias(SharpShader ss, string s, Expression e, Dictionary<string, string> aliases)
        {
            if (!CheckArgumentCount(ss, s, e, 2))
                return false;
            aliases[e.Args[0]] = e.Args[1];
            return true;
        }
        private static bool HandleLoadFromFile(SharpShader ss, string s, Expression e, Dictionary<string, string> aliases)
        {
            if (!CheckArgumentCount(ss, s, e, 1))
                return false;

            var path = ApplyAliases(e.Args[0], aliases);
            /*
            if (!File.Exists(path))
            {
                ss.LastLoadMessages = FormErrorMessage(s, "{0} - file does not exist.", path);
                return false;
            }*/

            var task = new LoadImageFromFileTask(path);
            ss.Tasks.Add(task);
            return true;
        }
        private static bool HandleSetRead(SharpShader ss,string s, Expression e, Dictionary<string, string> aliases)
        {
            if (!CheckArgumentCount(ss, s, e, 1))
                return false;
            var name = ApplyAliases(e.Args[0], aliases);
            ss.Tasks.Add(new LockImageTask(name, ImageLockMode.ReadOnly));
            return true;
        }
        private static bool HandleSetWrite(SharpShader ss,string s, Expression e, Dictionary<string, string> aliases)
        {
            if (!CheckArgumentCount(ss, s, e, 1))
                return false;
            var name = ApplyAliases(e.Args[0], aliases);
            ss.Tasks.Add(new LockImageTask(name, ImageLockMode.WriteOnly));
            return true;
        }
        private static bool HandleSetReadWrite(SharpShader ss,string s, Expression e, Dictionary<string, string> aliases)
        {
            if (!CheckArgumentCount(ss, s, e, 1))
                return false;
            var name = ApplyAliases(e.Args[0], aliases);
            ss.Tasks.Add(new LockImageTask(name, ImageLockMode.ReadWrite));
            return true;
        }
        private static bool HandleSetNoAccess(SharpShader ss,string s, Expression e, Dictionary<string, string> aliases)
        {
            if (!CheckArgumentCount(ss, s, e, 1))
                return false;
            var name = ApplyAliases(e.Args[0], aliases);
            ss.Tasks.Add(new LockImageTask(name, ImageLockMode.UserInputBuffer));
            return true;
        }
        private static bool HandleSaveToFile(SharpShader ss, string s, Expression e, Dictionary<string, string> aliases)
        {
            if (!CheckArgumentCount(ss, s, e, 2, 3))
                return false;
            var name = ApplyAliases(e.Args[0], aliases);
            if (e.ArgsCount == 2)
            {
                var format = e.Args[1].ToLowerInvariant();
                ImageFormat eformat = null;
                if (!TryParse(format, out eformat))
                {
                    ss.LastLoadMessages = FormErrorMessage(s, "Unrecognized image format - '{0}'.", format);
                    return false;
                }
                ss.Tasks.Add(new SaveImageToFileTask(name, eformat));
            }
            else
            {
                var path = ApplyAliases(e.Args[1], aliases);
                var format = e.Args[2].ToLowerInvariant();
                ImageFormat eformat = null;
                if (!TryParse(format, out eformat))
                {
                    ss.LastLoadMessages = FormErrorMessage(s, "Unrecognized image format - '{0}'.", format);
                    return false;
                }
                ss.Tasks.Add(new SaveImageToFileTask(name, eformat, path));
            }
            return true;
        }
        private static bool HandleCreateTarget(SharpShader ss, string s, Expression e, Dictionary<string, string> aliases)
        {
            if (!CheckArgumentCount(ss, s, e, 2))
                return false;
            var src = ApplyAliases(e.Args[0], aliases);
            var tt = ApplyAliases(e.Args[1], aliases);
            ss.Tasks.Add(new CreateTargetForImageTask(src, tt));
            return true;
        }
        private static bool HandleProcess(SharpShader ss, string s, Expression e, Dictionary<string, string> aliases)
        {
            if (!CheckArgumentCount(ss, s, e, 3, 5))
                return false;
            var imgName = ApplyAliases(e.Args[0], aliases);
            var ttName = ApplyAliases(e.Args[1], aliases);
            var shaderPath = ApplyAliases(e.Args[2], aliases);
            if (!File.Exists(shaderPath))
            {
                ss.LastLoadMessages = FormErrorMessage(s, "Shader code not found at '{0}'", shaderPath);
                return false;
            }
            DProcessPixel shader;
            if (!ss.CompileShader(s, shaderPath, out shader))
                return false;
            var task = new ProcessFullImageTask(imgName, shader, ttName);
            ss.Tasks.Add(task);
            if (e.ArgsCount == 3)
                return true;

            int v;
            if (!int.TryParse(e.Args[3], out v) || v < 1)
            {
                ss.LastLoadMessages = FormErrorMessage(s, "Argument #4 (ThreadID) should be natural number (1+), got '{0}'",
                    e.Args[3]);
                return false;
            }
            task.ThreadsToUse = v;
            if (e.ArgsCount == 4)
                return true; 
            
            if (!int.TryParse(e.Args[4], out v) || v < 1)
            {
                ss.LastLoadMessages = FormErrorMessage(s, "Argument #5 (FragmentSize) should be natural number (1+), got '{0}'",
                    e.Args[4]);
                return false;
            }
            task.MaxFragmentHeight = task.MaxFragmentWidth = v;
            return true;
        }

        private List<MethodInfo> _shaders = new List<MethodInfo>();

        private bool CompileShader(string line, string shaderPath, out DProcessPixel shader)
        {
            shader = null;
            try
            {
                var shaderCode = File.ReadAllText(shaderPath);
                shaderCode = @"
using System.Drawing; 
using System.Drawing.Imaging; 
using ImageProcessing;
using ImageProcessing.SharpShader;
namespace Shader { 
    public class ShaderClass {
        " + shaderCode + @"        
    }
}";
                var providerOptions = new Dictionary<string, string>
                {
                      //{"CompilerVersion", "v4.5"}
                };

                var compilerParams = new CompilerParameters
                {
                    GenerateExecutable = false,
                    ReferencedAssemblies = { "System.Core.dll", "System.Drawing.dll", "ImageProcessing.dll" },
                    GenerateInMemory = true,
                    //OutputAssembly = shaderPath + ".dll"
                };

                var provider = CodeDomProvider.CreateProvider("C#", providerOptions);//new CSharpCodeProvider(providerOptions);
                var results = provider.CompileAssemblyFromSource(compilerParams, shaderCode);
                if (results.Errors.Count > 0)
                {
                    LastLoadMessages = FormErrorMessage(line, "Shader code at '{0}' compilation failed, errors are:",
                        shaderPath);
                    foreach (var error in results.Errors)
                    {
                        LastLoadMessages += Environment.NewLine + error;
                    }
                    return false;
                }
                var ass = results.CompiledAssembly;
                var shaderClass = ass.GetType("Shader.ShaderClass");
                var shaderInfo = shaderClass.GetMethod("main");/*, BindingFlags.Static, null, new [] 
                {
                  typeof(int), typeof(int), typeof(ImageProcessingThreadContext)
                }, null*/
                if (shaderInfo == null)
                {
                    LastLoadMessages = FormErrorMessage(line, @"Shader code at '{0}' compilation failed - no 'main' function found.",
                        shaderPath);
                    return false;
                }
                var parms = shaderInfo.GetParameters();
                if (!shaderInfo.IsStatic || shaderInfo.ReturnType != typeof(void) || 
                    !shaderInfo.IsPublic || parms.Length != 3 
                    || parms[0].ParameterType != typeof(int) 
                    || parms[1].ParameterType != typeof(int) 
                    || parms[2].ParameterType != typeof(ImageProcessingThreadContext))
                {
                    LastLoadMessages = FormErrorMessage(line, @"Shader code at '{0}' compilation failed - 'main' function should be: public static void main(int, int, ImageProcessingThreadContext).",
                        shaderPath);
                    return false;
                }
                _shaders.Add(shaderInfo);//to keap shaderInfo in the heap (neaded?)
                shader = (x, y, context) => shaderInfo.Invoke(null, new object[] { x, y, context });
            }
            catch (Exception ex)
            {
                LastLoadMessages = FormErrorMessage(line, "Shader code at '{0}' compilation failed, exception: {1}",
                    shaderPath, ex.Message);
                return false;
            }
            return true;
        }
        private static bool TryParse(string s, out ImageFormat eformat)
        {
            eformat = null;
            switch (s)
            {
                case "bmp":eformat = ImageFormat.Bmp;
                    break;
                case "emf":eformat = ImageFormat.Emf;
                    break;
                case "exif":eformat = ImageFormat.Exif;
                    break;
                case "gif":eformat = ImageFormat.Gif;
                    break;
                case "ico":case "icon":eformat = ImageFormat.Icon;
                    break;
                case "jpg":case "jpeg":eformat = ImageFormat.Jpeg;
                    break;
                case "png":eformat = ImageFormat.Png;
                    break;
                case "tiff":eformat = ImageFormat.Tiff;
                    break;
                case "wmf":eformat = ImageFormat.Wmf;
                    break;
                default:
                    return false;
            }
            return true;
        }

        private static string FormErrorMessage(string line, string message, params object[] args)
        {
            return string.Format("SS Error in line:{0} {1}{0}", Environment.NewLine, line) + string.Format(message, args);
        }

        private static string ApplyAliases(string s, IReadOnlyDictionary<string, string> aliases)
        {
            string pathCopy;
            return aliases.TryGetValue(s, out pathCopy) ? pathCopy : s;
        }
    }
}
