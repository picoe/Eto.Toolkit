using System;
using System.Reflection;
using Eto.Forms;

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
                        string k, f;
                        LanguageGlsl.GetKeywords(out k, out f);
                        return new string[] { k, f };
                    }
                case ProgrammingLanguage.Python:
                    return new string[]
                    {
                        // import keyword
                        // keyword.kwlist
                        "and as assert break class continue def del elif else except exec finally for from global if import in is lambda not or pass print raise return try while with yield"
                    };
                default:
                    return new string[0];
            }
        }

        readonly ProgrammingLanguage _language;
        public CodeEditor(ProgrammingLanguage language)
        {
            _language = language;
            Handler.SetProgrammingLanguage( language, GetKeywords(language) );

            SetColor(Section.Comment, Drawing.Colors.DarkGray, Drawing.Colors.White);
            SetColor(Section.Keyword, Drawing.Colors.Blue, Drawing.Colors.White);
            SetColor(Section.LineNumber, Drawing.Colors.Gray, Drawing.Colors.White);
        }

        new IHandler Handler => (IHandler)base.Handler;

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

        public int LineNumberColumnWidth
        {
            get => Handler.LineNumberColumnWidth;
            set => Handler.LineNumberColumnWidth = value;
        }

        public void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background)
        {
            Handler.SetColor(section, foreground, background);
        }

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

        public void ShowWhitespace() => Handler.ShowWhitespace();

        public void ShowWhitespaceWithColor(Eto.Drawing.Color color) => Handler.ShowWhitespaceWithColor(color);

        public event EventHandler TextChanged
        {
            add
            {
                Handler.TextChanged += value;
            }
            remove
            {
                Handler.TextChanged -= value;
            }
        }

        public new interface IHandler : Control.IHandler
        {
            string Text { get; set; }
            void SetProgrammingLanguage(ProgrammingLanguage language, string[] keywordSets);
            string FontName { get; set; }
            int FontSize { get; set; }
            int LineNumberColumnWidth { get; set; }
            void SetColor(Section section, Eto.Drawing.Color foreground, Eto.Drawing.Color background);

            void SetupIndicatorStyles();
            void ClearAllErrorIndicators();
            void ClearAllWarningIndicators();
            void ClearAllTypeNameIndicators();
            void AddErrorIndicator(int position, int length);
            void AddWarningIndicator(int position, int length);
            void AddTypeNameIndicator(int position, int length);
            void ShowWhitespace();
            void ShowWhitespaceWithColor(Eto.Drawing.Color color);

            event EventHandler TextChanged;
        }
    }

    public enum Section
    {
        Comment,
        Keyword,
        LineNumber
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
}
