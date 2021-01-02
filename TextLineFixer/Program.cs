using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TextShift;
namespace TextLineFixer
{

   
    

    /// <summary>
    /// source and target encoding
    /// </summary>
    class Config
    {
        /// <summary>
        /// Specifies the encoding that will be read from the source
        /// </summary>
        public Encoding Source = Encoding.Default;
        /// <summary>
        /// Specifies the encoding we emit to the target location
        /// </summary>
        public Encoding Target = Encoding.Default;

       public enum ProcessMode
        {
            ProcessingMode = 1,
            ResultsMode = 2,
            AppendOutput = 4,
            ReplaceOutput=8
        }

        public ProcessMode Mode = ProcessMode.ProcessingMode;
        
    }
    class Program
    {
        
        static Config Config = new Config();



        /// <summary>
        /// This is the class that transforms the text. The console app is what converts between the encodings.
        /// </summary>
        static TextTransformer Megatron = new TextTransformer();

        /// <summary>
        /// In the event of no arguments being passed, we read the built in Usage.txt file and show that to the user.
        /// </summary>
        /// <remarks>Usage.txt is assumed to be ANSCII 8-bit plain text.</remarks>
        static void Usage()
        {
            var UsageTextData = Assembly.GetExecutingAssembly().GetManifestResourceStream("TextShift.Usage.txt");
            byte[] Data = new byte[UsageTextData.Length];
            UsageTextData.Read(Data, 0, Data.Length - 1);
            Console.Write(Encoding.ASCII.GetString(Data));
        }


        /// <summary>
        /// holds the location of the source
        /// </summary>
        static string SourceLocation =null;
        /// <summary>
        /// holds the location of the target
        /// </summary>
        static string TargetLocation = null;

        /// <summary>
        /// the open stream to the source
        /// </summary>
        static Stream SourceStream;
        /// <summary>
        /// the open strema to the target
        /// </summary>
        static Stream TargetStream;

        /// <summary>
        /// if true, replacing the target is ok.
        /// </summary>
        static bool OkToOverwrite = false;

        /// <summary>
        /// handle the arguments
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="ArgumentNullException"> is thrown if args is null or its length is 0.  Caller is expected to show usage.txt if that happens.</exception>
        /// <exception cref="ArgumentException">is thrown if a malformed argument is passed. Caller is expected to exit</exception>
        static void HandleArgs(string[] args)
        {
            if ( ( args == null) || (args.Length < 3) )
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
                            case "IN:ASCII":
                            case "IN:ANSI":
                                Config.Source = Encoding.ASCII;
                                break;
                            case "IN:BigEndianUnicode":
                                Config.Source = Encoding.BigEndianUnicode;
                                break;
                            case "IN:UTF7":
                                Config.Source = Encoding.UTF7;
                                break;
                            case "IN:UTF8":
                                Config.Source = Encoding.UTF8;
                                break;
                            case "IN:UTF32":
                                Config.Source = Encoding.UTF32;
                                break;
                            case "IN:LilEndianUnicode":
                            case "IN:UNICODE":
                                Config.Source = Encoding.Unicode;
                                break;
                            case "MODE:VIEW":
                                Config.Mode |= Config.ProcessMode.ResultsMode;
                                Config.Mode -= Config.ProcessMode.ProcessingMode;
                                break;
                            case "MODE:CHANGE":
                                Config.Mode |= Config.ProcessMode.ResultsMode;
                                Config.Mode -= Config.ProcessMode.ProcessingMode;
                                break;
                            case "OUT:ASCII":
                            case "OUT:ANSI":
                                Config.Target= Encoding.ASCII;
                                break;
                            case "OUT:BigEndianUnicode":
                                Config.Target = Encoding.BigEndianUnicode;
                                break;
                            case "OUT:UTF7":
                                Config.Target = Encoding.UTF7;
                                break;
                            case "OUT:UTF8":
                                Config.Target = Encoding.UTF8;
                                break;
                            case "OUT:UTF32":
                                Config.Target = Encoding.UTF32;
                                break;
                            case "OUT:LilEndianUnicode":
                            case "OUT:UNICODE":
                                Config.Target = Encoding.Unicode;
                                break;
                              case "?":
                            case "HELP":
                                // this triggers the usage file being displayed.
                                throw new ArgumentNullException();
                                
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

                return;
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
                            Console.WriteLine("Error: Target {0} already exists. Won't overwrite this without specifying the -FORCE command", TargetLocation);
                            SourceStream.Close();
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Warning: Target {0} exists and 'FORCE' mode is specified. This will overwrite the target", TargetLocation);
                        }
                    }

                    try
                    {
                        TargetStream = File.Open(TargetLocation, FileMode.OpenOrCreate);
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

                
                

            }
            Console.CursorVisible = true;


            SourceString = Config.Source.GetString(SourceBytes);


            if (Config.Mode.HasFlag(Config.ProcessMode.ProcessingMode))
            {
                ProcessedString = Megatron.Apply(SourceString);

            }
            else
            {
                ProcessedString = Megatron.GetHits(SourceString);
            }

            TargetBytes = Config.Target.GetBytes(ProcessedString);
            if (TargetBytes.Length != 0)
            {
                TargetStream.Write(TargetBytes, 0, TargetBytes.Length - 1);
                TargetStream.Flush();
            }
            Console.ReadKey();
        }
    }
}
