using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ProtobufCSharp
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("proto")]
    internal class ProtobufClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry = null;

        static ProtobufClassifier diffClassifier;

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            if (diffClassifier == null)
                diffClassifier = new ProtobufClassifier(ClassificationRegistry);

            return diffClassifier;
        }
    }
}
