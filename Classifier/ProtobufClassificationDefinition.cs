using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ProtobufCSharp
{
    internal static class ProtobufClassificationDefinition
    {
        #region Content Type and File Extension Definitions

        [Export]
        [Name("proto")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition ProtobufContentTypeDefinition = null;

        [Export]
        [FileExtension(".proto")]
        [ContentType("proto")]
        internal static FileExtensionToContentTypeDefinition ProtobufFileExtensionDefinition = null;

        #endregion

        #region Classification Type Definitions

        [Export]
        [Name("proto")]
        internal static ClassificationTypeDefinition protobufClassificationDefinition = null;

        [Export]
        [Name("proto.operator")]
        [BaseDefinition("proto")]
        internal static ClassificationTypeDefinition protobufOperatorDefinition = null;

        [Export]
        [Name("proto.comment")]
        [BaseDefinition("proto")]
        internal static ClassificationTypeDefinition protobufCommentDefinition = null;
        
        #endregion

        #region Classification Format Productions

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "proto.operator")]
        [Name("proto.operator")]
        internal sealed class ProtobufOperatorFormat : ClassificationFormatDefinition
        {
            public ProtobufOperatorFormat()
            {
                this.ForegroundColor = Color.FromRgb(255, 128, 0);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "proto.comment")]
        [Name("proto.comment")]
        internal sealed class ProtobufCommentFormat : ClassificationFormatDefinition
        {
            public ProtobufCommentFormat()
            {
                this.ForegroundColor = Color.FromRgb(87, 166, 74);
            }
        }

        #endregion
    }
}
