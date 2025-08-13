using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using EZGO.Api.Models;
using EZGO.Maui.Core.Interfaces.Sign;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.Signatures
{
    public class SignatureHelperControl : NotifyPropertyChanged
    {
        private readonly ISignService _signService;
        private readonly ISignatureService signatureService;

        private StreamImageSource signatureStream;
        private StreamImageSource signature2Stream;

        private Stream firstSignatureStream;
        private Stream secondSignatureStream;

        public bool IsDoubleSignatureRequired { get; set; } = false;

        public string CoSignerName { get; set; }

        public int CoSignerId { get; set; }

        public ICommand ResetSignatureCommand { get; private set; }

        public ICommand ResetSignature2Command { get; private set; }

        public SignatureHelperControl(ISignService signService) : this()
        {
            _signService = signService;
        }

        public SignatureHelperControl()
        {
            using var scope = App.Container.CreateScope();
            signatureService = scope.ServiceProvider.GetService<ISignatureService>();

            ResetSignatureCommand = new Command(() => { ResetSignature(Constants.ResetSignatureMessage); });
            ResetSignature2Command = new Command(() => { ResetSignature(Constants.ResetSignature2Message); });
        }

        public bool SaveSignatureStreams(StreamImageSource firstSignature, StreamImageSource secondSignature)
        {
            try
            {
                bool result = false;

                if (firstSignature != null)
                {
                    signatureStream = firstSignature;

                    if (IsDoubleSignatureRequired && secondSignature != null)
                    {
                        signature2Stream = secondSignature;
                        result = true;
                    }
                    else
                        result = true;
                }

                return result;
            }
            catch
            {
                Debug.WriteLine("Encountered an error during saving the signatures");
                return false;
            }
        }

        private async Task SaveStreams()
        {
            if (signatureStream != null)
                firstSignatureStream = await signatureStream.Stream.Invoke(new System.Threading.CancellationToken());

            if (IsDoubleSignatureRequired && signature2Stream != null)
                secondSignatureStream = await signature2Stream.Stream.Invoke(new System.Threading.CancellationToken());
        }

        public async Task Submit(PostTemplateModel model)
        {
            var signatures = await SaveAndGetSignatures();

            model.Signatures = signatures;

            if (model.Id == 0)
            {
                model.CreatedBy = UserSettings.Fullname;
                model.CreatedById = UserSettings.Id;
            }
            else
            {
                model.ModifiedBy = UserSettings.Fullname;
                model.ModifiedById = UserSettings.Id;
            }

            await _signService.PostAndSignTemplateAsync(model);
        }

        public async Task<List<Signature>> SaveAndGetSignatures()
        {
            await SaveStreams();

            string signatureFilename = await signatureService.SaveSignatureAsync(firstSignatureStream);

            List<Signature> signatures = new List<Signature>
                {
                    new Signature
                    {
                        SignatureImage = signatureFilename,
                        SignedBy = UserSettings.Fullname,
                        SignedById = UserSettings.Id,
                        SignedAt = DateTime.UtcNow
                    }
                };

            if (IsDoubleSignatureRequired)
            {
                signatureFilename = await signatureService.SaveSignatureAsync(secondSignatureStream);

                signatures.Add(new Signature
                {
                    SignatureImage = signatureFilename,
                    SignedBy = CoSignerName,
                    SignedById = CoSignerId,
                    SignedAt = DateTime.UtcNow
                });
            }

            return signatures;
        }

        private void ResetSignature(string message) => MessagingCenter.Send(this, message);
    }
}
