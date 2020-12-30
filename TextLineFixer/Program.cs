using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
namespace TextLineFixer
{

    class ReplacemengRegEx : Regex
    { 
        public ReplacemengRegEx(string pattern, RegexOptions Options, string ReplaceWith):base(pattern, Options)
        {
            this.ReplaceString = ReplaceWith;
        }

        public string ReplaceString { get; set; }

        public string FancyName { get; set; }
    }

    class TextSource
    {

        public enum LineHandling
        {
            NoFix = 0,
            /// <summary>
            /// inst
            /// </summary>
            Fix = 1,

        }

        List<Regex> Passes = new List<Regex>();

        public void ForceString(string UseThis)
        {
            Source = null;
            DebubString = UseThis;
        }
        public TextSource(string Name)
        {
            Source = File.Open(Name, FileMode.Open);
        }

        public TextSource()
        {
            Source = null;
        }
        public TextSource(Stream UseThis)
        {
            Source = UseThis;
        }
        ~TextSource()
        {
            Source.Dispose();
        }

        private Stream Source;
        private string DebubString;

        /// <summary>
        /// bytes that have \r or \n get replaced with this
        /// </summary>
        public string PreferredLineEnd { get; set; } = "\r\n";

        /// <summary>
        /// If Null, 
        /// </summary>
        public Encoding TargetOutputFormat { get; set; } = Encoding.Default;

        bool VerboseOutput = true;

        /// <summary>
        /// Add passes to equalize partial lines.
        /// </summary>
        /// <param name="NewLine"></param>
        public void AddLineEndHandling(string NewLine)
        {
            ReplacemengRegEx Pass1 = new ReplacemengRegEx("\r[^\n]", RegexOptions.IgnoreCase, NewLine);
            ReplacemengRegEx Pass2 = new ReplacemengRegEx("[^\r]\n", RegexOptions.IgnoreCase, NewLine);
            Pass1.FancyName = "Single \\r to \\r\\n Replacer";
            Pass2.FancyName = "Single \\n to \\r\\n Replacement";
            Passes.Add(Pass1);
            Passes.Add(Pass2);
        }

        
        public bool DisplayFixedLines { get; set; } = true;
        private void Write(string input, Stream target, Encoding Format)
        {
            byte[] Data = Format.GetBytes(input);

            target.Write(Data, 0, Data.Length - 1);
        }

        private void Write(List<int> Input,Stream Target, Encoding Format)
        {
            Write(Input.ToString(), Target, Format);
        }


    }

    class Config
    {
        public Encoding Source = Encoding.Default;
        public Encoding Target = Encoding.Default;
        
    }
    class Program
    {
        static TextSource Source;
        static Stream Target=null;
        static Config Config = new Config();


        const string SOURCE = "SOURCE";
        const string TARGET = "TARGET";

        const string SPECIAL_CONSOLE_TARGET = "CON";

        static TextShift.TextTransformer Megatron = new TextShift.TextTransformer();
        static void Usage()
        {
            Console.Write(@"
                -In:Format
 
                -Out:Format

                    Specific the input and output file's encoding.  Internally the format goes from InFormat to Unicode String to Output format.


                -Source:
");
            
        }


        static string SourceLocation =null;
        static string TargetLocation = null;

        static Stream SourceStream;
        static Stream TargetStream;

        static bool OkToOverwrite = false;
        static void HandleArgs(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentNullException();
            }
            for (int step = 0; step < args.Length; step++)
            {
                if (args[step].Length > 0)
                {
                    if ( (args[step][0] == '-') || (args[step][0] == '/'))
                    {
                        switch (args[step].ToUpperInvariant().Substring(1))
                        {
                            case "S":
                            case "SOURCE":
                                if (step+1 >= args.Length)
                                {
                                    throw new ArgumentException("Please specify a source for text");
                                }
                                else
                                {
                                    step++;
                                    SourceLocation = args[step];
                                }
                                break;
                            case "T":
                            case "TARGET":
                                if (step+1 >= args.Length)
                                {
                                    throw new ArgumentException("Please specify a target to send text");
                                }
                                else
                                {
                                    step++;
                                    TargetLocation = args[step];
                                }
                                break;
                            case "TC":
                            case "TARGET:CON":
                                TargetLocation = "CON";
                                break;
                            case "LMN":
                            case "LINEMODE:NONE":
                                {
                                    Megatron.LinePreference = TextShift.TextTransformer.LineStyle.None;
                                    break;
                                }
                            case "LMW":
                            case "LINEMODE:WINDOWS":
                                {
                                    Megatron.LinePreference = TextShift.TextTransformer.LineStyle.WindowsStyle;
                                    break;
                                }
                            case "LMU":
                            case "LMM":
                            case "LINEMODE:UNIX":
                            case "LINEMODE:MAC":
                                {
                                    Megatron.LinePreference = TextShift.TextTransformer.LineStyle.UnixStyle;
                                }
                                break;
                            case "SMFS":
                            case "SPACEMODE:FORCESINGLE":
                                {
                                    Megatron.SpacePreference = TextShift.TextTransformer.SpaceStyle.ForceSingle;
                                }
                                break;
                            case "SMN":
                            case "SPACEMODE:NONE":
                                {
                                    Megatron.SpacePreference = TextShift.TextTransformer.SpaceStyle.None;
                                }
                                break;
                            
                            case "F":
                            case "FORCE":
                            case "ALLOWOVERWRITE":
                                {
                                    OkToOverwrite = true;
                                }
                                break;
                            case "IN:ANSI":
                                Config.Source = Encoding.ASCII;
                                    break;
                            case "IN:UNICODE":
                                Config.Source = Encoding.Unicode;
                                break;
                            case "OUT:ANSI":
                                Config.Target = Encoding.ASCII;
                                break;
                            case "OUT:UNICODE":
                                Config.Target = Encoding.Unicode;
                                break;
                            default:
                                {
                                    throw new ArgumentException("Unknown argument: " + args[step]);
                                }
                                
                        }

                        }

                    }
                }
            }


        
        static void Main(string[] args)
        {
            try
            {
                HandleArgs(args);
            }
            catch (ArgumentNullException)
            {
                Usage();
                return;
            }
            catch (ArgumentException e)
            {
                // in this case an exepction means invalid argument.
                Console.WriteLine("Error: Unexpected or invalid arg. {0}", e.Message);
            }


            if (SourceLocation == null)
            {
                Console.WriteLine("Error: No Source specified. Please specifiy a source using the -SOURCE command");
                return;
            }
            else
            {
                if (File.Exists(SourceLocation) == false)
                {
                    Console.WriteLine("Error: File {0} was not found.", SourceLocation);
                }
                else
                {
                    try
                    {
                        SourceStream = File.OpenRead(SourceLocation);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("Error: Current User does not have permission to open source file {0}", SourceLocation);
                        return;
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("IO Error happened. Message is {0}", e.Message);
                        return;
                    }
                }
            }
            if (TargetLocation == null)
            {
                Console.WriteLine("Error: No Target specified. Please specifiy a target using the -TARGET command");
                return;
            }
            else
            {
                if (string.Compare(TargetLocation, "CON", true) == 0)
                {
                    TargetStream = Console.OpenStandardOutput();
                }
                else
                {
                    if (File.Exists(TargetLocation) ==true)
                    {
                        if (OkToOverwrite == false)
                        {
                            Console.WriteLine("Error: Target {0} already exists. Won't overwrite this without specifying the -FORCE command");
                            SourceStream.Close();
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Warning: Target {0} exists and 'FORCE' mode is specified. This will overwrite the target");
                        }
                    }

                    try
                    {
                        TargetStream = File.OpenWrite(TargetLocation);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("Error: Current User does not have permission to open target file {0}", SourceLocation);
                        return;
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("IO Error happened. Message is {0}", e.Message);
                        return;
                    }
                }
            }


            byte[] SourceBytes = new byte[SourceStream.Length];
            byte[] TargetBytes;
            string SourceString;
            string ProcessedString;

            const string AsyncMessage = "Reading Source. One Momement";
            const string Dot1 = "  .  ";
            const string Dot2 = Dot1 + Dot1;
            const string Dot3 = Dot2 + Dot1;
            Console.WriteLine();
            Console.CursorVisible = false;
            while (true)
            {
                int o = 1;
                int stopspace;
                if (SourceStream.ReadAsync(SourceBytes, 0, (int)SourceStream.Length).IsCompletedSuccessfully)
                {
                    Console.WriteLine("Done.");
                    break;
                }
                
                Console.Write(AsyncMessage);

                Console.SetCursorPosition(AsyncMessage.Length, Console.CursorTop);
                for (int step = 0; step < o; step++)
                {
                    Console.Write(" ");
                }
                switch (o)
                {
                    case 1:
                        Console.Write(Dot1);
                        break;
                    case 2:
                        Console.Write(Dot2);
                        break;
                    case 3:
                        Console.Write(Dot3);
                        break;
                    default:
                        o = 1;
                        Console.Write(Dot1);
                        break;

                }

                o++;
                

            }
            Console.CursorVisible = true;


            SourceString = Config.Source.GetString(SourceBytes);

            ProcessedString = Megatron.Apply(SourceString);


            TargetBytes = Config.Target.GetBytes(ProcessedString);
            TargetStream.Write(TargetBytes, 0, TargetBytes.Length-1);
            TargetStream.Flush();
            Console.ReadKey();
        }
    }
}
