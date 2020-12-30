# TextShift
This explores modifying text based on regular expressions and also some conversion between encodings.


The inspirationffor this is me wanting to have a way to fix mismatched lines. It grew from there. Run the app with /? To see the full command line. 


  Current Settings include what to do with new line chars of 10, 13 aka '\r\n' for Windows C/C++/C# users. 
Default is leave lines alone but you can specify to replace line endings with '\r\n', '\r' or '\n' to suit your operating system preference. 
The short form to swap line endings out to Windows friendly is pass -LMW. To get the more Unix style line ending of '\n' alone, pass 
-LMU or -LMM.  Important! This effect ALL line endings in the source text.


Another setting is to tell TextShift what to do with variying number of spaces.  You can leave it unchanged or tell TextShift to
swap instances of more than one 32 ' ' char out with a single ' ' char.  To do with pass pass -SMS as an argument.


By Default  TextShift, inherits whatever the default encoding for text is on your system as specfied by C#'s Encoding.Default class. 
You can overwrite this on both -Source and -Target text locations by specifiying -IN:XXX for the source and -OUT:XXX for the target.


XXX can be one of these and arguments work for both -IN and -OUT. For example -IN:ANSI and -OUT:UNICODE

ASCII -> Plain ANSI 8-bit Text
ANSI -> Same

UTF7 -> 7-bit unicode

UTF8 -> 8 bit unicode

UTF32 -> Unicode encoding to use 4 bytes per char


BigEndianUnicode -> 16-bit Bit Endian Encoding


LilEndianUnicode -> 16-bit Little Endian Encoding
Unicode   -> same
