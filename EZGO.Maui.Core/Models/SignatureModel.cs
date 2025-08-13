using EZGO.Api.Models;

namespace EZGO.Maui.Core.Models
{
    public class SignatureModel : EZGO.Api.Models.Signature
    {
        /// <summary>
        /// used for first tappings, remember status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// used in offline mode, default true
        /// </summary>
        public bool IsLocal { get; set; } = true;

        public Signature ToSignature()
        {
            return new Signature()
            {
                SignatureImage = SignatureImage,
                SignedAt = SignedAt,
                SignedBy = SignedBy,
                SignedById = SignedById,
                SignedByPicture = SignedByPicture,
            };
        }

        public SignatureModel() { }

        public SignatureModel(Signature signature, bool isLocal = false)
        {
            SignatureImage = signature.SignatureImage;
            SignedAt = signature.SignedAt;
            SignedBy = signature.SignedBy;
            SignedById = signature.SignedById;
            SignedByPicture = signature.SignedByPicture;
            IsLocal = isLocal;
        }
    }
}
