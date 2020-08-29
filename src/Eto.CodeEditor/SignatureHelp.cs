using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eto.CodeEditor
{
    public class Parameter
    {
        public String TypeName { get; }
        public String Name { get; }
    }

    public class Signature
    {
        public Signature(string signatureString)
        {
            if (signatureString == null || signatureString == "")
                throw new ArgumentNullException();
            var s = signatureString;
            var methodName = s.Substring(0, s.IndexOf('('));
            var shortMethodName = methodName.Substring(s.LastIndexOf('.') + 1);
            var parameters = s.Substring(s.IndexOf('('));
            Display = $"{shortMethodName}{parameters}";
        }

        public void AddParameter(Parameter parameter) => parameters.Add(parameter);
        public string Display { get;}

        private readonly List<Parameter> parameters = new List<Parameter>();
        private const char parameterSeparator = ',';
    }

    public class Signatures
    {
        private Action<string> logger;
        public static Signatures Create(List<string> signatureStrings, Action<string> logger = null)
        {
            var signatures = signatureStrings
                .Select(s => new Signature(s));
            return new Signatures(signatures.ToList(), logger);
        }

        private Signatures(List<Signature> signatures, Action<string> logger)
        {
            this.logger = logger;
            if (signatures == null || signatures.Count == 0)
                throw new ArgumentNullException();
            this.signatures = signatures;
        }

        public bool HasOverloads => signatures.Count > 1;

        public string CurrentSignatureDisplay => $"{displayPrefix}{signatures[currentSignatureIndex].Display}";

        public void SetPreviousAsCurrent() => SetCurrent(false);

        public void SetNextAsCurrent() => SetCurrent(true);

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
        private string displayPrefix => signatures.Count < 2
            ? ""
            : $"{Convert.ToChar(0x1)} {currentSignatureIndex+1} of {signatures.Count} {Convert.ToChar(0x2)} ";
    }
}
