using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace ProtobufCSharp
{
    public class ProtobufClassifier : IClassifier
    {
        private readonly IClassificationTypeRegistryService mClassificationTypeRegistry;
        private static Dictionary<string, object> mOperators = new Dictionary<string, object>()
        {
            { "message", null },

            { "double", null },
            { "float", null },
            { "int32", null },
            { "int64", null },
            { "uint32", null },
            { "uint64", null },
            { "sint32", null },
            { "sint64", null },
            { "fixed32", null },
            { "fixed64", null },
            { "sfixed32", null },
            { "sfixed64", null },
            { "bool", null },
            { "string", null },
            { "bytes", null },
            { "enum", null },
            { "repeated", null },
            { "import", null },
            { "public", null },
            { "Any", null },
            { "oneof", null },
            { "map", null },
            { "syntax", null },
            { "package", null },
            { "option", null },
            { "csharp_namespace", null },
            { "service", null },
            { "rpc", null },
        };

        internal ProtobufClassifier(IClassificationTypeRegistryService registry)
        {
            mClassificationTypeRegistry = registry;
        }

        #region Public Events
#pragma warning disable 67
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
#pragma warning restore 67
        #endregion // Public Events

        #region Public Methods
        /// <summary>
        /// Classify the given spans, which, for proto files, classifies
        /// a line at a time.
        /// </summary>
        /// <param name="span">The span of interest in this projection buffer.</param>
        /// <returns>The list of <see cref="ClassificationSpan"/> as contributed by the source buffers.</returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;

            var spans = new List<ClassificationSpan>();

            if (snapshot.Length == 0)
                return spans;


            var startno = span.Start.GetContainingLine().LineNumber;
            var endno = (span.End - 1).GetContainingLine().LineNumber;
            for (int i = startno; i <= endno; i++)
            {
                ITextSnapshotLine line = snapshot.GetLineFromLineNumber(i);
                var strings = new List<string>();

                var text = line.Extent.GetText();
                if (text != null && text.Trim().StartsWith("//"))
                {
                    var type = mClassificationTypeRegistry.GetClassificationType("proto.comment");
                    var ssp = new SnapshotPoint(line.Snapshot, line.Start.Position);
                    var ss = new SnapshotSpan(ssp, line.Length);
                    spans.Add(new ClassificationSpan(ss, type));
                }
                else
                {
                    var op = mOperators.Keys.FirstOrDefault(c => Regex.IsMatch(text, "\\b" + c + "\\b"));
                    while (op != null)
                    {
                        var match = Regex.Match(text, "\\b" + op + "\\b");
                        if (!match.Success)
                        {
                            break;
                        }

                        var index = match.Index;
                        if (index >= 0)
                        {
                            if (index > 0)
                            {
                                var previous = text.Substring(0, index);
                                strings.Add(text.Substring(0, index));
                                text = text.Substring(index);
                            }
                            strings.Add(text.Substring(0, op.Length));
                            text = text.Substring(op.Length);
                        }

                        op = mOperators.Keys.FirstOrDefault(c => Regex.IsMatch(text, "\\b" + c + "\\b"));
                    }
                    if (text.Length > 0)
                    {
                        strings.Add(text);
                    }

                    var l = 0;
                    foreach (var s in strings)
                    {
                        if (mOperators.ContainsKey(s))
                        {
                            var type = mClassificationTypeRegistry.GetClassificationType("proto.operator");
                            var ssp = new SnapshotPoint(line.Snapshot, line.Start.Position + l);
                            var ss = new SnapshotSpan(ssp, s.Length);
                            spans.Add(new ClassificationSpan(ss, type));
                        }
                        else
                        {
                            var type = mClassificationTypeRegistry.GetClassificationType("text");
                            var ssp = new SnapshotPoint(line.Snapshot, line.Start.Position + l);
                            var ss = new SnapshotSpan(ssp, s.Length);
                            spans.Add(new ClassificationSpan(ss, type));
                        }
                        l += s.Length;
                    }
                }
            }

            return spans;
        }


        #endregion
    }
}
