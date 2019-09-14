using System;
using System.Reflection;
using Eto.Forms;
using System.Text.RegularExpressions;
using System.Linq;

namespace Eto.CodeEditor
{
    [Handler(typeof(IHandler))]
    public class CodeEditor : Control
    {
        static string[] GetKeywords(ProgrammingLanguage language)
        {
            switch (language)
            {
                case ProgrammingLanguage.CSharp:
                    return new string[]
                    {
                    "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while",
                    "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void"
                    };
                case ProgrammingLanguage.GLSL:
                    {
                        LanguageGlsl.GetKeywords(out string k, out string f);
                        return new string[] { k, f };
                    }
                case ProgrammingLanguage.Python:
                    return new string[]
                    {
                        "and as assert break class continue def del elif else except exec finally for from global if import in is lambda not or pass print raise return try while with yield",
                        "None is not def"
                    };
                case ProgrammingLanguage.VB:
                    {
                        LanguageVbNet.GetKeywords(out string k, out string f);
                        return new string[] { k, f };
                    }
                default:
                    return new string[0];
            }
        }

        readonly ProgrammingLanguage _language;
        public CodeEditor(ProgrammingLanguage language, bool darkMode=false)
        {
            AutoIndentEnabled = true;
            _language = language;
            Handler.SetProgrammingLanguage( language, GetKeywords(language) );

            Handler.CharAdded += CodeEditor_CharAdded;
            var backgroundColor = darkMode ? Eto.Drawing.Color.FromArgb(30,30,30) : Eto.Drawing.Colors.White;
            SetColor(Section.Default, darkMode ? Drawing.Color.FromArgb(212,212,212) : Drawing.Colors.Black, backgroundColor);
            SetColor(Section.Comment, darkMode ? Drawing.Color.FromArgb(106, 153, 85) : Drawing.Colors.DarkGray, backgroundColor);
            SetColor(Section.Keyword1, darkMode ? Drawing.Color.FromArgb(197, 134, 192) : Drawing.Colors.Blue, backgroundColor);
            SetColor(Section.Keyword2, darkMode ? Drawing.Color.FromArgb(197, 134, 192) : Drawing.Colors.Blue, backgroundColor);
            SetColor(Section.Strings, darkMode ? Drawing.Color.FromArgb(206, 145, 120) : Drawing.Color.FromArgb(163, 21, 21), backgroundColor);
            SetColor(Section.LineNumber, darkMode ? Drawing.Color.FromArgb(160, 160, 160) : Drawing.Colors.Gray, backgroundColor);
            SetColor(Section.DefName, darkMode ? Drawing.Color.FromArgb(220, 220, 170) : Drawing.Color.FromArgb(64, 174, 215), backgroundColor);
            SetColor(Section.Preprocessor, darkMode ? Drawing.Colors.DarkGray : Drawing.Colors.DarkGray, backgroundColor);
        }

        void CodeEditor_CharAdded(object sender, CharAddedEventArgs e)
        {
            if (AutoIndentEnabled)
                AutoIndent.IndentationCheck(e.Char, this);
        }

        new IHandler Handler => (IHandler)base.Handler;

        public bool AutoIndentEnabled { get; set; }

        public string Text
        {
            get => Handler.Text;
            set => Handler.Text = value;
        }

        public ProgrammingLanguage Language
        {
            get => _language;
        }

        public string FontName
        {
            get => Handler.FontName;
            set => Handler.FontName = value;
        }

        public int FontSize
        {
            get => Handler.FontSize;
            set => Handler.FontSize = value;
        }

        public int TabWidth
        {
            get => Handler.TabWidth;
            set => Handler.TabWidth = value;
        }

        public bool ReplaceTabsWithSpaces
        {
            get => Handler.ReplaceTabsWithSpaces;
            set => Handler.ReplaceTabsWithSpaces = value;
        }

        public int LineNumberColumnWidth
        {
            get => Handler.LineNumberColumnWidth;
            set => Handler.LineNumberColumnWidth = value;
        }

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            Handler.SetColor(section, foreground, background);
        }

        public int CurrentPosition
        {
            get => Handler.CurrentPosition;
            set => Handler.CurrentPosition = value;
        }

        public int CurrentPositionInLine => Handler.CurrentPositionInLine;

        public int CurrentLineNumber => Handler.CurrentLineNumber;

        public int GetLineIndentation(int lineNumber) => Handler.GetLineIndentation(lineNumber);
        public void SetLineIndentation(int lineNumber, int indentation) => Handler.SetLineIndentation(lineNumber, indentation);

        public char GetLineLastChar(int lineNumber) => Handler.GetLineLastChar(lineNumber);

        public string GetLineText(int lineNumber) => Handler.GetLineText(lineNumber);

        public int GetLineLength(int lineNumber) => Handler.GetLineLength(lineNumber);

        public void SetupIndicatorStyles()
        {
            Handler.SetupIndicatorStyles();
        }
        public void ClearAllErrorIndicators()
        {
            Handler.ClearAllErrorIndicators();
        }
        public void ClearAllWarningIndicators()
        {
            Handler.ClearAllWarningIndicators();
        }
        public void ClearAllTypeNameIndicators()
        {
            Handler.ClearAllTypeNameIndicators();
        }
        public void AddErrorIndicator(int position, int length)
        {
            Handler.AddErrorIndicator(position, length);
        }
        public void AddWarningIndicator(int position, int length)
        {
            Handler.AddWarningIndicator(position, length);
        }
        public void AddTypeNameIndicator(int position, int length)
        {
            Handler.AddTypeNameIndicator(position, length);
        }

        public bool IsWhitespaceVisible => Handler.IsWhitespaceVisible;
        public void ShowWhitespace() => Handler.ShowWhitespace();
        public void HideWhitespace() => Handler.HideWhitespace();
        public bool AreIndentationGuidesVisible => Handler.AreIndentationGuidesVisible;
        public void ShowIndentationGuides() => Handler.ShowIndentationGuides();
        public void HideIndentationGuides() => Handler.HideIndentationGuides();

        public void ShowWhitespaceWithColor(Eto.Drawing.Color color) => Handler.ShowWhitespaceWithColor(color);

        public bool AutoCompleteActive { get => Handler.AutoCompleteActive; }
        public void InsertText(int position, string text) { Handler.InsertText(position, text); }
        public void DeleteRange(int position, int length) { Handler.DeleteRange(position, length); }

        public void ReplaceTarget(string text, int start, int end) => Handler.ReplaceTarget(text, start, end);
        public void ReplaceFirstOccuranceInLine(string oldText, string newText, int lineNumber) =>
            Handler.ReplaceFirstOccuranceInLine(oldText, newText, lineNumber);

        public int WordStartPosition(int position, bool onlyWordCharacters) { return Handler.WordStartPosition(position, onlyWordCharacters); }
        public string GetTextRange(int position, int length) { return Handler.GetTextRange(position, length); }
        public void AutoCompleteShow(int lenEntered, string list) { Handler.AutoCompleteShow(lenEntered, list); }


        public event EventHandler<CharAddedEventArgs> CharAdded
        {
            add { Handler.CharAdded += value; }
            remove { Handler.CharAdded -= value; }
        }

        public event EventHandler<TextChangedEventArgs> TextChanged
        {
            add { Handler.TextChanged += value; }
            remove { Handler.TextChanged -= value; }
        }

        // only call from InsertCheck handler
        public void ChangeInsertion(string text)
        {
            Handler.ChangeInsertion(text);
        }



        public new interface IHandler : Control.IHandler
        {
            string Text { get; set; }
            void SetProgrammingLanguage(ProgrammingLanguage language, string[] keywordSets);
            string FontName { get; set; }
            int FontSize { get; set; }
            int TabWidth { get; set; }
            bool ReplaceTabsWithSpaces { get; set; }
            int LineNumberColumnWidth { get; set; }
            void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background);
            int CurrentPosition { get; set; }
            int CurrentPositionInLine { get; }
            int CurrentLineNumber { get; }

            int GetLineIndentation(int lineNumber);
            void SetLineIndentation(int lineNumber, int indentation);

            char GetLineLastChar(int lineNumber);

            string GetLineText(int lineNumber);
            int GetLineLength(int lineNumber);

            void SetupIndicatorStyles();
            void ClearAllErrorIndicators();
            void ClearAllWarningIndicators();
            void ClearAllTypeNameIndicators();
            void AddErrorIndicator(int position, int length);
            void AddWarningIndicator(int position, int length);
            void AddTypeNameIndicator(int position, int length);
            bool IsWhitespaceVisible { get; }
            void ShowWhitespace();
            void HideWhitespace();
            void ShowWhitespaceWithColor(Eto.Drawing.Color color);
            bool AreIndentationGuidesVisible { get; }
            void ShowIndentationGuides();
            void HideIndentationGuides();

            bool AutoCompleteActive { get; }
            void InsertText(int position, string text);
            int ReplaceTarget(string text, int start, int end);
            void ReplaceFirstOccuranceInLine(string oldText, string newText, int lineNumber);
            void DeleteRange(int position, int length);
            int WordStartPosition(int position, bool onlyWordCharacters);
            string GetTextRange(int position, int length);
            void AutoCompleteShow(int lenEntered, string list);


            event EventHandler<CharAddedEventArgs> CharAdded;
            event EventHandler<TextChangedEventArgs> TextChanged;
            event EventHandler<InsertCheckEventArgs> InsertCheck;
            void ChangeInsertion(string text); // only call from InsertCheck handler
        }
    }

    public enum Section
    {
        Default,
        Comment,
        Keyword1,
        Keyword2,
        Strings,
        LineNumber,
        DefName,
        Preprocessor
    }

    public enum ProgrammingLanguage
    {
        None,
        CSharp,
        GLSL,
        VB,
        Python
    }

    class LanguageGlsl
    {
        public static void GetKeywords(out string keywords, out string functions)
        {
            keywords = _keywords.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ");
            functions = _functions.Replace('\r', ' ').Replace('\n', ' ').Replace("  ", " ");
        }
        const string _keywords = @"attribute const uniform varying layout
centroid flat smooth noperspective
patch sample
break continue do for while switch case default
if else
subroutine
in out inout
float double int void bool true false
invariant
discard return
mat2 mat3 mat4 dmat2 dmat3 dmat4
mat2x2 mat2x3 mat2x4 dmat2x2 dmat2x3 dmat2x4
mat3x2 mat3x3 mat3x4 dmat3x2 dmat3x3 dmat3x4
mat4x2 mat4x3 mat4x4 dmat4x2 dmat4x3 dmat4x4
vec2 vec3 vec4 ivec2 ivec3 ivec4 bvec2 bvec3 bvec4 dvec2 dvec3 dvec4
uint uvec2 uvec3 uvec4
lowp mediump highp precision
sampler1D sampler2D sampler3D samplerCube
sampler1DShadow sampler2DShadow samplerCubeShadow
sampler1DArray sampler2DArray
sampler1DArrayShadow sampler2DArrayShadow
isampler1D isampler2D isampler3D isamplerCube
isampler1DArray isampler2DArray
usampler1D usampler2D usampler3D usamplerCube
usampler1DArray usampler2DArray
sampler2DRect sampler2DRectShadow isampler2DRect usampler2DRect
samplerBuffer isamplerBuffer usamplerBuffer
sampler2DMS isampler2DMS usampler2DMS
sampler2DMSArray isampler2DMSArray usampler2DMSArray
samplerCubeArray samplerCubeArrayShadow isamplerCubeArray usamplerCubeArray
struct
";

        const string _functions = @"radians
degrees
sin
cos
tan
asin
acos
atan
pow
exp
log
exp2
log2
sqrt
inversesqrt
abs
sign
floor
ceil
fract
mod
min
max
clamp
mix
step
smoothstep
length
distance
dot
cross
normalize
faceforward
reflect
refract
matrixCompMult
lessThan
lessThanEqual
greaterThan
greaterThanEqual
equal
notEqual
any
all
not
dFdx
dFdy
fwidth
noise1
noise2
noise3
noise4
outerProduct
transpose
trunc
round
roundEven
modf
isnan
isinf
sinh
cosh
tanh
asinh
acosh
atanh
textureSize
texture
textureProj
textureLod
textureOffset
texelFetch
texelFetchOffset
textureProjOffset
textureLodOffset
textureProjLod
textureProjLodOffset
textureGrad
textureGradOffset
textureProjGrad
textureProjGradOffset
determinant
inverse
EmitVertex
EndPrimitive
packSnorm2x16
unpackUnorm2x16
packUnorm2x16
unpackUnorm2x16
packHalf2x16
unpackHalf2x16
floatBitsToInt
floatBitsToUint
intBitsToFloat
uintBitsToFloat
fma
barrier
interpolateAtCentroid
interpolateAtSample
interpolateAtOffset
frexp
ldexp
packUnorm2x16
packUnorm4x8
packSnorm4x8
unpackUnorm2x16
unpackSnorm2x16
unpackUnorm4x8
unpackSnorm4x8
packDouble2x32
unpackDouble2x32
uaddCarry
usubBorrow
umulExtended
imulExtended
bitfieldExtract
bitfieldInsert
bitfieldReverse
bitCount
findLSB
findMSB
textureQueryLod
textureGather
textureGatherOffset
textureGatherOffsets
EmitStreamVertex
EndStreamPrimitive
packSnorm2x16
unpackSnorm2x16
packHalf2x16
unpackHalf2x16
atomicCounterIncrement
atomicCounterDecrement
atomicCounter
memoryBarrier
imageLoad
imageStore
imageAtomicAdd
imageAtomicMin
imageAtomicMax
imageAtomicAnd
imageAtomicOr
imageAtomicXor
imageAtomicExchange
imageAtomicCompSwap
textureQueryLevels
atomicAdd
atomicMin
atomicMax
atomicAnd
atomicOr
atomicXor
atomicExchange
atomicCompSwap
imageSize
memoryBarrierAtomicCounter
memoryBarrierBuffer
memoryBarrierShared
memoryBarrierImage
groupMemoryBarrier
";
    }


    class LanguageVbNet
    {
        public static void GetKeywords(out string keywords, out string functions)
        {
            keywords = _keywords.Replace("\r", " ").Replace("\n", " ").Replace("  ", " ");
            functions = _functions.Replace('\r', ' ').Replace('\n', ' ').Replace("  ", " ");
        }
        const string _keywords = @"debug
release
addhandler
addressof
aggregate
alias
and
andalso
ansi
as
assembly
auto
binary
boolean
byref
byte
byval
call
case
catch
cbool
cbyte
cchar
cdate
cdbl
cdec
char
cint
class
clng
cobj
compare
const
continue
csbyte
cshort
csng
cstr
ctype
cuint
culng
cushort
custom
date
decimal
declare
default
delegate
dim
directcast
distinct
do
double
each
else
elseif
end
endif
enum
equals
erase
error
event
exit
explicit
false
finally
for
friend
from
function
get
gettype
getxmlnamespace
global
gosub
goto
group
handles
if
implements
imports
in
inherits
integer
interface
into
is
isfalse
isnot
istrue
join
key
let
lib
like
long
loop
me
mid
mod
module
mustinherit
mustoverride
my
mybase
myclass
namespace
narrowing
new
next
not
nothing
notinheritable
notoverridable
object
of
off
on
operator
option
optional
or
order
orelse
overloads
overridable
overrides
paramarray
partial
preserve
private
property
protected
public
raiseevent
readonly
redim
rem
removehandler
resume
return
sbyte
select
set
shadows
shared
short
single
skip
static
step
stop
strict
string
structure
sub
synclock
take
text
then
throw
to
true
try
trycast
typeof
uinteger
ulong
unicode
until
ushort
using
variant
wend
when
where
while
widening
with
withevents
writeonly
xor";

        const string _functions = @"
";
    }

}
