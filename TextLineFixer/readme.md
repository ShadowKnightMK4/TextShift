
Textshift came as a result of Wanting an easy way to fix mismatched line endings ('\r\n') chars.

It also has support for converting between multiple encodings and what to do with excess space chars. Below
is the list of commands. Commands are NOT case sensitive. -Soure is the same as -SoUrCe and so on.
Additional '-' or '/' will work to mark arguments, this means -Source is the same as /Source.
Additionally, most commands has a shorter version you can type. For example, -S is /SOURCE

/?
/HELP
	This displays the Usage.txt file contained within Textshift.

-S "SourceFile"
-SOURCE  "SourceFile"

	This command specifies a source to get text from. The file must exist and the user this program
is running under must have access to the file. If either of these cases aren't true, the app exists.


-T  "TargetFile"
-TARGET "TargetFile"

	This commands specifies a target to send the processed text too. The target must exist and the usr 
must also have access to the file. The app quits if these cases aren't true.

-TC
-TARGET:CON

This command tells the app to route the processed text to the console.



-LMN
-LINEMODE:NONE
This is a default. This means that line ending chars '\r' (10) and '\n' (13) are not touched.


-LMW
-LINEMODE:WINDOWS
This means that line ending such as '\r' and '\n' will be changed to '\r\n'. This is the usual ending for Windows based text.


-LMU
-LINEMODE:UNIX
This means that line endings like '\r', '\r\n' will be changed to '\n'


-LMC
-LINEMODE:MAC:
This means that line endings like '\r', '\r\n' will be changed to '\n'


-SMS
-SPACEMODE:SINGLE
This means that runs of non linding spaces will be replaced with a single whitespace.
The last -SPACEMODE is the one that will count.

-SMN
-SPACEMODE:NONE
This is a default. This means leave spaces alone. Important. The last -SPACEMODE is the one that will count.


-F
-FORCE
-ALLOWOVERWRITE
For non -TARGET:CON,  this means that you are ok with overwriting the output file. By default, TextShift won't do that.


-IN:ANSI
This says that the source is ended as 8-bit ANSI chars.
-IN:UNICODE
This says that the source is 16-bit Unicode


-OUT:ANSI
This says that the target should be encoded as 8-bit ASCII chars.
-OUT:UNICODE
This says that the target should be encoded as 16-bit Unicode



Additionally, the -Out and -IN command also support these encodings from the C# Encoding class.

BigEndianUnicode
UTF7 -> a 7-bit? encoding encoding
UTF8 -> a 1 byte? unicode encoding
UTF32 -> a 4 byte enicode encoding
LilEndianUnicode -> same as UNICODE
