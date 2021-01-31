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
            /// <summary>
            /// 
            /// </summary>
            ProcessingMode = 1,
            /// <summary>
            /// 
            /// </summary>
            ResultsMode = 2,
            /// <summary>
            /// /
            /// </summary>
            AppendOutput = 4,
            /// <summary>
            /// 
            /// </summary>
            ReplaceOutput=8,
            /// <summary>
            /// 
            /// </summary>
            ExportAsBytes = 16
                

        }

        public ProcessMode Mode = ProcessMode.ProcessingMode;

        /// <summary>
        /// charactors below this value are culled.
        /// </summary>
        public int CharLow = 0;
        /// <summary>
        /// chars about this value are culled.
        /// </summary>
        public int CharHigh = 255;
        
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
                            case "RANGE":
                                if (step+1 >= args.Length)
                                {
                                    throw new ArgumentException(StaticMessages.RangeNotSpecifiedMsg);
                                }
                                else
                                {
                                    step++;
                                    string first, second;
                                    first = second = string.Empty;

                                    int inner = 0;
                                    for (; inner < args[step].Length;inner++) 
                                    {
                                        if (char.IsDigit(args[step][inner]))
                                        {
                                            first += args[step][inner];
                                        }
                                        else
                                        {
                                            if (args[step][inner] == '-')
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                throw new ArgumentException(string.Format(FormatMessages.ExpectedDigitsNotThis, args[step][inner]));
                                            }
                                        }
                                    }

                                    for (; inner < args[step].Length; inner++)
                                    {
                                        if (char.IsDigit(args[step][inner]))
                                        {
                                            second += args[step][inner];
                                        }
                                        else
                                        {
                                      
                                        }
                                    }
                                }
                                break;
                            case "OUT:LilEndianUnicode":
                            case "OUT:UNICODE":
                                Config.Target = Encoding.Unicode;
                                break;
                             case "?":
                            case "HELP":
                                // this triggers the usage file being displayed.
                                throw new ArgumentNullException();
                            case "MODE:BYTELIST":
                            case "MBL":
                                Config.Mode |= Config.ProcessMode.ExportAsBytes;
                                break;
                            default:
                                {
                                    throw new ArgumentException(string.Format(FormatMessages.DefaultUnknownArgMessage, args[step]));
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
                Console.WriteLine(FormatMessages.InvalidArgumentMessage, e.Message);

                return;
            }


            if (SourceLocation == null)
            {
                Console.WriteLine(StaticMessages.NoSourceSpecifiedMsg);
                return;
            }
            else
            {
                if (File.Exists(SourceLocation) == false)
                {
                    Console.WriteLine(FormatMessages.FileNotFoundMessage, SourceLocation);
                }
                else
                {
                    try
                    {
                        SourceStream = File.OpenRead(SourceLocation);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine(FormatMessages.SecurityMessage, SourceLocation);
                        return;
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(FormatMessages.IoErrorMessage, e.Message);
                        return;
                    }
                }
            }
            if (TargetLocation == null)
            {
                Console.WriteLine(StaticMessages.NoTargetSpecifiedMsg);
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
                            Console.WriteLine(string.Format(FormatMessages.OverrideRefusalMessage,TargetLocation, TargetLocation));
                            SourceStream.Close();
                            return;
                        }
                        else
                        {
                            Console.WriteLine(string.Format(FormatMessages.OverrideAllowedMessage,TargetLocation), TargetLocation);
                        }
                    }

                    try
                    {
                        TargetStream = File.Open(TargetLocation, FileMode.OpenOrCreate);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine(string.Format(FormatMessages.SecurityMessage, SourceLocation));
                        return;
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(string.Format(FormatMessages.IoErrorMessage, e.Message));
                        return;
                    }
                }
            }


            byte[] SourceBytes = new byte[SourceStream.Length];
            byte[] TargetBytes;
            string SourceString;
            string ProcessedString;

            const string AsyncMessage = "Reading Source. {0} out of {1} KB.   {2}%    ";
            Console.WriteLine();
            Console.CursorVisible = false;
            while (true)
            {

                var TaskStatus = SourceStream.ReadAsync(SourceBytes, 0, (int)SourceStream.Length);

                if (SourceStream.Length != 0)
                {
                    Console.Write(string.Format(AsyncMessage, TaskStatus.Result, SourceStream.Length, ((float)((float)TaskStatus.Result/(float)SourceStream.Length)) *100 ));
                }

                if (TaskStatus.IsCompleted)
                {
                    Console.WriteLine("\r\nDone");
                    break;
                }
                if (TaskStatus.IsFaulted)
                {
                    Console.WriteLine("There was an error handlings this");
                    return;
                }
                Console.SetCursorPosition(AsyncMessage.Length, Console.CursorTop);


            }
            Console.CursorVisible = true;


            SourceString = Config.Source.GetString(SourceBytes);

            if (Config.Mode.HasFlag( Config.ProcessMode.ProcessingMode))
            {
                ProcessedString = Megatron.Apply(SourceString);
            }
            else
            {
                if (Config.Mode.HasFlag(Config.ProcessMode.ReplaceOutput))
                {
                    ProcessedString = Megatron.GetHits(SourceString);
                }
                else
                {
                    ProcessedString = string.Empty;
                }

            }

            if (Config.Mode.HasFlag(Config.ProcessMode.ExportAsBytes))
            {
                StringBuilder ret = new StringBuilder();
                ret.Append("{");
                for (int step =0; step < ProcessedString.Length;step++)
                {
                    if (step+1 != ProcessedString.Length)
                    {
                        ret.AppendFormat("{0}, ", ((int)ProcessedString[step]).ToString());
                    }
                    else
                    {
                        ret.AppendFormat("{0}", ProcessedString[step]);
                    }

                    if ( (step % 8) == 0)
                    {
                        ret.Append("\r\n");
                    }

                }
                ret.Append("}");
                ProcessedString = ret.ToString();
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
