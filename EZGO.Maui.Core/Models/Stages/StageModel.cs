using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using EZGO.Api.Models;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Tasks;
using NodaTime;

namespace EZGO.Maui.Core.Models.Stages
{
    public class StageModel : Stage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<TasksTaskModel> Tasks { get; set; }

        public bool IsHeaderVisible { get; set; } = true;

        public int ExpandedAccordionHeight { get; set; }

        public bool IsAccordionExpanded { get; set; }

        public List<int> SignedByUserIds => Signatures?
                                                    .Where(x => x.SignedById.HasValue)
                                                    .Select(x => x.SignedById.Value)
                                                    .ToList();

        public Signature FirstSignature { get => Signatures?.FirstOrDefault() ?? new Signature { SignedBy = "Unknown" }; }
        public Signature SecondSignature { get => Signatures?.ElementAtOrDefault(1) ?? null; }

        public List<SignatureModel> SignaturesList
        {
            get => Signatures?.Select(s => new SignatureModel
            {
                SignatureImage = s.SignatureImage,
                SignedAt = s.SignedAt,
                SignedBy = s.SignedBy,
                SignedById = s.SignedById,
                SignedByPicture = s.SignedByPicture
            }).ToList();
        }

        public LocalDateTime? LocalFirstSignedAt
        {
            get
            {
                if (FirstSignature.SignedAt.HasValue)
                    return Settings.ConvertDateTimeToLocal(FirstSignature.SignedAt.Value.ToLocalTime());
                return null;
            }
        }

        public bool ContainsTags => Tags?.Count > 0;

        public bool HasSignature => Signatures?.FirstOrDefault().SignatureImage != null;

        public bool HasNotes => !string.IsNullOrEmpty(ShiftNotes);

        public bool HasNotesOrSignature => HasNotes || HasSignature;
    }
}