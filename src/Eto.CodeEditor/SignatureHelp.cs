using System;
using System.Collections.Generic;
using System.Linq;

namespace Eto.CodeEditor
{
    public class Parameter
    {
        public String TypeName { get; }
        public String Name { get; }
    }

    public class Signature
    {
        public Signature(string signatureString, Action<string> logger = null)
        {
            this.logger = logger;
            if (signatureString == null || signatureString == "")
                throw new ArgumentNullException();
            var s = signatureString;
            var methodName = s.Substring(0, s.IndexOf('('));
            var shortMethodName = methodName.Substring(s.LastIndexOf('.') + 1);
            var parameterString = s.Substring(s.IndexOf('('));
            //parameterString = parameterString.Replace(" ", "X");
            parameterCount = parameterString.Replace(" ", "").Equals("()") ? 0 : parameterString.Cast<char>().Count(c => c == parameterSeparator) + 1;
            
            Display = $"{shortMethodName}{parameterString}";
            parameterStartAndEndIndexes = ParseParamStartEnd(Display);
            if (parameterCount > 0)
                currentParameterIndex = 0;
        }

        public string Display { get;}
        public bool HasMultipleParameters => parameterCount > 1;
        public bool HasNoParameters => parameterCount == 0;

        public Tuple<int, int> CurrentParameterStartAndEndIndex => currentParameterIndex == -1 ? Tuple.Create(-1, -1) : parameterStartAndEndIndexes[currentParameterIndex];

        private int parameterCount;
        private int currentParameterIndex = -1;
        private readonly List<Tuple<int,int>> parameterStartAndEndIndexes = null;
        private const char parameterSeparator = ',';
        private List<Tuple<int,int>> ParseParamStartEnd(string signatureStr)
        {
            logger?.Invoke($"ParseParamStartEnd: sig:{signatureStr}");
            var chars = new char[]{ '(', parameterSeparator, ')'};

            IEnumerable<int> indexesBeforeAndAfterPositions = signatureStr.Cast<char>()
              .Select((c, i) => chars.Contains(c) ? i : -1)
              .Where(i => i != -1)
              .SelectMany(i => new int[] { i - 1, i + 1 });

            IEnumerable<int> indexesBeforeAndAfterPositionsDiscardFirstAndLast = indexesBeforeAndAfterPositions
             .Skip(1)
             .TakeWhile(x => x != indexesBeforeAndAfterPositions.Last());

            IEnumerable<int> even = indexesBeforeAndAfterPositionsDiscardFirstAndLast.Where((i, idx) => idx % 2 == 0);
            IEnumerable<int> odd = indexesBeforeAndAfterPositionsDiscardFirstAndLast.Where((i, idx) => idx % 2 != 0);
            IEnumerable<Tuple<int, int>> startAndEnds = even.Zip(odd, (first,second) => Tuple.Create(first, second));

            startAndEnds = startAndEnds.Select(t => Tuple.Create(t.Item1, t.Item2 + 1)); // second param is not inclusive
#if DEBUG
            foreach (var t in startAndEnds)
                logger?.Invoke($"s:{t.Item1}, e:{t.Item2}");
#endif
            return startAndEnds.ToList();
        }
        private Action<string> logger;
    }

    public class Signatures
    {
        private Action<string> logger;

        public Signatures(List<string> signatureStrings, Action<string> logger)
        {
            this.logger = logger;
            if (signatureStrings == null || signatureStrings.Count == 0)
                throw new ArgumentNullException();
            this.signatures = signatureStrings.Select(s => new Signature(s, logger)).ToList();
        }

        public bool HasOverloads => signatures.Count > 1;

        public string CurrentSignatureDisplay => $"{DisplayPrefix}{signatures[currentSignatureIndex].Display}";

        public bool CurrentSignatureHasMultipleParameters => signatures[currentSignatureIndex].HasMultipleParameters;

        public bool CurrentSignatureHasNoParameters => signatures[currentSignatureIndex].HasNoParameters;

        public Tuple<int,int> CurrentSignatureCurrentParameterIndexes => signatures[currentSignatureIndex].CurrentParameterStartAndEndIndex;

        public void SetPreviousAsCurrent() => SetCurrent(false);

        public void SetNextAsCurrent() => SetCurrent(true);

        public string DisplayPrefix => signatures.Count < 2
            ? ""
            : $"{Convert.ToChar(0x1)} {currentSignatureIndex+1} of {signatures.Count} {Convert.ToChar(0x2)} ";

        private void SetCurrent(bool next)
        {
            logger?.Invoke($"csi:{currentSignatureIndex}, cnt:{signatures.Count}");
            if (next)
            {
                currentSignatureIndex++;
                if (currentSignatureIndex >= signatures.Count)
                    currentSignatureIndex = 0;
            }
            else
            {
                currentSignatureIndex--;
                if (currentSignatureIndex < 0)
                    currentSignatureIndex = signatures.Count - 1;
            }
        }

        private int currentSignatureIndex;
        private readonly List<Signature> signatures = null;
    }
}
